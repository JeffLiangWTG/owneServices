using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class JobDocAddressValidationForCMRTest : TestCaseWithFactory
	{
		public void TestMessageErrorForMissingFieldsWhenAddressOverriden()
		{
			declaration.ImporterDeliveryAddress.E2_AddressOverride = true;
			declaration.ImporterDeliveryAddress.E2_Address1 = "Address";
			declaration.ImporterDeliveryAddress.E2_City = "City";
			declaration.ImporterDeliveryAddress.E2_Postcode = "2000";
			declaration.ImporterDeliveryAddress.E2_State = "NSW";

			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_Address1Info, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_CityInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_StateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ImporterDeliveryAddress.E2_Address1 = "";
			declaration.ImporterDeliveryAddress.E2_City = "";
			declaration.ImporterDeliveryAddress.E2_Postcode = "";
			declaration.ImporterDeliveryAddress.E2_State = "";

			AssertEquals("Declaration should have a message error", true, declaration.HasMessageErrors);
			AssertHasMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_Address1Info, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_CityInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_StateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_OA_AddressInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestMessageErrorForMissingFieldsWhenAddressNotOverriden()
		{
			declaration.ImporterDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			declaration.ImporterDeliveryAddress.E2_AddressOverride = false;
			AssertHasMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_OA_AddressInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.RunPreSaveValidation();
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_Address1Info, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_CityInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_StateInfo, MandatoryValidation.YouHaveNotEntered);

			OrgAddress address = importer.Addresses.AddNew();
			declaration.ImporterDeliveryAddress.E2_OA_Address = address.PK;
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_OA_AddressInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_OA_AddressInfo, "The following fields are missing: ");

			address.OA_Address1 = "Address";
			address.OA_City = "City";
			address.OA_State = "State";
			address.OA_PostCode = "1000";

			declaration.RunPreSaveValidation();
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_OA_AddressInfo, "The following fields are missing: ");
		}

		public void TestWarnWhenAmendmentIsNeededWhenAddressOverriden()
		{
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.ImporterDeliveryAddress.E2_AddressOverride = true;
			declaration.ImporterDeliveryAddress.E2_Address1 = "Address";
			declaration.ImporterDeliveryAddress.E2_City = "City";
			declaration.ImporterDeliveryAddress.E2_Postcode = "1000";
			declaration.ImporterDeliveryAddress.E2_State = "NSW";
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration decLoaded = factory2.Load<JobDeclaration>(declaration.PK);
			CusEntryHeader entryLoaded = factory2.Load<CusEntryHeader>(entry.PK);
			entryLoaded.CH_Status = CustomsEntryStatus.NotSent.Code;
			AssertEquals("PreCondition:HasEntryWithPostLodgeStatus", false, decLoaded.CustomsEntryHeaders.HasEntryWithPostLodgeStatus);

			decLoaded.ImporterDeliveryAddress.E2_Address1 = "AddressChanged";
			decLoaded.ImporterDeliveryAddress.E2_City = "CityChanged";
			decLoaded.ImporterDeliveryAddress.E2_Postcode = "2000";
			decLoaded.ImporterDeliveryAddress.E2_State = "StateChanged";

			AssertNoWarningContaining(decLoaded.ImporterDeliveryAddress.E2_Address1Info, JobDocAddressValidationForCMR.AmendmentWarning);
			AssertNoWarningContaining(decLoaded.ImporterDeliveryAddress.E2_CityInfo, JobDocAddressValidationForCMR.AmendmentWarning);
			AssertNoWarningContaining(decLoaded.ImporterDeliveryAddress.E2_PostcodeInfo, JobDocAddressValidationForCMR.AmendmentWarning);
			AssertNoWarningContaining(decLoaded.ImporterDeliveryAddress.E2_StateInfo, JobDocAddressValidationForCMR.AmendmentWarning);

			decLoaded.RunPreSaveValidation();
			AssertNoWarningContaining(decLoaded.ImporterDeliveryAddress.E2_OA_AddressInfo, JobDocAddressValidationForCMR.AmendmentWarning);

			entryLoaded.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("PreCondition:HasEntryWithPostLodgeStatus", true, decLoaded.CustomsEntryHeaders.HasEntryWithPostLodgeStatus);

			decLoaded.ImporterDeliveryAddress.E2_Address1 = "AddressChanged1";
			decLoaded.ImporterDeliveryAddress.E2_City = "CityChanged1";
			decLoaded.ImporterDeliveryAddress.E2_Postcode = "2001";
			decLoaded.ImporterDeliveryAddress.E2_State = "StateChanged1";

			AssertHasWarningContaining(decLoaded.ImporterDeliveryAddress.E2_Address1Info, JobDocAddressValidationForCMR.AmendmentWarning);
			AssertHasWarningContaining(decLoaded.ImporterDeliveryAddress.E2_CityInfo, JobDocAddressValidationForCMR.AmendmentWarning);
			AssertHasWarningContaining(decLoaded.ImporterDeliveryAddress.E2_PostcodeInfo, JobDocAddressValidationForCMR.AmendmentWarning);
			AssertHasWarningContaining(decLoaded.ImporterDeliveryAddress.E2_StateInfo, JobDocAddressValidationForCMR.AmendmentWarning);
			AssertNoWarningContaining(decLoaded.ImporterDeliveryAddress.E2_OA_AddressInfo, JobDocAddressValidationForCMR.AmendmentWarning);
		}

		public void TestWarnWhenAmendmentIsNeededWhenAddressNotOverriden()
		{
			OrgAddress address1 = importer.Addresses.AddNew();
			address1.OA_City = "City";
			address1.OA_Address1 = "Address";
			address1.OA_PostCode = "1000";
			address1.OA_State = "NSW";

			OrgAddress address2 = importer.Addresses.AddNew();
			address2.OA_City = "City2";
			address2.OA_Address1 = "Address2";
			address2.OA_PostCode = "2000";
			address2.OA_State = "QLD";

			declaration.ImporterDeliveryAddress.E2_AddressOverride = false;
			declaration.ImporterDeliveryAddress.E2_OA_Address = address1.PK;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration decLoaded = factory2.Load<JobDeclaration>(declaration.PK);
			CusEntryHeader entryLoaded = factory2.Load<CusEntryHeader>(entry.PK);
			entryLoaded.CH_Status = CustomsEntryStatus.NotSent.Code;
			AssertEquals("PreCondition:HasEntryWithPostLodgeStatus", false, decLoaded.CustomsEntryHeaders.HasEntryWithPostLodgeStatus);

			decLoaded.ImporterDeliveryAddress.E2_OA_Address = address2.PK;
			AssertNoWarningContaining(decLoaded.ImporterDeliveryAddress.E2_OA_AddressInfo, JobDocAddressValidationForCMR.AmendmentWarning);

			entryLoaded.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("PreCondition:HasEntryWithPostLodgeStatus", true, decLoaded.CustomsEntryHeaders.HasEntryWithPostLodgeStatus);

			decLoaded.RunPreSaveValidation();
			AssertHasWarningContaining(decLoaded.ImporterDeliveryAddress.E2_OA_AddressInfo, JobDocAddressValidationForCMR.AmendmentWarning);
			AssertNoWarningContaining(decLoaded.ImporterDeliveryAddress.E2_Address1Info, JobDocAddressValidationForCMR.AmendmentWarning);
			AssertNoWarningContaining(decLoaded.ImporterDeliveryAddress.E2_CityInfo, JobDocAddressValidationForCMR.AmendmentWarning);
			AssertNoWarningContaining(decLoaded.ImporterDeliveryAddress.E2_PostcodeInfo, JobDocAddressValidationForCMR.AmendmentWarning);
			AssertNoWarningContaining(decLoaded.ImporterDeliveryAddress.E2_StateInfo, JobDocAddressValidationForCMR.AmendmentWarning);
		}

		public void TestThisValidationDoesNotGetFiredForOtherAddressTypes()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			declaration.SupplierPickupAddress.OrganisationPK = supplier.PK;
			declaration.SupplierPickupAddress.E2_OA_Address = ZGuid.Empty;
			AssertNoMessageErrors("This is a supplier Pickup address, importer delivery address is not applicable", declaration.SupplierPickupAddress.E2_OA_AddressInfo);

			declaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			AssertNoMessageErrors("This is a supplier Doc address, importer delivery address is not applicable", declaration.SupplierDocumentaryAddress.E2_OA_AddressInfo);
		}

		public void TestPostCodeValidatesInspectionLocation()
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
				declaration.ImporterDeliveryAddress.E2_Postcode = "";
				AQISConcernType concernType2 = declaration.AQISConcernTypes.AddNew();
				concernType2.Code = "RURL";
			}
			AssertNoMessageErrorContaining(declaration.AddInfo.ZA_AQISInspectLocation_HiddenInfo, "Delivery address post code is listed as SPLIT");
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, "Delivery address post code is listed as SPLIT");
			AssertNoWarningContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, "Delivery address post code is listed as SPLIT, tailgate inspection is required if the address is Rural.");

			declaration.ImporterDeliveryAddress.E2_Postcode = "123X";

			AssertHasMessageErrorContaining(declaration.AddInfo.ZA_AQISInspectLocation_HiddenInfo, "Delivery address post code is listed as SPLIT");
			AssertHasMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, "Delivery address post code is listed as SPLIT");
			AssertNoWarningContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, "Delivery address post code is listed as SPLIT, tailgate inspection is required if the address is Rural.");

			declaration.AddInfo.ZA_AQISInspectLocation_Hidden = "XXXXXXX";
			AssertNoMessageErrorContaining(declaration.AddInfo.ZA_AQISInspectLocation_HiddenInfo, "Delivery address post code is listed as SPLIT");
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, "Delivery address post code is listed as SPLIT");
			AssertHasWarningContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, "Delivery address post code is listed as SPLIT, tailgate inspection is required if the address is Rural.");

			declaration.ImporterDeliveryAddress.E2_Postcode = "";

			AssertNoMessageErrorContaining(declaration.AddInfo.ZA_AQISInspectLocation_HiddenInfo, "Delivery address post code is listed as SPLIT");
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, "Delivery address post code is listed as SPLIT");
			AssertNoWarningContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, "Delivery address post code is listed as SPLIT, tailgate inspection is required if the address is Rural.");
		}

		public void TestRuralPostCodeValidationOccuresWhenAttachedToShipment()
		{
			var refTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = new ZDateTime(2020, 1, 1);
			var endDate = new ZDateTime(2076, 6, 6);
			var postcode1 = refTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "AUPC", "123X", "Postcode", startDate, endDate);
			refTestHelper.CreateNewOrGetExistingCusCodeListAttribute(postcode1.PK, "PostcodeDeliveryClassification", "SPLIT");
			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_JS = shipment.PK;
			using (shipment.GetValidationSuspender())
			{
				var importer = OrgHeader.New(Factory);
				importer.OH_IsConsignee = true;
				shipment.ConsigneeDeliveryAddress.OrganisationPK = importer.PK;
				shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
				shipment.ConsigneeDeliveryAddress.E2_Postcode = "";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			}
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, "Delivery address post code is listed as SPLIT");
			shipment.ConsigneeDeliveryAddress.E2_Postcode = "123X";
			AssertHasMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, "Delivery address post code is listed as SPLIT");
		}

		#region Implementation

		JobDeclaration declaration;
		OrgHeader importer;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			importer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			importer.OH_IsConsignee = true;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "CMR";
			declaration.ImporterDeliveryAddress.OrganisationPK = importer.PK;
		}

		#endregion
	}
}
