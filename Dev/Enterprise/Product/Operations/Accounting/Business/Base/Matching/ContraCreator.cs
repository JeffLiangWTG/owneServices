using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Matching
{
	/// <summary>
	/// Class to encapsulate creation of Contras for Matching purposes
	/// </summary>
	public class ContraCreator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ContraCreator(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public Contra CreateContra(ZGuid primaryOrg, OrganizationSubBalance subBalance)
		{
			Contra newContra = Contra.New(Factory);
			OrgHeader aPOrg = null;
			OrgHeader aROrg = null;
			ZString orgDescription = ZString.Empty;

			if (subBalance.Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable)
			{
				aPOrg = Factory.Load<OrgHeader>(subBalance.Organization);
				aROrg = Factory.Load<OrgHeader>(primaryOrg);

				newContra.APRow.AH_OH = subBalance.Organization;
				newContra.APRow.AH_InvoiceAmount = -subBalance.Amount;
				newContra.APRow.AH_OutstandingAmount = -subBalance.Amount;
				newContra.APRow.AH_OSTotal = -subBalance.Amount;

				newContra.ARRow.AH_OH = primaryOrg;
				newContra.ARRow.AH_InvoiceAmount = subBalance.Amount;
				newContra.ARRow.AH_OutstandingAmount = subBalance.Amount;
				newContra.ARRow.AH_OSTotal = subBalance.Amount;
			}
			else if (subBalance.Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable)
			{
				aPOrg = Factory.Load<OrgHeader>(primaryOrg);
				aROrg = Factory.Load<OrgHeader>(subBalance.Organization);

				newContra.APRow.AH_OH = primaryOrg;
				newContra.APRow.AH_InvoiceAmount = subBalance.Amount;
				newContra.APRow.AH_OutstandingAmount = subBalance.Amount;
				newContra.APRow.AH_OSTotal = subBalance.Amount;

				newContra.ARRow.AH_OH = subBalance.Organization;
				newContra.ARRow.AH_InvoiceAmount = -subBalance.Amount;
				newContra.ARRow.AH_OutstandingAmount = -subBalance.Amount;
				newContra.ARRow.AH_OSTotal = -subBalance.Amount;
			}
			// Always flag as system generated
			newContra.ARRow.AH_TransactionCreatedByMatching = true;
			newContra.APRow.AH_TransactionCreatedByMatching = true;

			if (aROrg != null && aPOrg != null)
			{
				orgDescription = AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(LedgerTypes.AccountsReceivable + TransactionTypes.Contra,
						   Res.GetString("ef5d88c5-db2e-4503-950e-c9e6abdd567d", "CONTRA BETWEEN")) + " " +
						   Res.GetString("98509c24-2e10-4edc-8133-86d4e764ffcc", "(SYSTEM GENERATED)");
				newContra.APRow.AH_Desc = orgDescription;
				newContra.ARRow.AH_Desc = orgDescription;
			}

			return newContra;
		}
	}
}