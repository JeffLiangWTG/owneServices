using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(AdditionalInfo))]
sealed class AdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<AdditionalInfo>
{
	public void TestReadOnlyPropertiesInvoiceHeader_Reference()
	{
		var additionalInfoEXP = GetInvoiceHeaderAdditionalInfo(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Export);
		var additionalInfoIMP = GetInvoiceHeaderAdditionalInfo(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import);
		CombineAssertions(() =>
		{
			additionalInfoEXP.CSI_SubType = "INF";
			AssertEquals("ReadOnly if SubType is INF", true, additionalInfoEXP.CSI_ReferenceNumberInfo.ReadOnly);
			additionalInfoEXP.CSI_SubType = "REF";
			AssertEquals("Editable if SubType is not INF", false, additionalInfoEXP.CSI_ReferenceNumberInfo.ReadOnly);

			additionalInfoIMP.CSI_SubType = "INF";
			AssertEquals("ReadOnly if SubType is INF", true, additionalInfoIMP.CSI_ReferenceNumberInfo.ReadOnly);
			additionalInfoIMP.CSI_SubType = "REF";
			AssertEquals("Editable if SubType is not INF", false, additionalInfoIMP.CSI_ReferenceNumberInfo.ReadOnly);
		});
	}

	public void TestReadOnlyPropertiesEntryInstruction_Reference()
	{
		var additionalInfoEXP = GetEntryInstructionAdditionalInfo(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Export);
		var additionalInfoIMP = GetEntryInstructionAdditionalInfo(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import);
		CombineAssertions(() =>
		{
			additionalInfoEXP.CSI_SubType = "INF";
			AssertEquals("ReadOnly if SubType is INF", true, additionalInfoEXP.CSI_ReferenceNumberInfo.ReadOnly);
			additionalInfoEXP.CSI_SubType = "REF";
			AssertEquals("Editable if SubType is not INF", false, additionalInfoEXP.CSI_ReferenceNumberInfo.ReadOnly);

			additionalInfoIMP.CSI_SubType = "INF";
			AssertEquals("ReadOnly if SubType is INF", true, additionalInfoIMP.CSI_ReferenceNumberInfo.ReadOnly);
			additionalInfoIMP.CSI_SubType = "REF";
			AssertEquals("Editable if SubType is not INF", false, additionalInfoIMP.CSI_ReferenceNumberInfo.ReadOnly);
		});
	}

	public void TestReadOnlyPropertiesInvoiceHeader_Description()
	{
		var additionalInfoEXP = GetInvoiceHeaderAdditionalInfo(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Export);
		var additionalInfoIMP = GetInvoiceHeaderAdditionalInfo(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import);
		CombineAssertions(() =>
		{
			additionalInfoEXP.CSI_SubType = "INF";
			AssertEquals("Editable if SubType is not TRA/REF", false, additionalInfoEXP.CSI_DescriptionInfo.ReadOnly);
			additionalInfoEXP.CSI_SubType = "REF";
			AssertEquals("ReadOnly if SubType is REF", true, additionalInfoEXP.CSI_DescriptionInfo.ReadOnly);
			additionalInfoEXP.CSI_SubType = "TRA";
			AssertEquals("ReadOnly if SubType is TRA", true, additionalInfoEXP.CSI_DescriptionInfo.ReadOnly);

			additionalInfoIMP.CSI_SubType = "INF";
			AssertEquals("Editable if SubType is not TRA/REF", false, additionalInfoIMP.CSI_DescriptionInfo.ReadOnly);
			additionalInfoIMP.CSI_SubType = "REF";
			AssertEquals("ReadOnly if SubType is REF", true, additionalInfoIMP.CSI_DescriptionInfo.ReadOnly);
			additionalInfoIMP.CSI_SubType = "TRA";
			AssertEquals("ReadOnly if SubType is TRA", true, additionalInfoIMP.CSI_DescriptionInfo.ReadOnly);
		});
	}

	public void TestParentAsInvoiceHeader()
	{
		AssertType<JobComInvoiceHeader>(GetInvoiceHeaderAdditionalInfo(Factory).ParentAsInvoiceHeader);
	}

	public void TestParentAsInvoiceLine()
	{
		AssertType<JobComInvoiceLine>(GetInvoiceLineAdditionalInfo(Factory).ParentAsInvoiceLine);
	}

	public void TestValidationType()
	{
		AssertType<AdditionalInfoValidation>(GetInvoiceHeaderAdditionalInfo(Factory).Validation);
	}

	public void TestLookups()
	{
		AssertType<AdditionalInfoLookups>(GetInvoiceHeaderAdditionalInfo(Factory).Lookups);
	}

	public void TestPropertyAttributes()
	{
		var info = GetInvoiceHeaderAdditionalInfo(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("CSI_Description: Caption", "Description", info.CSI_DescriptionInfo.Description);
			AssertEquals("CSI_SubType: Caption", "Kind", info.CSI_SubTypeInfo.Description);
			AssertEquals("CSI_ReferenceNumber: Caption", "Reference", info.CSI_ReferenceNumberInfo.Description);
			AssertEquals("CSI_ReferenceNumber2: Caption", "Detail", info.CSI_ReferenceNumber2Info.Description);
			AssertEquals("CSI_RX_NKCurrency: Caption", "Currency", info.CSI_RX_NKCurrencyInfo.Description);
			AssertEquals("CSI_Code: Caption", "Full Type", info.CSI_CodeInfo.Description);

			AssertEquals("CSI_Description: MaxLength", 70, info.CSI_DescriptionInfo.MaxLength);
			AssertEquals("CSI_ReferenceNumber: MaxLength", 35, info.CSI_ReferenceNumberInfo.MaxLength);
			AssertEquals("CSI_SubType: MaxLength", 3, info.CSI_SubTypeInfo.MaxLength);
		});
	}

	public void TestReadOnlyPropertiesInvoiceLineExport_ReferenceNumber()
	{
		var additionalInfoEXP = GetInvoiceLineAdditionalInfo(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Export);
		var additionalInfoIMP = GetInvoiceLineAdditionalInfo(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import);
		CombineAssertions(() =>
		{
			additionalInfoEXP.CSI_SubType = "TRA";
			additionalInfoEXP.CSI_ReferenceNumber = "ref";
			AssertEquals("Editable if SubType is TRA", false, additionalInfoEXP.CSI_ReferenceNumberInfo.ReadOnly);
			AssertEquals("ReferenceNumber should keep value when not read only", "ref", additionalInfoEXP.CSI_ReferenceNumber);
			additionalInfoEXP.CSI_SubType = "REF";
			AssertEquals("Editable if SubType is REF", false, additionalInfoEXP.CSI_ReferenceNumberInfo.ReadOnly);
			AssertEquals("ReferenceNumber should keep value when not read only", "ref", additionalInfoEXP.CSI_ReferenceNumber);
			additionalInfoEXP.CSI_SubType = "INF";
			AssertEquals("Read only if SubType is INF", true, additionalInfoEXP.CSI_ReferenceNumberInfo.ReadOnly);
			AssertEquals("ReferenceNumber should clar when read only", string.Empty, additionalInfoEXP.CSI_ReferenceNumber);

			additionalInfoIMP.CSI_SubType = "INF";
			AssertEquals("Read only if SubType is INF", true, additionalInfoIMP.CSI_ReferenceNumberInfo.ReadOnly);
			additionalInfoIMP.CSI_SubType = "REF";
			AssertEquals("Editable if SubType is not INF", false, additionalInfoIMP.CSI_ReferenceNumberInfo.ReadOnly);
		});
	}

	public void TestReadOnlyPropertiesInvoiceLineExport_Amount()
	{
		var additionalInfo = GetInvoiceLineAdditionalInfo(Factory);
		additionalInfo.CSI_SubType = "INF";
		CombineAssertions(() =>
		{
			AssertEquals("Read only if no attribute Value", true, additionalInfo.CSI_ValueInfo.ReadOnly);
			AddAttributeForReadOnlyProperty(additionalInfo, UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value);
			AssertEquals("Editable once attribute Value exists", false, additionalInfo.CSI_ValueInfo.ReadOnly);
		});
	}

	public void TestReadOnlyPropertiesInvoiceLineExport_Currency()
	{
		var additionalInfo = GetInvoiceLineAdditionalInfo(Factory);
		additionalInfo.CSI_SubType = "INF";
		CombineAssertions(() =>
		{
			AssertEquals("Read only if no attribute Value", true, additionalInfo.CSI_RX_NKCurrencyInfo.ReadOnly);
			AddAttributeForReadOnlyProperty(additionalInfo, UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value);
			AssertEquals("Editable once attribute Value exists", false, additionalInfo.CSI_RX_NKCurrencyInfo.ReadOnly);
		});
	}

	public void TestReadOnlyPropertiesInvoiceLineExport_ReferenceNumber2()
	{
		var additionalInfo = GetInvoiceLineAdditionalInfo(Factory);
		additionalInfo.CSI_SubType = "INF";
		CombineAssertions(() =>
		{
			AssertEquals("Read only if no attribute Detail", true, additionalInfo.CSI_ReferenceNumber2Info.ReadOnly);
			AddAttributeForReadOnlyProperty(additionalInfo, UniversalReferenceConstants.RefCusCodeListAttributes.Name.Detail);
			AssertEquals("Editable once attribute Detail exists", false, additionalInfo.CSI_ReferenceNumber2Info.ReadOnly);
		});
	}

	public void TestReadOnlyPropertiesInvoiceLine_Description()
	{
		var additionalInfoEXP = GetInvoiceLineAdditionalInfo(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Export);
		var additionalInfoIMP = GetInvoiceLineAdditionalInfo(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import);
		CombineAssertions(() =>
		{
			additionalInfoEXP.CSI_Description = "desc";
			additionalInfoEXP.CSI_SubType = "INF";
			AssertEquals("Editable if SubType is not TRA/REF", false, additionalInfoEXP.CSI_DescriptionInfo.ReadOnly);
			AssertEquals("Description should keep value when not read only", "desc", additionalInfoEXP.CSI_Description);
			additionalInfoEXP.CSI_SubType = "REF";
			AssertEquals("Read once SubType is REF", true, additionalInfoEXP.CSI_DescriptionInfo.ReadOnly);
			AssertEquals("Description should be cleared when it becomes read only", string.Empty, additionalInfoEXP.CSI_Description);
			additionalInfoEXP.CSI_SubType = "TRA";
			AssertEquals("Read once SubType is TRA", true, additionalInfoEXP.CSI_DescriptionInfo.ReadOnly);

			additionalInfoIMP.CSI_SubType = "INF";
			AssertEquals("Editable if SubType is not TRA/REF", false, additionalInfoIMP.CSI_DescriptionInfo.ReadOnly);
			additionalInfoIMP.CSI_SubType = "REF";
			AssertEquals("Read once SubType is REF", true, additionalInfoIMP.CSI_DescriptionInfo.ReadOnly);
			additionalInfoIMP.CSI_SubType = "TRA";
			AssertEquals("Read once SubType is TRA", true, additionalInfoIMP.CSI_DescriptionInfo.ReadOnly);
		});
	}

	public void TestReadOnlyPropertiesEntryInstruction_Description()
	{
		var additionalInfoEXP = GetEntryInstructionAdditionalInfo(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Export);
		var additionalInfoIMP = GetEntryInstructionAdditionalInfo(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import);
		CombineAssertions(() =>
		{
			additionalInfoEXP.CSI_Description = "desc";
			additionalInfoEXP.CSI_SubType = "INF";
			AssertEquals("Editable if SubType is not TRA/REF", false, additionalInfoEXP.CSI_DescriptionInfo.ReadOnly);
			AssertEquals("Description should keep value when not read only", "desc", additionalInfoEXP.CSI_Description);
			additionalInfoEXP.CSI_SubType = "REF";
			AssertEquals("Read once SubType is REF", true, additionalInfoEXP.CSI_DescriptionInfo.ReadOnly);
			AssertEquals("Description should be cleared when it becomes read only", string.Empty, additionalInfoEXP.CSI_Description);
			additionalInfoEXP.CSI_SubType = "TRA";
			AssertEquals("Read once SubType is TRA", true, additionalInfoEXP.CSI_DescriptionInfo.ReadOnly);

			additionalInfoIMP.CSI_SubType = "INF";
			AssertEquals("Editable if SubType is not TRA/REF", false, additionalInfoIMP.CSI_DescriptionInfo.ReadOnly);
			additionalInfoIMP.CSI_SubType = "REF";
			AssertEquals("Read once SubType is REF", true, additionalInfoIMP.CSI_DescriptionInfo.ReadOnly);
			additionalInfoIMP.CSI_SubType = "TRA";
			AssertEquals("Read once SubType is TRA", true, additionalInfoIMP.CSI_DescriptionInfo.ReadOnly);
		});
	}

	void AddAttributeForReadOnlyProperty(AdditionalInfo additionalInfo, string attributeName)
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, parent: eunZZZ);
		helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, "AI44E");
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, Core.Constants.CountryCodes.Belgium);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, attributeName, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, Core.Constants.CountryCodes.Belgium);

		var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, "BE01", "BE01 DESC");
		helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
		helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeName, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
		Factory.Save();
		additionalInfo.CSI_Code = "BE01";
	}

	protected override IEnumerable<AdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return GetInvoiceHeaderAdditionalInfo(factory);
	}

	protected override BusinessObject GetNewBusinessObject() => GetInvoiceHeaderAdditionalInfo(Factory);

	static AdditionalInfo GetInvoiceHeaderAdditionalInfo(BusinessObjectFactory factory, string messageType = Common.Shared.SharedJobMessageTypeList.Codes.Export)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		var invoice = declaration.Invoices.AddNew();
		return invoice.AdditionalInfos.AddNew();
	}

	static AdditionalInfo GetInvoiceLineAdditionalInfo(BusinessObjectFactory factory, string messageType = Common.Shared.SharedJobMessageTypeList.Codes.Export)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		return invoiceLine.AdditionalInfos.AddNew();
	}

	static AdditionalInfo GetEntryInstructionAdditionalInfo(BusinessObjectFactory factory, string messageType = Common.Shared.SharedJobMessageTypeList.Codes.Export)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		return entryInstruction.AdditionalInfos.AddNew();
	}
}
