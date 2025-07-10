using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(InvoiceHeaderAdditionalInfo))]
sealed class InvoiceHeaderAdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<InvoiceHeaderAdditionalInfo>
{
	public void TestCSI_SubTypeMaxLength()
	{
		AssertEquals("MaxLength", 3, invoiceHeaderAdditionalInfo.CSI_SubTypeInfo.MaxLength);
	}

	public void TestCSI_CodeMaxLength()
	{
		AssertEquals("MaxLength", 5, invoiceHeaderAdditionalInfo.CSI_CodeInfo.MaxLength);
	}

	public void TestCSI_ReferenceNumberMaxLength()
	{
		AssertEquals("MaxLength", 70, invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo.MaxLength);
	}

	public void TestCSI_ReferenceNumberReadOnly()
	{
		invoiceHeaderAdditionalInfo.CSI_SubType = "INF";
		AssertEquals("When CSI_SubType = INF, ReadOnly", true, invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo.ReadOnly);

		invoiceHeaderAdditionalInfo.CSI_SubType = "REF";
		AssertEquals("When CSI_SubType = REF, ReadOnly", false, invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo.ReadOnly);

		invoiceHeaderAdditionalInfo.CSI_SubType = "TRA";
		AssertEquals("When CSI_SubType = TRA, ReadOnly", false, invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo.ReadOnly);
	}

	public void TestCSI_DescriptionMaxLength()
	{
		AssertEquals("MaxLength", 512, invoiceHeaderAdditionalInfo.CSI_DescriptionInfo.MaxLength);
	}

	public void TestCSI_DescriptionReadOnly()
	{
		invoiceHeaderAdditionalInfo.CSI_SubType = "INF";
		AssertEquals("When CSI_SubType = INF, ReadOnly", false, invoiceHeaderAdditionalInfo.CSI_DescriptionInfo.ReadOnly);

		invoiceHeaderAdditionalInfo.CSI_SubType = "REF";
		AssertEquals("When CSI_SubType = REF, ReadOnly", true, invoiceHeaderAdditionalInfo.CSI_DescriptionInfo.ReadOnly);

		invoiceHeaderAdditionalInfo.CSI_SubType = "TRA";
		AssertEquals("When CSI_SubType = TRA, ReadOnly", true, invoiceHeaderAdditionalInfo.CSI_DescriptionInfo.ReadOnly);
	}

	public void TestLookupsType()
	{
		AssertType<InvoiceHeaderAdditionalInfoLookups>("Type", invoiceHeaderAdditionalInfo.Lookups);
	}

	public void TestValidationType()
	{
		AssertType<InvoiceHeaderAdditionalInfoValidation>("Without Declaration, Validation Type", invoiceHeaderAdditionalInfo.Validation);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoice = declaration.Invoices.AddNew();
		var additionalInfo = invoice.AdditionalInfos.AddNew();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
		{
			AssertType<InvoiceHeaderAdditionalInfoValidation>("For non Ucc6 Export, Validation Type", additionalInfo.Validation);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			AssertType<Ucc6ExportInvoiceHeaderAdditionalInfoValidation>("For Ucc6 Export, Validation Type", additionalInfo.Validation);
		}
	}

	public void TestCSI_ReferenceNumberClearedOnCSI_SubTypeChange()
	{
		invoiceHeaderAdditionalInfo.CSI_ReferenceNumber = "REFNO";
		invoiceHeaderAdditionalInfo.CSI_SubType = "REF";
		AssertEquals("When CSI_SubType is set to REF, CSI_ReferenceNumber", "REFNO", invoiceHeaderAdditionalInfo.CSI_ReferenceNumber);

		invoiceHeaderAdditionalInfo.CSI_SubType = "TRA";
		AssertEquals("When CSI_SubType is set to TRA, CSI_ReferenceNumber", "REFNO", invoiceHeaderAdditionalInfo.CSI_ReferenceNumber);

		invoiceHeaderAdditionalInfo.CSI_SubType = "INF";
		AssertEquals("When CSI_SubType is set to INF, CSI_ReferenceNumber", "", invoiceHeaderAdditionalInfo.CSI_ReferenceNumber);
	}

	public void TestCSI_DescriptionClearedOnCSI_SubTypeChange()
	{
		invoiceHeaderAdditionalInfo.CSI_Description = "DESCR";
		invoiceHeaderAdditionalInfo.CSI_SubType = "INF";
		AssertEquals("When CSI_SubType is set to INF, CSI_Description", "DESCR", invoiceHeaderAdditionalInfo.CSI_Description);

		invoiceHeaderAdditionalInfo.CSI_SubType = "REF";
		AssertEquals("When CSI_SubType is set to REF, CSI_Description", "", invoiceHeaderAdditionalInfo.CSI_Description);

		invoiceHeaderAdditionalInfo.CSI_Description = "DESCR";
		invoiceHeaderAdditionalInfo.CSI_SubType = "TRA";
		AssertEquals("When CSI_SubType is set to TRA, CSI_Description", "", invoiceHeaderAdditionalInfo.CSI_Description);
	}

	public void TestRefCusCode()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		var dataGroupingEun = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
		var dataGroupingIT = helper.CreateNewOrGetExistingDataGrouping("IT", "Italy", dataGroupingEun);

		var exportAddRef = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportAddDocAdditionalReference;
		helper.CreateCusCodeList(dataGroupingIT.ZZZ_DataGrouping, exportAddRef, "AB01C", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeList(dataGroupingEun.ZZZ_DataGrouping, exportAddRef, "BC02D", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		var addInfoRef = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
		helper.CreateCusCodeList(dataGroupingIT.ZZZ_DataGrouping, addInfoRef, "UF10W", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeList(dataGroupingEun.ZZZ_DataGrouping, addInfoRef, "GH89Y", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoiceHeader = declaration.Invoices.AddNew();
		var additionalInfo = invoiceHeader.AdditionalInfos.AddNew();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			CombineAssertions("EXP declaration", () =>
			{
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				additionalInfo.CSI_Code = "AB01C";

				AssertEquals("Data grouping: IT, CodeType: AR44E", "AB01C", additionalInfo.RefCusCode.ZZD_Code);

				additionalInfo.CSI_Code = "BC02D";
				AssertNull("Data grouping: EU, CodeType: AR44E", additionalInfo.RefCusCode);

				additionalInfo.CSI_Code = "UF10W";
				AssertNull("Data grouping: EU, CodeType: ADDIN", additionalInfo.RefCusCode);
			});
		}

		declaration.JE_MessageType = "IMP";
		invoiceHeader = declaration.Invoices.AddNew();
		additionalInfo = invoiceHeader.AdditionalInfos.AddNew();

		CombineAssertions("IMP declaration", () =>
		{
			additionalInfo.CSI_Code = "UF10W";
			AssertEquals("Data grouping: IT, CodeType: ADDIN", "UF10W", additionalInfo.RefCusCode.ZZD_Code);

			additionalInfo.CSI_Code = "GH89Y";
			AssertEquals("Data grouping: EU, CodeType: AADIN", "GH89Y", additionalInfo.RefCusCode.ZZD_Code);

			additionalInfo.CSI_Code = "AB01C";
			AssertNull("Data grouping: EU, CodeType: AR44E", additionalInfo.RefCusCode);
		});
	}

	protected override IEnumerable<InvoiceHeaderAdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var invoiceHeaderAdditionalInfo = declaration.Invoices.AddNew().AdditionalInfos.AddNew();
		return new InvoiceHeaderAdditionalInfo[] { invoiceHeaderAdditionalInfo };
	}

	protected override void SetUp()
	{
		base.SetUp();
		var invoice = Factory.New<JobComInvoiceHeader>();
		invoiceHeaderAdditionalInfo = invoice.AdditionalInfos.AddNew();
	}

	InvoiceHeaderAdditionalInfo invoiceHeaderAdditionalInfo;
}
