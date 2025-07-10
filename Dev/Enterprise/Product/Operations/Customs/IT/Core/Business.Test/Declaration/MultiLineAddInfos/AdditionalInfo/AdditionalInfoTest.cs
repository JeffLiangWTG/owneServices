using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(AdditionalInfo))]
sealed class AdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<AdditionalInfo>
{
	public void TestDescriptionMaxLength()
	{
		AssertEquals(512, additionalInfo.CSI_DescriptionInfo.MaxLength);
	}

	public void TestCheckCSI_CodeMaxLength()
	{
		AssertEquals(5, additionalInfo.CSI_CodeInfo.MaxLength);
	}

	public void TestCheckCSI_ReferenceNumberMaxLength()
	{
		AssertEquals(70, additionalInfo.CSI_ReferenceNumberInfo.MaxLength);
	}

	public void TestSetDescriptionFromSelectedCode()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var countryCode = Core.Constants.CountryCodes.Italy;
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(countryCode, "Italy", eun);

		var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
		helper.CreateNewOrGetExistingCusCodeType(codeType, "Additional Information");
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", codeType, countryCode);

		var cusCode = helper.CreateCusCodeList(countryCode, codeType, "11111", "ABC description for test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var attribute = cusCode.Attributes.AddNew("Direction", "IMPORT");

		Factory.Save();

		declaration.JE_MessageType = "IMP";

		additionalInfo.CSI_Code = "XYZ";
		AssertEquals("When CSI Code not present in Lookups", "", additionalInfo.CSI_Description);

		additionalInfo.CSI_Code = "11111";
		AssertEquals("When CSI Code present in Lookups", "ABC description for test", additionalInfo.CSI_Description);

		additionalInfo.CSI_Code = "XYZ";
		AssertEquals("When CSI Code not present in Lookups, dont clear existing description in the box", "ABC description for test", additionalInfo.CSI_Description);
	}

	public void TestSetDescriptionForCode_Ucc6Export()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy");

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "Additional Information Code Type");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "AI1", "AddInfo1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		declaration.JE_MessageType = "EXP";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInfo.CSI_Code = "AI1";
			AssertEquals("AddInfo1", ZString.Empty, additionalInfo.CSI_Description);
		}
	}

	public void TestLookupsType()
	{
		AssertLookupType(isUcc6: true, expectedExportLookupType: typeof(Ucc6ExportAdditionalInfoLookups));
		AssertLookupType(isUcc6: false, expectedExportLookupType: typeof(AdditionalInfoLookups));

		void AssertLookupType(bool isUcc6, Type expectedExportLookupType)
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUcc6))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				additionalInfo = invoiceLine.AdditionalInfos.AddNew();
				AssertType<AdditionalInfoLookups>(additionalInfo.Lookups);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				additionalInfo = invoiceLine.AdditionalInfos.AddNew();
				AssertType(expectedExportLookupType, additionalInfo.Lookups);
			}
		}
	}

	public void TestValidationType()
	{
		AssertValidationType(isUcc6: true, expectedValidationType: typeof(Ucc6ExportInvoiceLineAdditionalInfoValidation));
		AssertValidationType(isUcc6: false, expectedValidationType: typeof(AdditionalInfoValidation));

		void AssertValidationType(bool isUcc6, Type expectedValidationType)
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUcc6))
			{
				CombineAssertions($"When UCC6:{isUcc6}", () =>
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					AssertType<AdditionalInfoValidation>("Import Declaration", additionalInfo.Validation);

					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					additionalInfo = invoiceLine.AdditionalInfos.AddNew();
					AssertType("Export Declaration", expectedValidationType, additionalInfo.Validation);
				});
			}
		}
	}

	public void TestValidationType_WhenParentIsEntryInstruction()
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryInstructionAdditionalInfo = entryInstruction.AdditionalInfos.AddNew();

		AssertValidationType(isUcc6: true, expectedValidationType: typeof(Ucc6ExportEntryInstructionAdditionalInfoValidation));
		AssertValidationType(isUcc6: false, expectedValidationType: typeof(AdditionalInfoValidation));

		void AssertValidationType(bool isUcc6, Type expectedValidationType)
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUcc6))
			{
				CombineAssertions($"When UCC6: {isUcc6}", () =>
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					AssertType<AdditionalInfoValidation>("Import Declaration", entryInstructionAdditionalInfo.Validation);

					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					entryInstructionAdditionalInfo = entryInstruction.AdditionalInfos.AddNew();
					AssertType("Export Declaration", expectedValidationType, entryInstructionAdditionalInfo.Validation);
				});
			}
		}
	}

	public void TestIsDescriptionReadOnly()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("UCC6, Export, Kind: INF", false, additionalInfo.IsDescriptionReadOnly);

				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("UCC6, Export, Kind: REF", true, additionalInfo.IsDescriptionReadOnly);

				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
				AssertEquals("UCC6, Export, Kind: TRA", true, additionalInfo.IsDescriptionReadOnly);

				additionalInfo.CSI_SubType = ZString.Empty;
				AssertEquals("UCC6, Export, Kind: Empty", true, additionalInfo.IsDescriptionReadOnly);

				additionalInfo.CSI_SubType = "XYZ";
				AssertEquals("UCC6, Export, Kind: Invalid Code", true, additionalInfo.IsDescriptionReadOnly);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertEquals("UCC6, Import, Kind: TRA", false, additionalInfo.IsDescriptionReadOnly);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("Non UCC6, Export, Kind: INF", false, additionalInfo.IsDescriptionReadOnly);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("Non UCC6, Import, Kind: INF", false, additionalInfo.IsDescriptionReadOnly);
			});
		}
	}

	public void TestIsReferenceNumberReadOnly()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("UCC6, Export, Kind: INF", true, additionalInfo.IsReferenceNumberReadOnly);

				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("UCC6, Export, Kind: REF", false, additionalInfo.IsReferenceNumberReadOnly);

				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
				AssertEquals("UCC6, Export, Kind: TRA", false, additionalInfo.IsReferenceNumberReadOnly);

				additionalInfo.CSI_SubType = ZString.Empty;
				AssertEquals("UCC6, Export, Kind: Empty", true, additionalInfo.IsReferenceNumberReadOnly);

				additionalInfo.CSI_SubType = "XYZ";
				AssertEquals("UCC6, Export, Kind: Invalid Code", true, additionalInfo.IsReferenceNumberReadOnly);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertEquals("UCC6, Import, Kind: TRA", false, additionalInfo.IsReferenceNumberReadOnly);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("Non UCC6, Export, Kind: INF", false, additionalInfo.IsReferenceNumberReadOnly);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("Non UCC6, Import, Kind: INF", false, additionalInfo.IsReferenceNumberReadOnly);
			});
		}
	}

	public void TestCSI_DescriptionReset()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			additionalInfo.CSI_Description = "DESCRIPTION";
			additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals("CSI_Description", "DESCRIPTION", additionalInfo.CSI_Description);

			additionalInfo.CSI_Description = "DESCRIPTION";
			additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			AssertEquals("CSI_Description", ZString.Empty, additionalInfo.CSI_Description);

			additionalInfo.CSI_Description = "DESCRIPTION";
			additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			AssertEquals("CSI_Description", ZString.Empty, additionalInfo.CSI_Description);

			additionalInfo.CSI_Description = "DESCRIPTION";
			additionalInfo.CSI_SubType = ZString.Empty;
			AssertEquals("CSI_Description", ZString.Empty, additionalInfo.CSI_Description);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			additionalInfo.CSI_Description = "DESCRIPTION";
			additionalInfo.CSI_SubType = ZString.Empty;
			AssertEquals("CSI_Description", "DESCRIPTION", additionalInfo.CSI_Description);

			additionalInfo.CSI_Description = "DESCRIPTION";
			additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals("CSI_Description", "DESCRIPTION", additionalInfo.CSI_Description);
		}
	}

	public void TestCSI_ReferenceNumberReset()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			additionalInfo.CSI_ReferenceNumber = "REF1234";
			additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			AssertEquals("CSI_ReferenceNumber", "REF1234", additionalInfo.CSI_ReferenceNumber);

			additionalInfo.CSI_ReferenceNumber = "REF1234";
			additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals("CSI_ReferenceNumber", ZString.Empty, additionalInfo.CSI_ReferenceNumber);

			additionalInfo.CSI_ReferenceNumber = "REF1234";
			additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			AssertEquals("CSI_ReferenceNumber", "REF1234", additionalInfo.CSI_ReferenceNumber);

			additionalInfo.CSI_ReferenceNumber = "REF1234";
			additionalInfo.CSI_SubType = ZString.Empty;
			AssertEquals("CSI_ReferenceNumber", ZString.Empty, additionalInfo.CSI_ReferenceNumber);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			additionalInfo.CSI_ReferenceNumber = "REF1234";
			additionalInfo.CSI_SubType = ZString.Empty;
			AssertEquals("CSI_ReferenceNumber", "REF1234", additionalInfo.CSI_ReferenceNumber);

			additionalInfo.CSI_ReferenceNumber = "REF1234";
			additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals("CSI_ReferenceNumber", "REF1234", additionalInfo.CSI_ReferenceNumber);
		}
	}

	public void TestRefCusCode()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var dataGroupingEun = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
		var dataGroupingIT = helper.CreateNewOrGetExistingDataGrouping("IT", "Italy", dataGroupingEun);

		var exportAddRef = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportAddDocAdditionalReference;

		helper.CreateCusCodeList(dataGroupingIT.ZZZ_DataGrouping, exportAddRef, "AB01C", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeList(dataGroupingEun.ZZZ_DataGrouping, exportAddRef, "BC02D", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		var addInfoRef = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;

		helper.CreateCusCodeList(dataGroupingIT.ZZZ_DataGrouping, addInfoRef, "UF10W", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeList(dataGroupingEun.ZZZ_DataGrouping, addInfoRef, "GH89Y", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();

		declaration.JE_MessageType = "EXP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var additionalInfo = invoiceLine.AdditionalInfos.AddNew();

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
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		additionalInfo = invoiceLine.AdditionalInfos.AddNew();

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

	protected override IEnumerable<AdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		Factory.Save();
		yield return additionalInfo;
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		additionalInfo = invoiceLine.AdditionalInfos.AddNew();
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	AdditionalInfo additionalInfo;
}
