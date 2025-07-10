using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(MoveInDestinationValidation))]
	sealed class MoveInDestinationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var validCode = Factory.CreateJapanBondedAreaCode();

			var expectedMessage = "Destination must be unique.";
			var moveInDestination2 = CusEntryInstruction.MoveInDestinationInfos.AddNew();

			MoveInDestination.CSI_Code = moveInDestination2.CSI_Code = "1Y";
			MoveInDestination.Validation.ValidateCSI_Code();
			moveInDestination2.Validation.ValidateCSI_Code();
			AssertHasMessageError(MoveInDestination.CSI_CodeInfo, expectedMessage);
			AssertHasMessageError(moveInDestination2.CSI_CodeInfo, expectedMessage);

			var expectedMessage2 = "Destinations in the grid must be from the same customs.";
			MoveInDestination.CSI_Code = "2Y";
			MoveInDestination.Validation.ValidateCSI_Code();
			moveInDestination2.Validation.ValidateCSI_Code();
			AssertHasMessageError(MoveInDestination.CSI_CodeInfo, expectedMessage2);
			AssertHasMessageError(moveInDestination2.CSI_CodeInfo, expectedMessage2);

			ValidationTestHelper.AssertInvalidCodeMessageError(MoveInDestination.CSI_CodeInfo, "XXX", validCode);

			MoveInDestination.Validation.ValidateCSI_Code();
			AssertNoMessageErrorContaining(MoveInDestination.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			CusEntryInstruction.SetupECRTestingContext();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(MoveInDestination.CSI_CodeInfo);
		}

		public void TestCheckCSI_DateOfIssue()
		{
			JobDeclaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
			JobDeclaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(MoveInDestination.CSI_DateOfIssueInfo);

			var expectedMessage = "Move-In Date must be within 13 days from today.";
			AssertEntityValidation(MoveInDestination)
				.WhenProperty(x => x.Parent.ExportControlNumber, Is.EqualTo(ZString.Empty))
				.WhenProperty(x => x.CSI_DateOfIssue, Is.EqualToAnyOf(ZDateTime.Today.AddDays(14), ZDateTime.Today.AddDays(-1)))
				.ShouldCheckThat(x => x.CSI_DateOfIssueInfo, Has.MessageErrorContaining(expectedMessage));

			AssertEntityValidation(MoveInDestination)
				.WhenProperty(x => x.Parent.ExportControlNumber, Is.EqualTo("123456"))
				.WhenProperty(x => x.CSI_DateOfIssue, Is.EqualToAnyOf(ZDateTime.Today.AddDays(14), ZDateTime.Today.AddDays(-1)))
				.ShouldCheckThat(x => x.CSI_DateOfIssueInfo, Has.NoMessageErrorContaining(expectedMessage));

			var messageSendingContext = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.ECR };
			using (JobDeclaration.SetCurrentMessageSendingContext(messageSendingContext))
			{
				AssertEntityValidation(MoveInDestination)
				.WhenProperty(x => x.Parent.ExportControlNumber, Is.EqualTo("123456"))
				.WhenProperty(x => x.CSI_DateOfIssue, Is.EqualToAnyOf(ZDateTime.Today.AddDays(14), ZDateTime.Today.AddDays(-1)))
				.ShouldCheckThat(x => x.CSI_DateOfIssueInfo, Has.MessageErrorContaining(expectedMessage));
			}

			messageSendingContext.ProcedureCode = JPProcedureCodeList.Codes.EDA;
			using (JobDeclaration.SetCurrentMessageSendingContext(messageSendingContext))
			{
				AssertEntityValidation(MoveInDestination)
				.WhenProperty(x => x.Parent.ExportControlNumber, Is.EqualTo("123456"))
				.WhenProperty(x => x.CSI_DateOfIssue, Is.EqualToAnyOf(ZDateTime.Today.AddDays(14), ZDateTime.Today.AddDays(-1)))
				.ShouldCheckThat(x => x.CSI_DateOfIssueInfo, Has.NoMessageErrorContaining(expectedMessage));
			}
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			var validCode = Factory.CreateJapanBondedAreaCode();
			ValidationTestHelper.AssertInvalidCodeMessageError(MoveInDestination.CSI_ReferenceNumberInfo, "XXX", validCode);
		}

		public void TestCheckCSI_Quantity()
		{
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(MoveInDestination.CSI_QuantityInfo);

			MoveInDestination.Validation.ValidateCSI_Quantity();
			AssertNoMessageErrorContaining(MoveInDestination.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);

			CusEntryInstruction.SetupECRTestingContext();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(MoveInDestination.CSI_QuantityInfo);

			var expectedMessage = "The total quantity (Move in) must not exceed the cargo quantity.";
			CusEntryInstruction.CEI_CargoQuantity = 5;
			MoveInDestination.CSI_Quantity = 6;
			AssertHasMessageError(MoveInDestination.CSI_QuantityInfo, expectedMessage);

			MoveInDestination.CSI_Quantity = 5;
			AssertNoMessageError(MoveInDestination.CSI_QuantityInfo, expectedMessage);
		}

		public void TestCheckCSI_Quantity2()
		{
			var expectedMessage = "is too large, the maximum value allowed for Weight is 999,999.999.";

			MoveInDestination.CSI_Quantity2 = 1000000;
			AssertHasErrorContaining(MoveInDestination.CSI_Quantity2Info, expectedMessage);

			MoveInDestination.CSI_Quantity2 = 999999.999;
			AssertNoErrorContaining(MoveInDestination.CSI_Quantity2Info, expectedMessage);

			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(MoveInDestination.CSI_Quantity2Info);

			MoveInDestination.CSI_Quantity2 = 0;
			AssertNoMessageErrorContaining(MoveInDestination.CSI_Quantity2Info, MandatoryValidation.YouHaveNotEntered);

			CusEntryInstruction.SetupECRTestingContext();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(MoveInDestination.CSI_Quantity2Info);

			expectedMessage = "The total weight (Move in) must not exceed the cargo weight.";
			CusEntryInstruction.CEI_GrossWeight = 5;
			CusEntryInstruction.CEI_GrossWeightUnit = Weight.Kilograms;
			MoveInDestination.CSI_Quantity2 = 6;
			AssertHasMessageError(MoveInDestination.CSI_Quantity2Info, expectedMessage);

			MoveInDestination.CSI_Quantity2 = 5;
			AssertNoMessageError(MoveInDestination.CSI_Quantity2Info, expectedMessage);
		}

		public void TestCheckCSI_Quantity3()
		{
			var expectedMessage = "is too large, the maximum value allowed for Volume is 999,999.999.";

			MoveInDestination.CSI_Quantity3 = 1000000;
			AssertHasErrorContaining(MoveInDestination.CSI_Quantity3Info, expectedMessage);

			MoveInDestination.CSI_Quantity3 = 999999.999;
			AssertNoErrorContaining(MoveInDestination.CSI_Quantity3Info, expectedMessage);

			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(MoveInDestination.CSI_Quantity3Info);

			expectedMessage = "The total volume (Move in) must not exceed the cargo volume.";
			CusEntryInstruction.CEI_Volume = 5;
			CusEntryInstruction.CEI_VolumeUnit = VolumeList.Codes.BoardFoot;
			MoveInDestination.CSI_Quantity3 = 6;
			AssertHasMessageError(MoveInDestination.CSI_Quantity3Info, expectedMessage);

			MoveInDestination.CSI_Quantity3 = 5;
			AssertNoMessageError(MoveInDestination.CSI_Quantity3Info, expectedMessage);
		}

		public void TestCheckCSI_Description()
		{
			MoveInDestination.Validation.ValidateCSI_Description();
			AssertNoMessageErrorContaining(MoveInDestination.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			CusEntryInstruction.SetupECRTestingContext();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(MoveInDestination.CSI_DescriptionInfo);
		}

		MoveInDestination MoveInDestination => moveInDestination ??= CusEntryInstruction.MoveInDestinationInfos.AddNew();
		MoveInDestination moveInDestination;

		CusEntryInstruction CusEntryInstruction => cusEntryInstruction ??= JobDeclaration.CustomsEntryInstructions.AddNew();
		CusEntryInstruction cusEntryInstruction;

		JobDeclaration JobDeclaration => jobDeclaration ??= Factory.New<JobDeclaration>();
		JobDeclaration jobDeclaration;
	}
}
