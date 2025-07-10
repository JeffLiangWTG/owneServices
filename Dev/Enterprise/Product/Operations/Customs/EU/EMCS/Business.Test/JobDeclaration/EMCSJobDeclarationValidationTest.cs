using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	public class EMCSJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJE_OwnerRef_Mandatory()
		{
			CombineAssertions(() =>
			{
				declaration.JE_OwnerRef = ZString.Empty;
				AssertHasMessageErrorContaining("Empty", declaration.JE_OwnerRefInfo, MandatoryValidation.YouHaveNotEntered);
				declaration.JE_OwnerRef = "1234";
				AssertNoMessageErrorContaining("Entered", declaration.JE_OwnerRefInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJE_OwnerRef_UniqueNumberCheckingDifferentOrgDifferentRef()
		{
			var existingDeclaration = declaration;
			var newDeclaration = Factory.New<EMCSJobDeclaration>();
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			existingDeclaration.SupplierDocumentaryAddress.OrganisationPK = org1.PK;
			existingDeclaration.JE_OwnerRef = "1";
			newDeclaration.SupplierDocumentaryAddress.OrganisationPK = org2.PK;
			newDeclaration.JE_OwnerRef = "2";

			newDeclaration.Validation.ValidateJE_OwnerRef();
			AssertEquals("Different local reference numbers and consignors, no error should appear", false, newDeclaration.JE_OwnerRefInfo.HasError(expectedOwnerRefErrorMessage));
		}

		public void TestCheckJE_OwnerRef_UniqueNumberCheckingSameOrgSameRefSameDeclarationType()
		{
			var existingDeclaration = declaration;
			var newDeclaration = Factory.New<EMCSJobDeclaration>();
			var org1 = Factory.New<OrgHeader>();
			existingDeclaration.SupplierDocumentaryAddress.OrganisationPK = org1.PK;
			existingDeclaration.JE_OwnerRef = "1";
			existingDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			newDeclaration.SupplierDocumentaryAddress.OrganisationPK = org1.PK;
			newDeclaration.JE_OwnerRef = "1";
			newDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;

			newDeclaration.Validation.ValidateJE_OwnerRef();
			AssertHasError("Same consignor and same local refrence number, message error should appear", newDeclaration.JE_OwnerRefInfo, expectedOwnerRefErrorMessage);
		}

		public void TestCheckJE_OwnerRef_UniqueNumberCheckingSameOrgSameRefDifferentDeclarationType()
		{
			var existingDeclaration = declaration;
			var newDeclaration = Factory.New<EMCSJobDeclaration>();
			var org1 = Factory.New<OrgHeader>();
			existingDeclaration.SupplierDocumentaryAddress.OrganisationPK = org1.PK;
			existingDeclaration.JE_OwnerRef = "1";
			existingDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			newDeclaration.SupplierDocumentaryAddress.OrganisationPK = org1.PK;
			newDeclaration.JE_OwnerRef = "1";
			newDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;

			newDeclaration.Validation.ValidateJE_OwnerRef();
			AssertEquals("Same consignor and same local refrence number but different declaration types, message error should not appear", false, newDeclaration.JE_OwnerRefInfo.HasError(expectedOwnerRefErrorMessage));
		}

		public void TestCheckJE_OwnerRef_UniqueNumberCheckingSameOrgDifferentRef()
		{
			var existingDeclaration = declaration;
			var newDeclaration = Factory.New<EMCSJobDeclaration>();
			var org1 = Factory.New<OrgHeader>();
			existingDeclaration.SupplierDocumentaryAddress.OrganisationPK = org1.PK;
			existingDeclaration.JE_OwnerRef = "1";
			newDeclaration.SupplierDocumentaryAddress.OrganisationPK = org1.PK;
			newDeclaration.JE_OwnerRef = "2";

			newDeclaration.Validation.ValidateJE_OwnerRef();
			AssertEquals("Same consignor but different local reference numbers, no error should appear", false, newDeclaration.JE_OwnerRefInfo.HasError(expectedOwnerRefErrorMessage));
		}

		public void TestCheckJE_OwnerRef_UniqueNumberCheckingDifferentOrgSameRef()
		{
			var existingDeclaration = declaration;
			var newDeclaration = Factory.New<EMCSJobDeclaration>();
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			existingDeclaration.SupplierDocumentaryAddress.OrganisationPK = org1.PK;
			existingDeclaration.JE_OwnerRef = "2";
			newDeclaration.SupplierDocumentaryAddress.OrganisationPK = org2.PK;
			newDeclaration.JE_OwnerRef = "2";

			newDeclaration.Validation.ValidateJE_OwnerRef();
			AssertEquals("Different consignor but same local reference numbers, no error should appear", false, newDeclaration.JE_OwnerRefInfo.HasError(expectedOwnerRefErrorMessage));
		}

		public void TestCheckJE_DeclarantType()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_DeclarantTypeInfo, "X", EMCSEntryTypeList.Codes.Consignee);
		}

		public void TestCheckJE_MessageType()
		{
			AssertEquals(EMCSJobDeclaration.EMCSMessageTypeCode, declaration.JE_MessageType);
		}

		public void TestCheckJE_MessageSubType()
		{
			var message = "Destination Type must be 1 when Guarantor(s) is 0.";

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_MessageSubTypeInfo, "X", EMCSDestinationTypeList.Codes.Reserved);

			declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.GuarantorNotRequiredQualifyingSameMemberStateMovements;
			declaration.JE_MessageSubType = ZString.Empty;
			AssertHasMessageError(declaration.JE_MessageSubTypeInfo, message);

			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;
			AssertNoMessageError(declaration.JE_MessageSubTypeInfo, message);

			message = "Destination Type should not be 8 when Guarantor(s) contains a 4.";

			declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.JointGuaranteeOfTheConsignorOfTheTransporterAndOfTheConsignee;
			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown;
			AssertHasMessageError(declaration.JE_MessageSubTypeInfo, message);

			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee;
			AssertNoMessageError(declaration.JE_MessageSubTypeInfo, message);

			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;
			AssertHasMessageError(declaration.JE_MessageSubTypeInfo, "Customs Office (Office of Delivery) is required when Destination Type = 6 - Destination - Export.");

			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDelivery);
			declaration.Validation.ValidateJE_MessageSubType();
			AssertNoMessageError(declaration.JE_MessageSubTypeInfo, "Customs Office (Office of Delivery) is required when Destination Type = 6 - Destination - Export.");
		}

		public void TestCheckJE_TransportModeInvalidCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_TransportModeInfo, "XXX", "AIR");
		}

		public void TestCheckJE_TransportModeMandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_TransportModeInfo);
		}

		public void TestCheckJE_TransportModeForGuarantorType5()
		{
			const string errorMessage = "Transport Mode must be SEA or FIX when Guarantor Type is 5.";
			declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.GuarantorNotRequiredQualifyingMemberStateToEuMovements;

			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
				AssertHasMessageError("Mail", declaration.JE_TransportModeInfo, errorMessage);

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertNoMessageError("Sea", declaration.JE_TransportModeInfo, errorMessage);

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertHasMessageError("Air", declaration.JE_TransportModeInfo, errorMessage);

				declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
				AssertNoMessageError("Fixed Transport", declaration.JE_TransportModeInfo, errorMessage);
			});
		}

		public void TestCheckJE_TransportModeForDestinationType8()
		{
			const string errorMessage = "Transport Mode must be SEA or IWT when Destination Type is 8.";
			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown;

			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertHasMessageError("Air", declaration.JE_TransportModeInfo, errorMessage);

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertNoMessageError("Sea", declaration.JE_TransportModeInfo, errorMessage);

				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				AssertHasMessageError("Rail", declaration.JE_TransportModeInfo, errorMessage);

				declaration.JE_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
				AssertNoMessageError("Inland Waterway", declaration.JE_TransportModeInfo, errorMessage);
			});
		}

		public void TestCheckJE_TransportModeIfNoTransportDetails()
		{
			const string errorMessage = "Transport Details are missing.";
			CombineAssertions(() =>
			{
				declaration.Validation.ValidateJE_TransportMode();
				AssertHasMessageError("No Transport Details", declaration.JE_TransportModeInfo, errorMessage);

				declaration.CusContainers.AddNew();
				declaration.Validation.ValidateJE_TransportMode();
				AssertNoMessageError("Has Transport Detail", declaration.JE_TransportModeInfo, errorMessage);
			});
		}

		public void TestCheckJE_DateAtOrigin_Mandatory()
		{
			CombineAssertions(() =>
			{
				declaration.JE_DateAtOrigin = ZDateTime.Empty;
				AssertHasMessageErrorContaining("", declaration.JE_DateAtOriginInfo, MandatoryValidation.YouHaveNotEntered);
				declaration.JE_DateAtOrigin = ZDateTime.Now;
				AssertNoMessageErrorContaining("", declaration.JE_DateAtOriginInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJE_DateAtOrigin_DeferredSubmission_No()
		{
			declaration.ZG_DeferredSubmission = EMCSDeferredSubmissionList.Codes.No;
			const string cannotBeEarlierThanToday = "Dispatch Time Cannot be earlier than Today";
			const string cannotBeLaterThanDaysFromToday = "Dispatch Time Cannot be later than 7 Days from Today";
			CombineAssertions(() =>
			{
				declaration.JE_DateAtOrigin = ZDateTime.Today.AddDays(-1);
				AssertHasMessageErrorContaining("Earlier Date", declaration.JE_DateAtOriginInfo, cannotBeEarlierThanToday);

				declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
				declaration.Validation.ValidateJE_DateAtOrigin();
				AssertNoMessageErrorContaining("Earlier Date, JE_DeclarantType <> Consignor", declaration.JE_DateAtOriginInfo, cannotBeEarlierThanToday);

				declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
				declaration.JE_EntryStatus = EntryStatusList.Codes.REJ;
				declaration.Validation.ValidateJE_DateAtOrigin();
				AssertNoMessageErrorContaining("Earlier Date, JE_EntryStatus not empty", declaration.JE_DateAtOriginInfo, cannotBeEarlierThanToday);

				declaration.JE_EntryStatus = ZString.Empty;
				declaration.JE_DateAtOrigin = ZDateTime.Today.AddDays(8);
				AssertHasMessageErrorContaining("Greater than 7", declaration.JE_DateAtOriginInfo, cannotBeLaterThanDaysFromToday);

				declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
				declaration.Validation.ValidateJE_DateAtOrigin();
				AssertNoMessageErrorContaining("Greater than 7, JE_DeclarantType <> Consignor", declaration.JE_DateAtOriginInfo, cannotBeLaterThanDaysFromToday);

				declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
				declaration.JE_EntryStatus = EntryStatusList.Codes.REJ;
				declaration.Validation.ValidateJE_DateAtOrigin();
				AssertNoMessageErrorContaining("Greater than 7, JE_EntryStatus not empty", declaration.JE_DateAtOriginInfo, cannotBeLaterThanDaysFromToday);

				declaration.JE_EntryStatus = ZString.Empty;
				declaration.JE_DateAtOrigin = ZDateTime.Today.AddDays(7);
				AssertNoMessageErrorContaining("Not Early", declaration.JE_DateAtOriginInfo, cannotBeEarlierThanToday);
				AssertNoMessageErrorContaining("Not Late", declaration.JE_DateAtOriginInfo, cannotBeLaterThanDaysFromToday);
			});
		}

		public void TestCheckJE_DateAtOrigin_DeferredSubmission_Yes()
		{
			declaration.ZG_DeferredSubmission = EMCSDeferredSubmissionList.Codes.Yes;
			const string cannotBeLaterThanTodayWithDeferredSubmission = "Dispatch Time Cannot be later than Today with Deferred Submission.";
			CombineAssertions(() =>
			{
				declaration.JE_DateAtOrigin = ZDateTime.Now;
				AssertNoMessageErrorContaining("Current Date", declaration.JE_DateAtOriginInfo, cannotBeLaterThanTodayWithDeferredSubmission);

				declaration.JE_DateAtOrigin = ZDateTime.Today.AddDays(1);
				AssertHasMessageErrorContaining("Future Date", declaration.JE_DateAtOriginInfo, cannotBeLaterThanTodayWithDeferredSubmission);

				declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
				declaration.Validation.ValidateJE_DateAtOrigin();
				AssertNoMessageErrorContaining("Future Date, JE_DeclarantType <> Consignor", declaration.JE_DateAtOriginInfo, cannotBeLaterThanTodayWithDeferredSubmission);

				declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
				declaration.JE_EntryStatus = EntryStatusList.Codes.REJ;
				declaration.Validation.ValidateJE_DateAtOrigin();
				AssertNoMessageErrorContaining("Future Date, JE_EntryStatus not empty", declaration.JE_DateAtOriginInfo, cannotBeLaterThanTodayWithDeferredSubmission);

				declaration.JE_EntryStatus = ZString.Empty;
				declaration.JE_DateAtOrigin = ZDateTime.Today.AddDays(-1);
				AssertNoMessageErrorContaining("Past Date", declaration.JE_DateAtOriginInfo, cannotBeLaterThanTodayWithDeferredSubmission);
			});
		}

		public void TestCheckSpecialInstructions()
		{
			AssertEquals(ZString.Empty, declaration.SpecialInstructions);
			AssertNotEquals(Core.Constants.TransportModes.Other, declaration.JE_TransportMode);

			declaration.Validation.ValidateAll();
			AssertNoMessageErrors(declaration.SpecialInstructionsInfo);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			declaration.Validation.ValidateAll();
			AssertHasMessageErrors(declaration.SpecialInstructionsInfo);

			declaration.SpecialInstructions = "abc123";
			AssertNoMessageErrors(declaration.SpecialInstructionsInfo);
		}

		public void TestCheckSpecialInstructions_AllCharactersAllowed()
		{
			declaration.SpecialInstructions = "abc123%!";
			AssertNoMessageErrors(declaration.SpecialInstructionsInfo);
		}

		public void TestTooManyDocuments()
		{
			AssertCollectionMaxCount(declaration.Documents, 9, "There are too many Documents. Maximum of 9.");
		}

		public void TestTooManyContainers()
		{
			AssertCollectionMaxCount(declaration.CusContainers, 99, "There are too many Transports. Maximum of 99.");
		}

		void AssertCollectionMaxCount(IBusinessObjectCollection collection, int maxCount, string message)
		{
			var row = collection.AddNew();
			AssertEquals(0, row.RowNotifications.Count());

			for (var idx = 0; idx < (maxCount - 1); idx++)
			{
				row = collection.AddNew();
			}
			AssertEquals(maxCount, collection.Count);
			AssertEquals(0, row.RowNotifications.Count());

			row = collection.AddNew();
			AssertEquals(1, row.RowNotifications.Count());
			AssertEquals(message, row.RowNotifications.First().Message);
		}

		public void TestCheckJourneyTimeFormatPart_MaximumJourneyDays_Other()
		{
			TestMaximumJourneyDays(Core.Constants.TransportModes.Other, 45);
		}

		public void TestCheckJourneyTimeFormatPart_MaximumJourneyDays_Sea()
		{
			TestMaximumJourneyDays(Core.Constants.TransportModes.Sea, 45);
		}

		public void TestCheckJourneyTimeFormatPart_MaximumJourneyDays_Rail()
		{
			TestMaximumJourneyDays(Core.Constants.TransportModes.Rail, 35);
		}

		public void TestCheckJourneyTimeFormatPart_MaximumJourneyDays_Road()
		{
			TestMaximumJourneyDays(Core.Constants.TransportModes.Road, 35);
		}

		public void TestCheckJourneyTimeFormatPart_MaximumJourneyDays_InlandWaterwayTransport()
		{
			TestMaximumJourneyDays(Core.Constants.TransportModes.InlandWaterwayTransport, 35);
		}

		public void TestCheckJourneyTimeFormatPart_MaximumJourneyDays_Air()
		{
			TestMaximumJourneyDays(Core.Constants.TransportModes.Air, 20);
		}

		public void TestCheckJourneyTimeFormatPart_MaximumJourneyDays_Mail()
		{
			TestMaximumJourneyDays(Core.Constants.TransportModes.Mail, 30);
		}

		public void TestCheckJourneyTimeFormatPart_MaximumJourneyDays_FixedTransportInstallations()
		{
			TestMaximumJourneyDays(Core.Constants.TransportModes.FixedTransportInstallations, 15);
		}

		public void TestCheckJourneyTimeFormatPart_MaximumJourneyDays_Default()
		{
			TestMaximumJourneyDays("XXX", 45);
		}

		void TestMaximumJourneyDays(ZString transportMode, ZInt maxJourneyDay)
		{
			declaration.JE_TransportMode = transportMode;
			declaration.UpdateJourneyTime(maxJourneyDay + 1, JourneyTimeUnitList.Codes.Days);
			declaration.Validation.ValidateJourneyTime();
			AssertHasMessageError(declaration.JourneyTimeFormatPartInfo, $"For Transport Mode '{transportMode}' the maximum Journey Time is '{maxJourneyDay}' Days");

			declaration.UpdateJourneyTime(maxJourneyDay, JourneyTimeUnitList.Codes.Days);
			declaration.Validation.ValidateJourneyTime();
			AssertNoMessageError(declaration.JourneyTimeFormatPartInfo, $"For Transport Mode '{transportMode}' the maximum Journey Time is '{maxJourneyDay}' Days");
		}

		public void TestCheckJourneyTimeFormatPartUnitHours()
		{
			const string maximumErrorMessage = "Journey Time Cannot exceed 24 Hours";
			declaration.JourneyTimeFormatPart = JourneyTimeUnitList.Codes.Hours;
			declaration.JourneyTimeNumericPart = 25;
			CombineAssertions(() =>
			{
				AssertHasMessageError("To large", declaration.JourneyTimeFormatPartInfo, maximumErrorMessage);
				declaration.JourneyTimeNumericPart = 24;
				AssertNoMessageError("Maximum Value", declaration.JourneyTimeFormatPartInfo, maximumErrorMessage);
			});
		}

		public void TestCheckJourneyTimeFormatMinimum()
		{
			const string minimumErrorMessage = "Journey Time Must be greater than zero";
			CombineAssertions(() =>
			{
				foreach (var unit in new JourneyTimeUnitList().GetAllCodes())
				{
					declaration.JourneyTimeFormatPart = unit;
					declaration.JourneyTimeNumericPart = -1;
					AssertHasMessageError(unit + " Too small", declaration.JourneyTimeFormatPartInfo, minimumErrorMessage);
					declaration.JourneyTimeNumericPart = 0;
					AssertHasMessageError(unit + " Not entered", declaration.JourneyTimeFormatPartInfo, minimumErrorMessage);
					declaration.JourneyTimeNumericPart = 1;
					AssertNoMessageError(unit + " minimum value", declaration.JourneyTimeFormatPartInfo, minimumErrorMessage);
				}
			});
		}

		public void TestCheckJourneyTimeFormatList()
		{
			CombineAssertions(() =>
			{
				const string enterAValidSelection = "Enter a valid selection.";
				declaration.JourneyTimeFormatPart = "Z";
				AssertHasError("Invalid code", declaration.JourneyTimeFormatPartInfo, enterAValidSelection);
				declaration.JourneyTimeFormatPart = JourneyTimeUnitList.Codes.Hours;
				AssertNoError("Valid code", declaration.JourneyTimeFormatPartInfo, enterAValidSelection);
			});
		}

		public void TestCheckJE_OH_Supplier()
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.RemoveAndDeleteAll();

			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;
			declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.JointGuaranteeOfTheConsignorAndOfTheOwnerOfTheExciseProducts;

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_OH_SupplierInfo);

			var message = $"{declaration.JE_OH_SupplierInfo.HumanReadableName} does not specify the Trader Excise Number when Guarantor(s) contains a 1 and Destination Type is not 4, please see Organization -> Details -> Config -> Registration Numbers / Codes.";

			declaration.Validation.ValidateJE_OH_Supplier();
			AssertHasMessageError(declaration.JE_OH_SupplierInfo, message);

			declaration.JE_OH_Supplier = org.PK;

			declaration.Validation.ValidateJE_OH_Supplier();
			AssertHasMessageError(declaration.JE_OH_SupplierInfo, message);

			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TEST", Core.Constants.CountryCodes.Greece);

			declaration.Validation.ValidateJE_OH_Supplier();
			AssertNoMessageError(declaration.JE_OH_SupplierInfo, message);
		}

		public void TestCheckJE_OH_Importer()
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.RemoveAndDeleteAll();

			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown;

			ValidationTestHelper.AssertIfIsEnteredMessageError(declaration.JE_OH_ImporterInfo);

			void AssertHasCusCodeWithDestinationType(string code, string description, string destinationType, string countryCode)
			{
				var message = $"{declaration.JE_OH_ImporterInfo.HumanReadableName} does not specify the {description} when Destination Type is {destinationType}, please see Organization -> Details -> Config -> Registration Numbers / Codes.";

				org.CustomsCodes.RemoveAndDeleteAll();
				declaration.JE_OH_Importer = org.PK;
				declaration.JE_MessageSubType = destinationType;

				declaration.Validation.ValidateJE_OH_Importer();
				AssertHasMessageError(declaration.JE_OH_ImporterInfo, message);

				org.CustomsCodes.AddNew(code, "TEST", countryCode);

				declaration.Validation.ValidateJE_OH_Importer();
				AssertNoMessageError(declaration.JE_OH_ImporterInfo, message);
			}

			CombineAssertions(() =>
			{
				AssertHasCusCodeWithDestinationType(OrgCusCode.EuropeanUnionSharedCodeTypes.ConsigneeExemptNo, "Consignee Exempt Number", EMCSDestinationTypeList.Codes.DestinationExemptedConsignee, GlbCompany.CurrentCompany.Country.Code);
				AssertHasCusCodeWithDestinationType(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI Code", EMCSDestinationTypeList.Codes.DestinationExport, Core.Constants.CountryCodes.Greece);
			});
		}

		public void TestCheckJE_OH_ImporterWithGuarantorType()
		{
			const string message = "Consignee must have a Trader Excise Number starts with 2 alpha characters and 11 alphanumeric when Guarantor(s) contains a 4 and Destination Type is not 4, please see Organization -> Details -> Config -> Registration Numbers / Codes.";

			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.RemoveAndDeleteAll();

			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_MessageSubType = ZString.Empty;

			declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.OwnerOfTheExciseProducts;
			declaration.Validation.ValidateJE_OH_Importer();

			AssertNoMessageError(declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, message);

			declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.Consignee;
			declaration.Validation.ValidateJE_OH_Importer();

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_OH_ImporterInfo);
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, message);

			declaration.JE_OH_Importer = org.PK;
			declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.JointGuaranteeOfTheConsignorAndOfTheConsignee;
			declaration.Validation.ValidateJE_OH_Importer();

			AssertNoMessageError(declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, message);

			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationDirectDelivery;
			declaration.Validation.ValidateJE_OH_Importer();

			AssertNoMessageError(declaration.JE_OH_ImporterInfo, message);

			var code = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, "TEST", Core.Constants.CountryCodes.Greece);
			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;
			declaration.Validation.ValidateJE_OH_Importer();

			AssertHasMessageError(declaration.JE_OH_ImporterInfo, message);

			code.OK_CustomsRegNo = "EU12345678901";
			declaration.Validation.ValidateJE_OH_Importer();

			AssertNoMessageError(declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, message);
		}

		public void TestCheckPackages()
		{
			const string errorMessage = "At least 1 package is required.";
			CombineAssertions(() =>
			{
				declaration.RunPreSaveValidation();
				AssertHasRowMessageError("No packages", declaration, errorMessage);

				declaration.EMCSPackages.AddNew();
				declaration.RunPreSaveValidation();
				AssertNoRowMessageError("Has package", declaration, errorMessage);
			});
		}

		public void TestCheckDestinationTypeCodeWhenSubmissionTypeIsAssigned()
		{
			var submissionCodes = new[] { EMCSSubmissionTypeList.Codes.StandardSubmission, EMCSSubmissionTypeList.Codes.SubmissionForExport, EMCSSubmissionTypeList.Codes.SubmissionForDutyPaidB2B };
			var destinationCodes = new[] { EMCSDestinationTypeList.Codes.DestinationTaxWarehouse, EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee, EMCSDestinationTypeList.Codes.DestinationTemporaryRegisteredConsignee, EMCSDestinationTypeList.Codes.DestinationDirectDelivery,
											EMCSDestinationTypeList.Codes.DestinationExemptedConsignee,  EMCSDestinationTypeList.Codes.DestinationExport,EMCSDestinationTypeList.Codes.Reserved, EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown,
											EMCSDestinationTypeList.Codes.DestinationCertifiedConsignee,EMCSDestinationTypeList.Codes.DestinationTemporaryCertifiedConsignee,EMCSDestinationTypeList.Codes.DestinationReturnPlaceOfDispatchConsignor };
			CombineAssertions(() =>
			{
				foreach (var submissionType in submissionCodes)
				{
					declaration.ZG_SubmissionType = submissionType;
					switch (submissionType)
					{
						case EMCSSubmissionTypeList.Codes.StandardSubmission:
							var submissionType1Message = $"Destination Type can be 1,2,3,4,5,6, or 8 when Submission Type is 1.";
							foreach (var messageSubType in destinationCodes)
							{
								declaration.JE_MessageSubType = messageSubType;
								declaration.Validation.ValidateJE_MessageSubType();
								switch (messageSubType)
								{
									case EMCSDestinationTypeList.Codes.Reserved:
									case EMCSDestinationTypeList.Codes.DestinationCertifiedConsignee:
									case EMCSDestinationTypeList.Codes.DestinationTemporaryCertifiedConsignee:
									case EMCSDestinationTypeList.Codes.DestinationReturnPlaceOfDispatchConsignor:
										AssertHasMessageError(declaration.JE_MessageSubTypeInfo, submissionType1Message);
										break;
									default:
										AssertNoMessageError(declaration.JE_MessageSubTypeInfo, submissionType1Message);
										break;
								}
							}
							break;
						case EMCSSubmissionTypeList.Codes.SubmissionForExport:
							var submissionType2Message = $"Destination Type must be 6 when Submission Type is 2.";
							foreach (var messageSubType in destinationCodes)
							{
								declaration.JE_MessageSubType = messageSubType;
								declaration.Validation.ValidateJE_MessageSubType();
								switch (messageSubType)
								{
									case EMCSDestinationTypeList.Codes.DestinationExport:
										AssertNoMessageError(declaration.JE_MessageSubTypeInfo, submissionType2Message);
										break;
									default:
										AssertHasMessageError(declaration.JE_MessageSubTypeInfo, submissionType2Message);
										break;
								}
							}
							break;
						case EMCSSubmissionTypeList.Codes.SubmissionForDutyPaidB2B:
							var submissionType3Message = $"Destination Type must be either 9 or 10 when Submission Type is 3.";
							foreach (var messageSubType in destinationCodes)
							{
								declaration.JE_MessageSubType = messageSubType;
								declaration.Validation.ValidateJE_MessageSubType();
								switch (messageSubType)
								{
									case EMCSDestinationTypeList.Codes.DestinationCertifiedConsignee:
									case EMCSDestinationTypeList.Codes.DestinationTemporaryCertifiedConsignee:
										AssertNoMessageError(declaration.JE_MessageSubTypeInfo, submissionType3Message);
										break;
									default:
										AssertHasMessageError(declaration.JE_MessageSubTypeInfo, submissionType3Message);
										break;
								}
							}
							break;
					}
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
		readonly string expectedOwnerRefErrorMessage = "Another Declaration with this Reference Number and Declaration Type is already existing. Please use another Local Reference Number.";
	}
}
