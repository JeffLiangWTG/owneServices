using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public partial class OrganizationBalanceSheet : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrganizationBalanceSheet(ZGuid primaryOrg, ZString primaryLedger, BusinessObjectFactory factory)
			: base(factory)
		{
			fPrimaryOrganization = primaryOrg;
			fPrimaryLedger = primaryLedger;
		}

		readonly ZGuid fPrimaryOrganization;
		readonly ZString fPrimaryLedger;

		public void AddIMatching(IMatching transaction)
		{
			Transactions.Add(transaction);
		}

		public void AddIMatchingCollection(IMatchingCollection transactions)
		{
			this.Transactions.AddRange(transactions);
		}

		// Determines whether to create Transfers or Contras based on the ledger types of 
		// existing transactions
		#region CreateDynamicTransactions

		public IMatchingCollection CreateDynamicTransactions()
		{
			CreateSubBalances(); // regenerates the collections of SubBalances because transaction may have been added

			DynamicTransactionCreator dynTransactCreator = null;

			if (ARSubBalances.Count == 0 && APSubBalances.Count != 0)
			{
				// have to create APTransfers
				dynTransactCreator = new DynamicTransactionCreatorAP(APSubBalances, fPrimaryOrganization, Factory);
			}
			else if (ARSubBalances.Count != 0 && APSubBalances.Count == 0)
			{
				// have to create ARTransfers
				dynTransactCreator = new DynamicTransactionCreatorAR(ARSubBalances, fPrimaryOrganization, Factory);
			}
			else if (ARSubBalances.Count != 0 && APSubBalances.Count != 0) // have to create Contras and Transfers
			{
				// Note: the choice of primary ledger is finalized here

				// If the primary organization has transactions in AR but no 
				// transactions in AP, use AR as primary ledger
				// If the primary organization has transactions in AP but no 
				// transactions in AR, use AP as primary ledger
				// If primary org has transactions in both AR and AP, 
				// use the user's choice of ledger as the primary ledger
				// Else use AR as the primary ledger

				// If primary ledger is AR, this means create Contras from AP to AR for non-Primary Org AP Transactions 
				// then create Transfers in AR for non-Primary Org AR Transactions.  
				// Do it other way round if primary ledger is AP

				OrganizationSubBalance possiblePrimaryOrgAR = ARSubBalances.GetOrganizationSubBalance(fPrimaryOrganization);
				OrganizationSubBalance possiblePrimaryOrgAP = APSubBalances.GetOrganizationSubBalance(fPrimaryOrganization);

				// Primary Org has transactions in AR but none in AP
				if (possiblePrimaryOrgAR != null && possiblePrimaryOrgAP == null)
				{
					dynTransactCreator =
						new DynamicTransactionCreatorPrimaryLedgerAR(ARSubBalances,
						APSubBalances, fPrimaryOrganization, Factory);
				}
				// Primary Org has transactions in AP but none in AR
				else if (possiblePrimaryOrgAP != null && possiblePrimaryOrgAR == null)
				{
					dynTransactCreator =
						new DynamicTransactionCreatorPrimaryLedgerAP(ARSubBalances,
						APSubBalances, fPrimaryOrganization, Factory);
				}
				// Primary Org has transactions in both AR and AP - let the user decide
				else if (possiblePrimaryOrgAP != null && possiblePrimaryOrgAR != null)
				{
					if (PrimaryOrgBizO.OH_IsCreditor)
					{
						dynTransactCreator = new DynamicTransactionCreatorPrimaryLedgerAP(ARSubBalances, APSubBalances, fPrimaryOrganization, Factory);
					}
					else
					{
						dynTransactCreator = new DynamicTransactionCreatorPrimaryLedgerAR(ARSubBalances, APSubBalances, fPrimaryOrganization, Factory);
					}
				}
				else
				{
					if (fPrimaryLedger == LedgerTypes.AccountsPayable)
					{
						dynTransactCreator = new DynamicTransactionCreatorPrimaryLedgerAP(ARSubBalances, APSubBalances, fPrimaryOrganization, Factory);
					}
					else if (fPrimaryLedger == LedgerTypes.AccountsReceivable)
					{
						dynTransactCreator = new DynamicTransactionCreatorPrimaryLedgerAR(ARSubBalances, APSubBalances, fPrimaryOrganization, Factory);
					}
				}
			}

			if (dynTransactCreator != null)
			{
				return dynTransactCreator.CreateTransactions();
			}
			else
			{
				return null;
			}
		}

		#endregion

		// Creates all Organization Sub-Balances for each ledger from the transactions in this.Transactions
		// **Main Reason for having a separate OrganizationBalanceSheet class
		#region CreateSubBalances

		public void CreateSubBalances()
		{
			// recreate the sub-balances
			fARSubBalances = null;
			fAPSubBalances = null;

			List<ZGuid> uniqueOrgs = GetUniqueOrganizations();
			foreach (ZGuid organization in uniqueOrgs)
			{
				ZDecimal totalARBalance = Transactions.GetOrganizationBalanceAmount(organization, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
				if (totalARBalance != 0)
				{
					ARSubBalances.Add(new OrganizationSubBalance(organization, totalARBalance, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
				}
				ZDecimal totalAPBalance = Transactions.GetOrganizationBalanceAmount(organization, ZArchitecture.Core.LedgerTypes.AccountsPayable);
				if (totalAPBalance != 0)
				{
					APSubBalances.Add(new OrganizationSubBalance(organization, totalAPBalance, ZArchitecture.Core.LedgerTypes.AccountsPayable));
				}
			}
		}

		#endregion

		protected List<ZGuid> GetUniqueOrganizations()
		{
			List<ZGuid> uniqueOrgs = new List<ZGuid>();
			foreach (IMatching transaction in Transactions)
			{
				if (!uniqueOrgs.Contains(transaction.Organisation))
				{
					uniqueOrgs.Add(transaction.Organisation);
				}
			}
			return uniqueOrgs;
		}

		// Transactions must contain transactions involving the primary 
		// organization
		#region Transactions

		public IMatchingCollection Transactions
		{
			get
			{
				if (fTransactions == null)
				{
					fTransactions = new IMatchingCollection(Factory);
				}
				return fTransactions;
			}
		}

		IMatchingCollection fTransactions;

		#endregion

		#region ARSubBalances

		public OrganizationSubBalanceCollection ARSubBalances
		{
			get
			{
				if (fARSubBalances == null)
				{
					fARSubBalances = new OrganizationSubBalanceCollection(Factory);
				}
				return fARSubBalances;
			}
		}

		OrganizationSubBalanceCollection fARSubBalances;

		#endregion

		#region APSubBalances

		public OrganizationSubBalanceCollection APSubBalances
		{
			get
			{
				if (fAPSubBalances == null)
				{
					fAPSubBalances = new OrganizationSubBalanceCollection(Factory);
				}
				return fAPSubBalances;
			}
		}

		OrganizationSubBalanceCollection fAPSubBalances;

		#endregion

		#region PrimaryOrgBizO

		OrgHeader PrimaryOrgBizO
		{
			get
			{
				OrgHeader bizOReturn = null;
				if (fPrimaryOrganization.IsValid)
				{
					if (fPrimaryOrgBizO != null)
					{
						bizOReturn = fPrimaryOrgBizO;
					}
					else
					{
						fPrimaryOrgBizO = Factory.Load<OrgHeader>(fPrimaryOrganization);
						bizOReturn = fPrimaryOrgBizO;
					}
				}
				return bizOReturn;
			}
		}

		OrgHeader fPrimaryOrgBizO;

		#endregion
	}
}
