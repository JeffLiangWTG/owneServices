using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Netting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocNettingMovement : DocBaseWrapper
	{
		protected DocNettingMovement(NettingMovement nettingMovement, BusinessObjectFactory factoryToWrap)
			: base(nettingMovement, factoryToWrap)
		{
			Argument.NotNull(nettingMovement, "NettingMovement");
		}

		public static DocNettingMovement New(NettingMovement nettingMovement, BusinessObjectFactory factoryToWrap)
		{
			return new DocNettingMovement(nettingMovement, factoryToWrap);
		}

		NettingMovement NettingMovement
		{
			get { return (NettingMovement)WrappedObject; }
		}

		public ZString CompanyCode
		{
			get { return NettingMovement.CompanyCode; }
		}

		public ZString Currency
		{
			get { return NettingMovement.Currency; }
		}

		public ZDecimal Amount
		{
			get { return NettingMovement.MovementAmount; }
		}

		public ZDecimal SignedAmount
		{
			get { return NettingMovement.SignedMovementAmount; }
		}

		public ZString NettingCurrency
		{
			get { return NettingMovement.NettingCurrency; }
		}

		public ZDecimal ReportingRate
		{
			get { return NettingMovement.ReportingRate; }
		}

		public ZDecimal ReportingAmount
		{
			get { return NettingMovement.NettingCurrencyReportingAmount; }
		}

		public ZDecimal DealtRate
		{
			get { return NettingMovement.Dealtrate; }
		}

		public ZDecimal DealtAmount
		{
			get { return NettingMovement.NettingCurrencyDealtAmount; }
		}

		public ZString OrgFullName
		{
			get { return NettingMovement.Organization != null ? NettingMovement.Organization.OH_FullName : ZString.Empty; }
		}

		public DocBankAccount NettingCentreBankAccount
		{
			get
			{
				var bankAccount = AccBankAccount.GetDefaultReceiptBankAccount(NettingMovement.Currency, GlbBranch.CurrentBranch, Factory);
				return DocBankAccount.New(bankAccount, Factory);
				//BankName
				//BankAddress
				//AccountNum
				//BSB
				//SWIFT
			}
		}

		public ZString ParticipantBankName
		{
			get
			{
				return NettingMovement.NettingAccountDetails != null ? NettingMovement.NettingAccountDetails.A1_BankName : ZString.Empty;
			}
		}

		public ZString ParticipantBranchName
		{
			get
			{
				return NettingMovement.NettingAccountDetails != null ? NettingMovement.NettingAccountDetails.A1_BankBsb : ZString.Empty;
			}
		}

		public ZString ParticipantAccountName
		{
			get
			{
				return NettingMovement.NettingAccountDetails != null ? NettingMovement.NettingAccountDetails.A1_AccountName : ZString.Empty;
			}
		}

		public ZString ParticipantAccountNumber
		{
			get
			{
				return NettingMovement.NettingAccountDetails != null ? NettingMovement.NettingAccountDetails.A1_BankAccount : ZString.Empty;
			}
		}

		public ZString ParticipantSWIFT
		{
			get
			{
				return NettingMovement.NettingAccountDetails != null ? NettingMovement.NettingAccountDetails.A1_BankSwift : ZString.Empty;
			}
		}

		public ZString ParticipantIBAN
		{
			get
			{
				return NettingMovement.NettingAccountDetails != null ? NettingMovement.NettingAccountDetails.A1_IBANNumber : ZString.Empty;
			}
		}
	}
}
