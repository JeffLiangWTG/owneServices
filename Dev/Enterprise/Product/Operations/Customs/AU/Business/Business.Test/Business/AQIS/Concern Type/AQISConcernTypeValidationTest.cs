using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AQISConcernTypeValidationTest : AQISSingleValueValidationTest
	{
		public override void TestCodeAgainstLookupList()
		{
			AQISConcernType concernType = new AQISConcernType(Factory);
			AssertNoMessageErrors("No Message Error", concernType.CodeInfo);

			concernType.Code = "AAA";
			AssertHasMessageErrors("Message Error", concernType.CodeInfo);
		}

		public void TestDuplicateAQISConcernTypes()
		{
			CMRAqisConcern cMRConcernType1 = CMRAqisConcern.New(Factory);
			cMRConcernType1.QN_AQISConcernType = "1";

			CMRAqisConcern cMRConcernType2 = CMRAqisConcern.New(Factory);
			cMRConcernType2.QN_AQISConcernType = "2";

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			AQISConcernType concernType1 = declaration.AQISConcernTypes.AddNew();
			concernType1.Code = "1";
			AssertNoMessageErrors("No Message Error", concernType1.CodeInfo);

			AQISConcernType concernType2 = declaration.AQISConcernTypes.AddNew();
			concernType2.Code = "1";
			AssertHasMessageErrors("Has Message Error", concernType2.CodeInfo);

			concernType2.Code = "2";
			AssertNoMessageErrors("No Message Error", concernType2.CodeInfo);
		}

		public override void TestNumberOfCodesEntered()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			AQISConcernType concernType1 = declaration.AQISConcernTypes.AddNew();
			concernType1.Code = "1";
			AQISConcernType concernType2 = declaration.AQISConcernTypes.AddNew();
			concernType2.Code = "2";
			AQISConcernType concernType3 = declaration.AQISConcernTypes.AddNew();
			concernType3.Code = "3";
			AQISConcernType concernType4 = declaration.AQISConcernTypes.AddNew();
			concernType4.Code = "4";
			AQISConcernType concernType5 = declaration.AQISConcernTypes.AddNew();
			concernType5.Code = "5";
			AQISConcernType concernType6 = declaration.AQISConcernTypes.AddNew();
			concernType6.Code = "6";
			concernType6.Validation.ValidateAll();
			AssertNoErrors("Concern Type 1", concernType1.CodeInfo);
			AssertNoErrors("Concern Type 2", concernType2.CodeInfo);
			AssertNoErrors("Concern Type 3", concernType3.CodeInfo);
			AssertNoErrors("Concern Type 4", concernType4.CodeInfo);
			AssertNoErrors("Concern Type 5", concernType5.CodeInfo);
			AssertNoErrors("Concern Type 6", concernType6.CodeInfo);

			AQISConcernType concernType7 = declaration.AQISConcernTypes.AddNew();
			concernType7.Code = "7";
			Factory.Save();
			concernType7.Validation.ValidateAll();
			AssertNoErrors("Concern Type 1", concernType1.CodeInfo);
			AssertNoErrors("Concern Type 2", concernType2.CodeInfo);
			AssertNoErrors("Concern Type 3", concernType3.CodeInfo);
			AssertNoErrors("Concern Type 4", concernType4.CodeInfo);
			AssertNoErrors("Concern Type 5", concernType5.CodeInfo);
			AssertNoErrors("Concern Type 6", concernType6.CodeInfo);
			AssertHasErrors("Concern Type 7", concernType7.CodeInfo);

			declaration.AQISConcernTypes.RemoveAndDelete(concernType2);
			concernType7.Validation.ValidateAll();
			AssertNoErrors("Concern Type 1", concernType1.CodeInfo);
			AssertNoErrors("Concern Type 3", concernType3.CodeInfo);
			AssertNoErrors("Concern Type 4", concernType4.CodeInfo);
			AssertNoErrors("Concern Type 5", concernType5.CodeInfo);
			AssertNoErrors("Concern Type 6", concernType6.CodeInfo);
			AssertNoErrors("Concern Type 7", concernType7.CodeInfo);
		}

		public void TestSettingCodeValidatesDeliveryPostCodeAndInspectionLocation()
		{
			var refTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = new ZDateTime(2020, 1, 1);
			var endDate = new ZDateTime(2076, 6, 6);
			var postcode1 = refTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "AUPC", "123X", "Postcode", startDate, endDate);
			refTestHelper.CreateNewOrGetExistingCusCodeListAttribute(postcode1.PK, "PostcodeDeliveryClassification", "SPLIT");
			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (declaration.GetValidationSuspender())
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
				declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				OrgHeader importer = OrgHeader.New(Factory);
				importer.OH_IsConsignee = true;
				declaration.ImporterDeliveryAddress.OrganisationPK = importer.PK;
				declaration.ImporterDeliveryAddress.E2_AddressOverride = true;
				declaration.ImporterDeliveryAddress.E2_Postcode = "123X";
				declaration.AddInfo.ZA_AQISInspectLocation_Hidden = "INSPECTION LOCATION";
			}
			AssertNoMessageErrorContaining(declaration.AddInfo.ZA_AQISInspectLocation_HiddenInfo, "Delivery address post code is listed as SPLIT");
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, "Delivery address post code is listed as SPLIT");

			AQISConcernType concernType1 = declaration.AQISConcernTypes.AddNew();
			concernType1.Code = "OTHR";

			AssertHasMessageErrorContaining(declaration.AddInfo.ZA_AQISInspectLocation_HiddenInfo, "Delivery address post code is listed as SPLIT");
			AssertHasMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, "Delivery address post code is listed as SPLIT");

			AQISConcernType concernType2 = declaration.AQISConcernTypes.AddNew();
			concernType2.Code = "RURL";

			AssertNoMessageErrorContaining(declaration.AddInfo.ZA_AQISInspectLocation_HiddenInfo, "Delivery address post code is listed as SPLIT");
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, "Delivery address post code is listed as SPLIT");
		}

		AQISConcernType concernType;
		public override AQISSingleValueBusinessObject BizObjToTest => concernType ?? (concernType = new AQISConcernType(Factory));
	}
}
