using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class PaymentDetailRetrieverIncludingAQISTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAA111BBB";
			entryHeader.CH_TotalPaid = 300m;
			EFTPaymentInformationCollection payInfos = new EFTPaymentInformationCollection(testDec);
			AssertEquals("PayInfos has one item", 1, payInfos.Count);
			payInfos[0].CustomsChargeAmountPayableNow = 100m;
			payInfos[0].AQISServicePaymentAmountPayableNow = 200m;

			PaymentDetailRetrieverIncludingAQISForTest testClass = new PaymentDetailRetrieverIncludingAQISForTest(testDec, CMRMessageTypes.LodgeWithPay, null);
			AssertEquals("Customs Charge", 300m, testClass.customsChargeAmount);
			AssertEquals("AQIS Amount", 0m, testClass.aQISAmount);

			testClass = new PaymentDetailRetrieverIncludingAQISForTest(testDec, CMRMessageTypes.Amendment, null);
			AssertEquals("Customs Charge", 300m, testClass.customsChargeAmount);
			AssertEquals("AQIS Amount", 0m, testClass.aQISAmount);

			testClass = new PaymentDetailRetrieverIncludingAQISForTest(testDec, CMRMessageTypes.Withdrawal, null);
			AssertEquals("Customs Charge", 300m, testClass.customsChargeAmount);
			AssertEquals("AQIS Amount", 0m, testClass.aQISAmount);

			testClass = new PaymentDetailRetrieverIncludingAQISForTest(testDec, CMRMessageTypes.PreLodge, null);
			AssertEquals("Customs Charge", 300m, testClass.customsChargeAmount);
			AssertEquals("AQIS Amount", 0m, testClass.aQISAmount);

			testClass = new PaymentDetailRetrieverIncludingAQISForTest(testDec, CMRMessageTypes.LodgeWithoutPay, null);
			AssertEquals("Customs Charge", 300m, testClass.customsChargeAmount);
			AssertEquals("AQIS Amount", 0m, testClass.aQISAmount);

			testClass = new PaymentDetailRetrieverIncludingAQISForTest(testDec, CMRMessageTypes.Payment, payInfos);
			AssertEquals("Customs Charge", 100m, testClass.customsChargeAmount);
			AssertEquals("AQIS Amount", 200m, testClass.aQISAmount);
		}

		public void TestIsPayingCustomsChargeAndIsPayingAQISChargeOnly()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAA111BBB";
			EFTPaymentInformationCollection payInfos = new EFTPaymentInformationCollection(testDec);
			AssertEquals("PayInfos has one item", 1, payInfos.Count);

			payInfos[0].CustomsChargeAmountPayableNow = 100m;
			PaymentDetailRetrieverIncludingAQISForTest testClass = new PaymentDetailRetrieverIncludingAQISForTest(testDec, CMRMessageTypes.Payment, payInfos);
			AssertEquals("IsPayingCustomsCharge", true, testClass.IsPayingCustomsCharge);
			AssertEquals("IsPayingAQISChargeOnly", false, testClass.IsPayingAQISChargeOnly);
			AssertEquals("ShouldIgnorePaymentMethodsOnDeclaration", false, testClass.ShouldIgnorePaymentMethodsOnDeclaration);

			payInfos[0].AQISServicePaymentAmountPayableNow = 100m;
			testClass = new PaymentDetailRetrieverIncludingAQISForTest(testDec, CMRMessageTypes.Payment, payInfos);
			AssertEquals("IsPayingCustomsCharge", true, testClass.IsPayingCustomsCharge);
			AssertEquals("IsPayingAQISChargeOnly", false, testClass.IsPayingAQISChargeOnly);
			AssertEquals("ShouldIgnorePaymentMethodsOnDeclaration", false, testClass.ShouldIgnorePaymentMethodsOnDeclaration);

			payInfos[0].CustomsChargeAmountPayableNow = 0m;
			testClass = new PaymentDetailRetrieverIncludingAQISForTest(testDec, CMRMessageTypes.Payment, payInfos);
			AssertEquals("IsPayingCustomsCharge", false, testClass.IsPayingCustomsCharge);
			AssertEquals("IsPayingAQISChargeOnly", true, testClass.IsPayingAQISChargeOnly);
			AssertEquals("ShouldIgnorePaymentMethodsOnDeclaration", true, testClass.ShouldIgnorePaymentMethodsOnDeclaration);
		}

		public void TestImporterWillPayDeclarationForAQISPayment()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			OrgHeader importer = OrgHeader.New(Factory);
			importer.MiscServ.OM_IMEftQuarantineFromImport = true;
			testDec.JE_OH_Importer = importer.PK;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAA111BBB";
			EFTPaymentInformationCollection payInfos = new EFTPaymentInformationCollection(testDec);
			payInfos[0].AQISServicePaymentAmountPayableNow = 10m;
			payInfos[0].CustomsChargeAmountPayableNow = 0m;

			PaymentDetailRetrieverIncludingAQISForTest testClass = new PaymentDetailRetrieverIncludingAQISForTest(testDec, CMRMessageTypes.Payment, payInfos);
			AssertEquals("AQIS will be paid by importer", true, testClass.ImporterWillPayDeclaration());

			importer.MiscServ.OM_IMEftQuarantineFromImport = false;
			testClass = new PaymentDetailRetrieverIncludingAQISForTest(testDec, CMRMessageTypes.Payment, payInfos);
			AssertEquals("AQIS will be paid by importer", false, testClass.ImporterWillPayDeclaration());
		}

		public void TestTotalAmountPayable()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAA111BBB";
			EFTPaymentInformationCollection payInfos = new EFTPaymentInformationCollection(testDec);
			payInfos[0].AQISServicePaymentAmountPayableNow = 100m;
			payInfos[0].CustomsChargeAmountPayableNow = 0m;

			PaymentDetailRetrieverIncludingAQISForTest testClass = new PaymentDetailRetrieverIncludingAQISForTest(testDec, CMRMessageTypes.Payment, payInfos);
			AssertEquals("Total Amount payable", 100m, testClass.TotalAmountPayable);

			payInfos[0].CustomsChargeAmountPayableNow = 200m;
			testClass = new PaymentDetailRetrieverIncludingAQISForTest(testDec, CMRMessageTypes.Payment, payInfos);
			AssertEquals("Total Amount payable", 300m, testClass.TotalAmountPayable);
		}

		public void TestPaymentPartyForAQISChargeWhenPaymentMethodIsSet()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			OrgHeader importer = OrgHeader.New(Factory);
			importer.MiscServ.OM_IMEftQuarantineFromImport = false;
			importer.MiscServ.OM_IMEftCustomsFromImport = true;
			testDec.JE_OH_Importer = importer.PK;

			testDec.JE_PaymentMethod = "IMP";

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAA111BBB";
			EFTPaymentInformationCollection payInfos = new EFTPaymentInformationCollection(testDec);
			payInfos[0].AQISServicePaymentAmountPayableNow = 100m;
			payInfos[0].CustomsChargeAmountPayableNow = 0m;

			PaymentDetailRetrieverIncludingAQISForTest testClass = new PaymentDetailRetrieverIncludingAQISForTest(testDec, CMRMessageTypes.Payment, payInfos);
			AssertEquals("Payment Party for AQIS Charge", PaymentParty.Broker, testClass.PartyToPayEntry);
		}
	}

	class PaymentDetailRetrieverIncludingAQISForTest : PaymentDetailRetrieverIncludingAQIS
	{
		public PaymentDetailRetrieverIncludingAQISForTest(JobDeclaration declaration, CMRMessageTypes messageType, EFTPaymentInformationCollection eFTPayInfos) : base(declaration, messageType, eFTPayInfos)
		{
		}

		new internal bool ShouldIgnorePaymentMethodsOnDeclaration => base.ShouldIgnorePaymentMethodsOnDeclaration;
	}
}
