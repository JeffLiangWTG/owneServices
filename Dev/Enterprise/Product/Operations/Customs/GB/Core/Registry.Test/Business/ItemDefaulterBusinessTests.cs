using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Registry.Testing
{
	[TestedType(typeof(ItemDefaulterSetting))]
	public class ItemDefaulterSettingTest : RegistryBusinessObjectTemplateTestCase<ItemDefaulterSetting>
	{
		public void TestValidation()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "C601", "C601 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "Additional Information");
			var cusCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "NCGDS", "NCGDS DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Level", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			cusCode2.Attributes.AddNew("Direction", "IMPORT");
			cusCode2.Attributes.AddNew("Direction", "EXPORT");
			cusCode2.Attributes.AddNew("Level", "ITEM");

			factory.Save();

			var item = new ItemDefaulterSetting(factory);
			item.SourceType = "XX";
			AssertHasErrorContaining(item.SourceTypeInfo, "valid source type");
			item.SourceType = SourceTypesList.Codes.SupportingDocumentBox44;
			AssertNoErrorContaining(item.SourceTypeInfo, "valid source type");

			item.SourceValue = "XX";
			AssertHasErrorContaining(item.SourceValueInfo, "valid");
			item.SourceType = SourceTypesList.Codes.CustomsProcedureCodeBox37;
			item.SourceValue = "YY";
			AssertNoErrorContaining(item.SourceValueInfo, "valid");
			item.SourceType = SourceTypesList.Codes.SupportingDocumentBox44;
			item.SourceValue = "ABC123";
			AssertHasErrorContaining(item.SourceValueInfo, "valid");
			item.SourceValue = "C601";
			AssertNoErrorContaining(item.SourceValueInfo, "valid");

			item.TargetType = "XX";
			AssertHasErrorContaining(item.TargetTypeInfo, "valid target type");
			item.TargetType = ZString.Empty;
			AssertHasErrorContaining(item.TargetTypeInfo, "valid target type");
			item.TargetType = TargetTypesList.Codes.SupportingDocumentBox44;
			AssertNoErrorContaining(item.TargetTypeInfo, "valid target type");

			item.TargetType = "XX";
			item.TargetCode = "XX";
			AssertHasErrorContaining(item.TargetCodeInfo, "Target type does not allow this value");
			item.TargetType = TargetTypesList.Codes.SupervisingOfficeBox44;
			item.TargetCode = "YY";
			AssertHasErrorContaining(item.TargetCodeInfo, "This field should be blank when the target type is SPOFF");
			item.TargetType = TargetTypesList.Codes.RegistrationNumberFromExporter;
			item.TargetCode = "ZZ";
			AssertHasErrorContaining(item.TargetCodeInfo, "registration code type");
			item.TargetType = TargetTypesList.Codes.RegistrationNumberFromImporter;
			item.TargetCode = "XX";
			AssertHasErrorContaining(item.TargetCodeInfo, "registration code type");
			item.TargetType = TargetTypesList.Codes.SupportingDocumentBox44;
			item.TargetCode = "XX";
			AssertHasErrorContaining(item.TargetCodeInfo, "valid");
			item.TargetType = TargetTypesList.Codes.SupportingDocumentBox44;
			item.TargetCode = "C601";
			AssertNoErrorContaining(item.TargetCodeInfo, "valid");
			AssertNoErrorContaining(item.TargetCodeInfo, "Target type does not allow this value");
			AssertNoErrorContaining(item.TargetCodeInfo, "This field should be blank when the target type is SPOFF");
			AssertNoErrorContaining(item.TargetCodeInfo, "registration code type");
			item.TargetType = TargetTypesList.Codes.SetReferenceFromInvoiceNumber;
			item.TargetCode = "1234";
			AssertHasErrorContaining(item.TargetCodeInfo, "blank");
			item.TargetCode = ZString.Empty;
			AssertNoErrorContaining(item.TargetCodeInfo, "blank");

			item.TargetType = TargetTypesList.Codes.AdditionalInformationStatementBox44;
			item.TargetCode = "NCGDS";
			AssertNoErrorContaining(item.TargetTypeInfo, "valid");
			AssertNoErrorContaining(item.TargetCodeInfo, "valid");
			AssertNoErrorContaining(item.TargetCodeInfo, "Target type does not allow this value");

			item.TargetType = TargetTypesList.Codes.ClientEoriForDucrTickbox;
			item.TargetCode = ZString.Empty;
			AssertNoErrorContaining(item.TargetTypeInfo, "valid");
			AssertNoErrorContaining(item.TargetCodeInfo, "Target type does not allow this value");

			item.ClearAllNotifications();
			item.SourceType = SourceTypesList.Codes.Preference;
			item.SourceValue = ZString.Empty;
			AssertHasErrorContaining(item.SourceValueInfo, "enter a source value");
			item.SourceValue = "1234";
			AssertNoErrorContaining(item.SourceTypeInfo, "enter a source value");
			item.TargetType = ZString.Empty;
			AssertHasErrorContaining(item.TargetTypeInfo, "valid target type");
			item.TargetType = TargetTypesList.Codes.SetReferenceFromInvoiceNumber;
			AssertHasErrorContaining(item.TargetTypeInfo, "allowed with source type SUPPD");
			item.TargetType = TargetTypesList.Codes.RegistrationNumberFromImporter;
			item.TargetCode = ZString.Empty;
			AssertHasErrorContaining(item.TargetCodeInfo, "not exist for UK companies");
			item.TargetCode = "REX";
			AssertNoErrorContaining(item.TargetCodeInfo, "not exist for UK companies");
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ItemDefaulterSetting GetBusinessObjectToClone()
		{
			return new ItemDefaulterSetting(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override ItemDefaulterSetting GetBusinessObjectToSerialise()
		{
			return CreateNewItemDefaulterSetting_CpcToC601(Factory);
		}

		public static ItemDefaulterSetting CreateNewItemDefaulterSetting_CpcToC601(BusinessObjectFactory factory)
		{
			var item = new ItemDefaulterSetting(factory);
			item.SourceType = SourceTypesList.Codes.CustomsProcedureCodeBox37;
			item.SourceValue = "4100000";
			item.TargetType = TargetTypesList.Codes.SupportingDocumentBox44;
			item.TargetCode = "C601";
			return item;
		}

		public static ItemDefaulterSetting CreateNewItemDefaulterSetting_CpcToSpoff(BusinessObjectFactory factory)
		{
			var hmrcAddress = factory.Load<OrgAddress>(new ZGuid("3A6474C2-B99E-4338-9C9F-ACB95C715B06"));
			if (hmrcAddress == null)
			{
				Assert("Pre-Req failed - OrgAddress with PK C3F842EF-3BE5-448C-BED3-0017B232C624 was not found in test database.  There's nothing special about this address, it was picked at random, but it still needs to exist.", false);
			}
			var item = new ItemDefaulterSetting(factory);
			item.SourceType = SourceTypesList.Codes.CustomsProcedureCodeBox37;
			item.SourceValue = "4100000";
			item.TargetType = TargetTypesList.Codes.SupervisingOfficeBox44;
			item.TargetCode = "";
			item.TargetOrgAddress = hmrcAddress.PK;
			return item;
		}

		public static ItemDefaulterSetting CreateNewItemDefaulterSetting_C601ToIpr(OrgHeader iprClient)
		{
			iprClient.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.InwardProcessingReliefNumber, "IP/1234/567/00");
			var item = new ItemDefaulterSetting(iprClient.Factory);
			item.SourceType = SourceTypesList.Codes.SupportingDocumentBox44;
			item.SourceValue = "C601";
			item.TargetType = TargetTypesList.Codes.RegistrationNumberFromImporter;
			item.TargetCode = OrgCusCode.EuropeanUnionSharedCodeTypes.InwardProcessingReliefNumber;
			return item;
		}
	}

	[TestedType(typeof(ItemDefaulterSettingCollection))]
	public class ItemDefaulterSettingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ItemDefaulterSettingCollection>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return ItemDefaulterSettingTest.CreateNewItemDefaulterSetting_CpcToC601(Factory);
		}

		protected override ItemDefaulterSettingCollection GetCollectionToTest()
		{
			return new ItemDefaulterSettingCollection();
		}
	}

	[TestedType(typeof(ItemDefaulterRegistryDataType))]
	public class ItemDefaulterRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ItemDefaulterRegistryDataType>
	{
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.UnitedKingdom,
				new string[] { importCodeType, exportCodeType }, "C601", "Test C601", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			factory.Save();

			var item1 = ItemDefaulterSettingTest.CreateNewItemDefaulterSetting_CpcToSpoff(factory);
			var item2 = ItemDefaulterSettingTest.CreateNewItemDefaulterSetting_CpcToC601(factory);
			var item3 = ItemDefaulterSettingTest.CreateNewItemDefaulterSetting_C601ToIpr(factory.New<OrgHeader>());
			var coll = new ItemDefaulterSettingCollection();
			coll.Add(item1);
			coll.Add(item2);
			coll.Add(item3);

			var bytes = new byte[]
			{
			60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
			0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,73,0,116,0,101,0,109,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,101,0,114,0,83,0,101,0,116,0,116,0,105,0,110,
			0,103,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,
			0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,
			0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,
			0,34,0,62,0,60,0,73,0,116,0,101,0,109,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,101,0,114,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,83,0,111,0,117,0,114,0,99,0,101,0,84,0,121,
			0,112,0,101,0,62,0,67,0,80,0,67,0,60,0,47,0,83,0,111,0,117,0,114,0,99,0,101,0,84,0,121,0,112,0,101,0,62,0,60,0,83,0,111,0,117,0,114,0,99,0,101,0,86,0,97,0,108,0,117,0,101,0,62,0,52,
			0,49,0,48,0,48,0,48,0,48,0,48,0,60,0,47,0,83,0,111,0,117,0,114,0,99,0,101,0,86,0,97,0,108,0,117,0,101,0,62,0,60,0,84,0,97,0,114,0,103,0,101,0,116,0,84,0,121,0,112,0,101,0,62,0,83,
			0,80,0,79,0,70,0,70,0,60,0,47,0,84,0,97,0,114,0,103,0,101,0,116,0,84,0,121,0,112,0,101,0,62,0,60,0,84,0,97,0,114,0,103,0,101,0,116,0,79,0,114,0,103,0,65,0,100,0,100,0,114,0,101,0,115,
			0,115,0,62,0,51,0,97,0,54,0,52,0,55,0,52,0,99,0,50,0,45,0,98,0,57,0,57,0,101,0,45,0,52,0,51,0,51,0,56,0,45,0,57,0,99,0,57,0,102,0,45,0,97,0,99,0,98,0,57,0,53,0,99,0,55,
			0,49,0,53,0,98,0,48,0,54,0,60,0,47,0,84,0,97,0,114,0,103,0,101,0,116,0,79,0,114,0,103,0,65,0,100,0,100,0,114,0,101,0,115,0,115,0,62,0,60,0,84,0,97,0,114,0,103,0,101,0,116,0,67,0,111,
			0,100,0,101,0,32,0,47,0,62,0,60,0,47,0,73,0,116,0,101,0,109,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,101,0,114,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,73,0,116,0,101,0,109,
			0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,101,0,114,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,83,0,111,0,117,0,114,0,99,0,101,0,84,0,121,0,112,0,101,0,62,0,67,0,80,0,67,0,60,
			0,47,0,83,0,111,0,117,0,114,0,99,0,101,0,84,0,121,0,112,0,101,0,62,0,60,0,83,0,111,0,117,0,114,0,99,0,101,0,86,0,97,0,108,0,117,0,101,0,62,0,52,0,49,0,48,0,48,0,48,0,48,0,48,0,60,
			0,47,0,83,0,111,0,117,0,114,0,99,0,101,0,86,0,97,0,108,0,117,0,101,0,62,0,60,0,84,0,97,0,114,0,103,0,101,0,116,0,84,0,121,0,112,0,101,0,62,0,83,0,85,0,80,0,80,0,68,0,60,0,47,0,84,
			0,97,0,114,0,103,0,101,0,116,0,84,0,121,0,112,0,101,0,62,0,60,0,84,0,97,0,114,0,103,0,101,0,116,0,79,0,114,0,103,0,65,0,100,0,100,0,114,0,101,0,115,0,115,0,62,0,48,0,48,0,48,0,48,0,48,
			0,48,0,48,0,48,0,45,0,48,0,48,0,48,0,48,0,45,0,48,0,48,0,48,0,48,0,45,0,48,0,48,0,48,0,48,0,45,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,60,0,47,
			0,84,0,97,0,114,0,103,0,101,0,116,0,79,0,114,0,103,0,65,0,100,0,100,0,114,0,101,0,115,0,115,0,62,0,60,0,84,0,97,0,114,0,103,0,101,0,116,0,67,0,111,0,100,0,101,0,62,0,67,0,54,0,48,0,49,
			0,60,0,47,0,84,0,97,0,114,0,103,0,101,0,116,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,73,0,116,0,101,0,109,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,101,0,114,0,83,0,101,0,116,0,116,0,105,
			0,110,0,103,0,62,0,60,0,73,0,116,0,101,0,109,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,101,0,114,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,83,0,111,0,117,0,114,0,99,0,101,0,84,
			0,121,0,112,0,101,0,62,0,83,0,85,0,80,0,80,0,68,0,60,0,47,0,83,0,111,0,117,0,114,0,99,0,101,0,84,0,121,0,112,0,101,0,62,0,60,0,83,0,111,0,117,0,114,0,99,0,101,0,86,0,97,0,108,0,117,
			0,101,0,62,0,67,0,54,0,48,0,49,0,60,0,47,0,83,0,111,0,117,0,114,0,99,0,101,0,86,0,97,0,108,0,117,0,101,0,62,0,60,0,84,0,97,0,114,0,103,0,101,0,116,0,84,0,121,0,112,0,101,0,62,0,82,
			0,69,0,71,0,73,0,77,0,60,0,47,0,84,0,97,0,114,0,103,0,101,0,116,0,84,0,121,0,112,0,101,0,62,0,60,0,84,0,97,0,114,0,103,0,101,0,116,0,79,0,114,0,103,0,65,0,100,0,100,0,114,0,101,0,115,
			0,115,0,62,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,45,0,48,0,48,0,48,0,48,0,45,0,48,0,48,0,48,0,48,0,45,0,48,0,48,0,48,0,48,0,45,0,48,0,48,0,48,0,48,0,48,0,48,0,48,
			0,48,0,48,0,48,0,48,0,48,0,60,0,47,0,84,0,97,0,114,0,103,0,101,0,116,0,79,0,114,0,103,0,65,0,100,0,100,0,114,0,101,0,115,0,115,0,62,0,60,0,84,0,97,0,114,0,103,0,101,0,116,0,67,0,111,
			0,100,0,101,0,62,0,73,0,80,0,82,0,60,0,47,0,84,0,97,0,114,0,103,0,101,0,116,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,73,0,116,0,101,0,109,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,101,
			0,114,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,73,0,116,0,101,0,109,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,101,0,114,0,83,0,101,
			0,116,0,116,0,105,0,110,0,103,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(coll, bytes)
			};
		}

		protected override ItemDefaulterRegistryDataType GetNewDataType()
		{
			return new ItemDefaulterRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "ItemDefaulterRegistryItemEditor";
			}
		}
	}
}
