using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportEntryMessageSendingAction))]
	sealed class ExportEntryMessageSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMovementReferenceNumberAsDetails()
		{
			entry.MovementReferenceNumberSetter("423798234789", ZDateTime.BrettsBirthday);

			AssertEquals("423798234789", action.Details);
		}

		public void TestMovementReferenceNumber_ReadOnly()
		{
			CombineAssertions(() =>
			{
				entry.MovementReferenceNumberSetter("19DE586600822952E8");
				var action = new ExportEntryMessageSendingAction(entry);
				action.EntryType = ExportEntryTypeList.Codes.SupplementaryExportDeclaration;
				AssertEquals("When MRN is not empty and EntryType is ENT", true, action.MovementReferenceNumberInfo.ReadOnly);

				entry.CH_EntryStatus = ZString.Empty;
				entry.MovementReferenceNumberSetter(ZString.Empty);
				action = new ExportEntryMessageSendingAction(entry);
				action.EntryType = ExportEntryTypeList.Codes.ExportAmendment;
				AssertEquals("When MRN is empty, EntryStatus is empty and EntryType is AMD", false, action.MovementReferenceNumberInfo.ReadOnly);

				entry.MovementReferenceNumberSetter("19DE586600822952E8");
				action = new ExportEntryMessageSendingAction(entry);
				action.EntryType = ExportEntryTypeList.Codes.ExportDeclaration;
				AssertEquals("When MRN is not empty, EntryStatus is empty and EntryType is DAT", true, action.MovementReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestMovementReferenceNumber_MaxLength()
		{
			AssertEquals(18, action.MovementReferenceNumberInfo.MaxLength);
		}

		public void TestMovementReferenceNumber_CreateAndUpdateMRNEntryNum()
		{
			const string mrn1 = "19DE586600822952E8";
			const string mrn2 = "21DE586600822952E8";
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			entry = declaration.CustomsEntryHeaders.AddNew();
			action = (ExportEntryMessageSendingAction)GetNewBusinessObject();
			Factory.Save();
			AssertEquals("Prereq: No MRNEntryNum", null, CusEntryNumber.Load<CusEntryNumber>(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany));
			AssertEquals("PreReq: JobDeclaration has no changes => Save-Button disabled", expected: false, declaration.HasChanges);

			CombineAssertions(() =>
			{
				var readOnlyFactory = new ReadOnlyBusinessObjectFactory();
				action.MovementReferenceNumber = mrn1;
				AssertEquals("JobDeclaration has changes => Save-Button enabled", expected: true, declaration.HasChanges);
				AssertNotNull("MRNEntryNum created", CusEntryNumber.Load<CusEntryNumber>(Factory, CusEntryNumberTypes.Standard.MovementReferenceNumber, mrn1, Core.Constants.CountryCodes.Germany).SingleOrDefault());
				Factory.Save();
				AssertNotNull("MRNEntryNum loaded from different ObjectFactory", CusEntryNumber.Load<CusEntryNumber>(readOnlyFactory, CusEntryNumberTypes.Standard.MovementReferenceNumber, mrn1, Core.Constants.CountryCodes.Germany).SingleOrDefault());
				AssertEquals("JobDeclaration has no changes after saved => Save-Button disabled", expected: false, declaration.HasChanges);

				action.MovementReferenceNumber = mrn2;
				AssertEquals("JobDeclaration has changes after modifying mrn => Save-Button enabled", expected: true, declaration.HasChanges);
				AssertNotNull("MRNEntryNum updated", CusEntryNumber.Load<CusEntryNumber>(Factory, CusEntryNumberTypes.Standard.MovementReferenceNumber, mrn2, Core.Constants.CountryCodes.Germany).SingleOrDefault());
				Factory.Save();
				AssertNotNull("Updated MRNEntryNum loaded from different ObjectFactory", CusEntryNumber.Load<CusEntryNumber>(readOnlyFactory, CusEntryNumberTypes.Standard.MovementReferenceNumber, mrn2, Core.Constants.CountryCodes.Germany).SingleOrDefault());
				AssertEquals("JobDeclaration has no changes after saved 2nd time => Save-Button disabled", expected: false, declaration.HasChanges);
			});
		}

		public void TestLocalReferenceNumber()
		{
			declaration.JE_OwnerRef = "12345";
			AssertEquals("12345", new ExportEntryMessageSendingAction(entry).LocalReferenceNumber);
		}

		public void TestLocalReferenceNumber_MaxLength()
		{
			AssertEquals(22, action.LocalReferenceNumberInfo.MaxLength);
		}

		public void TestProxyProperties()
		{
			EntryInstruction.CEI_Style = "S1";
			EntryInstruction.CEI_SubStyle = "1";
			EntryInstruction.CEI_Description = "Test Instruction";
			entry.MovementReferenceNumberSetter("423798234789", ZDateTime.BrettsBirthday);
			entry.CH_EntryStatus = "REL";
			declaration.JE_CustomsOffice = "Office3";

			AssertEquals("S1", action.ProcedureType);
			AssertEquals("1", action.Variant);
			AssertEquals("Test Instruction", action.Description);
			AssertEquals("423798234789", action.Details);
			AssertEquals("REL", action.EntryStatus);
			AssertEquals("Office3", action.ExportCustomsOffice);

			entry.CH_CEI_Instruction = ZGuid.Empty;
			AssertEquals("", action.ProcedureType);
			AssertEquals("", action.Variant);
			AssertEquals("", action.Description);
		}

		public void TestEntryTypeClearIrrelevantData()
		{
			action.EntryType = ExportEntryTypeList.Codes.ExitToExport;
			action.Annotation = "Exit to export";
			action.ExitType = ExportExitTypeList.Codes._2;
			action.ExitDate = ZDateTime.Today;
			action.ExitCustomsOffice = "Office1";
			action.AlternativeEvidences.AddNew();

			CombineAssertions(() =>
			{
				action.EntryType = ExportEntryTypeList.Codes.CancellationRequest;
				AssertEquals("Annotation", "Exit to export", action.Annotation);
				AssertEquals("ExitType", ZString.Empty, action.ExitType);
				AssertEquals("ExitDate", ZDateTime.Empty, action.ExitDate);
				AssertEquals("ExitCustomsOffice", ZString.Empty, action.ExitCustomsOffice);
				AssertEquals("AlternativeEvidences.Count", 0, action.AlternativeEvidences.Count);
			});
		}

		public void TestDefaultExitCustomsOfficeFromDeclaration()
		{
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.CentralOfficeCommonDomain, "Office1");
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "Office2");

			action.EntryType = ExportEntryTypeList.Codes.ExitToExport;
			AssertEquals("Office2", action.ExitCustomsOffice);
		}

		public void TestEntryType_WhenShouldSendSetToTrue_AndEntryStatusEmpty_ShouldSetEntryTypeToDAT()
		{
			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("ShouldSend should be false by default.", false, action.ShouldSend);
				AssertEquals("EntryStatus should be empty by default.", ZString.Empty, action.EntryStatus);
				AssertEquals("EntryType should be empty by default.", ZString.Empty, action.EntryType);
			});

			action.ShouldSend = true;
			AssertEquals("When EntryStatus is empty and Send is ticked, EntryType should be set automatically for convenience.",
				ExportEntryTypeList.Codes.ExportDeclaration, action.EntryType);
		}

		public void TestEntryType_WhenShouldSendSetToTrue_AndEntryStatusNotEmpty_ShouldNotSetEntryType()
		{
			action.MessagingObject.CH_EntryStatus = EntryStatusList.Codes.SentAndInitiallyRejected;

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("ShouldSend should be false by default.", false, action.ShouldSend);
				AssertEquals("EntryType should be empty by default.", ZString.Empty, action.EntryType);
				AssertNotEquals("EntryType should now be set to anything non-empty.", ZString.Empty, action.EntryStatus);
			});

			action.ShouldSend = true;
			AssertEquals("When EntryStatus is not empty and Send is ticked, EntryType should not be changed.",
				ZString.Empty, action.EntryType);
		}

		public void TestSetShouldSendToTrue_WhenEntryTypeAlreadySet_ShouldNotChangeEntryType()
		{
			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("ShouldSend should be false by default.", false, action.ShouldSend);
				AssertEquals("EntryStatus should be empty by default.", ZString.Empty, action.EntryStatus);
			});

			action.EntryType = ExportEntryTypeList.Codes.CancellationRequest;
			action.ShouldSend = true;

			AssertEquals("If the user has already set the EntryType, it would be rude to change it on them.",
				ExportEntryTypeList.Codes.CancellationRequest, action.EntryType);
		}

		public void TestEntryType_ListValidation()
		{
			action.EntryType = "XX";
			AssertHasMessageErrorContaining(action.EntryTypeInfo, ListValidation.InvalidCodeMessageError);
			action.EntryType = ExportEntryTypeList.Codes.ExportDeclaration;
			AssertNoMessageErrorContaining(action.EntryTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestValidateEntryType_CAN()
		{
			AssertValidateEntryType_EntryStatus(ExportEntryTypeList.Codes.CancellationRequest,
				"The current Entry Status does not allow this type of message (CAN) to be accepted.",
				new[] { "110", "130", "131", "132", "141", "142", "500", "501", "502", "541", "542", "570" });
		}

		public void TestValidateEntryType_DAT()
		{
			AssertValidateEntryType_EntryStatus(ExportEntryTypeList.Codes.ExportDeclaration,
				"The current Entry Status does not allow this type of message (DAT) to be accepted.",
				new[] { "" });
		}

		public void TestValidateEntryType_AMD_EntryStatus()
		{
			AssertValidateEntryType_EntryStatus(ExportEntryTypeList.Codes.ExportAmendment,
				"The current Entry Status and/ or Type(Procedure) does not allow this type of message (AMD) to be accepted.",
				new[] { "110", "130", "131", "132" },
				ExportDeclarationTypeTimeList.Codes._00,
				ExportDeclarationTypeProcedureList.Codes._000100);
		}

		public void TestValidateEntryType_AMD_TypeTime()
		{
			const string messageError = "The current Entry Status and/ or Type(Procedure) does not allow this type of message (AMD) to be accepted.";
			action.EntryType = ExportEntryTypeList.Codes.ExportAmendment;
			EntryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._10;
			EntryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
			entry.CH_EntryStatus = "110";
			CombineAssertions(() =>
			{
				action.ShouldSend = false;
				AssertNoEntryStatusMessageError("Invalid TypeTime but ShouldSend = False", messageError);

				action.ShouldSend = true;
				AssertHasEntryStatusMessageError("Invalid TypeTime and ShouldSend = True", messageError);

				EntryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
				AssertNoEntryStatusMessageError("Valid TypeTime", messageError);

				var invalidTypeTimes = new ExportDeclarationTypeTimeList().GetAllCodes().ToList();
				invalidTypeTimes.Remove(ExportDeclarationTypeTimeList.Codes._00);
				foreach (var invalidTypeTime in invalidTypeTimes)
				{
					EntryInstruction.CEI_SubStyle = invalidTypeTime;
					AssertHasEntryStatusMessageError($"Invalid TypeTime '{invalidTypeTime}'", messageError);
				}
			});
		}

		public void TestValidateEntryType_AMD_TypeProcedure()
		{
			AssertValidateEntryType_TypeProcedure(ExportEntryTypeList.Codes.ExportAmendment,
				"The current Entry Status and/ or Type(Procedure) does not allow this type of message (AMD) to be accepted.",
				new[]
				{
					ExportDeclarationTypeProcedureList.Codes._000100,
					ExportDeclarationTypeProcedureList.Codes._000110,
					ExportDeclarationTypeProcedureList.Codes._000200,
					ExportDeclarationTypeProcedureList.Codes._000210,
					ExportDeclarationTypeProcedureList.Codes._000901,
					ExportDeclarationTypeProcedureList.Codes._000902,
					ExportDeclarationTypeProcedureList.Codes._110100,
					ExportDeclarationTypeProcedureList.Codes._110110,
					ExportDeclarationTypeProcedureList.Codes._110200,
					ExportDeclarationTypeProcedureList.Codes._110210,
					ExportDeclarationTypeProcedureList.Codes._120100,
					ExportDeclarationTypeProcedureList.Codes._120110,
					ExportDeclarationTypeProcedureList.Codes._120200,
					ExportDeclarationTypeProcedureList.Codes._120210,
					ExportDeclarationTypeProcedureList.Codes._200100,
					ExportDeclarationTypeProcedureList.Codes._200110,
					ExportDeclarationTypeProcedureList.Codes._200200,
					ExportDeclarationTypeProcedureList.Codes._200210
				},
				"110",
				ExportDeclarationTypeTimeList.Codes._00,
				ExportDeclarationTypeProcedureList.Codes._000000);
		}

		public void TestValidateEntryType_ENT_EntryStatus()
		{
			AssertValidateEntryType_EntryStatus(ExportEntryTypeList.Codes.SupplementaryExportDeclaration,
				"The current Entry Status and/ or Type(Procedure) does not allow this type of message (ENT) to be accepted.",
				new[] { "131", "141", "501", "541" },
				typeProcedure: ExportDeclarationTypeProcedureList.Codes._000110);
		}

		public void TestValidateEntryType_ENT_TypeProcedure()
		{
			AssertValidateEntryType_TypeProcedure(ExportEntryTypeList.Codes.SupplementaryExportDeclaration,
				"The current Entry Status and/ or Type(Procedure) does not allow this type of message (ENT) to be accepted.",
				new[]
				{
					ExportDeclarationTypeProcedureList.Codes._000110,
					ExportDeclarationTypeProcedureList.Codes._000210,
					ExportDeclarationTypeProcedureList.Codes._000410,
					ExportDeclarationTypeProcedureList.Codes._001310,
					ExportDeclarationTypeProcedureList.Codes._001410,
					ExportDeclarationTypeProcedureList.Codes._110110,
					ExportDeclarationTypeProcedureList.Codes._110210,
					ExportDeclarationTypeProcedureList.Codes._110410,
					ExportDeclarationTypeProcedureList.Codes._111310,
					ExportDeclarationTypeProcedureList.Codes._111410,
					ExportDeclarationTypeProcedureList.Codes._120110,
					ExportDeclarationTypeProcedureList.Codes._120210,
					ExportDeclarationTypeProcedureList.Codes._200110,
					ExportDeclarationTypeProcedureList.Codes._200210,
					ExportDeclarationTypeProcedureList.Codes._200410,
					ExportDeclarationTypeProcedureList.Codes._201310,
					ExportDeclarationTypeProcedureList.Codes._201410
				},
				"131",
				typeProcedure: ExportDeclarationTypeProcedureList.Codes._000000);
		}

		public void TestValidateMovementReferenceNumber_ENT_Mandatory()
		{
			const string msgNotEntered = "You have not entered the MRN of the incomplete Export Declaration.";
			action.EntryType = ExportEntryTypeList.Codes.SupplementaryExportDeclaration;

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(action.MovementReferenceNumberInfo, msgNotEntered);
		}

		public void TestValidateMovementReferenceNumber_ENT_Format() => AssertValidateMovementReferenceNumber_Format(ExportEntryTypeList.Codes.SupplementaryExportDeclaration);
		public void TestValidateMovementReferenceNumber_AMD_Format() => AssertValidateMovementReferenceNumber_Format(ExportEntryTypeList.Codes.ExportAmendment);
		public void TestValidateMovementReferenceNumber_CAN_Format() => AssertValidateMovementReferenceNumber_Format(ExportEntryTypeList.Codes.CancellationRequest);
		public void TestValidateMovementReferenceNumber_EXT_Format() => AssertValidateMovementReferenceNumber_Format(ExportEntryTypeList.Codes.ExitToExport);

		void AssertValidateMovementReferenceNumber_Format(ZString entryType)
		{
			const string message = "a MRN structure is required (18 alphanumeric characters). Please enter a MRN in the following format with only numbers and upper case letters:";
			action.EntryType = entryType;
			action.MovementReferenceNumber = "12345678901234567";

			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("Invalid format", action.MovementReferenceNumberInfo, message);

				action.MovementReferenceNumber = "19ZZ586600822952E8";
				AssertNoMessageErrorContaining("Valid format", action.MovementReferenceNumberInfo, message);
			});
		}

		public void TestValidateMovementReferenceNumber_ENT_CountryCode() => AssertValidateMovementReferenceNumber_CountryCode(ExportEntryTypeList.Codes.SupplementaryExportDeclaration);
		public void TestValidateMovementReferenceNumber_AMD_CountryCode() => AssertValidateMovementReferenceNumber_CountryCode(ExportEntryTypeList.Codes.ExportAmendment);
		public void TestValidateMovementReferenceNumber_CAN_CountryCode() => AssertValidateMovementReferenceNumber_CountryCode(ExportEntryTypeList.Codes.CancellationRequest);
		public void TestValidateMovementReferenceNumber_EXT_CountryCode() => AssertValidateMovementReferenceNumber_CountryCode(ExportEntryTypeList.Codes.ExitToExport);

		void AssertValidateMovementReferenceNumber_CountryCode(ZString entryType)
		{
			const string message = "MRN does not contain a valid country/region code";
			action.EntryType = entryType;
			action.MovementReferenceNumber = "19ZZ586600822952E8";

			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("Invalid country code", action.MovementReferenceNumberInfo, message);

				action.MovementReferenceNumber = "19DE586600822952E8";
				AssertNoMessageErrorContaining("Valid country code", action.MovementReferenceNumberInfo, message);
			});
		}

		public void TestValidateMovementReferenceNumber_ENT_CheckDigit() => AssertValidateMovementReferenceNumber_CheckDigit(ExportEntryTypeList.Codes.SupplementaryExportDeclaration);
		public void TestValidateMovementReferenceNumber_AMD_CheckDigit() => AssertValidateMovementReferenceNumber_CheckDigit(ExportEntryTypeList.Codes.ExportAmendment);
		public void TestValidateMovementReferenceNumber_CAN_CheckDigit() => AssertValidateMovementReferenceNumber_CheckDigit(ExportEntryTypeList.Codes.CancellationRequest);
		public void TestValidateMovementReferenceNumber_EXT_CheckDigit() => AssertValidateMovementReferenceNumber_CheckDigit(ExportEntryTypeList.Codes.ExitToExport);

		void AssertValidateMovementReferenceNumber_CheckDigit(ZString entryType)
		{
			const string msgInvalidCheckDigit = "MRN does not have a valid last digit. The last digit should be 8";
			action.EntryType = entryType;
			action.MovementReferenceNumber = "19DE586600822952E9";

			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("Invalid format", action.MovementReferenceNumberInfo, msgInvalidCheckDigit);

				action.MovementReferenceNumber = "19DE586600822952E8";
				AssertNoMessageErrorContaining("Valid format", action.MovementReferenceNumberInfo, msgInvalidCheckDigit);
			});
		}

		public void TestEntryType_MandatoryValidation()
		{
			action.ShouldSend = false;
			action.ValidateEntryType();
			AssertNoErrorContaining(action.EntryTypeInfo, "Please enter a value");
			action.ShouldSend = true;
			action.EntryType = ZString.Empty;
			action.ValidateEntryType();
			AssertHasErrorContaining(action.EntryTypeInfo, "Please enter a value");
		}

		public void TestEntryType_PresetSecurityType()
		{
			CombineAssertions(() =>
			{
				AssertPresetSecurityType(EntryStyleListExport.Codes.ExportToSpecialTerritory, ExportEntryTypeList.Codes.ExportDeclaration, ExportSecurityTypeList.Codes.NotUsed);
				AssertPresetSecurityType(EntryStyleListExport.Codes.ExportNormal, ExportEntryTypeList.Codes.ExportDeclaration, ExportSecurityTypeList.Codes.EXS);
				AssertPresetSecurityType(EntryStyleListExport.Codes.ExportToEFTAMember, ExportEntryTypeList.Codes.ExportDeclaration, ExportSecurityTypeList.Codes.NotUsed);
				AssertPresetSecurityType(EntryStyleListExport.Codes.ExportToSpecialTerritory, ExportEntryTypeList.Codes.ExportAmendment, ExportSecurityTypeList.Codes.NotUsed);
				AssertPresetSecurityType(EntryStyleListExport.Codes.ExportNormal, ExportEntryTypeList.Codes.ExportAmendment, ExportSecurityTypeList.Codes.EXS);
				AssertPresetSecurityType(EntryStyleListExport.Codes.ExportToEFTAMember, ExportEntryTypeList.Codes.ExportAmendment, ExportSecurityTypeList.Codes.EXS);
			});

			void AssertPresetSecurityType(ZString entryStyle, ZString entryType, ZString expectedSecurityType)
			{
				declaration.JE_EntryStyle = entryStyle;
				action.EntryType = entryType;
				AssertEquals($"Expected SecurityType: {expectedSecurityType} for EntryType: {entryType} and EntryStyle: {entryStyle}.", expectedSecurityType, action.SecurityType);
			}
		}

		[TestDate(2022, 10, 19)]
		public void TestEntryType_ValidCusEntryNumIssueDate_Validation()
		{
			const string msgError = "The Follow Up Procedure can be initiated earliest 70 days after Release for Export (19.10.2022).";
			var date = new System.DateTime(2022, 10, 19);
			entry.CH_EntryStatus = "500";
			entry.MovementReferenceNumberSetter("DE221234567891236", date);

			action.EntryType = ExportEntryTypeList.Codes.ExitToExport;

			CombineAssertions(() =>
			{
				AssertHasEntryStatusMessageError("Should have error", msgError);

				entry.MovementReferenceNumberSetter("DE221234567891236", date.AddDays(-71));

				AssertNoEntryStatusMessageError("Should not have error", msgError);
			});
		}

		public void TestAnnotationReadOnly()
		{
			Assert(action.AnnotationInfo.ReadOnly);

			action.EntryType = ExportEntryTypeList.Codes.ExportDeclaration;
			Assert(action.AnnotationInfo.ReadOnly);

			action.EntryType = ExportEntryTypeList.Codes.ExitToExport;
			Assert(!action.AnnotationInfo.ReadOnly);

			action.EntryType = ExportEntryTypeList.Codes.CancellationRequest;
			Assert(!action.AnnotationInfo.ReadOnly);
		}

		public void TestAnnotationCaption()
		{
			action.EntryType = ExportEntryTypeList.Codes.ExportDeclaration;
			AssertEquals("Annotation", action.AnnotationCaption.Caption);

			action.EntryType = ExportEntryTypeList.Codes.CancellationRequest;
			AssertEquals("Reason for Cancellation", action.AnnotationCaption.Caption);
		}

		public void TestAnnotation_MaxLength()
		{
			AssertEquals(512, action.AnnotationInfo.MaxLength);
		}

		public void TestAnnotationValidation()
		{
			CombineAssertions(() =>
			{
				action.EntryType = ExportEntryTypeList.Codes.CancellationRequest;
				action.Annotation = "";
				AssertHasMessageErrorContaining("EntryType is 'CAN' and Annotation is empty", action.AnnotationInfo, MandatoryValidation.YouHaveNotEntered);

				action.EntryType = ExportEntryTypeList.Codes.ExportDeclaration;
				action.Annotation = "";
				AssertNoMessageErrorContaining("EntryType is 'DAT' and Annotation is empty", action.AnnotationInfo, MandatoryValidation.YouHaveNotEntered);

				action.EntryType = ExportEntryTypeList.Codes.ExitToExport;
				action.Annotation = "";
				AssertNoMessageErrorContaining("EntryType is 'EXT' and ExitType is empty and Annotation is empty", action.AnnotationInfo, MandatoryValidation.YouHaveNotEntered);

				action.ExitType = ExportExitTypeList.Codes._2;
				action.Annotation = "";
				AssertHasMessageErrorContaining("EntryType is 'EXT' and ExitType is '2' and Annotation is empty", action.AnnotationInfo, MandatoryValidation.YouHaveNotEntered);

				action.ExitType = ExportExitTypeList.Codes._4;
				action.Annotation = "";
				AssertNoMessageErrorContaining("EntryType is 'EXT' and ExitType is '4' and Annotation is empty", action.AnnotationInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestExitTypeReadOnly()
		{
			Assert(action.ExitTypeInfo.ReadOnly);

			action.EntryType = ExportEntryTypeList.Codes.ExportDeclaration;
			Assert(action.ExitTypeInfo.ReadOnly);

			action.EntryType = ExportEntryTypeList.Codes.ExitToExport;
			Assert(!action.ExitTypeInfo.ReadOnly);

			action.EntryType = ExportEntryTypeList.Codes.CancellationRequest;
			Assert(action.ExitTypeInfo.ReadOnly);
		}

		public void TestExitTypeValidation()
		{
			CombineAssertions(() =>
			{
				action.EntryType = ExportEntryTypeList.Codes.ExitToExport;
				action.ExitType = "";
				AssertHasMessageErrorContaining("EntryType is 'EXT' and ExitType is empty", action.ExitTypeInfo, MandatoryValidation.YouHaveNotEntered);

				action.ExitType = "XXX";
				AssertNoMessageErrorContaining("EntryType is 'EXT' and ExitType isn't empty", action.ExitTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("EntryType is 'EXT' and ExitType is invalid", action.ExitTypeInfo, ListValidation.InvalidCodeMessageError.ToString());

				action.ExitType = ExportExitTypeList.Codes._2;
				AssertNoMessageErrorContaining("EntryType is 'EXT' and ExitType is valid", action.ExitTypeInfo, ListValidation.InvalidCodeMessageError.ToString());

				action.EntryType = ExportEntryTypeList.Codes.ExportDeclaration;
				action.ExitType = "";
				AssertNoMessageErrorContaining("EntryType is 'DAT' and ExitType is empty", action.ExitTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestExitTypeValidation_AlternativeEvidenceIsRequired()
		{
			var message = "You have not entered an Alternative Evidence.";
			action.EntryType = ExportEntryTypeList.Codes.ExitToExport;
			action.ExitType = ExportExitTypeList.Codes._4;
			CombineAssertions(() =>
			{
				AssertHasMessageError("ExitType is '4'", action.ExitTypeInfo, message);

				action.ExitType = ExportExitTypeList.Codes._2;
				AssertNoMessageError("ExitType isn't '4'", action.ExitTypeInfo, message);

				action.AlternativeEvidences.AddNew();
				action.ExitType = ExportExitTypeList.Codes._4;
				AssertNoMessageError("AlternativeEvidences isn't empty", action.ExitTypeInfo, message);
			});
		}

		public void TestExitDateReadOnly()
		{
			Assert(action.ExitDateInfo.ReadOnly);

			action.EntryType = ExportEntryTypeList.Codes.ExportDeclaration;
			Assert(action.ExitDateInfo.ReadOnly);

			action.EntryType = ExportEntryTypeList.Codes.ExitToExport;
			Assert(!action.ExitDateInfo.ReadOnly);

			action.EntryType = ExportEntryTypeList.Codes.CancellationRequest;
			Assert(action.ExitDateInfo.ReadOnly);
		}

		[TestDate(2019, 10, 24, 8, 1, 1)]
		public void TestValidateExitDate()
		{
			action.EntryType = ExportEntryTypeList.Codes.ExitToExport;
			action.ExitDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(action.ExitDateInfo, MandatoryValidation.YouHaveNotEntered);

			action.ExitType = ExportExitTypeList.Codes._2;
			action.ExitDate = ZDateTime.Today.AddDays(-1);
			AssertHasMessageError(action.ExitDateInfo, "Exit Date must not be in the past for the selected Exit Type.");

			action.ExitDate = ZDateTime.Today.AddHours(1);
			AssertNoMessageError(action.ExitDateInfo, "Exit Date must not be in the past for the selected Exit Type.");

			action.ExitType = ExportExitTypeList.Codes._4;
			action.ExitDate = ZDateTime.Today.AddDays(1);
			AssertHasMessageError(action.ExitDateInfo, "Exit Date must not be in the future for the selected Exit Type.");

			action.ExitDate = ZDateTime.Today.AddHours(-1);
			AssertNoMessageError(action.ExitDateInfo, "Exit Date must not be in the future for the selected Exit Type.");

			entry.MovementReferenceNumberSetter("324098", new ZDateTime(2019, 10, 2, 10, 10, 10));
			action.ExitDate = new ZDateTime(2019, 10, 1);
			AssertHasMessageError(action.ExitDateInfo, "Exit Date must not be earlier than Release Date.");

			action.ExitDate = new ZDateTime(2019, 10, 2);
			AssertNoMessageError(action.ExitDateInfo, "Exit Date must not be earlier than Release Date.");
		}

		public void TestExitCustomsOfficeReadOnly()
		{
			Assert(action.ExitCustomsOfficeInfo.ReadOnly);

			action.EntryType = ExportEntryTypeList.Codes.ExportDeclaration;
			Assert(action.ExitCustomsOfficeInfo.ReadOnly);

			action.EntryType = ExportEntryTypeList.Codes.ExitToExport;
			Assert(!action.ExitCustomsOfficeInfo.ReadOnly);

			action.EntryType = ExportEntryTypeList.Codes.CancellationRequest;
			Assert(action.ExitCustomsOfficeInfo.ReadOnly);
		}

		public void TestValidateExitCustomsOffice()
		{
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004323", "GERMAN OFFICE1", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004324", "GERMAN OFFICE2", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExitInland);
			Factory.Save();

			action.EntryType = ExportEntryTypeList.Codes.ExitToExport;
			action.ExitCustomsOffice = ZString.Empty;
			AssertHasMessageErrorContaining(action.ExitCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			action.ExitCustomsOffice = ZString.Empty;
			AssertHasMessageErrorContaining(action.ExitCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			action.ExitCustomsOffice = "Office1";
			AssertNoMessageErrorContaining(action.ExitCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(action.ExitCustomsOfficeInfo, ListValidation.InvalidCodeMessageError);

			action.ExitCustomsOffice = "DE004324";
			AssertNoMessageErrorContaining(action.ExitCustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestRunPreSaveValidation()
		{
			action.ShouldSend = true;
			action.EntryType = ExportEntryTypeList.Codes.ExitToExport;
			action.RunPreSaveValidation();
			AssertHasMessageErrorContaining(action.ExitDateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(action.ExitCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			action.ExitType = ExportExitTypeList.Codes._2;
			action.RunPreSaveValidation();
			AssertHasMessageErrorContaining(action.AnnotationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestAnnotationValidatesWhenEntryTypeCANIsSelected()
		{
			action.ShouldSend = true;
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(action.AnnotationInfo, MandatoryValidation.YouHaveNotEntered);
				action.EntryType = ExportEntryTypeList.Codes.CancellationRequest;
				AssertHasMessageErrorContaining(action.AnnotationInfo, MandatoryValidation.YouHaveNotEntered);
				action.Annotation = "this is a test reason to satisfy validation";
				AssertNoMessageErrorContaining(action.AnnotationInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestAnnotationErrorMessageIsCorrectWhenCAN()
		{
			action.ShouldSend = true;
			action.EntryType = ExportEntryTypeList.Codes.CancellationRequest;
			action.RunPreSaveValidation();
			AssertHasMessageErrorContaining(action.AnnotationInfo, MandatoryValidation.YouHaveNotEntered + " a Reason.");
		}

		public void TestSecurity_MaxLenght()
		{
			AssertEquals(1, action.SecurityTypeInfo.MaxLength);
		}

		public void TestSecurityType_InputValue()
		{
			CombineAssertions(() =>
			{
				action.SecurityType = "1";
				AssertHasMessageErrorContaining(action.SecurityTypeInfo, ListValidation.InvalidCodeMessageError);

				action.SecurityType = "0";
				AssertNoMessageErrorContaining(action.SecurityTypeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestSecurityType_Readonly()
		{
			Assert(action.SecurityTypeInfo.ReadOnly);

			action.EntryType = ExportEntryTypeList.Codes.ExportDeclaration;
			Assert(!action.SecurityTypeInfo.ReadOnly);

			action.EntryType = ExportEntryTypeList.Codes.ExitToExport;
			Assert(action.SecurityTypeInfo.ReadOnly);
		}

		public void TestSecurityType_Mandatory()
		{
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(action.SecurityTypeInfo, MandatoryValidation.YouHaveNotEntered);

				action.EntryType = ExportEntryTypeList.Codes.ExportDeclaration;
				action.SecurityType = ExportSecurityTypeList.Codes.EXS;
				AssertNoMessageErrorContaining(action.SecurityTypeInfo, MandatoryValidation.YouHaveNotEntered);

				action.SecurityType = ZString.Empty;
				AssertHasMessageErrorContaining(action.SecurityTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestAlternativeEvidences()
		{
			CombineAssertions(() =>
			{
				AssertType<AlternativeEvidenceCollection>("Type", action.AlternativeEvidences);
				AssertEquals("IsRegisteredEditableChildObject", true, action.IsRegisteredEditableChildObject(action.AlternativeEvidences));
			});
		}

		public void TestAlternativeEvidences_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ReadOnly is true by default", true, action.AlternativeEvidences.ReadOnly);

				action.EntryType = ExportEntryTypeList.Codes.ExitToExport;
				AssertEquals("EntryType is 'EXT' and ReadOnly is false", false, action.AlternativeEvidences.ReadOnly);

				action.EntryType = ExportEntryTypeList.Codes.CancellationRequest;
				AssertEquals("EntryType isn't 'EXT' and ReadOnly is true", true, action.AlternativeEvidences.ReadOnly);
			});
		}

		public void TestEntryLines()
		{
			_ = entry.MergedLines.AddNew();
			_ = entry.MergedLines.AddNew();
			CombineAssertions(() =>
			{
				var entryLines = action.EntryLines;
				AssertType<ExportEntryLineCollection>("Type", entryLines);
				AssertEquals("Count", 2, entryLines.Count);
				AssertEquals("IsRegisteredEditableChildObject", true, action.IsRegisteredEditableChildObject(entryLines));
			});
		}

		public void TestValidateLocalReferenceNumber_UniqueForCurrentDeclaration()
		{
			declaration.CustomsEntryHeaders.AddNew();
			var parent = new ExportDeclarationMessageSendingActionParent(declaration);

			CombineAssertions(() =>
			{
				const string message = "LRN number must be unique.";

				var action1 = parent.SendingObjectsCollection[0];
				action1.LocalReferenceNumber = "12345";

				var action2 = parent.SendingObjectsCollection[1];
				action2.LocalReferenceNumber = "12345";
				AssertNoMessageError("action1.ShouldSend is false and action2.ShouldSend is false, no Message Error", action2.LocalReferenceNumberInfo, message);

				action2.ShouldSend = ZBool.True;
				AssertNoMessageError("action1.ShouldSend is false and action2.ShouldSend is true, no Message Error", action2.LocalReferenceNumberInfo, message);

				action1.ShouldSend = ZBool.True;
				action2.ValidateLocalReferenceNumber();
				AssertHasMessageError("action1.ShouldSend is true and action2.ShouldSend is true, has Message Error", action2.LocalReferenceNumberInfo, message);

				action2.LocalReferenceNumber = "ABCDE";
				AssertNoMessageError("LocalReferenceNumber is unique, no Message Error", action2.LocalReferenceNumberInfo, message);
			});
		}

		public void TestValidateLocalReferenceNumber_UniqueForAllExportDeclarations()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			Factory.Save();

			CombineAssertions(() =>
			{
				const string message = "LRN number must be unique. LRN 12345 was already used on Job B00001001.";

				EntryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
				declaration.JE_OA_Representative = orgAddress.PK;

				var otherEntryHeader = CreateOtherEntryHeader(orgAddress, string.Empty);
				Factory.Save();
				action.ShouldSend = ZBool.True;
				AssertNoMessageError("LocalReferenceNumber is empty", action.LocalReferenceNumberInfo, message);

				otherEntryHeader.LocalReferenceNumber = "ABCDE";
				Factory.Save();
				action.LocalReferenceNumber = "12345";
				AssertNoMessageError("UnMatched LocalReferenceNumber", action.LocalReferenceNumberInfo, message);

				otherEntryHeader.LocalReferenceNumber = "12345";
				foreach (var entryStatus in new ZString[] { "", "191", "192", "520" })
				{
					otherEntryHeader.CH_EntryStatus = entryStatus;
					Factory.Save();
					action.ValidateLocalReferenceNumber();
					AssertNoMessageError($"UnMatched EntryStatus: {entryStatus}", action.LocalReferenceNumberInfo, message);
				}

				otherEntryHeader.CH_EntryStatus = "501";
				var otherDeclaration = otherEntryHeader.Declaration;
				otherDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				Factory.Save();
				action.ValidateLocalReferenceNumber();
				AssertNoMessageError("UnMatched JE_MessageType", action.LocalReferenceNumberInfo, message);

				entry.LocalReferenceNumber = "12345";
				entry.CH_EntryStatus = "501";
				Factory.Save();
				action.ValidateLocalReferenceNumber();
				AssertEquals("UnMatched own Declaration", false, action.LocalReferenceNumberInfo.Notifications.Any(x => x.Message == $"LRN number must be unique. LRN 12345 was already used on Job {declaration.JE_DeclarationReference}."));

				otherDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				Factory.Save();
				action.ShouldSend = ZBool.False;
				AssertNoMessageError("ShouldSend is false", action.LocalReferenceNumberInfo, message);

				action.ShouldSend = ZBool.True;
				AssertHasMessageError("Matched other Declaration", action.LocalReferenceNumberInfo, message);
			});
		}

		public void TestValidateLocalReferenceNumber_UniqueForAllExportDeclarations_Constellation3rdDigitIs0()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var otherEntryHeader = CreateOtherEntryHeader(orgAddress);
			var otherDeclaration = otherEntryHeader.Declaration;
			otherDeclaration.JE_OA_DeclarantAddress = orgAddress.PK;
			Factory.Save();

			CombineAssertions(() =>
			{
				const string message = "LRN number must be unique. LRN 12345 was already used on Job B00001001.";

				EntryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0001;
				declaration.JE_OA_DeclarantAddress = orgAddress.PK;
				action.ShouldSend = ZBool.True;
				action.LocalReferenceNumber = "12345";
				AssertHasMessageError("Matched other Declaration with JE_OA_DeclarantAddress is equal to currentDeclarantAddressPK and EntryInstruction is Constellation3rdDigitIs0", action.LocalReferenceNumberInfo, message);

				otherDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				otherDeclaration.JE_OA_Representative = orgAddress.PK;
				otherEntryHeader.EntryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
				action.ValidateLocalReferenceNumber();
				AssertHasMessageError("Matched other Declaration with JE_OA_Representative is equal to currentDeclarantAddressPK and EntryInstruction is Constellation3rdDigitIs1", action.LocalReferenceNumberInfo, message);

				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				action.ValidateLocalReferenceNumber();
				AssertNoMessageError("currentDeclarantAddressPK is empty", action.LocalReferenceNumberInfo, message);
			});
		}

		public void TestValidateLocalReferenceNumber_UniqueForAllExportDeclarations_Constellation3rdDigitIs1()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var otherEntryHeader = CreateOtherEntryHeader(orgAddress);
			var otherDeclaration = otherEntryHeader.Declaration;
			Factory.Save();

			CombineAssertions(() =>
			{
				const string message = "LRN number must be unique. LRN 12345 was already used on Job B00001001.";

				EntryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
				declaration.JE_OA_Representative = orgAddress.PK;
				action.ShouldSend = ZBool.True;
				action.LocalReferenceNumber = "12345";
				AssertHasMessageError("Matched other Declaration with JE_OA_DeclarantAddress is equal to currentRepresentativePK and EntryInstruction is Constellation3rdDigitIs0", action.LocalReferenceNumberInfo, message);

				otherDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				otherDeclaration.JE_OA_Representative = orgAddress.PK;
				otherEntryHeader.EntryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
				action.ValidateLocalReferenceNumber();
				AssertHasMessageError("Matched other Declaration with JE_OA_Representative is equal to currentRepresentativePK and EntryInstruction is Constellation3rdDigitIs1", action.LocalReferenceNumberInfo, message);

				declaration.JE_OA_Representative = ZGuid.Empty;
				action.ValidateLocalReferenceNumber();
				AssertNoMessageError("currentRepresentativePK is empty", action.LocalReferenceNumberInfo, message);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExportEntryMessageSendingAction(entry);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			entry = declaration.CustomsEntryHeaders.AddNew();
			action = (ExportEntryMessageSendingAction)GetNewBusinessObject();
		}
		ExportEntryMessageSendingAction action;
		JobDeclaration declaration;
		CusEntryHeader entry;

		CusEntryInstruction EntryInstruction
		{
			get
			{
				if (entryInstruction == null)
				{
					entryInstruction = declaration.CustomsEntryInstructions.AddNew();
					entry.CH_CEI_Instruction = entryInstruction.PK;
				}
				return entryInstruction;
			}
		}
		CusEntryInstruction entryInstruction;

		void AssertValidateEntryType_EntryStatus(string entryType, string messageError, string[] validEntryStatuses, string typeTime = "", string typeProcedure = "")
		{
			action.EntryType = entryType;
			EntryInstruction.CEI_SubStyle = typeTime;
			EntryInstruction.CEI_Style = typeProcedure;
			entry.CH_EntryStatus = "100";
			CombineAssertions(() =>
			{
				action.ShouldSend = false;
				AssertNoEntryStatusMessageError("Invalid EntryStatus but ShouldSend = False", messageError);

				action.ShouldSend = true;
				AssertHasEntryStatusMessageError("Invalid EntryStatus and ShouldSend = True", messageError);

				foreach (var entryStatus in validEntryStatuses)
				{
					entry.CH_EntryStatus = entryStatus;
					AssertNoEntryStatusMessageError($"Valid EntryStatus '{entryStatus}'", messageError);
				}
			});
		}

		void AssertValidateEntryType_TypeProcedure(string entryType, string messageError, string[] validTypeProcedures, string entryStatus, string typeTime = "", string typeProcedure = "")
		{
			action.EntryType = entryType;
			EntryInstruction.CEI_SubStyle = typeTime;
			EntryInstruction.CEI_Style = typeProcedure;
			entry.CH_EntryStatus = entryStatus;
			CombineAssertions(() =>
			{
				action.ShouldSend = false;
				AssertNoEntryStatusMessageError("Invalid TypeProcedure but ShouldSend = False", messageError);

				action.ShouldSend = true;
				AssertHasEntryStatusMessageError("Invalid TypeProcedure and ShouldSend = True", messageError);

				foreach (var validTypeProcedure in validTypeProcedures)
				{
					EntryInstruction.CEI_Style = validTypeProcedure;
					AssertNoEntryStatusMessageError($"Valid TypeProcedure '{validTypeProcedure}'", messageError);
				}

				var invalidTypeProcedures = new ExportDeclarationTypeProcedureList().GetAllCodes().Except(validTypeProcedures).ToList();
				foreach (var invalidTypeProcedure in invalidTypeProcedures)
				{
					EntryInstruction.CEI_Style = invalidTypeProcedure;
					AssertHasEntryStatusMessageError($"Invalid TypeProcedure '{invalidTypeProcedure}'", messageError);
				}
			});
		}

		void AssertNoEntryStatusMessageError(string message, string messageError)
		{
			action.ValidateEntryType();
			AssertNoMessageError(message, action.EntryTypeInfo, messageError);
		}

		void AssertHasEntryStatusMessageError(string message, string messageError)
		{
			action.ValidateEntryType();
			AssertHasMessageError(message, action.EntryTypeInfo, messageError);
		}

		CusEntryHeader CreateOtherEntryHeader(OrgAddress orgAddress, string localReferenceNumber = "12345")
		{
			var otherDeclaration = Factory.New<JobDeclaration>();
			otherDeclaration.JE_DeclarationReference = "B00001001";
			otherDeclaration.JE_OA_DeclarantAddress = orgAddress.PK;
			var otherInstruction = otherDeclaration.CustomsEntryInstructions.AddNew();
			otherInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0001;
			var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
			otherEntryHeader.CH_CEI_Instruction = otherInstruction.PK;
			otherEntryHeader.CH_EntryStatus = "501";
			otherEntryHeader.LocalReferenceNumber = localReferenceNumber;

			return otherEntryHeader;
		}
	}
}
