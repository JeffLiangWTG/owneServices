using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCGL_Type_BR_PN_TS_FR06()
		{
			var warningMessage = "Type of location should not be B when Temporary Storage is LAD.";
			var header = Factory.New<TemporaryStorageHeader>();
			var location = header.GoodsLocation;

			header.AMA_ManifestType = FRConstants.TemporaryStorage.AppCodeIST;
			location.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			AssertNoWarning("ManifestType: IST, CGL_Type: B", location.CGL_TypeInfo, warningMessage);

			header.AMA_ManifestType = FRConstants.TemporaryStorage.AppCodeLAD;
			location.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			AssertHasWarning("ManifestType: LAD, CGL_Type: B", location.CGL_TypeInfo, warningMessage);

			location.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
			AssertNoWarning("ManifestType: LAD, CGL_Type: C", location.CGL_TypeInfo, warningMessage);
		}

		public void TestCheckCGL_Type_BR_PN_TS_FR08()
		{
			var warningMessage = "Type of location should not be C when Temporary Storage is IST.";
			var header = Factory.New<TemporaryStorageHeader>();
			var location = header.GoodsLocation;

			header.AMA_ManifestType = FRConstants.TemporaryStorage.AppCodeLAD;
			location.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
			AssertNoWarning("ManifestType: LAD, CGL_Type: C", location.CGL_TypeInfo, warningMessage);

			header.AMA_ManifestType = FRConstants.TemporaryStorage.AppCodeIST;
			location.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
			AssertHasWarning("ManifestType: IST, CGL_Type: C", location.CGL_TypeInfo, warningMessage);

			location.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			AssertNoWarning("ManifestType: IST, CGL_Type: B", location.CGL_TypeInfo, warningMessage);
		}

		public void TestCheckCGL_Type()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var goodsLocation = Factory.New<CusGoodsLocation>();
			goodsLocation.Parent = entryInstruction;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(goodsLocation.CGL_TypeInfo);
		}

		public void TestCheckBR_PN_TS_FR01()
		{
			CombineAssertions("BR_PN_TS_FR01 rule only applies on PNTS of type PN, TS OR TC, and should trigger an error when Good Location Type is A and  qualifier is not V.", () =>
			{
				AssertQualifierHasMessageError(false, PNTSMessageTypeList.Codes.Transfer, CusGoodsLocationQualifierList.Codes.EoriNumber);
				AssertQualifierHasMessageError(false, PNTSMessageTypeList.Codes.Deconsolidation, CusGoodsLocationQualifierList.Codes.EoriNumber);
				AssertQualifierHasMessageError(true, PNTSMessageTypeList.Codes.PreLodgedTempStorage, CusGoodsLocationQualifierList.Codes.EoriNumber);
				AssertQualifierHasMessageError(true, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, CusGoodsLocationQualifierList.Codes.EoriNumber);
				AssertQualifierHasMessageError(true, PNTSMessageTypeList.Codes.PresentationNotification, CusGoodsLocationQualifierList.Codes.EoriNumber);
				AssertQualifierHasMessageError(false, PNTSMessageTypeList.Codes.PreLodgedTempStorage, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
				AssertQualifierHasMessageError(false, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
				AssertQualifierHasMessageError(false, PNTSMessageTypeList.Codes.PresentationNotification, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
			});

			var declaration = Factory.New<JobDeclaration>();
			var goodsLocation = declaration.GoodsLocation;
			goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			AssertNoMessageError("Rule BR_PN_TS_FR01 only applies for PNTS.", goodsLocation.CGL_QualifierInfo, "Qualifier of identification must be V when type is A.");

			void AssertQualifierHasMessageError(bool shouldhaveMessageError, string messageType, string qualifier)
			{
				var header = Factory.New<TemporaryStorageHeader>();
				header.AMA_MessageType = messageType;
				var goodsLocation = header.GoodsLocation;
				goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
				goodsLocation.CGL_Qualifier = qualifier;
				if (shouldhaveMessageError)
				{
					AssertHasMessageError(goodsLocation.CGL_QualifierInfo, "Qualifier of identification must be V when type is A.");
				}
				else
				{
					AssertNoMessageError(goodsLocation.CGL_QualifierInfo, "Qualifier of identification must be V when type is A.");
				}
			}
		}

		public void TestCheckBR_PN_TS_FR03()
		{
			CombineAssertions("BR_PN_TS_FR03 rule only applies on PNTS of type PN, TS OR TC, and should trigger an error when Good Location Type is D.", () =>
			{
				AssertTypeHasMessageError(false, PNTSMessageTypeList.Codes.Transfer, CusGoodsLocationTypeList.Codes.Other);
				AssertTypeHasMessageError(false, PNTSMessageTypeList.Codes.Deconsolidation, CusGoodsLocationTypeList.Codes.Other);
				AssertTypeHasMessageError(true, PNTSMessageTypeList.Codes.PreLodgedTempStorage, CusGoodsLocationTypeList.Codes.Other);
				AssertTypeHasMessageError(true, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, CusGoodsLocationTypeList.Codes.Other);
				AssertTypeHasMessageError(true, PNTSMessageTypeList.Codes.PresentationNotification, CusGoodsLocationTypeList.Codes.Other);
				AssertTypeHasMessageError(false, PNTSMessageTypeList.Codes.PreLodgedTempStorage, CusGoodsLocationTypeList.Codes.AuthorizedPlace);
				AssertTypeHasMessageError(false, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, CusGoodsLocationTypeList.Codes.AuthorizedPlace);
				AssertTypeHasMessageError(false, PNTSMessageTypeList.Codes.PresentationNotification, CusGoodsLocationTypeList.Codes.AuthorizedPlace);
			});

			var declaration = Factory.New<JobDeclaration>();
			var goodsLocation = declaration.GoodsLocation;
			goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.Other;
			AssertNoMessageError("Rule BR_PN_TS_FR03 only applies for PNTS.", goodsLocation.CGL_TypeInfo, "Type of location cannot be D.");

			void AssertTypeHasMessageError(bool shouldhaveMessageError, string messageType, string goodsLocationType)
			{
				var header = Factory.New<TemporaryStorageHeader>();
				header.AMA_MessageType = messageType;
				var goodsLocation = header.GoodsLocation;
				goodsLocation.CGL_Type = goodsLocationType;

				if (shouldhaveMessageError)
				{
					AssertHasMessageError(goodsLocation.CGL_TypeInfo, "Type of location cannot be D.");
				}
				else
				{
					AssertNoMessageError(goodsLocation.CGL_TypeInfo, "Type of location cannot be D.");
				}
			}
		}

		public void TestCheckBR_PN_TS_FR04()
		{
			CombineAssertions("BR_PN_TS_FR04 rule only applies on PNTS of type PN, TS OR TC, and should trigger an error when type is B or C and Qualifier is not Y.", () =>
			{
				AssertQualifierHasMessageError(false, PNTSMessageTypeList.Codes.Transfer, CusGoodsLocationTypeList.Codes.ApprovedPlace, CusGoodsLocationQualifierList.Codes.EoriNumber);
				AssertQualifierHasMessageError(false, PNTSMessageTypeList.Codes.Deconsolidation, CusGoodsLocationTypeList.Codes.ApprovedPlace, CusGoodsLocationQualifierList.Codes.EoriNumber);
				AssertQualifierHasMessageError(false, PNTSMessageTypeList.Codes.Transfer, CusGoodsLocationTypeList.Codes.AuthorizedPlace, CusGoodsLocationQualifierList.Codes.EoriNumber);
				AssertQualifierHasMessageError(false, PNTSMessageTypeList.Codes.Deconsolidation, CusGoodsLocationTypeList.Codes.AuthorizedPlace, CusGoodsLocationQualifierList.Codes.EoriNumber);
				AssertQualifierHasMessageError(true, PNTSMessageTypeList.Codes.PreLodgedTempStorage, CusGoodsLocationTypeList.Codes.ApprovedPlace, CusGoodsLocationQualifierList.Codes.EoriNumber);
				AssertQualifierHasMessageError(true, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, CusGoodsLocationTypeList.Codes.ApprovedPlace, CusGoodsLocationQualifierList.Codes.EoriNumber);
				AssertQualifierHasMessageError(true, PNTSMessageTypeList.Codes.PresentationNotification, CusGoodsLocationTypeList.Codes.ApprovedPlace, CusGoodsLocationQualifierList.Codes.EoriNumber);
				AssertQualifierHasMessageError(false, PNTSMessageTypeList.Codes.PreLodgedTempStorage, CusGoodsLocationTypeList.Codes.ApprovedPlace, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
				AssertQualifierHasMessageError(false, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, CusGoodsLocationTypeList.Codes.ApprovedPlace, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
				AssertQualifierHasMessageError(false, PNTSMessageTypeList.Codes.PresentationNotification, CusGoodsLocationTypeList.Codes.ApprovedPlace, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
				AssertQualifierHasMessageError(true, PNTSMessageTypeList.Codes.PreLodgedTempStorage, CusGoodsLocationTypeList.Codes.AuthorizedPlace, CusGoodsLocationQualifierList.Codes.EoriNumber);
				AssertQualifierHasMessageError(true, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, CusGoodsLocationTypeList.Codes.AuthorizedPlace, CusGoodsLocationQualifierList.Codes.EoriNumber);
				AssertQualifierHasMessageError(true, PNTSMessageTypeList.Codes.PresentationNotification, CusGoodsLocationTypeList.Codes.AuthorizedPlace, CusGoodsLocationQualifierList.Codes.EoriNumber);
				AssertQualifierHasMessageError(false, PNTSMessageTypeList.Codes.PreLodgedTempStorage, CusGoodsLocationTypeList.Codes.AuthorizedPlace, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
				AssertQualifierHasMessageError(false, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, CusGoodsLocationTypeList.Codes.AuthorizedPlace, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
				AssertQualifierHasMessageError(false, PNTSMessageTypeList.Codes.PresentationNotification, CusGoodsLocationTypeList.Codes.AuthorizedPlace, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
			});

			var declaration = Factory.New<JobDeclaration>();
			var goodsLocation = declaration.GoodsLocation;
			goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			AssertNoMessageError("Rule BR_PN_TS_FR04 only applies for PNTS.", goodsLocation.CGL_QualifierInfo, "Qualifier of identification must be Y when type is B or C.");

			void AssertQualifierHasMessageError(bool shouldhaveMessageError, string messageType, string goodsLocationType, string qualifier)
			{
				var header = Factory.New<TemporaryStorageHeader>();
				header.AMA_MessageType = messageType;
				var goodsLocation = header.GoodsLocation;
				goodsLocation.CGL_Type = goodsLocationType;
				goodsLocation.CGL_Qualifier = qualifier;
				if (shouldhaveMessageError)
				{
					AssertHasMessageError(goodsLocation.CGL_QualifierInfo, "Qualifier of identification must be Y when type is B or C.");
				}
				else
				{
					AssertNoMessageError(goodsLocation.CGL_QualifierInfo, "Qualifier of identification must be Y when type is B or C.");
				}
			}
		}

		public void TestQualifierAgainstTypeInError()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var goodsLocation = header.GoodsLocation;
			goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			AssertNoMessageError(goodsLocation.CGL_QualifierInfo, "The chosen Qualifier does not match the Location Type.");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var header2 = Factory.New<TemporaryStorageHeader>();
				var goodsLocation2 = header2.GoodsLocation;
				goodsLocation2.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
				goodsLocation2.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
				AssertHasMessageError(goodsLocation2.CGL_QualifierInfo, "The chosen Qualifier does not match the Location Type.");
			}
		}

		public void TestCheckCGL_AdditionalIdentifierFromEntryInstruction()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryInstruction = dec.CustomsEntryInstructions.AddNew();
			var goodsLocation = entryInstruction.GoodsLocation;
			goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;

			var message = "Additional Identifier requires a 4AN reference";

			goodsLocation.AdditionalIdentifier = ZString.Empty;
			AssertHasMessageErrorContaining("AdditionalIdentifier must be exactly 4 alphanumeric characters for Designated Location and CustomsOfficeIdentifier - AdditionalIdentifier is empty", goodsLocation.AdditionalIdentifierInfo, message);

			goodsLocation.AdditionalIdentifier = "AN12345";
			AssertHasMessageErrorContaining("AdditionalIdentifier must be exactly 4 alphanumeric characters for Designated Location and CustomsOfficeIdentifier - too many characters", goodsLocation.AdditionalIdentifierInfo, message);

			goodsLocation.AdditionalIdentifier = "AN";
			AssertHasMessageErrorContaining("AdditionalIdentifier must be exactly 4 alphanumeric characters for Designated Location and CustomsOfficeIdentifier - too few characters", goodsLocation.AdditionalIdentifierInfo, message);

			goodsLocation.AdditionalIdentifier = "AN1*";
			AssertHasMessageErrorContaining("AdditionalIdentifier must be exactly 4 alphanumeric characters for Designated Location and CustomsOfficeIdentifier - contains invalid character", goodsLocation.AdditionalIdentifierInfo, message);

			goodsLocation.AdditionalIdentifier = "AN12";
			AssertNoMessageErrorContaining("AdditionalIdentifier must be exactly 4 alphanumeric characters for Designated Location and CustomsOfficeIdentifier - valid value", goodsLocation.AdditionalIdentifierInfo, message);
		}

		public void TestCheck_BR_PN_TS_02()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var goodsLocation = header.GoodsLocation;
			goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;

			var message = "Additional Identifier must contain up to 4 alphanumeric characters.";
			goodsLocation.AdditionalIdentifier = ZString.Empty;
			AssertHasMessageErrorContaining("AdditionalIdentifier must not be empty for Designated Location and CustomsOfficeIdentifier - AdditionalIdentifier is empty", goodsLocation.AdditionalIdentifierInfo, MandatoryValidation.YouHaveNotEntered);

			goodsLocation.AdditionalIdentifier = "AN12345";
			AssertHasMessageErrorContaining("AdditionalIdentifier must be 4AN max for Designated Location and CustomsOfficeIdentifier - too much char", goodsLocation.AdditionalIdentifierInfo, message);

			goodsLocation.AdditionalIdentifier = "AN1*";
			AssertHasMessageErrorContaining("AdditionalIdentifier must be 4AN max for Designated Location and CustomsOfficeIdentifier - contains an other symbol that AN", goodsLocation.AdditionalIdentifierInfo, message);

			goodsLocation.AdditionalIdentifier = "AN12";
			AssertNoMessageErrorContaining("AdditionalIdentifier must be 4AN max for Designated Location and CustomsOfficeIdentifier - contains 4 AN", goodsLocation.AdditionalIdentifierInfo, message);
			AssertNoMessageErrorContaining("AdditionalIdentifier must not be empty for Designated Location and CustomsOfficeIdentifier - AdditionalIdentifier is not empty", goodsLocation.AdditionalIdentifierInfo, MandatoryValidation.YouHaveNotEntered);

			goodsLocation.AdditionalIdentifier = "AN";
			AssertNoMessageErrorContaining("AdditionalIdentifier must be 4AN max for Designated Location and CustomsOfficeIdentifier - contains 2 AN", goodsLocation.AdditionalIdentifierInfo, message);

			goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;

			goodsLocation.AdditionalIdentifier = "AN12345";
			AssertNoMessageErrorContaining("AdditionalIdentifier must be 4AN for Designated Location and CustomsOfficeIdentifier - Qualifier is not CustomsOfficeIdentifier", goodsLocation.AdditionalIdentifierInfo, message);
			goodsLocation.AdditionalIdentifier = ZString.Empty;
			AssertNoMessageErrorContaining("AdditionalIdentifier must not be empty for Designated Location and CustomsOfficeIdentifier - Qualifier is not CustomsOfficeIdentifier and AdditionalIdentifier is empty", goodsLocation.AdditionalIdentifierInfo, MandatoryValidation.YouHaveNotEntered);

			goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			goodsLocation.AdditionalIdentifier = "AN12345";
			AssertNoMessageErrorContaining("AdditionalIdentifier must be 4AN for Designated Location and CustomsOfficeIdentifier - type is not DesignatedLocation", goodsLocation.AdditionalIdentifierInfo, message);
			goodsLocation.AdditionalIdentifier = ZString.Empty;
			AssertNoMessageErrorContaining("AdditionalIdentifier must not be empty for Designated Location and CustomsOfficeIdentifier - type is not DesignatedLocation and AdditionalIdentifier is empty", goodsLocation.AdditionalIdentifierInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
