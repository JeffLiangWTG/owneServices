using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	public class InvoiceChargeTest : EU.Business.Declaration.Testing.InvoiceChargeTest
	{
		public void TestLookups()
		{
			var parent = Factory.New<InvoiceCharge>();
			AssertType<InvoiceChargeLookups>(parent.Lookups);
		}

		public void TestValidation()
		{
			var parent = Factory.New<InvoiceCharge>();
			AssertType<InvoiceChargeValidation>(parent.Validation);
		}

		public override void TestIsIncludedInLinesReadOnly()
		{
			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";

			var aDD = invoice.Charges.AddNew();
			aDD.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;

			var oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = UCCCustomsChargeTypeList.Codes.TransportCostsCharge;

			var oTH = invoice.Charges.AddNew();
			oTH.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;

			CombineAssertions(() =>
			{
				AssertEquals("ADD Included in ITOT should be readonly", false, aDD.J7_IsIncludedInITOTInfo.ReadOnly);
				AssertEquals("OFT Included in ITOT should be readonly", false, oFT.J7_IsIncludedInITOTInfo.ReadOnly);
				AssertEquals("OTH  Included in ITOT should be readonly", false, oTH.J7_IsIncludedInITOTInfo.ReadOnly);
			});
		}

		public void TestJ7_Calc_IsIncludedInInvoiceExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";

			InvoiceCharge aDD = invoice.Charges.AddNew();
			aDD.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			aDD.J7_Amount = 10m;
			aDD.J7_RX_NKCurrency = "EUR";

			InvoiceCharge oTN = invoice.Charges.AddNew();
			oTN.J7_ChargeType = ESCustomsChargeTypeList.Codes.OtherNationalPayments;
			oTN.J7_Amount = 10m;
			oTN.J7_RX_NKCurrency = "EUR";
			CombineAssertions("For Export Declarations", () =>
			{
				AssertEquals("J7_Calc_IsIncludedInInvoice is not readonly", false, aDD.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

				aDD.J7_Calc_IsIncludedInInvoiceAmount = false;
				AssertEquals("Setting this to false should set J7_IsNotIncludedInInvoice as true", true, aDD.J7_IsNotIncludedInInvoice);

				AssertEquals("J7_Calc_IsIncludedInInvoice should not be readonly", false, oTN.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
				AssertEquals("OTN's J7_Calc_IsIncludedInInvoice should be true for FOB Invoice", true, oTN.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("OTN's J7_IsNotIncludedInInvoice should be false for FOB Invoice", false, oTN.J7_IsNotIncludedInInvoice);

				invoice.JZ_IncoTerm = "EXW";
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount for ADD should stay", false, aDD.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount for OTN should be changed", false, oTN.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_IsNotIncludedInInvoice for OFT should be changed", true, oTN.J7_IsNotIncludedInInvoice);
			});
		}

		public void TestReadOnlyOfIsIncludedInInvoiceExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";

			InvoiceCharge oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = UCCCustomsChargeTypeList.Codes.TransportCostsCharge;
			oFT.J7_Amount = 10m;

			InvoiceCharge oTN = invoice.Charges.AddNew();
			oTN.J7_ChargeType = ESCustomsChargeTypeList.Codes.OtherNationalPayments;
			oTN.J7_Amount = 10m;

			CombineAssertions("For Export Declarations", () =>
			{
				AssertEquals("IsIncludedInInvoiceAmount", false, oFT.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("IsIncludedInInvoice should not be readonly", false, oFT.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
				AssertEquals("IsIncludedInInvoiceAmount for OTN by default", true, oTN.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("IsIncludedInInvoice should not be readonly", false, oTN.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

				Factory.Save();

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				JobComInvoiceHeader invoiceLoaded = factory2.Load<JobComInvoiceHeader>(invoice.PK);
				InvoiceCharge oFTLoaded = invoiceLoaded.Charges[CustomsChargeTypeList.Codes.OverseasFreight];
				InvoiceCharge oTNLoaded = invoiceLoaded.Charges[ESCustomsChargeTypeList.Codes.OtherNationalPayments];

				AssertEquals("IsIncludedInInvoice for OFT should not be readonly", false, oFTLoaded.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
				AssertEquals("IsIncludedInInvoice for OTN should not be readonly", false, oTNLoaded.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
			});
		}

		public void TestResetDefaultIsIncludedInAmountExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";

			InvoiceCharge oTN = invoice.Charges.AddNew();
			oTN.J7_ChargeType = ESCustomsChargeTypeList.Codes.OtherNationalPayments;
			oTN.J7_Amount = 100m;
			oTN.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

			CombineAssertions("For Export Declarations", () =>
			{
				AssertEquals("Default IsIncludedInAmount for OTN", true, oTN.J7_Calc_IsIncludedInInvoiceAmount);

				invoice.JZ_IncoTerm = "EXW";
				AssertEquals("Default IsIncludedInAmount for OTN", false, oTN.J7_Calc_IsIncludedInInvoiceAmount);
			});
		}

		public void TestJ7_IsNotIncludedInInvoiceSetOnFactorySavingExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";

			InvoiceCharge cOM = invoice.Charges.AddNew();
			cOM.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			cOM.J7_Amount = 0m;
			CombineAssertions("For Import Declarations", () =>
			{
				AssertEquals("J7_IsNotIncludedInInvoice", false, cOM.J7_IsNotIncludedInInvoice);

				cOM.J7_Calc_IsIncludedInInvoiceAmount = false;

				Factory.Save();

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				InvoiceCharge cOMLoaded = factory2.Load<InvoiceCharge>(cOM.PK);
				AssertEquals("J7_IsNotIncludedInInvoice was set to true when saving", true, cOMLoaded.J7_IsNotIncludedInInvoice);
			});

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			InvoiceCharge oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 0m;
			CombineAssertions("For Export Declarations", () =>
			{
				AssertEquals("J7_IsNotIncludedInInvoice", false, oFT.J7_IsNotIncludedInInvoice);

				oFT.J7_Calc_IsIncludedInInvoiceAmount = false;

				Factory.Save();

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				InvoiceCharge oFTLoaded = factory2.Load<InvoiceCharge>(oFT.PK);
				AssertEquals("J7_IsNotIncludedInInvoice was set to true when saving", true, oFTLoaded.J7_IsNotIncludedInInvoice);
			});
		}

		protected override void SetupAllTestObjects()
		{
			base.SetupAllTestObjects();
			testDec.JE_MessageType = MessageTypeList.Codes.Import;
		}

		protected override string GetIncotermToTestIsIncludedInInvoice() => "EXW";
	}
}
