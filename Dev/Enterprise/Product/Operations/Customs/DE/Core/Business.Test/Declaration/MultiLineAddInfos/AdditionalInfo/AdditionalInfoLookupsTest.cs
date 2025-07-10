using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class AdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList_ExportInvoiceHeader_INF()
		{
			invoiceAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
			AssertCodeList(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, RefCusCodeListLevelType.Header, new AdditionalInfoLookups(invoiceAdditionalInfo));
		}

		public void TestCodeList_ExportInvoiceHeader_REF()
		{
			invoiceAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			AssertCodeList(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44E, RefCusCodeListLevelType.Header, new AdditionalInfoLookups(invoiceAdditionalInfo));
		}

		public void TestCodeList_ExportInvoiceHeader_TRA()
		{
			invoiceAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.TransportDocuments;
			AssertCodeList(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44E, RefCusCodeListLevelType.Header, new AdditionalInfoLookups(invoiceAdditionalInfo));
		}

		public void TestCodeList_INF_ExportInvoiceLine()
		{
			invoiceLineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
			AssertCodeList(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, RefCusCodeListLevelType.Item, new AdditionalInfoLookups(invoiceLineAdditionalInfo));
		}

		public void TestCodeList_INF_ExportCusClassPartPivot()
		{
			var additionalInfo = AdditionalInfoTest.CreateAdditionalInfoWithPivotParent(Factory);
			additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
			AssertCodeList(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, RefCusCodeListLevelType.Item, new AdditionalInfoLookups(additionalInfo));
		}

		public void TestCodeList_REF_ExportInvoiceLine()
		{
			invoiceLineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			AssertCodeList(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44E, RefCusCodeListLevelType.Item, new AdditionalInfoLookups(invoiceLineAdditionalInfo));
		}

		public void TestCodeList_REF_ExportCusClassPartPivot()
		{
			var additionalInfo = AdditionalInfoTest.CreateAdditionalInfoWithPivotParent(Factory);
			additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			AssertCodeList(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44E, RefCusCodeListLevelType.Item, new AdditionalInfoLookups(additionalInfo));
		}

		public void TestCodeList_AUT_ExportInvoiceLine()
		{
			invoiceLineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
			AssertCodeList(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AA44E, RefCusCodeListLevelType.Item, new AdditionalInfoLookups(invoiceLineAdditionalInfo));
		}

		public void TestCodeList_AUT_ExportCusClassPartPivot()
		{
			var additionalInfo = AdditionalInfoTest.CreateAdditionalInfoWithPivotParent(Factory);
			additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
			AssertCodeList(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AA44E, RefCusCodeListLevelType.Item, new AdditionalInfoLookups(additionalInfo));
		}

		public void TestKindList_InvoiceHeader()
		{
			var lookups = new AdditionalInfoLookups(invoiceAdditionalInfo);
			var list = lookups.KindList;
			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements("List", new[] { AdditionalDocTypeList.Codes.AdditionalReference, AdditionalDocTypeList.Codes.AdditionalInformation, AdditionalDocTypeList.Codes.TransportDocuments }, list.GetAllCodes());
				AssertSame("Cached", list, lookups.KindList);
			});
		}

		public void TestKindList_InvoiceLine()
		{
			var lookups = new AdditionalInfoLookups(invoiceLineAdditionalInfo);
			var list = lookups.KindList;
			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements("List", new[] { AdditionalDocTypeList.Codes.AdditionalReference, AdditionalDocTypeList.Codes.AdditionalInformation, AdditionalDocTypeList.Codes.Authorization }, list.GetAllCodes());
				AssertSame("Cached", list, lookups.KindList);
			});
		}

		public void TestKindList_ExitSummary()
		{
			var additionalInfo = CreateAdditionalInfoFromExitDetail();
			var lookups = new AdditionalInfoLookups(additionalInfo);
			var list = lookups.KindList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { AdditionalDocTypeList.Codes.AdditionalInformation }, list.GetAllCodes());
				AssertSame("Cached", list, lookups.KindList);
			});
		}

		public void TestCodeList_ExitSummary()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListType.Code.Code_AI44X, "AI44X");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, "AI44E");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, RefCusCodeListType.Code.Code_AI44X, "DE01", "DE01 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, RefCusCodeListType.Code.Code_AI44X, "DE02", "DE02 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, RefCusCodeListType.Code.Code_AI44X, "DE03", "DE03 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, RefCusCodeListType.Code.Code_AI44X, "IT01", "IT01 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, "DE04", "DE04 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var additionalInfo = CreateAdditionalInfoFromExitDetail();
			var lookups = new AdditionalInfoLookups(additionalInfo);
			var list = (ZZRefCusCodeListCombinedCollection)lookups.CodeList;
			list.Load();

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "DE01", "DE02", "DE03" }, list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
				AssertSame("Cached", list, lookups.CodeList);
			});
		}

		void AssertCodeList(string codeType, string levelAttribute, AdditionalInfoLookups lookups)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_C0754, "C0754");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", codeType, Core.Constants.CountryCodes.Germany);

			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, "DE01", "DE01 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, "DE02", "DE02 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, "DE03", "Invalid attribute", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_C0754, "DE04", "Invalid type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code5 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, "DE05", "Invalid date", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddMonths(-1));
			var code6 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, codeType, "IT01", "Invalid grouping", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code3.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute == RefCusCodeListLevelType.Item ? RefCusCodeListLevelType.Header : RefCusCodeListLevelType.Item);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code5.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code6.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute);

			if (codeType.In(new[] { UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44E, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44E }) && levelAttribute == RefCusCodeListLevelType.Header)
			{
				var code7 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, UniversalReferenceConstants.SupportingDocumentTypes._9ZZY, "Code will get filtered", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				var code8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, UniversalReferenceConstants.SupportingDocumentTypes._9ZZX, "Code will get filtered", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(code7.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute);
				helper.CreateNewOrGetExistingCusCodeListAttribute(code8.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute);
				if (codeType == UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44E)
				{
					var code9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, UniversalReferenceConstants.SupportingDocumentTypes.N720, "Code will get filtered", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
					var code10 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, UniversalReferenceConstants.SupportingDocumentTypes.C613, "Code will get filtered", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
					var code11 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, UniversalReferenceConstants.SupportingDocumentTypes.C614, "Code will get filtered", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
					helper.CreateNewOrGetExistingCusCodeListAttribute(code9.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute);
					helper.CreateNewOrGetExistingCusCodeListAttribute(code10.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute);
					helper.CreateNewOrGetExistingCusCodeListAttribute(code11.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute);
				}
			}

			Factory.Save();

			var codeList = (ZZRefCusCodeListCombinedCollection)lookups.CodeList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "DE01", "DE02" }, codeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceAdditionalInfo = invoice.AdditionalInfos.AddNew();
			invoiceLineAdditionalInfo = invoiceLine.AdditionalInfos.AddNew();
		}
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		AdditionalInfo invoiceAdditionalInfo;
		AdditionalInfo invoiceLineAdditionalInfo;

		AdditionalInfo CreateAdditionalInfoFromExitDetail() => Factory.New<CusExitDetail>().AdditionalInfos.AddNew();
	}
}
