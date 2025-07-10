using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AddInfoQuarantineExDocHeaderValidationTest : AUAddInfoValidationTest
	{
		public void TestCheckZH_PrintLocation()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_PrintLocationInfo.HasMessageErrors());
			eXDOCHeader.QH_PrintLocation = "ASDF";
			AssertHasError("Invalid Code entered", eXDOCHeader.QH_PrintLocationInfo, "Enter a valid selection.");
			eXDOCHeader.QH_PrintLocation = QuarantineExDocHeaderLookups.AqisPlaceCode;
			AssertNoError(eXDOCHeader.QH_PrintLocationInfo, "Enter a valid selection.");
			eXDOCHeader.QH_PrintLocation = EXDOCCodeOrganisation.Codes.Code;
			AssertNoError(eXDOCHeader.QH_PrintLocationInfo, "Enter a valid selection.");
		}

		public void TestCheckZH_StorageLocation()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_StorageLocationInfo.HasMessageErrors());
			eXDOCHeader.QH_StorageLocation = "ASDF";
			AssertHasError("Invalid Code entered", eXDOCHeader.QH_StorageLocationInfo, "Enter a valid selection.");
			eXDOCHeader.QH_StorageLocation = QuarantineExDocHeaderLookups.AqisPlaceCode;
			AssertHasError("Invalid Code entered", eXDOCHeader.QH_StorageLocationInfo, "Enter a valid selection.");
			eXDOCHeader.QH_StorageLocation = EXDOCCodeOrganisation.Codes.Code;
			AssertNoError(eXDOCHeader.QH_StorageLocationInfo, "Enter a valid selection.");
		}

		public void TestCheckZH_AuthorisationLocation()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_AuthorisationLocationInfo.HasMessageErrors());
			eXDOCHeader.QH_AuthorisationLocation = "ASDF";
			AssertHasError("Invalid Code entered", eXDOCHeader.QH_AuthorisationLocationInfo, "Enter a valid selection.");
			eXDOCHeader.QH_AuthorisationLocation = QuarantineExDocHeaderLookups.AqisPlaceCode;
			AssertNoError(eXDOCHeader.QH_AuthorisationLocationInfo, "Enter a valid selection.");
			eXDOCHeader.QH_AuthorisationLocation = EXDOCCodeOrganisation.Codes.Code;
			AssertNoError(eXDOCHeader.QH_AuthorisationLocationInfo, "Enter a valid selection.");
		}

		public void TestCheckZH_ForwardLocation()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_ForwardLocationInfo.HasMessageErrors());
			eXDOCHeader.QH_ForwardLocation = "ASDF";
			AssertHasError("Invalid Code entered", eXDOCHeader.QH_ForwardLocationInfo, "Enter a valid selection.");
			eXDOCHeader.QH_ForwardLocation = QuarantineExDocHeaderLookups.AqisPlaceCode;
			AssertHasError("Invalid Code entered", eXDOCHeader.QH_ForwardLocationInfo, "Enter a valid selection.");
			eXDOCHeader.QH_ForwardLocation = EXDOCCodeOrganisation.Codes.Code;
			AssertNoError(eXDOCHeader.QH_ForwardLocationInfo, "Enter a valid selection.");
		}

		public void TestCheckZH_TransferEDIUserLocation()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_TransferEDIUserLocationInfo.HasMessageErrors());
			eXDOCHeader.QH_TransferEDIUserLocation = "ASDF";
			AssertHasError("Invalid Code entered", eXDOCHeader.QH_TransferEDIUserLocationInfo, "Enter a valid selection.");
			eXDOCHeader.QH_TransferEDIUserLocation = QuarantineExDocHeaderLookups.AqisPlaceCode;
			AssertHasError("Invalid Code entered", eXDOCHeader.QH_TransferEDIUserLocationInfo, "Enter a valid selection.");
			eXDOCHeader.QH_TransferEDIUserLocation = EXDOCCodeOrganisation.Codes.Code;
			AssertNoError(eXDOCHeader.QH_TransferEDIUserLocationInfo, "Enter a valid selection.");
		}

		public void TestCheckZH_TransferExporterLocation()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_TransferExporterLocationInfo.HasMessageErrors());
			eXDOCHeader.QH_TransferExporterLocation = "ASDF";
			AssertHasError("Invalid Code entered", eXDOCHeader.QH_TransferExporterLocationInfo, "Enter a valid selection.");
			eXDOCHeader.QH_TransferExporterLocation = QuarantineExDocHeaderLookups.AqisPlaceCode;
			AssertHasError("Invalid Code entered", eXDOCHeader.QH_TransferExporterLocationInfo, "Enter a valid selection.");
			eXDOCHeader.QH_TransferExporterLocation = EXDOCCodeOrganisation.Codes.Code;
			AssertNoError(eXDOCHeader.QH_TransferExporterLocationInfo, "Enter a valid selection.");
		}

		public void TestCheckZH_ApprovedCertifier()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("ACERT", "ACERT");
			refHelper.CreateNewOrGetExistingCusCodeList("AU", "ACERT", "H0001", "ADELAIDE MOSQUE ISLAMIC SOCIETY OF SOUTH AUSTRALIA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			Assert("Pre-Condition", !eXDOCHeader.QH_ApprovedCertifierInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_ApprovedCertifier = "XXXXX";
			AssertHasMessageError("Invalid Code entered", eXDOCHeader.QH_ApprovedCertifierInfo, "The code you have selected is not in the list.");
			eXDOCHeader.QH_ApprovedCertifier = "H0001";
			AssertNoMessageError(eXDOCHeader.QH_ApprovedCertifierInfo, "The code you have selected is not in the list.");
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_ApprovedCertifier = "XXXXX";
			AssertHasMessageErrorContaining(eXDOCHeader.QH_ApprovedCertifierInfo, "Approved Certifier for products other than meat");
			eXDOCHeader.QH_ApprovedCertifier = "H0001";
			AssertHasMessageErrorContaining(eXDOCHeader.QH_ApprovedCertifierInfo, "Approved Certifier for products other than meat");
			eXDOCHeader.QH_ApprovedCertifier = ZString.Empty;
			AssertNoMessageErrorContaining(eXDOCHeader.QH_ApprovedCertifierInfo, "Approved Certifier for products other than meat");
		}

		public void TestCheckZH_AvAnimalAge()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_AvAnimalAgeInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_AvAnimalAge = "XXXXX";
			AssertHasMessageError("Invalid Code entered", eXDOCHeader.QH_AvAnimalAgeInfo, "The code you have selected is not in the list.");
			eXDOCHeader.QH_AvAnimalAge = EXDOCAverageAgeOfAnimalsCodes.Codes.L1;
			AssertNoMessageError(eXDOCHeader.QH_AvAnimalAgeInfo, "The code you have selected is not in the list.");
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_AvAnimalAge = "XXXXX";
			AssertHasMessageErrorContaining(eXDOCHeader.QH_AvAnimalAgeInfo, "Average Age of Animals for products other than meat");
			eXDOCHeader.QH_AvAnimalAge = EXDOCAverageAgeOfAnimalsCodes.Codes.M1;
			AssertHasMessageErrorContaining(eXDOCHeader.QH_AvAnimalAgeInfo, "Average Age of Animals for products other than meat");
			eXDOCHeader.QH_AvAnimalAge = ZString.Empty;
			AssertNoMessageErrorContaining(eXDOCHeader.QH_AvAnimalAgeInfo, "Average Age of Animals for products other than meat");
		}

		public void TestCheckZH_TrueAndCompleteIndicator()
		{
			const string errorText = "The true and complete question needs to be answered when produce type is dairy, eggs, fish or meat.";
			Assert("Pre-Condition", !eXDOCHeader.QH_TrueAndCompleteIndicatorInfo.HasMessageErrors());

			var exDocMessage = eXDOCHeader.Messages.AddNew();
			exDocMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.EXDOC;

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			eXDOCHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.No;
			eXDOCHeader.QH_TrueAndCompleteIndicator = ZString.Empty;
			AssertHasMessageError("True and Complete needs to be set for Eggs", eXDOCHeader.QH_TrueAndCompleteIndicatorInfo, errorText);

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.No;
			eXDOCHeader.QH_TrueAndCompleteIndicator = ZString.Empty;
			AssertHasMessageError("True and Complete needs to be set for Fish", eXDOCHeader.QH_TrueAndCompleteIndicatorInfo, errorText);

			eXDOCHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.No;
			AssertNoMessageError(eXDOCHeader.QH_TrueAndCompleteIndicatorInfo, errorText);

			eXDOCHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.Yes;
			AssertNoMessageError(eXDOCHeader.QH_TrueAndCompleteIndicatorInfo, errorText);

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.No;
			eXDOCHeader.QH_TrueAndCompleteIndicator = ZString.Empty;
			AssertHasMessageError("True and Complete needs to be set for Meat", eXDOCHeader.QH_TrueAndCompleteIndicatorInfo, errorText);

			eXDOCHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.No;
			AssertNoMessageError(eXDOCHeader.QH_TrueAndCompleteIndicatorInfo, errorText);

			eXDOCHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.Yes;
			AssertNoMessageError(eXDOCHeader.QH_TrueAndCompleteIndicatorInfo, errorText);

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.No;
			eXDOCHeader.QH_TrueAndCompleteIndicator = ZString.Empty;
			AssertHasMessageError(eXDOCHeader.QH_TrueAndCompleteIndicatorInfo, errorText);

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.No;
			eXDOCHeader.QH_TrueAndCompleteIndicator = ZString.Empty;
			AssertNoMessageError("True and Complete not required for GrainsAndPlants", eXDOCHeader.QH_TrueAndCompleteIndicatorInfo, errorText);

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.No;
			eXDOCHeader.QH_TrueAndCompleteIndicator = ZString.Empty;
			AssertNoMessageError("True and Complete not required for Horticulture", eXDOCHeader.QH_TrueAndCompleteIndicatorInfo, errorText);

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			eXDOCHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.No;
			eXDOCHeader.QH_TrueAndCompleteIndicator = ZString.Empty;
			AssertNoMessageError(eXDOCHeader.QH_TrueAndCompleteIndicatorInfo, errorText);

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			eXDOCHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.No;
			eXDOCHeader.QH_TrueAndCompleteIndicator = ZString.Empty;
			AssertNoMessageError(eXDOCHeader.QH_TrueAndCompleteIndicatorInfo, errorText);

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			eXDOCHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.No;
			eXDOCHeader.QH_TrueAndCompleteIndicator = ZString.Empty;
			AssertNoMessageError(eXDOCHeader.QH_TrueAndCompleteIndicatorInfo, errorText);

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_TrueAndCompleteIndicator = ZString.Empty;
			AssertHasMessageError("True and Complete needs to be set for Dairy", eXDOCHeader.QH_TrueAndCompleteIndicatorInfo, errorText);

			exDocMessage.Delete();
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			AssertEquals(true, eXDOCHeader.IsNEXDOCSActive);
			AssertNoMessageError("NEXDOC doesn't need to be set True and Complete Indicator", eXDOCHeader.QH_TrueAndCompleteIndicatorInfo, errorText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			eXDOCHeader = invoiceHeader.QuarantineExDocHeader;
		}

		QuarantineExDocHeader eXDOCHeader;
	}
}
