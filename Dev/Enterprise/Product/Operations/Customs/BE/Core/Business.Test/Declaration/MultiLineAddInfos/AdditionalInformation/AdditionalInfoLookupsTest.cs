using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.BE.Business.Testing;

class AdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeList_ExportInvoiceHeader_Default()
	{
		InvoiceAdditionalInfo.CSI_SubType = null;
		AssertCodeList(new[] { UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44E, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44E }, RefCusCodeListLevelType.Header, new AdditionalInfoLookups(InvoiceAdditionalInfo));
	}

	public void TestCodeList_ExportInvoiceHeader_INF()
	{
		InvoiceAdditionalInfo.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalInformation;
		AssertCodeList(new[] { UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E }, RefCusCodeListLevelType.Header, new AdditionalInfoLookups(InvoiceAdditionalInfo));
	}

	public void TestCodeList_ExportInvoiceHeader_REF()
	{
		InvoiceAdditionalInfo.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalReference;
		AssertCodeList(new[] { UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44E }, RefCusCodeListLevelType.Header, new AdditionalInfoLookups(InvoiceAdditionalInfo));
	}

	public void TestCodeList_ExportInvoiceHeader_TRA()
	{
		InvoiceAdditionalInfo.CSI_SubType = BEAdditionalDocTypeList.Codes.TransportDocuments;
		AssertCodeList(new[] { UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44E }, RefCusCodeListLevelType.Header, new AdditionalInfoLookups(InvoiceAdditionalInfo));
	}

	public void TestCodeList_ImportInvoiceHeader_Default()
	{
		var additionalInfo = GetInvoiceHeaderAdditionalInfo(Common.Shared.SharedJobMessageTypeList.Codes.Import);
		AssertCodeList(new[] { UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44I }, RefCusCodeListLevelType.Header, new AdditionalInfoLookups(additionalInfo));
	}

	public void TestCodeList_ImportInvoiceHeader_REF()
	{
		var additionalInfo = GetInvoiceHeaderAdditionalInfo(Common.Shared.SharedJobMessageTypeList.Codes.Import);
		additionalInfo.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalReference;
		AssertCodeList(new[] { UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44I }, RefCusCodeListLevelType.Header, new AdditionalInfoLookups(additionalInfo));
	}

	public void TestCodeList_ImportInvoiceHeader_INF()
	{
		var additionalInfo = GetInvoiceHeaderAdditionalInfo(Common.Shared.SharedJobMessageTypeList.Codes.Import);
		additionalInfo.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalInformation;
		AssertCodeList(new[] { UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44I }, RefCusCodeListLevelType.Header, new AdditionalInfoLookups(additionalInfo));
	}

	public void TestCodeList_ImportInvoiceHeader_TRA()
	{
		var additionalInfo = GetInvoiceHeaderAdditionalInfo(Common.Shared.SharedJobMessageTypeList.Codes.Import);
		additionalInfo.CSI_SubType = BEAdditionalDocTypeList.Codes.TransportDocuments;
		AssertCodeList(new[] { UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44I }, RefCusCodeListLevelType.Header, new AdditionalInfoLookups(additionalInfo));
	}

	public void TestCodeList_ImportInvoiceLine_TRA()
	{
		var additionalInfo = GetInvoiceLineAdditionalInfo(Common.Shared.SharedJobMessageTypeList.Codes.Import);
		AssertCodeList(new[] { UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44I }, RefCusCodeListLevelType.Item, new AdditionalInfoLookups(additionalInfo));
	}

	public void TestCodeList_ExportInvoiceLine_TRA()
	{
		var additionalInfo = GetInvoiceLineAdditionalInfo(Common.Shared.SharedJobMessageTypeList.Codes.Export);
		AssertCodeList(new[] { UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44E }, RefCusCodeListLevelType.Item, new AdditionalInfoLookups(additionalInfo));
	}

	public void TestKindList_InvoiceHeader()
	{
		var lookups = new AdditionalInfoLookups(InvoiceAdditionalInfo);
		var list = lookups.InvoiceHeaderKindList;
		CombineAssertions(() =>
		{
			AssertArrayEqualsByElements("List", new[] { BEAdditionalDocTypeList.Codes.AdditionalReference, BEAdditionalDocTypeList.Codes.AdditionalInformation, BEAdditionalDocTypeList.Codes.TransportDocuments }, list.GetAllCodes());
			AssertSame("Cached", list, lookups.InvoiceHeaderKindList);
		});
	}

	AdditionalInfo GetInvoiceHeaderAdditionalInfo(string messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		var additionalInfo = declaration.Invoices.AddNew().AdditionalInfos.AddNew();
		return additionalInfo;
	}

	AdditionalInfo GetInvoiceLineAdditionalInfo(string messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		var additionalInfo = declaration.Invoices.AddNew().InvoiceLines.AddNew().AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = BEAdditionalDocTypeList.Codes.TransportDocuments;
		return additionalInfo;
	}

	void AssertCodeList(string[] codeTypes, string levelAttribute, AdditionalInfoLookups lookups)
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, parent: eunZZZ);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZ);
		helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_C0754, "C0754");

		foreach (var codeType in codeTypes)
		{
			helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", codeType, Core.Constants.CountryCodes.Belgium);

			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, codeType, "BE01", "DE01 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, codeType, "BE02", "DE02 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, codeType, "BE03", "Invalid attribute", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_C0754, "BE04", "Invalid type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code5 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, codeType, "BE05", "Invalid date", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddMonths(-1));
			var code6 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, codeType, "IT01", "Invalid grouping", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code3.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute == RefCusCodeListLevelType.Item ? RefCusCodeListLevelType.Header : RefCusCodeListLevelType.Item);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code5.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code6.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttribute);
		}

		Factory.Save();

		var codeList = (ZZRefCusCodeListCombinedCollection)lookups.CodeList;
		codeList.Load();
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("Codes", new[] { "BE01", "BE02" }.SelectMany(x => codeTypes.Select(y => $"{y}_{x}")), codeList.Cast<ZZRefCusCodeListCombined>().Select(x => $"{x.ZZD_CodeType}_{x.ZZD_Code}"));
			AssertSame("Cached", codeList, lookups.CodeList);
		});
	}

	AdditionalInfo InvoiceAdditionalInfo
	{
		get
		{
			if (invoiceAdditionalInfo == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				var invoice = declaration.Invoices.AddNew();
				invoiceAdditionalInfo = invoice.AdditionalInfos.AddNew();
			}

			return invoiceAdditionalInfo;
		}
	}
	AdditionalInfo invoiceAdditionalInfo;
}
