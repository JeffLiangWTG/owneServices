using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCGL_Type()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var goodsLocation = temporaryStorageHeader.GoodsLocation;

			AssertMessageErrorDependingOnMessageType(PNTSMessageTypeList.Codes.CombinedTemporaryStorage, false);
			AssertMessageErrorDependingOnMessageType(PNTSMessageTypeList.Codes.PreLodgedTempStorage, true);
			AssertMessageErrorDependingOnMessageType(PNTSMessageTypeList.Codes.PresentationNotification, false);

			var goods = Factory.New<CusGoodsLocation>();
			goods.CGL_ParentID = Factory.New<JobDeclaration>().PK;
			goods.CGL_ParentTableCode = "JE";

			AssertNoExceptionThrown(() => goods.CGL_Type = "B");
			AssertNoExceptionThrown(() => goods.Validation.ValidateAll());
			AssertNoNotifications(goods.CGL_TypeInfo);

			var goodsWithInvalidParentID = Factory.New<CusGoodsLocation>();
			goodsWithInvalidParentID.CGL_ParentID = CargoWise.Types.ZGuid.Invalid;
			goodsWithInvalidParentID.CGL_ParentTableCode = "AMA";

			AssertNoExceptionThrown(() => goodsWithInvalidParentID.CGL_Type = "B");
			AssertNoExceptionThrown(() => goodsWithInvalidParentID.Validation.ValidateAll());
			AssertNoNotifications(goodsWithInvalidParentID.CGL_TypeInfo);
		}

		void AssertMessageErrorDependingOnMessageType(string messageType, bool shouldHaveMessageError)
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			temporaryStorageHeader.AMA_MessageType = messageType;

			var goodslocation = temporaryStorageHeader.GoodsLocation;
			var message = "For a Pre-Lodged declaration only Type A , B , C are allowed";

			goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
			AssertNoMessageError("No error if CGL_Type is A", goodslocation.CGL_TypeInfo, message);

			goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			AssertNoMessageError("No error if CGL_Type is B", goodslocation.CGL_TypeInfo, message);

			goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
			AssertNoMessageError("No error if CGL_Type is C", goodslocation.CGL_TypeInfo, message);

			if (shouldHaveMessageError)
			{
				goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.Other;
				AssertHasMessageError($"There should be an error as message type is {messageType} and CGL_type is not A, B or C.", goodslocation.CGL_TypeInfo, message);
			}
			else
			{
				goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.Other;
				AssertNoMessageError($"There shouldn't be any error error as message type is {messageType}.", goodslocation.CGL_TypeInfo, message);
			}
		}

		public void TestCheckCGL_Qualifier()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var goodslocation = temporaryStorageHeader.GoodsLocation;
			var message = "The chosen Qualifier does not match the Location Type.";

			CombineAssertions("Type is A, qualifier must be U or V", () =>
			{
				goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
				goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				AssertNoMessageError($"qualifier = U => no error", goodslocation.CGL_QualifierInfo, message);
				goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
				AssertNoMessageError($"qualifier = V => no error", goodslocation.CGL_QualifierInfo, message);
				goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
				AssertHasMessageError($"qualifier != V && != U => error", goodslocation.CGL_QualifierInfo, message);
			});

			CombineAssertions("Type is B, qualifier must be U", () =>
			{
				goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
				goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				AssertNoMessageError($"qualifier = U => no error", goodslocation.CGL_QualifierInfo, message);
				goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
				AssertHasMessageError($"qualifier != U => error", goodslocation.CGL_QualifierInfo, message);
			});

			CombineAssertions("Type is C, qualifier must be U", () =>
			{
				goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
				goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				AssertNoMessageError($"qualifier = U => no error", goodslocation.CGL_QualifierInfo, message);
				goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
				AssertHasMessageError($"qualifier != U => error", goodslocation.CGL_QualifierInfo, message);
			});

			CombineAssertions("Type is D, qualifier must be U, V, W, X or Y", () =>
			{
				goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.Other;
				goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				AssertNoMessageError($"qualifier = U => no error", goodslocation.CGL_QualifierInfo, message);
				goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
				AssertNoMessageError($"qualifier = V => no error", goodslocation.CGL_QualifierInfo, message);
				goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
				AssertNoMessageError($"qualifier = W => no error", goodslocation.CGL_QualifierInfo, message);
				goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
				AssertNoMessageError($"qualifier = X => no error", goodslocation.CGL_QualifierInfo, message);
				goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
				AssertNoMessageError($"qualifier = Y => no error", goodslocation.CGL_QualifierInfo, message);
				goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				AssertHasMessageError($"qualifier = Z => error", goodslocation.CGL_QualifierInfo, message);
			});

			CombineAssertions("Default cases", () =>
			{
				goodslocation.CGL_Type = "Z";
				goodslocation.CGL_Qualifier = "K";
				AssertNoMessageError($"qualifier not in the list, qualifier has value => no error", goodslocation.CGL_QualifierInfo, message);
				goodslocation.CGL_Type = "Z";
				goodslocation.CGL_Qualifier = ZString.Empty;
				AssertNoMessageError($"qualifier not in the list, qualifier has no value => no error", goodslocation.CGL_QualifierInfo, message);
				goodslocation.CGL_Type = ZString.Empty;
				goodslocation.CGL_Qualifier = "K";
				AssertNoMessageError($"qualifier not in the list, qualifier with value => no error", goodslocation.CGL_QualifierInfo, message);
				goodslocation.CGL_Type = ZString.Empty;
				goodslocation.CGL_Qualifier = ZString.Empty;
				AssertNoMessageError($"qualifier not in the list, qualifier has no value => no error", goodslocation.CGL_QualifierInfo, message);
			});

			CombineAssertions("No issue with goodsLocation not linked to temporaryStorage", () =>
			{
				var goods = Factory.New<CusGoodsLocation>();
				goods.CGL_ParentID = Factory.New<JobDeclaration>().PK;
				goods.CGL_ParentTableCode = "JE";

				AssertNoExceptionThrown(() => goods.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation);
				AssertNoExceptionThrown(() => goods.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode);
				AssertNoExceptionThrown(() => goods.Validation.ValidateAll());
				AssertNoNotifications(goods.CGL_TypeInfo);

				var goodsWithInvalidParentID = Factory.New<CusGoodsLocation>();
				goodsWithInvalidParentID.CGL_ParentID = CargoWise.Types.ZGuid.Invalid;
				goodsWithInvalidParentID.CGL_ParentTableCode = "AMA";

				AssertNoExceptionThrown(() => goodsWithInvalidParentID.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation);
				AssertNoExceptionThrown(() => goodsWithInvalidParentID.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode);
				AssertNoExceptionThrown(() => goodsWithInvalidParentID.Validation.ValidateAll());
				AssertNoNotifications(goodsWithInvalidParentID.CGL_TypeInfo);
			});
		}

		[ExpectNoExceptions]
		public void TestCheckCGL_QualifierTriggersAddressValidation()
		{
			var goodsLocationAddressMock = Factory.NewMoq<CusGoodsLocationAddress>();

			var goodsLocationAddressValidationMock = new Mock<CusGoodsLocationAddressValidation>(goodsLocationAddressMock.Object);
			goodsLocationAddressValidationMock
				.Protected()
				.Setup("CheckE2_Address1AndE2_Address2");
			goodsLocationAddressValidationMock
				.Protected()
				.Setup("CheckE2_GovRegNum");
			goodsLocationAddressValidationMock
				.Protected()
				.Setup("CheckE2_City");
			goodsLocationAddressValidationMock
				.Protected()
				.Setup("CheckE2_RN_NKCountryCode");
			goodsLocationAddressValidationMock
				.Protected()
				.Setup("CheckE2_GeoLocation");
			goodsLocationAddressValidationMock
				.Protected()
				.Setup("CheckE2_AdditionalAddressInformation");

			goodsLocationAddressMock
				.Protected()
				.Setup<JobDocAddressValidation>("GetNewValidation")
				.Returns(goodsLocationAddressValidationMock.Object);

			var goodsLocationMock = Factory.NewMoq<CusGoodsLocation>();
			goodsLocationMock
				.Protected().Setup<CusGoodsLocationAddress>("LoadOrCreateCusGoodsLocationAddress").Returns(goodsLocationAddressMock.Object);

			goodsLocationMock.Object.Validation.ValidateCGL_Qualifier();
			goodsLocationAddressValidationMock.VerifyAll();
		}

		public void TestCheckCGL_QualifierMandatory()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var goodsLocation = temporaryStorageHeader.GoodsLocation;
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(goodsLocation.CGL_QualifierInfo, goodsLocation.CGL_TypeInfo);
		}

		public void TestCheckCGL_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000002", "FRENCH OFFICE Of Destination", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			Factory.Save();

			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var goodslocation = temporaryStorageHeader.GoodsLocation;
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			goodslocation.Lookups.CustomsOfficeList.Load();
			ValidationTestHelper.AssertInvalidCodeMessageError(goodslocation.CGL_CustomsOfficeInfo, "FR000001", "FR000002");
		}

		public void TestRule061()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var goodslocation = temporaryStorageHeader.GoodsLocation;
			var message = "[C0061] Additional Identifier required when qualifier is 'U'.";

			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			AssertHasMessageError("No UNLOCO: " + message, goodslocation.CGL_AdditionalIdentifierInfo, message);

			goodslocation.Unlocode = "unloco";
			AssertNoMessageError("There is an unloco: " + message, goodslocation.CGL_AdditionalIdentifierInfo, message);
		}

		public void TestRule062()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var goodslocation = temporaryStorageHeader.GoodsLocation;
			var message = "[C0062] Customs Office required when qualifier is 'V'.";

			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			goodslocation.Validation.ValidateCGL_CustomsOffice();
			AssertHasMessageError("No customs office: " + message, goodslocation.CGL_CustomsOfficeInfo, message);

			goodslocation.CGL_CustomsOffice = "office";
			AssertNoMessageError("There is a customs office: " + message, goodslocation.CGL_CustomsOfficeInfo, message);
		}

		public void TestRule058()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			CombineAssertions("Type is optional only when Message Mode is TS and UNLOCO is not empty.", () =>
			{
				AssertCGL_TypeMandatory(true, temporaryStorageHeader, PNTSMessageTypeList.Codes.PresentationNotification, ZString.Empty);
				AssertCGL_TypeMandatory(true, temporaryStorageHeader, PNTSMessageTypeList.Codes.PresentationNotification, "FRPAR");
				AssertCGL_TypeMandatory(true, temporaryStorageHeader, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, ZString.Empty);
				AssertCGL_TypeMandatory(true, temporaryStorageHeader, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, "FRPAR");
				AssertCGL_TypeMandatory(true, temporaryStorageHeader, PNTSMessageTypeList.Codes.PreLodgedTempStorage, ZString.Empty);
				AssertCGL_TypeMandatory(false, temporaryStorageHeader, PNTSMessageTypeList.Codes.PreLodgedTempStorage, "FRPAR");
			});
		}

		[TestDate(2022, 07, 01)]
		public void TestRuleC0382()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType("CL148", "CL148 Desc.");
			helper.CreateCusCodeList("EUN", "CL148", "LU", "Luxembourg", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
			helper.CreateCusCodeList("EUN", "TEST", "CY", "Cyprus", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var goodsLocation = instruction.GoodsLocation;

			goodsLocation.Address.AuthorisationNumber = "auth";
			goodsLocation.CGL_Qualifier = "T";
			const string messageHouseNumber = "[C0382] Additional Identifier required when qualifier is 'T'.";
			var propertyInfoAdditionalIdentifier = goodsLocation.CGL_AdditionalIdentifierInfo;

			using var testContext = new EntryInstructionValidationDeciderTestContext(declaration, isUCC6: true);
			testContext.EnableRule(x => x.IsRuleC0382ActiveForGoodsLocationAddressHouseNumber);

			CombineAssertions(() =>
			{
				goodsLocation.Address.E2_RN_NKCountryCode = "LU";
				goodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
				AssertNoMessageError("Country with optional house number: " + messageHouseNumber, propertyInfoAdditionalIdentifier, messageHouseNumber);

				goodsLocation.Address.E2_RN_NKCountryCode = "CY";
				goodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
				AssertHasMessageError("Country with required house number: " + messageHouseNumber, propertyInfoAdditionalIdentifier, messageHouseNumber);

				goodsLocation.CGL_AdditionalIdentifier = "123";
				AssertNoMessageError("Country with required house number - house number is not empty: " + messageHouseNumber, propertyInfoAdditionalIdentifier, messageHouseNumber);

				testContext.DisableRule(x => x.IsRuleC0382ActiveForGoodsLocationAddressHouseNumber);
				goodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
				AssertNoMessageError("Country with required house number - rule C0382 is not active: " + messageHouseNumber, propertyInfoAdditionalIdentifier, messageHouseNumber);
			});
		}

		public void AssertCGL_TypeMandatory(bool shouldlocationOfGoodsTypeBeMandatory, TemporaryStorageHeader temporaryStorageHeader, ZString messageMode, ZString unloco)
		{
			temporaryStorageHeader.AMA_MessageType = messageMode;
			temporaryStorageHeader.PlaceOfUnloading = unloco;
			if (shouldlocationOfGoodsTypeBeMandatory)
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(temporaryStorageHeader.GoodsLocation.CGL_TypeInfo);
			}
			else
			{
				temporaryStorageHeader.GoodsLocation.CGL_Type = ZString.Empty;
				AssertNoMessageErrorContaining(temporaryStorageHeader.GoodsLocation.CGL_TypeInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestInvalidUnlocode() => CombineAssertions(() =>
		{
			var goodsLocation = Factory.New<CusGoodsLocation>();
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;

			goodsLocation.Unlocode = "BSASD";
			goodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertNoNotifications("Valid code", goodsLocation.UnlocodeInfo);

			goodsLocation.Unlocode = "00000";
			goodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertHasMessageErrorContaining("Invalid code", goodsLocation.UnlocodeInfo, ListValidation.InvalidCodeMessageError.ToString());

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			goodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertNoNotifications("Not Unlocode qualifier", goodsLocation.UnlocodeInfo);
		});
	}
}
