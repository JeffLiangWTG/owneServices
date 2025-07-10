using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.GB.Business.Testing
{
	class EnhancedValidationEntryWrapperTest : TestCaseWithFactory
	{
		public void TestCommodityLines()
		{
			invoiceLines[0].JI_Tariff = "1234567890";
			invoiceLines[1].JI_Tariff = "4242424242";
			invoiceLines[2].JI_Tariff = "1234567890";
			invoiceLines[3].JI_Tariff = "1234567890";
			invoiceLines[4].JI_Tariff = "4242424242";
			invoiceLines[5].JI_Tariff = "7654321098";
			invoiceLines[6].JI_Tariff = "7654321098";

			invoiceLines[0].JI_CountryOfOrigin = "ZA";
			invoiceLines[1].JI_CountryOfOrigin = "ZA";
			invoiceLines[2].JI_CountryOfOrigin = "ZA";
			invoiceLines[3].JI_CountryOfOrigin = "NO";
			invoiceLines[4].JI_CountryOfOrigin = "SE";
			invoiceLines[5].JI_CountryOfOrigin = "FI";
			invoiceLines[6].JI_CountryOfOrigin = "DK";

			var wrapper = new EnhancedValidationEntryWrapper(header);

			CombineAssertions(() =>
			{
				AssertEquals(5, wrapper.CommodityLineCount);
				AssertEquals("1234.56.78 90 from South Africa", wrapper.CommodityLine1);
				AssertEquals("4242.42.42 42 from South Africa", wrapper.CommodityLine2);
				AssertEquals("1234.56.78 90 from Norway", wrapper.CommodityLine3);
				AssertEquals("4242.42.42 42 from Sweden", wrapper.CommodityLine4);
				AssertEquals("7654.32.10 98 from Finland", wrapper.CommodityLine5);
			});
		}

		public void TestCommodityLineCount()
		{
			foreach (var invoiceLine in invoiceLines)
			{
				invoiceLine.JI_Tariff = "1234567890";
				invoiceLine.JI_CountryOfOrigin = "CH";
			}

			var wrapper = new EnhancedValidationEntryWrapper(header);

			AssertEquals(1, wrapper.CommodityLineCount);
		}

		public void TestTaxLines_None()
		{
			var wrapper = new EnhancedValidationEntryWrapper(header);
			AssertEquals(0, wrapper.TaxLineCount);
		}

		public void TestTaxLines_Five()
		{
			CreateTaxReferenceData();

			invoiceLines[0].JI_Tariff = "1111111111";
			invoiceLines[1].JI_Tariff = "2222222222";
			invoiceLines[2].JI_Tariff = "3333333333";
			invoiceLines[3].JI_Tariff = "4444444444";
			invoiceLines[4].JI_Tariff = "5555555555";
			invoiceLines[5].JI_Tariff = "6666666666";
			invoiceLines[6].JI_Tariff = "7777777777";

			AddFee(header.AllEntryLines[2], "A00");
			AddFee(header.AllEntryLines[3], "A00");
			AddFee(header.AllEntryLines[4], "A00");
			AddFee(header.AllEntryLines[5], "A00");
			AddFee(header.AllEntryLines[5], "A30");

			var wrapper = new EnhancedValidationEntryWrapper(header);

			CombineAssertions(() =>
			{
				AssertEquals(5, wrapper.TaxLineCount);
				AssertEquals("A00 (Import Duty) for 3333.33.33 33", wrapper.TaxLine1);
				AssertEquals("A00 (Import Duty) for 4444.44.44 44", wrapper.TaxLine2);
				AssertEquals("A00 (Import Duty) for 5555.55.55 55", wrapper.TaxLine3);
				AssertEquals("A00 (Import Duty) for 6666.66.66 66", wrapper.TaxLine4);
				AssertEquals("A30 (Anti-dumping Duty) for 6666.66.66 66", wrapper.TaxLine5);
			});
		}

		public void TestTaxLines_More()
		{
			CreateTaxReferenceData();

			invoiceLines[0].JI_Tariff = "1111111111";
			invoiceLines[1].JI_Tariff = "2222222222";
			invoiceLines[2].JI_Tariff = "3333333333";
			invoiceLines[3].JI_Tariff = "4444444444";
			invoiceLines[4].JI_Tariff = "1111111111";
			invoiceLines[5].JI_Tariff = "6666666666";
			invoiceLines[6].JI_Tariff = "7777777777";

			AddFee(header.AllEntryLines[0], "A00");
			AddFee(header.AllEntryLines[1], "A00");
			AddFee(header.AllEntryLines[2], "A00");
			AddFee(header.AllEntryLines[3], "A00");
			AddFee(header.AllEntryLines[4], "A00");
			AddFee(header.AllEntryLines[5], "A00");
			AddFee(header.AllEntryLines[5], "A00");
			AddFee(header.AllEntryLines[6], "A00");

			var wrapper = new EnhancedValidationEntryWrapper(header);

			CombineAssertions(() =>
			{
				AssertEquals(5, wrapper.TaxLineCount);
				AssertEquals("A00 (Import Duty) for 1111.11.11 11", wrapper.TaxLine1);
				AssertEquals("A00 (Import Duty) for 2222.22.22 22", wrapper.TaxLine2);
				AssertEquals("A00 (Import Duty) for 3333.33.33 33", wrapper.TaxLine3);
				AssertEquals("A00 (Import Duty) for 4444.44.44 44", wrapper.TaxLine4);
				AssertEquals("A00 (Import Duty) for 6666.66.66 66", wrapper.TaxLine5);
			});
		}

		void AddFee(CusEntryLine line, ZString taxType)
		{
			var fee = line.Fees.AddNew();
			fee.CF_RateOverrideReasonCode = "OVR";
			fee.CF_ChargeType = taxType;
		}

		void CreateTaxReferenceData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedKingdom, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty);
			var a00 = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, rateType.PK);
			a00.ZY1_Description = "Import Duty";
			var a30 = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, rateType.PK);
			a30.ZY1_Description = "Anti-dumping Duty";
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceLines = new JobComInvoiceLine[7];
			for (var i = 0; i < invoiceLines.Length; i++)
			{
				invoiceLines[i] = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLines[i].JI_Procedure = i.ToString();
			}
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			header = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First();
		}

		JobComInvoiceLine[] invoiceLines;
		CusEntryHeader header;
	}
}
