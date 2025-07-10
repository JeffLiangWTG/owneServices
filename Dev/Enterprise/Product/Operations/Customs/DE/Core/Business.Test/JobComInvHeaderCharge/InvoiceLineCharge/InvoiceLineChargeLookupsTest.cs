namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	public class InvoiceLineChargeLookupsTest : EU.Business.Declaration.Testing.InvoiceLineChargeLookupsTest
	{
		public void TestChargeTypeList_Import_OPF()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var fOPFCode = ImportChargeCodeList.Codes.OPF;
				AssertEquals("No OPF code default", false, lookups.ChargeTypeList.ContainsCode(fOPFCode));

				invoiceLine.JI_Procedure = "6121F01";
				AssertEquals("6121F01", false, lookups.ChargeTypeList.ContainsCode(fOPFCode));
				invoiceLine.JI_Procedure = "6321F01";
				AssertEquals("6321F01", false, lookups.ChargeTypeList.ContainsCode(fOPFCode));
				invoiceLine.JI_Procedure = "6121B02";
				AssertEquals("6121B02", false, lookups.ChargeTypeList.ContainsCode(fOPFCode));
				invoiceLine.JI_Procedure = "6321B02";
				AssertEquals("6321B02", false, lookups.ChargeTypeList.ContainsCode(fOPFCode));
				invoiceLine.JI_Procedure = "6121B03";
				AssertEquals("6121B03", false, lookups.ChargeTypeList.ContainsCode(fOPFCode));
				invoiceLine.JI_Procedure = "6321B03";
				AssertEquals("6321B03", false, lookups.ChargeTypeList.ContainsCode(fOPFCode));
				invoiceLine.JI_Procedure = "6121";
				AssertEquals("6121", true, lookups.ChargeTypeList.ContainsCode(fOPFCode));
				invoiceLine.JI_Procedure = "6321";
				AssertEquals("6321", true, lookups.ChargeTypeList.ContainsCode(fOPFCode));

				invoiceLine.JI_Procedure = "6122F01";
				AssertEquals("6122F01", false, lookups.ChargeTypeList.ContainsCode(fOPFCode));
				invoiceLine.JI_Procedure = "6322F01";
				AssertEquals("6322F01", false, lookups.ChargeTypeList.ContainsCode(fOPFCode));
				invoiceLine.JI_Procedure = "6122B02";
				AssertEquals("6122B02", true, lookups.ChargeTypeList.ContainsCode(fOPFCode));
				invoiceLine.JI_Procedure = "6322B02";
				AssertEquals("6322B02", true, lookups.ChargeTypeList.ContainsCode(fOPFCode));
				invoiceLine.JI_Procedure = "6122B03";
				AssertEquals("6122B03", true, lookups.ChargeTypeList.ContainsCode(fOPFCode));
				invoiceLine.JI_Procedure = "6322B03";
				AssertEquals("6322B03", true, lookups.ChargeTypeList.ContainsCode(fOPFCode));

				invoiceLine.JI_Procedure = "4022F01";
				AssertEquals("4022F01", false, lookups.ChargeTypeList.ContainsCode(fOPFCode));
				invoiceLine.JI_Procedure = "4022B02";
				AssertEquals("4022B02", false, lookups.ChargeTypeList.ContainsCode(fOPFCode));

				invoiceLine.JI_Procedure = "6122";
				AssertEquals("6122", true, lookups.ChargeTypeList.ContainsCode(fOPFCode));
				invoiceLine.JI_Procedure = "6322";
				var chargeTypeList = lookups.ChargeTypeList;
				AssertEquals("6322", true, chargeTypeList.ContainsCode(ImportChargeCodeList.Codes.OPF));

				AssertSame("Cached", chargeTypeList, lookups.ChargeTypeList);
			});
		}

		public void TestChargeTypeList_Import_ConcessionIs01()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Procedure = "4000E01";

			var chargeTypeList = lookups.ChargeTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("Concession is E01", false, chargeTypeList.ContainsCode(ImportChargeCodeList.Codes.INP));
				AssertSame("Cached", chargeTypeList, lookups.ChargeTypeList);
			});
		}

		public void TestChargeTypeList_Import_ConcessionNot01()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Procedure = "6122F01";

			var chargeTypeList = lookups.ChargeTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("Concession not E01", true, chargeTypeList.ContainsCode(ImportChargeCodeList.Codes.INP));
				AssertSame("Cached", chargeTypeList, lookups.ChargeTypeList);
			});
		}

		public void TestChargeTypeList_Import_NoAIRType()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var chargeTypeList = lookups.ChargeTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("No Charge Type AIR", false, chargeTypeList.ContainsCode(ImportChargeCodeList.Codes.AIR));
				AssertSame("Cached", chargeTypeList, lookups.ChargeTypeList);
			});
		}

		public void TestChargeTypeList_Export()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

			AssertType<ChargeCodeList>(lookups.ChargeTypeList);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineCharge = invoiceLine.Charges.AddNew();
			lookups = invoiceLineCharge.Lookups;
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		InvoiceLineChargeLookups lookups;
	}
}
