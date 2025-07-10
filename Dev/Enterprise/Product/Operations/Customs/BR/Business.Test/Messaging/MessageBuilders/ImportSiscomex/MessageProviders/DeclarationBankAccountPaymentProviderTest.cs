using System;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationBankAccountPaymentProviderTest : TestCaseWithFactory
	{
		public void TestBankAccount()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "C2";
			var oBRAddinfo = BROrgImpAddInfo.Get(orgHeader);
			oBRAddinfo.ZO_BankCode = "XXX";
			oBRAddinfo.ZO_BSBNumber = "111";
			oBRAddinfo.ZO_AccountNumber = "777";

			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_FullAccountNumber = "ZZZ";
			bankAccount.AB_BSB = "123";
			bankAccount.AB_AccountNum = "654";

			using (BRCustomsDataRegistry.Instance.TaxFeeCustomsPaymentBankAccount.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, bankAccount.PK.ToGuid()))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				declaration.JE_OH_Importer = orgHeader.PK;
				declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Default;

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
				var dataProvider = new ImportSiscomexProvider(sendingObject);

				CombineAssertions(() =>
				{
					Assert(dataProvider.DeclarationBankAccountPayment.BankCode.IsNullOrEmpty());
					Assert(dataProvider.DeclarationBankAccountPayment.BankBSBNumber.IsNullOrEmpty());
					Assert(dataProvider.DeclarationBankAccountPayment.BankAccountNumber.IsNullOrEmpty());
				});

				dataProvider = new ImportSiscomexProvider(sendingObject);
				declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Broker;

				CombineAssertions(() =>
				{
					AssertEquals("ZZZ", dataProvider.DeclarationBankAccountPayment.BankCode);
					AssertEquals("123", dataProvider.DeclarationBankAccountPayment.BankBSBNumber);
					AssertEquals("654", dataProvider.DeclarationBankAccountPayment.BankAccountNumber);
				});

				dataProvider = new ImportSiscomexProvider(sendingObject);
				declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Importer;

				CombineAssertions(() =>
				{
					AssertEquals("XXX", dataProvider.DeclarationBankAccountPayment.BankCode);
					AssertEquals("111", dataProvider.DeclarationBankAccountPayment.BankBSBNumber);
					AssertEquals("777", dataProvider.DeclarationBankAccountPayment.BankAccountNumber);
				});
			}
		}
	}
}
