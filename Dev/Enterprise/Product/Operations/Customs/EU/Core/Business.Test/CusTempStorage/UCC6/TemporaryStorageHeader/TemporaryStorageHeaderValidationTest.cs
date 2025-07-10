using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRule058_PlaceOfUnloading()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			CombineAssertions("Place of Unloading should be mandatory only if Message Mode is TS and Location of Goods is empty (Rule 058).", () =>
			{
				AssertPlaceOfUnloadingMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.PresentationNotification, ZString.Empty, ZString.Empty);
				AssertPlaceOfUnloadingMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.PresentationNotification, "UNLOC", ZString.Empty);
				AssertPlaceOfUnloadingMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.PresentationNotification, ZString.Empty, "V;D;LV00001");
				AssertPlaceOfUnloadingMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, ZString.Empty, ZString.Empty);
				AssertPlaceOfUnloadingMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, "UNLOC", ZString.Empty);
				AssertPlaceOfUnloadingMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.PresentationNotification, ZString.Empty, "V;D;LV00001");
				AssertPlaceOfUnloadingMandatory(true, temporaryStorageHeader, PNTSMessageTypeList.Codes.PreLodgedTempStorage, ZString.Empty, ZString.Empty);
				AssertPlaceOfUnloadingMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.PreLodgedTempStorage, "UNLOC", ZString.Empty);
				AssertPlaceOfUnloadingMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.PreLodgedTempStorage, ZString.Empty, "V;D;LV00001");
				AssertPlaceOfUnloadingMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.Deconsolidation, ZString.Empty, ZString.Empty);
			});
		}

		public void AssertPlaceOfUnloadingMandatory(bool shouldBeMandatory, TemporaryStorageHeader temporaryStorageHeader, ZString messageMode, ZString unloco, ZString locationOfGoods)
		{
			var errorMessage = TemporaryStorageHeaderValidation.Rule058ErrorMessage;
			temporaryStorageHeader.AMA_MessageType = messageMode;
			temporaryStorageHeader.PlaceOfUnloading = unloco;

			if (!string.IsNullOrEmpty(locationOfGoods))
			{
				var splitlocationOfGoods = locationOfGoods.Split(";");
				temporaryStorageHeader.GoodsLocation.CGL_Qualifier = splitlocationOfGoods[0];
				temporaryStorageHeader.GoodsLocation.CGL_Type = splitlocationOfGoods[1];
				temporaryStorageHeader.GoodsLocation.CGL_AdditionalIdentifier = splitlocationOfGoods[2];
				AssertEquals("Prerequisite: GoodsLocationDescription is not empty.", temporaryStorageHeader.GoodsLocationDescription, locationOfGoods);
			}
			else
			{
				temporaryStorageHeader.GoodsLocation.CGL_Qualifier = ZString.Empty;
				temporaryStorageHeader.GoodsLocation.CGL_Type = ZString.Empty;
				temporaryStorageHeader.GoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
				AssertEquals("Prerequisite: GoodsLocationDescription is empty.", temporaryStorageHeader.GoodsLocationDescription, ZString.Empty);
			}

			temporaryStorageHeader.Validation.ValidatePlaceOfUnloading();

			if (shouldBeMandatory)
			{
				AssertHasMessageError(temporaryStorageHeader.PlaceOfUnloadingInfo, errorMessage);
			}
			else
			{
				AssertNoMessageError(temporaryStorageHeader.PlaceOfUnloadingInfo, errorMessage);
			}
		}

		public void TestCheckRule058_LocationOfGoods()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			CombineAssertions("Location of Goods should be optional only when  Message Mode is TS and UNLOCO is not empty (Rule 058).", () =>
			{
				AssertLocationOfGoodsMandatory(true, temporaryStorageHeader, PNTSMessageTypeList.Codes.PresentationNotification, ZString.Empty, ZString.Empty);
				AssertLocationOfGoodsMandatory(true, temporaryStorageHeader, PNTSMessageTypeList.Codes.PresentationNotification, "UNLOC", ZString.Empty);
				AssertLocationOfGoodsMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.PresentationNotification, ZString.Empty, "V;D;LV00001");
				AssertLocationOfGoodsMandatory(true, temporaryStorageHeader, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, ZString.Empty, ZString.Empty);
				AssertLocationOfGoodsMandatory(true, temporaryStorageHeader, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, "UNLOC", ZString.Empty);
				AssertLocationOfGoodsMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.PresentationNotification, ZString.Empty, "V;D;LV00001");
				AssertLocationOfGoodsMandatory(true, temporaryStorageHeader, PNTSMessageTypeList.Codes.PreLodgedTempStorage, ZString.Empty, ZString.Empty);
				AssertLocationOfGoodsMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.PreLodgedTempStorage, "UNLOC", ZString.Empty);
				AssertLocationOfGoodsMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.PreLodgedTempStorage, ZString.Empty, "V;D;LV00001");
				AssertLocationOfGoodsMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.Deconsolidation, ZString.Empty, ZString.Empty);
			});
		}

		public void AssertLocationOfGoodsMandatory(bool shouldBeMandatory, TemporaryStorageHeader temporaryStorageHeader, ZString messageMode, ZString unloco, ZString locationOfGoods)
		{
			var errorMessage = messageMode == PNTSMessageTypeList.Codes.PreLodgedTempStorage ? TemporaryStorageHeaderValidation.Rule058ErrorMessage : MandatoryValidation.YouHaveNotEntered;
			temporaryStorageHeader.AMA_MessageType = messageMode;
			temporaryStorageHeader.PlaceOfUnloading = unloco;

			if (!string.IsNullOrEmpty(locationOfGoods))
			{
				var splitlocationOfGoods = locationOfGoods.Split(";");
				temporaryStorageHeader.GoodsLocation.CGL_Qualifier = splitlocationOfGoods[0];
				temporaryStorageHeader.GoodsLocation.CGL_Type = splitlocationOfGoods[1];
				temporaryStorageHeader.GoodsLocation.CGL_AdditionalIdentifier = splitlocationOfGoods[2];
				AssertEquals("Prerequisite: GoodsLocationDescription is not empty.", temporaryStorageHeader.GoodsLocationDescription, locationOfGoods);
			}
			else
			{
				temporaryStorageHeader.GoodsLocation.CGL_Qualifier = ZString.Empty;
				temporaryStorageHeader.GoodsLocation.CGL_Type = ZString.Empty;
				temporaryStorageHeader.GoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
				AssertEquals("Prerequisite: GoodsLocationDescription is empty.", temporaryStorageHeader.GoodsLocationDescription, ZString.Empty);
			}

			temporaryStorageHeader.Validation.ValidateGoodsLocationDescription();

			if (shouldBeMandatory)
			{
				AssertHasMessageErrorContaining(temporaryStorageHeader.GoodsLocationDescriptionInfo, errorMessage);
			}
			else
			{
				AssertNoMessageErrorContaining(temporaryStorageHeader.GoodsLocationDescriptionInfo, errorMessage);
			}
		}

		public void TestCheckRule058DependingOnIsRule058ActiveValue()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			using (var deciderTestContext = new TemporaryStorageValidationDeciderTestContext<ITemporaryStorageHeaderValidationDecider>(Factory))
			{
				deciderTestContext.DisableRule(x => x.IsRule058Active);
				AssertLocationOfGoodsMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.PresentationNotification, ZString.Empty, ZString.Empty);
				AssertPlaceOfUnloadingMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.PreLodgedTempStorage, ZString.Empty, ZString.Empty);

				deciderTestContext.EnableRule(x => x.IsRule058Active);
				AssertLocationOfGoodsMandatory(true, temporaryStorageHeader, PNTSMessageTypeList.Codes.PresentationNotification, ZString.Empty, ZString.Empty);
				AssertPlaceOfUnloadingMandatory(true, temporaryStorageHeader, PNTSMessageTypeList.Codes.PreLodgedTempStorage, ZString.Empty, ZString.Empty);
			}
		}

		public void TestCheckGoodsLocationDescription()
		{
			const string expectedMessage = "There are errors within the 'Location of Goods', please click on 'More..' to view the error information.";

			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			temporaryStorageHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;

			var provider = temporaryStorageHeader as ICusGoodsLocationProvider;

			CombineAssertions(() =>
			{
				provider.GoodsLocation.CGL_Type = "A";
				provider.GoodsLocation.CGL_Qualifier = "U";
				provider.ValidateGoodsLocationDescription();
				AssertHasMessageError("empty data present", temporaryStorageHeader.GoodsLocationDescriptionInfo, expectedMessage);

				provider.GoodsLocation.Unlocode = "U";
				provider.ValidateGoodsLocationDescription();
				AssertHasMessageError("invalid unlocode", temporaryStorageHeader.GoodsLocationDescriptionInfo, expectedMessage);

				provider.GoodsLocation.Unlocode = "BSASD";
				provider.ValidateGoodsLocationDescription();
				AssertNoMessageError("valid unlocode", temporaryStorageHeader.GoodsLocationDescriptionInfo, expectedMessage);
			});
		}

		public void TestCheckIsENSReuse()
		{
			var messagePreviousDocumentError = "In Case ENS Re-use is enabled, there must be one previous document of type N355 for each of the bill or at Header level.";
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			AssertNoWarning(temporaryStorageHeader.IsENSReuseInfo, "When ENS Re-use = True, only minimal data will be sent to customs including MRN and Bill numbers. Bill details and related items will not be sent.");

			temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			AssertNoMessageError("AMA_MessageType == PNTSMessageTypeList.Codes.PresentationNotification no need for 355 document", temporaryStorageHeader.IsENSReuseInfo, messagePreviousDocumentError);

			temporaryStorageHeader.Bills.RemoveAndDeleteAll();
			temporaryStorageHeader.IsENSReuse = true;
			temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			temporaryStorageHeader.Validation.ValidateIsENSReuse();
			AssertHasWarning(temporaryStorageHeader.IsENSReuseInfo, "When ENS Re-use = True, only minimal data will be sent to customs including MRN and Bill numbers. Bill details and related items will not be sent.");
			AssertHasMessageError("ENS Re-use = True and AMA_MessageType == PNTSMessageTypeList.Codes.PreLodgedTempStorage, there are no bills, but a document N355 should be present in each bill but there are no bills.", temporaryStorageHeader.IsENSReuseInfo, messagePreviousDocumentError);

			temporaryStorageHeader.PreviousDocuments.AddNew().CSI_Code = PreviousDocumentCodeList.Codes.N355;
			temporaryStorageHeader.Validation.ValidateIsENSReuse();
			AssertNoMessageErrorContaining("No such message error is expected when a document N355 is present at header level.", temporaryStorageHeader.IsENSReuseInfo, messagePreviousDocumentError);
			temporaryStorageHeader.PreviousDocuments.RemoveAll();

			temporaryStorageHeader.PreviousDocuments.AddNew().CSI_Code = "N821";
			temporaryStorageHeader.Validation.ValidateIsENSReuse();
			AssertHasMessageError("No such message error is expected when no document with CSI_Code N355 is present at header level.", temporaryStorageHeader.IsENSReuseInfo, messagePreviousDocumentError);
			temporaryStorageHeader.PreviousDocuments.RemoveAll();

			temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
			temporaryStorageHeader.Validation.ValidateIsENSReuse();
			AssertNoWarning("No validation when AMA_MessageType == PNTSMessageTypeList.Codes.Transfer", temporaryStorageHeader.IsENSReuseInfo, "When ENS Re-use = True, only minimal data will be sent to customs including MRN and Bill numbers. Bill details and related items will not be sent.");
			AssertNoMessageError("No validation when AMA_MessageType == PNTSMessageTypeList.Codes.Transfer.", temporaryStorageHeader.IsENSReuseInfo, messagePreviousDocumentError);

			temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
			temporaryStorageHeader.Validation.ValidateIsENSReuse();
			AssertNoWarning("No validation when AMA_MessageType == PNTSMessageTypeList.Codes.Deconsolidation", temporaryStorageHeader.IsENSReuseInfo, "When ENS Re-use = True, only minimal data will be sent to customs including MRN and Bill numbers. Bill details and related items will not be sent.");
			AssertNoMessageError("No validation when AMA_MessageType == PNTSMessageTypeList.Codes.Deconsolidation.", temporaryStorageHeader.IsENSReuseInfo, messagePreviousDocumentError);

			temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			temporaryStorageHeader.IsENSReuse = true;
			temporaryStorageHeader.Validation.ValidateIsENSReuse();
			AssertNoMessageError("ENS Re-use = True and AMA_MessageType == PNTSMessageTypeList.Codes.PresentationNotification and there are no bills, documents are not mandatory for bills.", temporaryStorageHeader.IsENSReuseInfo, messagePreviousDocumentError);

			temporaryStorageHeader.IsENSReuse = false;
			temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			temporaryStorageHeader.Validation.ValidateIsENSReuse();
			AssertNoMessageError("ENS Re-use = False even if AMA_MessageType == PNTSMessageTypeList.Codes.PreLodgedTempStorage, documents are not mandatory for bills.", temporaryStorageHeader.IsENSReuseInfo, messagePreviousDocumentError);

			temporaryStorageHeader.IsENSReuse = true;

			var bill = temporaryStorageHeader.Bills.AddNew();
			var bill2 = temporaryStorageHeader.Bills.AddNew();

			temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			bill.PreviousDocuments.AddNew().CSI_Code = PreviousDocumentCodeList.Codes.N355;
			temporaryStorageHeader.Validation.ValidateIsENSReuse();
			AssertHasMessageErrorContaining("AMA_MessageType == PNTSMessageTypeList.Codes.PreLodgedTempStorage, only one bill has a document N355 but a document N355 should be present in each bill.", temporaryStorageHeader.IsENSReuseInfo, messagePreviousDocumentError);

			temporaryStorageHeader.PreviousDocuments.AddNew().CSI_Code = PreviousDocumentCodeList.Codes.N355;
			temporaryStorageHeader.Validation.ValidateIsENSReuse();
			AssertNoMessageErrorContaining("No such message error is expected when a document N355 is present at header level.", temporaryStorageHeader.IsENSReuseInfo, messagePreviousDocumentError);
			temporaryStorageHeader.PreviousDocuments.RemoveAll();

			temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			temporaryStorageHeader.Validation.ValidateIsENSReuse();
			AssertHasMessageErrorContaining("AMA_MessageType != PNTSMessageTypeList.Codes.PresentationNotification, only one bill has a document N355 but a document N355 should be present in each bill.", temporaryStorageHeader.IsENSReuseInfo, messagePreviousDocumentError);

			temporaryStorageHeader.PreviousDocuments.AddNew().CSI_Code = PreviousDocumentCodeList.Codes.N355;
			temporaryStorageHeader.Validation.ValidateIsENSReuse();
			AssertNoMessageErrorContaining("No such message error is expected when a document N355 is present at header level.", temporaryStorageHeader.IsENSReuseInfo, messagePreviousDocumentError);
			temporaryStorageHeader.PreviousDocuments.RemoveAll();

			bill2.PreviousDocuments.AddNew().CSI_Code = PreviousDocumentCodeList.Codes.N355;
			temporaryStorageHeader.Validation.ValidateIsENSReuse();
			AssertNoMessageErrorContaining("AMA_MessageType == PNTSMessageTypeList.Codes.CombinedTemporaryStorage, each bill has a document N355.", temporaryStorageHeader.IsENSReuseInfo, messagePreviousDocumentError);

			temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			temporaryStorageHeader.Validation.ValidateIsENSReuse();
			AssertNoMessageErrorContaining("AMA_MessageType == PNTSMessageTypeList.Codes.PreLodgedTempStorage, each bill has a document N355.", temporaryStorageHeader.IsENSReuseInfo, messagePreviousDocumentError);
		}

		public void TestCheckLRN()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE1230789654", "BE");
			(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
			temporaryStorageHeader.LRN = ZString.Empty;
			AssertNoMessageErrors("There should be no error when EORI is not empty", temporaryStorageHeader.LRNInfo);

			temporaryStorageHeader.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, ZString.Empty, "BE");
			temporaryStorageHeader.LRN = "2212307896540000000010";
			AssertNoMessageErrors("There should be no error when EORI is empty and LRN is not empty", temporaryStorageHeader.LRNInfo);

			temporaryStorageHeader.LRN = ZString.Empty;
			AssertHasMessageError("There is an error because EORI can not be empty when LRN is empty", temporaryStorageHeader.LRNInfo, "Company or branch must have an EORI number in order to generate the LRN number.");

			using (TemporaryStorageConfigurationTestHelper.TemporarilyClearConfigurationAndSetSupportLRNGeneration(temporaryStorageHeader, configurationValue: false))
			{
				temporaryStorageHeader.Validation.ValidateLRN();
				AssertNoMessageErrors("When SupportLRNGeneration is no active, there should be no errors when both EORI and LRN are empty", temporaryStorageHeader.LRNInfo);
			}
		}

		[TestDate(2022, 12, 19)]
		public void TestLRNAfterSaving()
		{
			var temporaryStorageHeader1 = Factory.New<TemporaryStorageHeader>();
			(temporaryStorageHeader1 as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
			temporaryStorageHeader1.Factory.Save();
			AssertEquals("Create a new TemporaryStorageHeader and EORI is not configured, So LRN is empty after saving", ZString.Empty, temporaryStorageHeader1.LRN);
			temporaryStorageHeader1.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE1230789654", "BE");
			temporaryStorageHeader1.Factory.Save();
			AssertEquals("LRN is empty and EORI is configured, So LRN has a new value after saving", "2212307896540000000001", temporaryStorageHeader1.LRN);

			var temporaryStorageHeader2 = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader2.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE1230789655", "BE");
			(temporaryStorageHeader2 as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
			temporaryStorageHeader2.Factory.Save();
			AssertEquals("Create a new TemporaryStorageHeader and EORI is configured, So LRN has a new value after saving", "2212307896550000000001", temporaryStorageHeader2.LRN);
			temporaryStorageHeader2.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE1230789654", "BE");
			temporaryStorageHeader2.Factory.Save();
			AssertEquals("LRN is not empty and change EORI, So LRN is not changed after saving", "2212307896550000000001", temporaryStorageHeader2.LRN);
		}

		public void TestCheckAMA_MessageType()
		{
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(tempHeader.AMA_MessageTypeInfo, "NO", PNTSMessageTypeList.Codes.PresentationNotification);
		}

		public void TestCheckAMA_TransportMode()
		{
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(tempHeader.AMA_TransportModeInfo, tempHeader.AMA_MessageTypeInfo, new ZString[] { PNTSMessageTypeList.Codes.CombinedTemporaryStorage, PNTSMessageTypeList.Codes.PreLodgedTempStorage }.Cast<IZType>());
		}

		public void TestCheckAMA_OA_Carrier()
		{
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			tempHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			tempHeader.Validation.ValidateAll();
			AssertHasMessageError("When message mode is not TS, Carrier is mandatory.", tempHeader.AMA_OA_CarrierInfo, "You have not entered a Carrier.");

			tempHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			tempHeader.Validation.ValidateAMA_OA_Carrier();
			AssertNoMessageError("When message mode is TS, Carrier is not mandatory.", tempHeader.AMA_OA_CarrierInfo, "You have not entered a Carrier.");

			tempHeader.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			tempHeader.Validation.ValidateAMA_OA_Carrier();
			AssertHasMessageError("When message mode is not TS, Carrier is mandatory.", tempHeader.AMA_OA_CarrierInfo, "You have not entered a Carrier.");

			tempHeader.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
			tempHeader.Validation.ValidateAMA_OA_Carrier();
			AssertNoMessageError("When message mode is TF, Carrier is not mandatory.", tempHeader.AMA_OA_CarrierInfo, "You have not entered a Carrier.");

			tempHeader.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
			tempHeader.Validation.ValidateAMA_OA_Carrier();
			AssertNoMessageError("When message mode is DC, Carrier is not mandatory.", tempHeader.AMA_OA_CarrierInfo, "You have not entered a Carrier.");

			tempHeader.AMA_MessageType = ZString.Empty;
			tempHeader.Validation.ValidateAMA_OA_Carrier();
			AssertHasMessageError("When message mode is not TS, Carrier is mandatory.", tempHeader.AMA_OA_CarrierInfo, "You have not entered a Carrier.");

			var orgAddress = GetTestOrgAddress();
			tempHeader.AMA_OA_Carrier = orgAddress.PK;
			tempHeader.Validation.ValidateAMA_OA_Declarant();
			AssertHasMessageError("An EORI # should exist for Carrier.", tempHeader.AMA_OA_CarrierInfo, "An EORI # should exist for Carrier.");

			tempHeader.AMA_OA_Carrier = ZGuid.Empty;
			tempHeader.Validation.ValidateAMA_OA_Declarant();
			AssertNoMessageError("Validation of missing EORI# should not appear if no carrier was entered.", tempHeader.AMA_OA_CarrierInfo, "An EORI # should exist for Carrier.");

			var orgAddressWithEORI = GetTestOrgAddressWithEORI();
			tempHeader.AMA_OA_Carrier = orgAddressWithEORI.PK;
			tempHeader.Validation.ValidateAMA_OA_Declarant();
			AssertNoErrors(tempHeader.AMA_OA_CarrierInfo);
		}

		public void TestCheckAMA_OA_Declarant()
		{
			var expectedEORIError = "An EORI # should exist for Declarant.";
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			tempHeader.AMA_OA_Declarant = ZGuid.Empty;
			tempHeader.Validation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertHasMessageError("Declarant is mandatory.", tempHeader.AMA_OA_DeclarantInfo, "You have not entered a Declarant.");
				AssertNoMessageError("Error EORI should not be display if empty", tempHeader.AMA_OA_DeclarantInfo, expectedEORIError);

				var orgAddress = GetTestOrgAddress();
				tempHeader.AMA_OA_Declarant = orgAddress.PK;
				tempHeader.AMA_OA_Representative = orgAddress.PK;
				tempHeader.Validation.ValidateAMA_OA_Declarant();
				AssertHasMessageError("An EORI # should exist for Declarant.", tempHeader.AMA_OA_DeclarantInfo, expectedEORIError);
				AssertHasMessageError(tempHeader.AMA_OA_DeclarantInfo, "Declarant must not be the same as the Representative.");

				var orgAddressWithEORI = GetTestOrgAddressWithEORI();
				tempHeader.AMA_OA_Declarant = orgAddressWithEORI.PK;
				tempHeader.Validation.ValidateAMA_OA_Declarant();
				AssertNoErrors(tempHeader.AMA_OA_DeclarantInfo);
			});
		}

		public void TestCheckAMA_OA_PresenterHasEORI()
		{
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var orgAddress = GetTestOrgAddress();
			tempHeader.AMA_OA_Presenter = orgAddress.PK;
			tempHeader.Validation.ValidateAMA_OA_Presenter();
			AssertHasMessageError("An EORI # should exist for Presenter.", tempHeader.AMA_OA_PresenterInfo, "An EORI # should exist for Person Presenting the Goods.");

			var orgAddressWithEORI = GetTestOrgAddressWithEORI();
			tempHeader.AMA_OA_Presenter = orgAddressWithEORI.PK;
			tempHeader.Validation.ValidateAMA_OA_Presenter();
			AssertNoErrors(tempHeader.AMA_OA_PresenterInfo);
		}

		public void TestCheckAMA_OA_PresenterHasEORI_Deconsolidation()
		{
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var orgAddress = GetTestOrgAddress();
			tempHeader.AMA_OA_Presenter = orgAddress.PK;
			tempHeader.Validation.ValidateAMA_OA_Presenter();
			AssertHasMessageError("An EORI # should exist for Presenter.", tempHeader.AMA_OA_PresenterInfo, "An EORI # should exist for Person Presenting the Goods.");

			tempHeader.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
			tempHeader.Validation.ValidateAMA_OA_Presenter();
			AssertNoMessageError("Presenter (EORI) is not mandatory when Deconsolidation", tempHeader.AMA_OA_PresenterInfo, "An EORI # should exist for Person Presenting the Goods.");
		}

		public void TestCheckAMA_OA_PresenterMandatory()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.AMA_OA_PresenterInfo);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.AMA_OA_PresenterInfo);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			AssertNoMessageErrorContaining("No message error should show if the message type is TS even when presentation Customs office is empty.", header.AMA_OA_PresenterInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
			AssertNoMessageErrorContaining("No message error should show if the message type is DC even when Presenter is empty.", header.AMA_OA_PresenterInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAMA_OA_Representative()
		{
			var expectedEORIError = "An EORI # should exist for Representative.";
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var orgAddress = GetTestOrgAddress();
			tempHeader.AMA_OA_Declarant = orgAddress.PK;
			CombineAssertions(() =>
			{
				tempHeader.AMA_OA_Representative = ZGuid.Empty;
				AssertNoMessageError("Error EORI should not be display if empty", tempHeader.AMA_OA_RepresentativeInfo, expectedEORIError);

				tempHeader.AMA_OA_Representative = orgAddress.PK;
				AssertHasMessageError("Representative should not be the same as the Declarant.", tempHeader.AMA_OA_RepresentativeInfo, "Representative must not be the same as the Declarant.");
				AssertHasMessageError("Error EORI should be display if not empty and no eori", tempHeader.AMA_OA_RepresentativeInfo, expectedEORIError);

				var orgAddressWithEORI = GetTestOrgAddressWithEORI();
				tempHeader.AMA_OA_Representative = orgAddressWithEORI.PK;
				tempHeader.Validation.ValidateAMA_OA_Representative();
				AssertNoErrors(tempHeader.AMA_OA_RepresentativeInfo);
			});
		}

		OrgAddress GetTestOrgAddress()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgHeader.CustomsCodes.RemoveAll();
			AssertEquals("No CusCodes in this orgHeader", 0, orgHeader.CustomsCodes.Count);
			return orgAddress;
		}

		OrgAddress GetTestOrgAddressWithEORI()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = "EOR";
			orgCusCode.OK_CustomsRegNo = "FR0001";
			return orgAddress;
		}

		public void TestCheckAMA_CustomsOffice()
		{
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			tempHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;

			tempHeader.Validation.ValidateAMA_CustomsOffice();
			AssertHasMessageError("Supervising Customs Office is mandatory", tempHeader.AMA_CustomsOfficeInfo, "You have not entered a Supervising Customs Office.");

			tempHeader.AMA_CustomsOffice = "NOEXIST";
			tempHeader.Validation.ValidateAMA_CustomsOffice();
			AssertHasMessageError("Supervising Customs Office entered not in list", tempHeader.AMA_CustomsOfficeInfo, "The code you have selected is not in the list.");

			tempHeader.AMA_CustomsOffice = "FROFF002";
			tempHeader.Validation.ValidateAMA_CustomsOffice();
			AssertNoMessageErrors(tempHeader.AMA_CustomsOfficeInfo);
		}

		public void TestCheckAMA_DateAtCustomsOffice()
		{
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			tempHeader.AMA_MessageType = "TC";
			tempHeader.Validation.ValidateAMA_DateAtCustomsOffice();
			AssertHasMessageError("You have not entered a Goods Presentation Date.", tempHeader.AMA_DateAtCustomsOfficeInfo, "You have not entered a Goods Presentation Date.");

			tempHeader.AMA_MessageType = "PN";
			tempHeader.Validation.ValidateAMA_DateAtCustomsOffice();
			AssertHasMessageError("You have not entered a Goods Presentation Date.", tempHeader.AMA_DateAtCustomsOfficeInfo, "You have not entered a Goods Presentation Date.");

			tempHeader.AMA_MessageType = "TS";
			tempHeader.Validation.ValidateAMA_DateAtCustomsOffice();
			AssertNoErrors(tempHeader.AMA_DateAtCustomsOfficeInfo);
			tempHeader.AMA_DateAtCustomsOffice = ZDate.Today;
			AssertNoErrors(tempHeader.AMA_DateAtCustomsOfficeInfo);

			tempHeader.AMA_DateAtCustomsOffice = ZDateTime.Today.AddDays(1);
			tempHeader.Validation.ValidateAMA_DateAtCustomsOffice();
			AssertHasMessageError("Goods Presentation Date can't be a future date", tempHeader.AMA_DateAtCustomsOfficeInfo, "Goods Presentation Date can't be a future date.");

			tempHeader.AMA_DateAtCustomsOffice = ZDateTime.Today.AddDays(-1);
			tempHeader.Validation.ValidateAMA_DateAtCustomsOffice();
			AssertNoErrors(tempHeader.AMA_DateAtCustomsOfficeInfo);
		}

		public void TestCheckDeclarationDate()
		{
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			tempHeader.AMA_DateAtCustomsOffice = ZDate.Today.AddDays(-1);
			Factory.Save();

			tempHeader.DeclarationDate = ZDate.Today.AddDays(-1);
			AssertHasMessageError("Declaration Date must be different with Presentation Date.", tempHeader.DeclarationDateInfo, "Declaration Date must be different with Presentation Date.");

			tempHeader.DeclarationDate = ZDate.Empty;
			AssertNoErrors(tempHeader.DeclarationDateInfo);

			tempHeader.DeclarationDate = ZDate.Today.AddDays(1);
			AssertHasMessageError("Declaration Date can't be a future date.", tempHeader.DeclarationDateInfo, "Declaration Date can't be a future date.");

			tempHeader.DeclarationDate = ZDate.Today.AddDays(-1).AddHours(-25);
			AssertHasMessageError("Declaration Date can't be more than 24 hours prior to Presentation Date.", tempHeader.DeclarationDateInfo, "Declaration Date can't be more than 24 hours prior to Presentation Date.");
		}

		public void TestCheckAuthorizationNumber()
		{
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "OH2";
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			authorizationHeader.CPH_Number = "001";
			authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;
			var propertyInfo = tempHeader.AuthorizationNumberInfo;

			using var deciderTestContext = new TemporaryStorageValidationDeciderTestContext<ITemporaryStorageHeaderValidationDecider>(Factory);
			deciderTestContext.EnableRule(x => x.IsAuthorizationUsageCheckActive);

			CombineAssertions("When IsAuthorizationUsageForTSActive = true", () =>
			{
				tempHeader.AuthorizationType = "TST";
				tempHeader.AuthorizationNumber = "";
				AssertHasErrorContaining("AuthorizationType has value and AuthorizationNumber is Empty => error", propertyInfo, MandatoryValidation.MustBeEntered);

				tempHeader.AuthorizationNumber = "Cod";
				AssertNoErrorContaining("AuthorizationType has value and AuthorizationNumber has value => no error", propertyInfo, MandatoryValidation.MustBeEntered);

				tempHeader.AuthorizationType = "";
				tempHeader.AuthorizationNumber = "";
				AssertNoErrorContaining("AuthorizationType is empty and AuthorizationNumber is Empty => no error", propertyInfo, MandatoryValidation.MustBeEntered);

				tempHeader.AuthorizationNumber = "Cod";
				AssertNoErrorContaining("AuthorizationType is empty and AuthorizationNumber has value => no error", propertyInfo, MandatoryValidation.MustBeEntered);

				tempHeader.AuthorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				tempHeader.AuthorizationOwner = orgHeader2.PK;
				tempHeader.AuthorizationNumber = "001";
				tempHeader.Validation.ValidateAll();
				AssertHasMessageErrorContaining("ACT + 001 + OH2", propertyInfo, "Authorization number: 001 doesn't exist for Code: ACT, Owner: OH2");

				tempHeader.AuthorizationOwner = orgHeader.PK;
				tempHeader.AuthorizationNumber = "XXX";
				tempHeader.Validation.ValidateAll();
				AssertHasMessageErrorContaining("ACT + XXX + OH1", propertyInfo, "Authorization number: XXX doesn't exist for Code: ACT, Owner: OH1");

				tempHeader.AuthorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				tempHeader.AuthorizationNumber = "001";
				tempHeader.Validation.ValidateAll();
				AssertHasMessageErrorContaining("ACE + 001 + OH1", propertyInfo, "Authorization number: 001 doesn't exist for Code: ACE, Owner: OH1");

				tempHeader.AuthorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				tempHeader.Validation.ValidateAll();
				AssertNoNotifications("ACT + 001 + OH1", propertyInfo);
			});

			deciderTestContext.DisableRule(x => x.IsAuthorizationUsageCheckActive);

			CombineAssertions("When IsAuthorizationUsageForTSActive = false", () =>
			{
				tempHeader.AuthorizationType = "TST";
				tempHeader.AuthorizationNumber = "";
				AssertNoMessageError("AuthorizationType has value and AuthorizationNumber is empty", propertyInfo, MandatoryValidation.MustBeEntered);

				tempHeader.AuthorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				tempHeader.AuthorizationOwner = orgHeader2.PK;
				tempHeader.AuthorizationNumber = "001";
				tempHeader.Validation.ValidateAll();
				AssertNoMessageError("ACT + 001 + OH2", propertyInfo, "Authorization number: 001 doesn't exist for Code: ACT, Owner: OH2");

				tempHeader.AuthorizationOwner = orgHeader.PK;
				tempHeader.AuthorizationNumber = "XXX";
				tempHeader.Validation.ValidateAll();
				AssertNoMessageError("ACT + XXX + OH1", propertyInfo, "Authorization number: XXX doesn't exist for Code: ACT, Owner: OH1");

				tempHeader.AuthorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				tempHeader.AuthorizationNumber = "001";
				tempHeader.Validation.ValidateAll();
				AssertNoMessageError("ACE + 001 + OH1", propertyInfo, "Authorization number: 001 doesn't exist for Code: ACE, Owner: OH1");
			});
		}

		public void TestCheckAuthorizationType()
		{
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var message = "Both Authorization number and owner should be served.";
			var propertyInfo = tempHeader.AuthorizationTypeInfo;
			var orgHeader = Factory.New<OrgHeader>();

			using var deciderTestContext = new TemporaryStorageValidationDeciderTestContext<ITemporaryStorageHeaderValidationDecider>(Factory);
			deciderTestContext.EnableRule(x => x.IsAuthorizationUsageCheckActive);

			CombineAssertions("When IsAuthorizationUsageForTSActive = true", () =>
			{
				tempHeader.AuthorizationType = AuthorizationTypeList.Codes.TST;
				tempHeader.Validation.ValidateAll();
				AssertNoErrorContaining("AuthorizationType value is valid", propertyInfo, ListValidation.InvalidCodeError);

				tempHeader.AuthorizationType = "ERR";
				tempHeader.Validation.ValidateAll();
				AssertHasErrorContaining("AuthorizationType is not valid => Error", propertyInfo, ListValidation.InvalidCodeError);

				tempHeader.AuthorizationType = ZString.Empty;
				tempHeader.Validation.ValidateAll();
				AssertNoErrorContaining("AuthorizationType is empty as the other field no error", propertyInfo, ListValidation.InvalidCodeError);

				tempHeader.AuthorizationType = AuthorizationTypeList.Codes.TST;
				tempHeader.Validation.ValidateAll();
				AssertHasErrorContaining("AuthorizationType is not empty but owner and Number are => Error", propertyInfo, message);

				tempHeader.AuthorizationOwner = orgHeader.PK;
				tempHeader.Validation.ValidateAll();
				AssertHasErrorContaining("AuthorizationType is not empty but Number is => Error", propertyInfo, message);

				tempHeader.AuthorizationNumber = "COD";
				tempHeader.Validation.ValidateAll();
				AssertNoErrorContaining("AuthorizationType is not empty and owner and Number are not empty too => no Error", propertyInfo, message);

				tempHeader.AuthorizationOwner = ZGuid.Empty;
				tempHeader.Validation.ValidateAll();
				AssertHasErrorContaining("AuthorizationType is not empty but owner is => Error", propertyInfo, message);
			});

			deciderTestContext.DisableRule(x => x.IsAuthorizationUsageCheckActive);

			CombineAssertions("When IsAuthorizationUsageForTSActive = false", () =>
			{
				tempHeader.AuthorizationType = "ERR";
				tempHeader.Validation.ValidateAll();
				AssertNoMessageError("AuthorizationType is not valid", propertyInfo, ListValidation.InvalidCodeError);

				tempHeader.AuthorizationType = AuthorizationTypeList.Codes.TST;
				tempHeader.Validation.ValidateAll();
				AssertNoMessageError("AuthorizationType is not empty but owner and Number are", propertyInfo, message);

				tempHeader.AuthorizationOwner = orgHeader.PK;
				tempHeader.Validation.ValidateAll();
				AssertNoMessageError("AuthorizationType is not empty but Number is", propertyInfo, message);
			});
		}

		public void TestCheckAuthorizationOwner()
		{
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var propertyInfo = tempHeader.AuthorizationOwnerInfo;
			var orgHeader = Factory.New<OrgHeader>();

			using var deciderTestContext = new TemporaryStorageValidationDeciderTestContext<ITemporaryStorageHeaderValidationDecider>(Factory);
			deciderTestContext.EnableRule(x => x.IsAuthorizationUsageCheckActive);

			CombineAssertions("When IsAuthorizationUsageForTSActive = true", () =>
			{
				tempHeader.AuthorizationOwner = ZGuid.Empty;
				tempHeader.Validation.ValidateAll();
				AssertNoErrorContaining("AuthorizationOwner value is Empty => no error", propertyInfo, ListValidation.InvalidCodeError);

				tempHeader.AuthorizationOwner = ZGuid.Invalid;
				tempHeader.Validation.ValidateAll();
				AssertHasErrorContaining("AuthorizationOwner value is invalid => error", propertyInfo, ListValidation.InvalidCodeError);

				tempHeader.AuthorizationOwner = orgHeader.PK;
				tempHeader.Validation.ValidateAll();
				AssertNoErrorContaining("AuthorizationOwner value is valid => no error", propertyInfo, ListValidation.InvalidCodeError);
			});

			deciderTestContext.DisableRule(x => x.IsAuthorizationUsageCheckActive);

			CombineAssertions("When IsAuthorizationUsageForTSActive = false", () =>
			{
				tempHeader.AuthorizationOwner = ZGuid.Invalid;
				tempHeader.Validation.ValidateAll();
				AssertNoMessageError("AuthorizationOwner value is invalid", propertyInfo, ListValidation.InvalidCodeError);
			});
		}

		public void TestCheckPlaceOfUnloading()
		{
			SetUpUNLOCO();
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			header.PlaceOfUnloading = "LOCO2";
			AssertHasMessageError("Message error should be added if the code is not in the list", header.PlaceOfUnloadingInfo, "The code you have selected is not in the list.");

			header.PlaceOfUnloading = "LOCO1";
			AssertNoMessageErrors("No message error should be added if the code is selected from the list", header.PlaceOfUnloadingInfo);

			void SetUpUNLOCO()
			{
				RefUNLOCO loco1 = Factory.New<RefUNLOCO>();
				loco1.RL_Code = "LOCO1";
				loco1.RL_IATA = "XXX";
				Factory.Save();
			}
		}

		public void TestCheckPresentationCustomsOfficeMandatory()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			header.PresentationCustomsOfficeCode.CY_Data = ZString.Empty;
			header.Validation.ValidatePresentationCustomsOffice();

			AssertHasMessageErrorContaining("A message error should show if the message type is PN and presentation Customs office is empty.", header.PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			header.PresentationCustomsOfficeCode.CY_Data = "LV000000";
			header.Validation.ValidatePresentationCustomsOffice();
			AssertNoMessageErrorContaining("No message error should show if the message type is PN and presentation Customs office is not empty.", header.PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			header.PresentationCustomsOfficeCode.CY_Data = ZString.Empty;
			header.Validation.ValidatePresentationCustomsOffice();
			AssertHasMessageErrorContaining("A message error should show if the message type is TC and presentation Customs office is empty.", header.PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			header.PresentationCustomsOfficeCode.CY_Data = "LV000000";
			header.Validation.ValidatePresentationCustomsOffice();
			AssertNoMessageErrorContaining("No message error should show if the message type is TC and presentation Customs office is not empty.", header.PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			header.PresentationCustomsOfficeCode.CY_Data = ZString.Empty;
			header.Validation.ValidatePresentationCustomsOffice();
			AssertNoMessageErrorContaining("No message error should show if the message type is TS and presentation Customs office is empty.", header.PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckPresentationCustomsOfficeValid()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			header.PresentationCustomsOfficeCode.CY_Data = "LV000000";
			header.Validation.ValidatePresentationCustomsOffice();
			AssertHasMessageError("A message error should show if the code is not in the list.", header.PresentationCustomsOfficeInfo, ListValidation.InvalidCodeMessageError);

			header.PresentationCustomsOfficeCode.CY_Data = "FROFF001";
			header.Validation.ValidatePresentationCustomsOffice();
			AssertNoMessageError("No message error should show if the code is in the list.", header.PresentationCustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
		}

		[TestDate(2023, 01, 01)]
		public void TestCheckCRN_PrelodgeDate()
		{
			const string message = "Pre-Lodged status was granted over 30 days ago, so it\r\nis not allowed by customs to send another TSD message.";

			using var deciderTestContext = new TemporaryStorageValidationDeciderTestContext<ITemporaryStorageHeaderValidationDecider>(Factory);

			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			CombineAssertions("When IsPreLodgedStatusCheckActive = true", () =>
			{
				deciderTestContext.EnableRule(x => x.IsPreLodgedStatusCheckActive);
				temporaryStorageHeader.PreLodgedDate = ZDateTime.Today.AddDays(-31);
				temporaryStorageHeader.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated;
				temporaryStorageHeader.Validation.ValidateCRN();
				AssertNoMessageError("PreLodgedDate is lower than today for more than 30 days but status is not TemporaryStoragePreLodged so no error.", temporaryStorageHeader.CRNInfo, message);

				temporaryStorageHeader.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStoragePreLodged;
				temporaryStorageHeader.Validation.ValidateCRN();
				AssertHasMessageError("PreLodgedDate is lower than today for more than 30 days and status is not TemporaryStoragePreLodged so error.", temporaryStorageHeader.CRNInfo, message);

				temporaryStorageHeader.PreLodgedDate = ZDateTime.Today.AddDays(-30);
				temporaryStorageHeader.Validation.ValidateCRN();
				AssertNoMessageError("PreLodgedDate is lower than today for equal than 30 days and status is not TemporaryStoragePreLodged so no error.", temporaryStorageHeader.CRNInfo, message);

				temporaryStorageHeader.PreLodgedDate = ZDateTime.Today.AddDays(-25);
				temporaryStorageHeader.Validation.ValidateCRN();
				AssertNoMessageError("PreLodgedDate is lower than today for less than 30 days and status is not TemporaryStoragePreLodged so no error.", temporaryStorageHeader.CRNInfo, message);
			});

			deciderTestContext.DisableRule(x => x.IsPreLodgedStatusCheckActive);
			temporaryStorageHeader.PreLodgedDate = ZDateTime.Today.AddDays(-31);
			temporaryStorageHeader.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStoragePreLodged;
			temporaryStorageHeader.Validation.ValidateCRN();
			AssertNoMessageError("When IsPreLodgedStatusCheckActive = false, no error", temporaryStorageHeader.CRNInfo, message);
		}

		public void TestCRNOrMRNMustHaveAValueForTemporaryStorageActivated()
		{
			const string message = "Make sure CRN # Or MRN # exist.";

			using var deciderTestContext = new TemporaryStorageValidationDeciderTestContext<ITemporaryStorageHeaderValidationDecider>(Factory);

			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			CombineAssertions("When IsCheckCRNAndMRNForTSAActive = true", () =>
			{
				deciderTestContext.EnableRule(x => x.IsCheckCRNAndMRNForTSAActive);
				temporaryStorageHeader.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStoragePreLodged;
				temporaryStorageHeader.CRN = ZString.Empty;
				temporaryStorageHeader.MRN = ZString.Empty;
				temporaryStorageHeader.Validation.ValidateCRN();
				temporaryStorageHeader.Validation.ValidateMRN();
				AssertNoMessageError("At least CRN or MRN should have a value, status is not TemporaryStorageActivated, CRN is empty => no CRN error.", temporaryStorageHeader.CRNInfo, message);
				AssertNoMessageError("At least CRN or MRN should have a value, status is not TemporaryStorageActivated, CRN is empty => no MRN error.", temporaryStorageHeader.CRNInfo, message);

				temporaryStorageHeader.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated;
				temporaryStorageHeader.Validation.ValidateCRN();
				temporaryStorageHeader.Validation.ValidateMRN();
				AssertHasMessageError("At least CRN or MRN should have a value, status is TemporaryStorageActivated, CRN is empty => CRN error.", temporaryStorageHeader.CRNInfo, message);
				AssertHasMessageError("At least CRN or MRN should have a value, status is TemporaryStorageActivated, CRN is empty => MRN error.", temporaryStorageHeader.CRNInfo, message);

				temporaryStorageHeader.CRN = "12345678912";
				temporaryStorageHeader.MRN = ZString.Empty;
				temporaryStorageHeader.Validation.ValidateCRN();
				temporaryStorageHeader.Validation.ValidateMRN();
				AssertNoMessageError("At least CRN or MRN should have a value, CRN is not empty => no CRN error.", temporaryStorageHeader.CRNInfo, message);
				AssertNoMessageError("At least CRN or MRN should have a value, CRN is not empty => no MRN error.", temporaryStorageHeader.CRNInfo, message);

				temporaryStorageHeader.CRN = "12345678912";
				temporaryStorageHeader.MRN = "22345678912";
				temporaryStorageHeader.Validation.ValidateCRN();
				temporaryStorageHeader.Validation.ValidateMRN();
				AssertNoMessageError("At least CRN or MRN should have a value, CRN and MRN are not empty => no CRN error.", temporaryStorageHeader.CRNInfo, message);
				AssertNoMessageError("At least CRN or MRN should have a value, CRN and MRN are not empty => no MRN error.", temporaryStorageHeader.CRNInfo, message);

				temporaryStorageHeader.MRN = "12345678912";
				temporaryStorageHeader.CRN = ZString.Empty;
				temporaryStorageHeader.Validation.ValidateCRN();
				temporaryStorageHeader.Validation.ValidateMRN();
				AssertNoMessageError("At least CRN or MRN should have a value, MRN is not empty => no CRN error.", temporaryStorageHeader.CRNInfo, message);
				AssertNoMessageError("At least CRN or MRN should have a value, MRN is not empty => no MRN error.", temporaryStorageHeader.CRNInfo, message);
			});

			CombineAssertions("When IsCheckCRNAndMRNForTSAActive = false", () =>
			{
				deciderTestContext.DisableRule(x => x.IsCheckCRNAndMRNForTSAActive);
				temporaryStorageHeader.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated;
				temporaryStorageHeader.CRN = "";
				temporaryStorageHeader.MRN = "";
				temporaryStorageHeader.Validation.ValidateCRN();
				temporaryStorageHeader.Validation.ValidateMRN();
				AssertNoMessageError(temporaryStorageHeader.CRNInfo, message);
				AssertNoMessageError(temporaryStorageHeader.MRNInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetUpEuOffice();
		}

		void SetUpEuOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var enuZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: enuZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var cusCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FROFF001", "PARIS PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode1.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent);
			var cusCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FROFF002", "PARIS PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode2.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfLodgementEntry);
			var cusCode3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FROFF003", "PARIS PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode3.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CustomsOfficeForTemporaryStorage);
			Factory.Save();
		}
	}
}
