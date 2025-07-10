using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class AddInfoCusEntryInstructionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_SimplifiedGrantAuthorization_InvalidCodeOrEmpty()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(instruction.CEI_SimplifiedGrantAuthorizationInfo, "X", SimplifiedGrantAuthorizationList.Codes.N);
		}

		public void TestCheckZG_SimplifiedGrantAuthorization_CompletionCustomsOffice()
		{
			const string message = "You should enter at least one Completion Customs Office.";
			var instruction = Factory.CreateInwardProcessingInstruction();

			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
			AssertNoMessageError("CEI_SimplifiedGrantAuthorization isn't 'J'", instruction.CEI_SimplifiedGrantAuthorizationInfo, message);

			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
			AssertHasMessageError("CEI_SimplifiedGrantAuthorization is 'J'", instruction.CEI_SimplifiedGrantAuthorizationInfo, message);

			instruction.CompletionCustomsOffices.AddNew();
			instruction.AddInfoValidation.ValidateZG_SimplifiedGrantAuthorization();
			AssertNoMessageError("Has Completion Customs Office", instruction.CEI_SimplifiedGrantAuthorizationInfo, message);
		}

		public void TestCheckZG_SimplifiedGrantAuthorization_InwardProcessingPlaces()
		{
			const string message = "You should enter at least one Inward Processing Place.";
			var instruction = Factory.CreateInwardProcessingInstruction();

			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
			AssertNoMessageError("CEI_SimplifiedGrantAuthorization isn't 'J'", instruction.CEI_SimplifiedGrantAuthorizationInfo, message);

			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
			AssertHasMessageError("CEI_SimplifiedGrantAuthorization is 'J'", instruction.CEI_SimplifiedGrantAuthorizationInfo, message);

			instruction.InwardProcessingPlaces.AddNew();
			instruction.AddInfoValidation.ValidateZG_SimplifiedGrantAuthorization();
			AssertNoMessageError("Has Inward Processing Place", instruction.CEI_SimplifiedGrantAuthorizationInfo, message);
		}

		public void TestCheckZG_SimplifiedGrantAuthorization_EnabledInwardProcessingIsFalse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.VZA;
			ValidationTestHelper.AssertFieldIsNotMandatory(instruction.CEI_SimplifiedGrantAuthorizationInfo);
		}

		public void TestSealCountValidationIsNotPerformed()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.Seals.AddNew("TEST");
			var effectiveSealNumbers = instruction.GetEffectiveSealNumbers();
			instruction.ZG_SealsCount = effectiveSealNumbers.Count - 1;
			Assert("Should be no errors", !instruction.ZG_SealsCountInfo.HasMessageErrors());
		}

		public void TestCheckZG_AuthorisationNumber_Mandatory_EAV()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.CEI_AuthorisationNumberInfo);

			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
			ValidationTestHelper.AssertFieldIsNotMandatory(instruction.CEI_AuthorisationNumberInfo);
		}

		public void TestCheckZG_AuthorisationNumber_Mandatory_AAVVAV()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.CEI_AuthorisationNumberInfo);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.CEI_AuthorisationNumberInfo);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			ValidationTestHelper.AssertFieldIsNotMandatory(instruction.CEI_AuthorisationNumberInfo);
		}

		public void TestCheckZG_AuthorisationNumber_ListValidation()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "EDIBRNWIS";
			organisation.CreateAuthorisationRecord(Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER1");
			Factory.Save();

			var instruction = Factory.CreateInwardProcessingInstruction();
			instruction.JobDeclaration.JE_OA_DeclarantAddress = organisation.Addresses.AddNew().PK;
			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(instruction.CEI_AuthorisationNumberInfo, "NUMBER2", "NUMBER1");
		}

		public void TestCheckZG_CompletionDuration()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;

			instruction.CEI_CompletionDuration = ZInt.Zero;
			AssertHasMessageErrorContaining(instruction.CEI_CompletionDurationInfo, MandatoryValidation.YouHaveNotEntered);

			instruction.CEI_CompletionDuration = -1;
			AssertHasMessageErrorContaining(instruction.CEI_CompletionDurationInfo, MandatoryValidation.ValueCannotBeNegative);

			instruction.CEI_CompletionDuration = 12;
			AssertNoNotifications(instruction.CEI_CompletionDurationInfo);
		}

		public void TestCheckZG_CriteriaType()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;

			instruction.CEI_CriteriaType = ZString.Empty;
			AssertHasMessageErrorContaining(instruction.CEI_CriteriaTypeInfo, MandatoryValidation.YouHaveNotEntered);

			instruction.CEI_CriteriaType = "X";
			AssertNoMessageErrorContaining(instruction.CEI_CriteriaTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(instruction.CEI_CriteriaTypeInfo, ListValidation.InvalidCodeMessageError);

			instruction.CEI_CriteriaType = CriteriaTypeList.Codes._0;
			AssertNoNotifications(instruction.CEI_CriteriaTypeInfo);
		}

		public void TestValidateZG_EarlyClearanceFlag()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			AssertEquals("Precondition", true, instruction.IsEarlyClearanceFlagApplicable);

			var info = instruction.CEI_EarlyClearanceFlagInfo;
			instruction.CEI_EarlyClearanceFlag = "Z";
			AssertHasMessageError(info, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

			instruction.CEI_EarlyClearanceFlag = EarlyClearanceFlagsList.Codes.J;
			AssertNoMessageError(info, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

			instruction.CEI_EarlyClearanceFlag = ZString.Empty;
			AssertNoMessageError(info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckZG_LocalClearanceDate_Mandatory_Import()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				foreach (var ceiStyle in new[] { ImportDeclarationTypeList.Codes.AAV, ImportDeclarationTypeList.Codes.AZ, ImportDeclarationTypeList.Codes.AZL })
				{
					entryInstruction.CEI_Style = ceiStyle;
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(entryInstruction.CEI_LocalClearanceDateInfo, LocalClearanceDateMandatoryMessageError, $"CEI_Style: {ceiStyle}");
				}
			});
		}

		public void TestCheckZG_LocalClearanceDate_Optional_Import()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			ValidationTestHelper.AssertFieldIsNotMandatory(entryInstruction.CEI_LocalClearanceDateInfo, LocalClearanceDateMandatoryMessageError);
		}

		public void TestCheckZG_LocalClearanceDate_Optional_Export()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				foreach (var ceiStyle in new[] { ImportDeclarationTypeList.Codes.AAV, ImportDeclarationTypeList.Codes.AZ, ImportDeclarationTypeList.Codes.AZL, ImportDeclarationTypeList.Codes.VAV })
				{
					entryInstruction.CEI_Style = ceiStyle;
					ValidationTestHelper.AssertFieldIsNotMandatory(entryInstruction.CEI_LocalClearanceDateInfo, LocalClearanceDateMandatoryMessageError, $"CEI_Style: {ceiStyle}");
				}
			});
		}

		[TestDate(2022, 02, 10)]
		public void TestCheckZG_LocalClearanceDate_MustNotBeGreaterThanCurrentDate_Import()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var propertyInfo = entryInstruction.CEI_LocalClearanceDateInfo;
			CombineAssertions(() =>
			{
				foreach (var ceiStyle in new[] { ImportDeclarationTypeList.Codes.AAV, ImportDeclarationTypeList.Codes.AZ, ImportDeclarationTypeList.Codes.AZL })
				{
					entryInstruction.CEI_Style = ceiStyle;
					entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2022, 02, 1);
					AssertNoMessageError($"CEI_Style: {ceiStyle}, yesterday", propertyInfo, LocalClearanceDateMustNotBeInFutureMessageError);

					entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2022, 02, 10);
					AssertNoMessageError($"CEI_Style: {ceiStyle}, today", propertyInfo, LocalClearanceDateMustNotBeInFutureMessageError);

					entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2022, 02, 11);
					AssertHasMessageError($"CEI_Style: {ceiStyle}, tomorrow (in future)", propertyInfo, LocalClearanceDateMustNotBeInFutureMessageError);
				}
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
				entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2022, 02, 11);
				AssertNoMessageError("CEI_Style: VAV, tomorrow (in future)", propertyInfo, LocalClearanceDateMustNotBeInFutureMessageError);
			});
		}

		[TestDate(2022, 02, 10)]
		public void TestCheckZG_LocalClearanceDate_MustNotBeGreaterThanCurrentDate_Export()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var propertyInfo = entryInstruction.CEI_LocalClearanceDateInfo;
			CombineAssertions(() =>
			{
				foreach (var ceiStyle in new[] { ImportDeclarationTypeList.Codes.AAV, ImportDeclarationTypeList.Codes.AZ, ImportDeclarationTypeList.Codes.AZL, ImportDeclarationTypeList.Codes.VAV })
				{
					entryInstruction.CEI_Style = ceiStyle;
					entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2022, 02, 1);
					AssertNoMessageError($"Export: CEI_Style: {ceiStyle}, yesterday", propertyInfo, LocalClearanceDateMustNotBeInFutureMessageError);

					entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2022, 02, 10);
					AssertNoMessageError($"Export: CEI_Style: {ceiStyle}, today", propertyInfo, LocalClearanceDateMustNotBeInFutureMessageError);

					entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2022, 02, 11);
					AssertNoMessageError($"Export: CEI_Style: {ceiStyle}, tomorrow", propertyInfo, LocalClearanceDateMustNotBeInFutureMessageError);
				}
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2022, 02, 11);
				AssertHasMessageError("Import: CEI_Style: AAV, tomorrow (in future)", propertyInfo, LocalClearanceDateMustNotBeInFutureMessageError);
			});
		}

		[TestDate(2022, 02, 10)]
		public void TestCheckZG_LocalClearanceDate_MustNotBeEarlierThan3MonthsBeforeCurrentDate_Import()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var propertyInfo = entryInstruction.CEI_LocalClearanceDateInfo;
			CombineAssertions(() =>
			{
				foreach (var ceiStyle in new[] { ImportDeclarationTypeList.Codes.AAV, ImportDeclarationTypeList.Codes.AZ })
				{
					entryInstruction.CEI_Style = ceiStyle;
					entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2021, 11, 10);
					AssertNoMessageError($"CEI_Style: {ceiStyle}, 3 months earlier than today", propertyInfo, LocalClearanceDateMustNotBeEarlierThan3MonthsBeforeCurrentDateMessageError);

					entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2021, 11, 09);
					AssertHasMessageError($"CEI_Style: {ceiStyle}, 3 months and 1 day earlier than today", propertyInfo, LocalClearanceDateMustNotBeEarlierThan3MonthsBeforeCurrentDateMessageError);
				}
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
				entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2021, 11, 09);
				AssertNoMessageError("CEI_Style: AZL, 3 months and 1 day earlier than today", propertyInfo, LocalClearanceDateMustNotBeEarlierThan3MonthsBeforeCurrentDateMessageError);
			});
		}

		[TestDate(2022, 02, 10)]
		public void TestCheckZG_LocalClearanceDate_MustNotBeEarlierThan3MonthsBeforeCurrentDate_Export()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var propertyInfo = entryInstruction.CEI_LocalClearanceDateInfo;
			CombineAssertions(() =>
			{
				foreach (var ceiStyle in new[] { ImportDeclarationTypeList.Codes.AAV, ImportDeclarationTypeList.Codes.AZ, ImportDeclarationTypeList.Codes.AZL })
				{
					entryInstruction.CEI_Style = ceiStyle;
					entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2021, 11, 10);
					AssertNoMessageError($"Export: CEI_Style: {ceiStyle}, 3 months earlier than today", propertyInfo, LocalClearanceDateMustNotBeEarlierThan3MonthsBeforeCurrentDateMessageError);

					entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2021, 11, 09);
					AssertNoMessageError($"Export: CEI_Style: {ceiStyle}, 3 months and 1 day earlier than today", propertyInfo, LocalClearanceDateMustNotBeEarlierThan3MonthsBeforeCurrentDateMessageError);
				}
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2021, 11, 09);
				AssertHasMessageError("Import: CEI_Style: AZL, 3 months and 1 day earlier than today", propertyInfo, LocalClearanceDateMustNotBeEarlierThan3MonthsBeforeCurrentDateMessageError);
			});
		}

		public void TestCheckZG_PartyConstellation_Import()
		{
			var targetInfo = instruction.ZG_PartyConstellationInfo;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				instruction.AddInfoValidation.ValidateZG_PartyConstellation();
				AssertNoMessageErrors("PartyConstellation empty", targetInfo);

				instruction.ZG_PartyConstellation = "XXX";
				AssertNoMessageErrors("Invalid PartyConstellation", targetInfo);
			});
		}

		public void TestCheckZG_PartyConstellation_Export()
		{
			var targetInfo = instruction.ZG_PartyConstellationInfo;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				instruction.ZG_PartyConstellation = "XXX";
				AssertHasMessageErrorContaining("Invalid PartyConstellation", targetInfo, ListValidation.InvalidCodeMessageError.ToString());

				instruction.ZG_PartyConstellation = ZString.Empty;
				AssertHasMessageError("PartyConstellation empty", targetInfo, "You have not entered a value.");

				instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0000;
				AssertNoMessageErrors("Valid PartyConstellation", targetInfo);
			});
		}

		public void TestCheckZG_PartyConstellation_SupplierRequired()
		{
			const string errorMessage = "For this Party Constellation a Supplier must be entered on the Declaration Tab.";
			CombineAssertions(() =>
			{
				declaration.JE_OH_Supplier = ZGuid.Empty;
				instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0000;
				AssertNoRowMessageError("Party Constellation 0000, no Supplier", instruction, errorMessage);

				instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._1000;
				AssertHasRowMessageError("Party Constellation 1000, no Supplier", instruction, errorMessage);

				declaration.JE_OH_Supplier = ZGuid.BrettsGuid;
				instruction.AddInfoValidation.ValidateZG_PartyConstellation();
				AssertNoRowMessageError("Party Constellation 1000, has Supplier", instruction, errorMessage);
			});
		}

		public void TestCheckZG_ExitDate()
		{
			const string message = "The Exit Date must not be in the future.";
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				instruction.ZG_ExitDate = ZDateTime.Today.AddDays(1);
				AssertHasMessageError("ZG_ExitDate is in the future", instruction.ZG_ExitDateInfo, message);

				instruction.ZG_ExitDate = ZDateTime.Today;
				AssertNoMessageError("ZG_ExitDate isn't in the future", instruction.ZG_ExitDateInfo, message);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				instruction.ZG_ExitDate = ZDateTime.Today.AddDays(1);
				AssertNoMessageError("JE_MessageType is 'IMP'", instruction.ZG_ExitDateInfo, message);
			});
		}

		public void TestCheckZG_ExitDate_Mandatory()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._10;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.ZG_ExitDateInfo, MandatoryValidation.YouHaveNotEntered, "CEI_SubStyle 1st digit is '1'");

				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
				ValidationTestHelper.AssertFieldIsNotMandatory(instruction.ZG_ExitDateInfo, MandatoryValidation.YouHaveNotEntered, "CEI_SubStyle 1st digit is not '1'");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
		}

		CusEntryInstruction instruction;
		JobDeclaration declaration;

		const string LocalClearanceDateMandatoryMessageError = "Please enter a Local Clearance Date.";
		const string LocalClearanceDateMustNotBeInFutureMessageError = "Local Clearance Date cannot be in the future.";
		const string LocalClearanceDateMustNotBeEarlierThan3MonthsBeforeCurrentDateMessageError = "Local Clearance Date must not be more than three months in the past.";
	}
}
