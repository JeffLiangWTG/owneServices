using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class SupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList_Import_InvoiceHeader()
		{
			_ = new SupportingDocumentTestHelper(Factory);

			dec.JE_MessageType = MessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				var collectionForHeader = (ZZRefCusCodeListCombinedCollection)supportingDocumentForHeader.Lookups.CodeList;
				collectionForHeader.Load();
				AssertContainsExactElementsInAnyOrder("SupportingDocument for Header", new[] { "9004" }, collectionForHeader.Select(x => x.ZZD_Code));
				AssertSame("Cached", collectionForHeader, supportingDocumentForHeader.Lookups.CodeList);
			});
		}

		public void TestCodeList_Import_InvoiceLine()
		{
			_ = new SupportingDocumentTestHelper(Factory);

			dec.JE_MessageType = MessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				var collectionForLine = (ZZRefCusCodeListCombinedCollection)supportingDocumentForLine.Lookups.CodeList;
				collectionForLine.Load();
				AssertContainsExactElementsInAnyOrder("SupportingDocument for Line", new[] { "9002" }, collectionForLine.Select(x => x.ZZD_Code));
				AssertSame("Cached", collectionForLine, supportingDocumentForLine.Lookups.CodeList);
			});
		}

		public void TestCodeList_Export_InvoiceHeader()
		{
			_ = new SupportingDocumentTestHelper(Factory);

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				var collectionForHeader = (ZZRefCusCodeListCombinedCollection)supportingDocumentForHeader.Lookups.CodeList;
				collectionForHeader.Load();
				AssertContainsExactElementsInAnyOrder("SupportingDocument for Header", new[] { "3LLA232" }, collectionForHeader.Select(x => x.ZZD_Code));
				AssertSame("Cached", collectionForHeader, supportingDocumentForHeader.Lookups.CodeList);
			});
		}

		public void TestCodeList_Export_InvoiceLine()
		{
			_ = new SupportingDocumentTestHelper(Factory);

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				var collectionForLine = (ZZRefCusCodeListCombinedCollection)supportingDocumentForLine.Lookups.CodeList;
				collectionForLine.Load();
				AssertContainsExactElementsInAnyOrder("SupportingDocument for Line", new[] { "3LLA231" }, collectionForLine.Select(x => x.ZZD_Code));
				AssertSame("Cached", collectionForLine, supportingDocumentForLine.Lookups.CodeList);
			});
		}

		public void TestAvailabilityList()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			AssertType<AvailabilityList>("Should be AvailabilityList", supportingDocument.Lookups.AvailabilityList);
		}

		public void TestCustomsUQList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "TESTA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "TESTB", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "TESTC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				var importUQList = supportingDocumentForLine.Lookups.CustomsUQList;
				AssertEquals("When the declaration is NOT export, we get the CustomsUQList from the ZZ db", "TESTA, TESTB", importUQList.CodesAsString);
				AssertSame("Import, cached", importUQList, supportingDocumentForLine.Lookups.CustomsUQList);

				dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				var exportUQList = supportingDocumentForLine.Lookups.CustomsUQList;
				AssertType<SupportingDocumentsCustomsUQList>("When the declaration is export, we get the CustomsUQList from the XML list A0140", exportUQList);
				AssertSame("Export, cached", exportUQList, supportingDocumentForLine.Lookups.CustomsUQList);
			});
		}

		public void TestCustomsUQ2List_Export()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "DC44E", Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit, "MeasurementUnit",
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, Core.Constants.CountryCodes.Germany);

			var codeWithoutAttributeUOM = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, "DC44E", "EXP1", "EXP1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeWithoutAttributeUOM.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			var codeWithAttributeUOM = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, "DC44E", "3LLA231", "3LLA231", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeWithAttributeUOM.PK, UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit, "MeasurementUnit");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeWithAttributeUOM.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "MIL", "Kilograms", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KLT", "Kilograms", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGM", "Kilograms", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;

			supportingDocumentForLine.CSI_FullType = "EXP1";
			var lookups = supportingDocumentForLine.Lookups;

			CombineAssertions(() =>
			{
				AssertEquals("RefCusCode null", string.Empty, lookups.CustomsUQ2List.CodesAsString);

				supportingDocumentForLine.CSI_FullType = "EXP1";
				AssertEquals("RefCusCode doesn't have attribute", string.Empty, lookups.CustomsUQ2List.CodesAsString);

				supportingDocumentForLine.CSI_FullType = "3LLA231";
				AssertEquals("RefCusCode has attribute", "KGM, KLT, MIL", lookups.CustomsUQ2List.CodesAsString);
			});
		}

		public void TestCustomsUQ2List_Import()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "TESTA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "TESTB", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var lookups = supportingDocumentForLine.Lookups;
			var list = lookups.CustomsUQ2List;

			CombineAssertions(() =>
			{
				AssertEquals("List", "TESTA, TESTB", list.CodesAsString);
				AssertSame("Cached", list, lookups.CustomsUQ2List);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			dec = Factory.New<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			supportingDocumentForHeader = invoiceHeader.SupportingDocuments.AddNew();
			supportingDocumentForLine = invoiceLine.SupportingDocuments.AddNew();
		}

		JobDeclaration dec;
		SupportingDocument supportingDocumentForHeader;
		SupportingDocument supportingDocumentForLine;
	}
}
