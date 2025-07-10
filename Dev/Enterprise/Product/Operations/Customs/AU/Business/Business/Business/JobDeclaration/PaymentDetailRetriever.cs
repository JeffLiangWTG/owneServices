using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public enum PaymentParty { Importer, Broker, SecondBroker, Cash, DrawbackClaimant }
	public struct BankDetails
	{
		public ZString BSBNumber;
		public ZString AccountNumber;
		public ZString AccountName;
	}

	public class PaymentDetailRetriever
	{
		public PaymentDetailRetriever(JobDeclaration declaration)
		{
			this.Declaration = declaration;
		}

		public ZString PartyToPayString()
		{
			PaymentParty party = PartyToPayEntry;
			if (party == PaymentParty.Broker)
			{
				return JobDeclaration.PaymentMethods.Broker;
			}
			else if (party == PaymentParty.SecondBroker)
			{
				return JobDeclaration.PaymentMethods.SecondBroker;
			}
			else if (party == PaymentParty.Cash)
			{
				return JobDeclaration.PaymentMethods.Cash;
			}
			else if (party == PaymentParty.DrawbackClaimant)
			{
				return JobDeclaration.PaymentMethods.DrawbackClaimant;
			}
			else
			{
				return JobDeclaration.PaymentMethods.Importer;
			}
		}

		#region PartyToPayEntry

		public PaymentParty PartyToPayEntry
		{
			get
			{
				PaymentParty result;

				if (!Declaration.IsEntryForAnImporter)
				{
					if (ShouldIgnorePaymentMethodsOnDeclaration)
					{
						if (ImporterWillPayDeclaration())
						{
							result = PaymentParty.Importer;
						}
						else
						{
							result = Declaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.SecondBroker ? PaymentParty.SecondBroker : PaymentParty.Broker;
						}
					}
					else
					{
						if (Declaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.Default && ImporterWillPayDeclaration()
							|| Declaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.Importer)
						{
							result = PaymentParty.Importer;
						}
						else if (Declaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.DrawbackClaimant)
						{
							result = PaymentParty.DrawbackClaimant;
						}
						else if (Declaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.SecondBroker)
						{
							result = PaymentParty.SecondBroker;
						}
						else if (Declaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.Cash)
						{
							result = PaymentParty.Cash;
						}
						else
						{
							result = PaymentParty.Broker;
						}
					}
				}
				else
				{
					result = PaymentParty.Importer;
				}

				return result;
			}
		}

		protected virtual bool ShouldIgnorePaymentMethodsOnDeclaration
		{
			get { return false; }
		}

		#endregion

		public BankDetails GetBankDetails()
		{
			BankDetails result = new BankDetails();
			if (PartyToPayEntry == PaymentParty.Broker)
			{
				AccBankAccount account = (AccBankAccount)Declaration.Factory.Load(typeof(AccBankAccount), Env.Registry.CustomsPaymentBankAccount);
				if (account != null)
				{
					result.BSBNumber = account.AB_BSB.KeepChars("0123456789");
					result.AccountNumber = account.AB_AccountNum.KeepChars("0123456789");
				}
			}
			else if (PartyToPayEntry == PaymentParty.SecondBroker)
			{
				AccBankAccount account = (AccBankAccount)Declaration.Factory.Load(typeof(AccBankAccount), Env.Registry.CustomsSecondPaymentBankAccount);
				if (account != null)
				{
					result.BSBNumber = account.AB_BSB.KeepChars("0123456789");
					result.AccountNumber = account.AB_AccountNum.KeepChars("0123456789");
				}
			}
			else if (Declaration.Importer != null)
			{
				OrgHeader importer = Declaration.Importer;
				result.BSBNumber = importer.MiscServ.OM_IMEFTBankBSB.KeepChars("0123456789");
				result.AccountNumber = importer.MiscServ.OM_IMEFTBankAccount.KeepChars("0123456789");
				result.AccountName = importer.OH_FullNameTruncated;
			}
			return result;
		}

		#region Implementation

		protected readonly JobDeclaration Declaration;

		protected virtual internal bool ImporterWillPayDeclaration()
		{
			OrgHeader importer = Declaration.Importer;
			if (importer != null)
			{
				OrgMiscServ orgMiscServ = importer.MiscServ;
				if (orgMiscServ.OM_IMEFTBankBSB.IsEmpty || orgMiscServ.OM_IMEFTBankAccount.IsEmpty || !orgMiscServ.OM_IMEftCustomsFromImport)
				{
					return false;
				}
				else if (orgMiscServ.OM_IMMaxEFTAmount == 0 && orgMiscServ.OM_IMMinEFTAmount == 0)
				{
					return true;
				}
				else
				{
					ZDecimal totalAmountPayable = this.TotalAmountPayable;
					return (orgMiscServ.OM_IMMinEFTAmount <= totalAmountPayable && orgMiscServ.OM_IMMaxEFTAmount >= totalAmountPayable);
				}
			}
			else
			{
				return false;
			}
		}

		public ZDecimal TotalAmountPayable
		{
			get { return TotalAmountPayableCore; }
		}

		protected virtual ZDecimal TotalAmountPayableCore
		{
			get { return Declaration.CustomsEntryHeaders.TotalAmountPayableForThisSession; }
		}

		#endregion
	}
}
