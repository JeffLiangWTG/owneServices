using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.Testing.CusTempStorage.UCC6
{
	internal class FRTemporaryStorageHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAuthorizationNumber()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			temporaryStorageHeader.AuthorizationNumber = "ABC";
			AssertNoMessageError(temporaryStorageHeader.AuthorizationNumberInfo, "Authorization Number must not be entered when qualifier of Location of Goods is Y.");

			temporaryStorageHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			temporaryStorageHeader.AuthorizationNumber = "ABC";
			AssertHasMessageError(temporaryStorageHeader.AuthorizationNumberInfo, "Authorization Number must not be entered when qualifier of Location of Goods is Y.");

			temporaryStorageHeader.AuthorizationNumber = ZString.Empty;
			AssertNoMessageError(temporaryStorageHeader.AuthorizationNumberInfo, "Authorization Number must not be entered when qualifier of Location of Goods is Y.");
		}

		public void TestCheckAuthorizationType()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			temporaryStorageHeader.AuthorizationType = "ABC";
			AssertNoMessageError(temporaryStorageHeader.AuthorizationTypeInfo, "Authorization Type must not be entered when qualifier of Location of Goods is Y.");

			temporaryStorageHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			temporaryStorageHeader.AuthorizationType = "ABC";
			AssertHasMessageError(temporaryStorageHeader.AuthorizationTypeInfo, "Authorization Type must not be entered when qualifier of Location of Goods is Y.");

			temporaryStorageHeader.AuthorizationType = ZString.Empty;
			AssertNoMessageError(temporaryStorageHeader.AuthorizationTypeInfo, "Authorization Type must not be entered when qualifier of Location of Goods is Y.");
		}

		public void TestCheckAuthorizationOwner()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			temporaryStorageHeader.AuthorizationOwner = new ZGuid("E2B30F29-1426-4494-ABAB-FB7146D4E8F4");
			AssertNoMessageError(temporaryStorageHeader.AuthorizationOwnerInfo, "Authorization Owner must not be entered when qualifier of Location of Goods is Y.");

			temporaryStorageHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			temporaryStorageHeader.AuthorizationOwner = new ZGuid("E2B30F29-1426-4494-ABAB-FB7146D4E8F4");
			AssertHasMessageError(temporaryStorageHeader.AuthorizationOwnerInfo, "Authorization Owner must not be entered when qualifier of Location of Goods is Y.");

			temporaryStorageHeader.AuthorizationOwner = ZGuid.Empty;
			AssertNoMessageError(temporaryStorageHeader.AuthorizationOwnerInfo, "Authorization Owner must not be entered when qualifier of Location of Goods is Y.");
		}

		public void TestPlaceOfUnloadingMustNotBeEntered()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			CombineAssertions("Place of Unloading should not be entered if Message Mode is TS, PN or TC.", () =>
			{
				AssertPlaceOfUnloadingMustNotBeEntered(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.PresentationNotification, ZString.Empty);
				AssertPlaceOfUnloadingMustNotBeEntered(true, temporaryStorageHeader, PNTSMessageTypeList.Codes.PresentationNotification, "UNLOC");
				AssertPlaceOfUnloadingMustNotBeEntered(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, ZString.Empty);
				AssertPlaceOfUnloadingMustNotBeEntered(true, temporaryStorageHeader, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, "UNLOC");
				AssertPlaceOfUnloadingMustNotBeEntered(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.PreLodgedTempStorage, ZString.Empty);
				AssertPlaceOfUnloadingMustNotBeEntered(true, temporaryStorageHeader, PNTSMessageTypeList.Codes.PreLodgedTempStorage, "UNLOC");
				AssertPlaceOfUnloadingMustNotBeEntered(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.Deconsolidation, ZString.Empty);
				AssertPlaceOfUnloadingMustNotBeEntered(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.Deconsolidation, "UNLOC");
				AssertPlaceOfUnloadingMustNotBeEntered(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.Transfer, ZString.Empty);
				AssertPlaceOfUnloadingMustNotBeEntered(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.Transfer, "UNLOC");
			});
		}

		public void AssertPlaceOfUnloadingMustNotBeEntered(bool shouldBeEmpty, TemporaryStorageHeader temporaryStorageHeader, ZString messageMode, ZString unloco)
		{
			var errorMessage = "'PLACE OF UNLOADING' must not be entered.";
			temporaryStorageHeader.AMA_MessageType = messageMode;
			temporaryStorageHeader.PlaceOfUnloading = unloco;

			temporaryStorageHeader.Validation.ValidatePlaceOfUnloading();
			temporaryStorageHeader.Validation.ValidateGoodsLocationDescription();

			if (shouldBeEmpty)
			{
				AssertHasMessageError(temporaryStorageHeader.PlaceOfUnloadingInfo, errorMessage);
			}
			else
			{
				AssertNoMessageError(temporaryStorageHeader.PlaceOfUnloadingInfo, errorMessage);
			}
		}

		public void TestLocationOfGoodsIsMandatory()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			CombineAssertions("Location of Goods should be mandatory.", () =>
			{
				var listOfMessageType = new PNTSMessageTypeList().GetAllCodes();

				foreach (var messageType in listOfMessageType)
				{
					AssertLocationOfGoodsMandatory(true, temporaryStorageHeader, messageType, ZString.Empty);
					AssertLocationOfGoodsMandatory(false, temporaryStorageHeader, messageType, "V;D;LV00001");
				}
			});
		}

		public void AssertLocationOfGoodsMandatory(bool shouldBeMandatory, TemporaryStorageHeader temporaryStorageHeader, ZString messageMode, ZString locationOfGoods)
		{
			temporaryStorageHeader.AMA_MessageType = messageMode;

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
				AssertHasMessageErrorContaining(temporaryStorageHeader.GoodsLocationDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			}
			else
			{
				AssertNoMessageErrorContaining(temporaryStorageHeader.GoodsLocationDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}
	}
}
