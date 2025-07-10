using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	internal class DeclarationBankAccountPaymentProvider : IDeclarationBankAccountPayment
	{
		public DeclarationBankAccountPaymentProvider(JobDeclaration declaration)
		{
			if (declaration.JE_PaymentMethod == PaymentPartyCodeDescriptionList.Codes.Broker)
			{
				bankAccount = declaration.Factory.Load<AccBankAccount>(declaration.PaymentBankAccountPK);
			}
			else if (declaration.JE_PaymentMethod == PaymentPartyCodeDescriptionList.Codes.Importer)
			{
				orgImpAddInfo = declaration.ImporterAddInfo;
			}
		}

		public static DeclarationBankAccountPaymentProvider New(JobDeclaration declaration) => declaration == null ? null : new DeclarationBankAccountPaymentProvider(declaration);

		readonly AccBankAccount bankAccount;
		readonly BROrgImpAddInfo orgImpAddInfo;

		public string BankCode => bankAccount?.AB_FullAccountNumber ?? orgImpAddInfo?.ZO_BankCode;

		public string BankBSBNumber => bankAccount?.AB_BSB ?? orgImpAddInfo?.ZO_BSBNumber;

		public string BankAccountNumber => bankAccount?.AB_AccountNum ?? orgImpAddInfo?.ZO_AccountNumber;
	}
}
