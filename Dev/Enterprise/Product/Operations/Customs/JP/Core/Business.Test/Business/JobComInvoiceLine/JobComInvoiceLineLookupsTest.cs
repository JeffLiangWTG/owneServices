using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineLookups))]
	sealed class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDutyReductionExemptionRefundCodeList()
		{
			Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DutyExemptionCode, "D1", "D2");
			Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DutyExemptionRefundCode, "DE1", "DE2");
			Factory.Save();

			var declaration = Declaration;
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var list = InvoiceLine.Lookups.DutyReductionExemptionRefundCodeCollection;
			list.Load();

			var codes = list.Select(x => x.ZZD_Code);
			Assert(codes.Contains("D1"));
			Assert(codes.Contains("D2"));

			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
			list = InvoiceLine.Lookups.DutyReductionExemptionRefundCodeCollection;
			list.Load();

			codes = list.Select(x => x.ZZD_Code);
			Assert(codes.Contains("DE1"));
			Assert(codes.Contains("DE2"));
		}

		public void TestCustomsUQList()
		{
			var list = InvoiceLine.Lookups.CustomsUQList;
			AssertSame(Factory.GetCachedValue<CustomsUQList>(), list);
		}

		public void TestCOOT1List()
		{
			PrepareCodes();
			var list = InvoiceLine.Lookups.PreferenceList;
			AssertContainsExactElementsInAnyOrder(new[] { "GS", "SG" }, list.GetAllCodes());
		}

		public void TestCOOT2List()
		{
			PrepareCodes();
			var list = InvoiceLine.Lookups.OriginCertifierList;
			AssertContainsExactElementsInAnyOrder(new[] { "T" }, list.GetAllCodes());
		}

		public void TestCOOT3List()
		{
			PrepareCodes();
			var declaration = Declaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			InvoiceLine.JI_Calc_Preference = "";
			AssertEquals(0, InvoiceLine.Lookups.CertificateOfOriginCertifierList.Count);

			InvoiceLine.JI_Calc_Preference = "SG";
			AssertEquals(1, InvoiceLine.Lookups.CertificateOfOriginCertifierList.Count);
			Assert(InvoiceLine.Lookups.CertificateOfOriginCertifierList.ContainsCode("7"));
			Instruction.CEI_Style = JPImportDeclarationTypeList.Codes.R;
			AssertEquals(0, InvoiceLine.Lookups.CertificateOfOriginCertifierList.Count);

			InvoiceLine.JI_Calc_Preference = "GS";
			AssertEquals(0, InvoiceLine.Lookups.CertificateOfOriginCertifierList.Count);
			Instruction.CEI_Style = JPImportDeclarationTypeList.Codes.B;
			AssertEquals(1, InvoiceLine.Lookups.CertificateOfOriginCertifierList.Count);
			Assert(InvoiceLine.Lookups.CertificateOfOriginCertifierList.ContainsCode("M"));
		}

		void PrepareCodes()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);

			var universalReferenceTestHelper = new UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType1, "Japan Certificate Of Origin 1");
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType2, "Japan Certificate Of Origin 2");
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType3, "Japan Certificate Of Origin 3");
			var coot1Code1 = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType1, "SG", "Description for SG", startDate, endDate);
			var coot1Code2 = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType1, "GS", "Description for GS", startDate, endDate);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType2, "T", "Description for T", startDate, endDate);
			var coot3Code1 = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType3, "7", "Description for 7", startDate, endDate);
			var coot3Code2 = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType3, "M", "Description for M", startDate, endDate);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeListAttribute(coot1Code1.PK, "Preference", "EPA");
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeListAttribute(coot1Code2.PK, "Preference", "GSP");
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeListAttribute(coot1Code2.PK, "Preference", "LDC");
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeListAttribute(coot3Code1.PK, "Preference", "EPA");
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeListAttribute(coot3Code2.PK, "Preference", "GSP");
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeListAttribute(coot3Code2.PK, "Preference", "LDC");

			Factory.Save();
		}

		public void TestFEFTAArticle48List()
		{
			AssertType<FEFTAArticle48List>(InvoiceLine.Lookups.FEFTAArticle48List);
		}

		public void TestTradeControlOrderAppendixList()
		{
			TestDataCoreHelper.PrepareTradeControlOrderAppendixList(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			Factory.Save();

			var tradeControlOrderAppendixList = invoiceLine.Lookups.TradeControlOrderAppendixList;
			AssertEquals(2, tradeControlOrderAppendixList.Count);
			Assert(tradeControlOrderAppendixList.ContainsCode("12345"));
			Assert(tradeControlOrderAppendixList.ContainsCode("22345"));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			tradeControlOrderAppendixList = invoiceLine.Lookups.TradeControlOrderAppendixList;
			AssertEquals(2, tradeControlOrderAppendixList.Count);
			Assert(tradeControlOrderAppendixList.ContainsCode("1234"));
			Assert(tradeControlOrderAppendixList.ContainsCode("2345"));
		}

		public void TestConsumptionTaxExemptionIdList()
		{
			Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportConsumptionTaxExemptionCode, "B", "E", "G");

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertContainsExactElementsInExactOrder(["B", "E", "G"], InvoiceLine.Lookups.ConsumptionTaxExemptionIDList.GetAllCodes());
		}

		public void TestNACCSCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("JP", "HSN");
			Factory.Save();

			var tariff = helper.LoadOrCreateNewTariff("JP", tariffType.PK, "110100011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertContainsExactElementsInExactOrder(new[] { "X", "E", "Y", "T" }, InvoiceLine.Lookups.NACCSCodeList.GetAllCodes());
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertContainsExactElementsInExactOrder(new[] { "X", "E", "Y" }, InvoiceLine.Lookups.NACCSCodeList.GetAllCodes());

			InvoiceLine.JI_Tariff = "110100011";
			AssertContainsExactElementsInAnyOrder(new[] { "X", "E", "Y", "0" }, InvoiceLine.Lookups.NACCSCodeList.GetAllCodes());

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertContainsExactElementsInAnyOrder(new[] { "X", "E", "Y", "T", "0" }, InvoiceLine.Lookups.NACCSCodeList.GetAllCodes());
		}

		public void TestJI_StorageTypeList()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Empty list", 0, InvoiceLine.Lookups.StorageTypeList.Count);

				Instruction.CEI_Style = JPImportDeclarationTypeList.Codes.A;
				AssertSame("A list", Factory.GetCachedValue<StorageTypeListWhenDeclarationTypeIsA>(), InvoiceLine.Lookups.StorageTypeList);

				Instruction.CEI_Style = JPImportDeclarationTypeList.Codes.G;
				AssertSame("G list", Factory.GetCachedValue<StorageTypeListWhenDeclarationTypeIsG>(), InvoiceLine.Lookups.StorageTypeList);

				Instruction.CEI_Style = "";
				Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertSame("Sea list", Factory.GetCachedValue<StorageTypeListWhenTransportModeIsSea>(), InvoiceLine.Lookups.StorageTypeList);
			});
		}

		JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
		JobDeclaration declaration;

		CusEntryInstruction Instruction => instruction ??= Declaration.CustomsEntryInstructions.AddNew();
		CusEntryInstruction instruction;

		JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		JobComInvoiceHeader invoiceHeader;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				invoiceLine ??= InvoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = Instruction.PK;
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;
	}
}
