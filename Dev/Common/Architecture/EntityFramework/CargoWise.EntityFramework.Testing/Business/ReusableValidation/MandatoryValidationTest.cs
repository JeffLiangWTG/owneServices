using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework.Testing
{
	sealed class MandatoryValidationTest : TestCaseWithDummyForValidationTesting
	{
		public void GetErrorFieldFromPropertyInfo()
		{
			AssertEquals("Description", MandatoryValidation.GetErrorFieldFromProperyInfo(Dummy.Z0_DescriptionInfo));
			Dummy.Z0_DescriptionInfo.HumanReadableName = ZString.Empty;
			AssertEquals("value", MandatoryValidation.GetErrorFieldFromProperyInfo(Dummy.Z0_DescriptionInfo));
		}

		public void TestCheckEntered()
		{
			AssertEquals(false, Dummy.HasErrors);
			Dummy.Z0_Description = "rarar";
			MandatoryValidation.CheckEntered(Dummy.Z0_DescriptionInfo);
			AssertEquals(false, Dummy.HasErrors);

			AssertEquals(false, Dummy.HasErrors);
			Dummy.Z0_Description = "";
			MandatoryValidation.CheckEntered(Dummy.Z0_DescriptionInfo);
			AssertEquals(true, Dummy.HasErrors);
			AssertEquals("Expecting Error message.", "Please enter a Description.", Dummy.Z0_DescriptionInfo.GetErrors().GetFirstMessage());

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Dummy.Z0_DescriptionInfo, "Ararar");
			AssertEquals(true, Dummy.HasErrors);
			AssertEquals("Expecting Error message.", "Please enter an Ararar.", Dummy.Z0_DescriptionInfo.GetErrors().GetFirstMessage());

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Dummy.Z0_DescriptionInfo, "Ararar", "Prefix for test:");
			AssertEquals(true, Dummy.HasErrors);
			AssertEquals("Expecting Error message.", "Prefix for test:Please enter an Ararar.", Dummy.Z0_DescriptionInfo.GetErrors().GetFirstMessage());
		}

		public void TestCheckNotEntered()
		{
			AssertEquals(false, Dummy.HasErrors);
			Dummy.Z0_Description = "";
			MandatoryValidation.CheckNotEntered(Dummy.Z0_DescriptionInfo);
			AssertEquals(false, Dummy.HasErrors);

			AssertEquals(false, Dummy.HasErrors);
			Dummy.Z0_Description = "rarar";
			MandatoryValidation.CheckNotEntered(Dummy.Z0_DescriptionInfo);
			AssertEquals(true, Dummy.HasErrors);
			AssertEquals("Expecting Error message.", "Please do not enter a Description.", Dummy.Z0_DescriptionInfo.GetErrors().GetFirstMessage());

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotEntered(Dummy.Z0_DescriptionInfo, "Ararar");
			AssertEquals(true, Dummy.HasErrors);
			AssertEquals("Expecting Error message.", "Please do not enter an Ararar.", Dummy.Z0_DescriptionInfo.GetErrors().GetFirstMessage());
		}

		public void TestWarnIfNotEntered()
		{
			Dummy.Z0_Description = "rarar";
			MandatoryValidation.WarnIfNotEntered(Dummy.Z0_DescriptionInfo);
			AssertNoWarningContaining(Dummy.Z0_DescriptionInfo, "You have not entered a Description.");

			Dummy.Z0_Description = "";
			MandatoryValidation.WarnIfNotEntered(Dummy.Z0_DescriptionInfo);
			AssertHasWarning(Dummy.Z0_DescriptionInfo, "You have not entered a Description.");

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			Dummy.Z0_Description = "rarar";
			MandatoryValidation.WarnIfNotEntered(Dummy.Z0_DescriptionInfo, "Ararar");
			AssertNoWarningContaining(Dummy.Z0_DescriptionInfo, "You have not entered an Ararar.");

			Dummy.Z0_Description = "";
			MandatoryValidation.WarnIfNotEntered(Dummy.Z0_DescriptionInfo, "Ararar");
			AssertHasWarning(Dummy.Z0_DescriptionInfo, "You have not entered an Ararar.");
		}

		public void TestWarnIfNotEntered_WithWarningMessagePrefix()
		{
			ZString warningMessage = "TEST: You have not entered an Ararar.";
			Dummy.Z0_Description = "rarar";
			MandatoryValidationForTest.WarnIfNotEntered(Dummy.Z0_DescriptionInfo, "Ararar", "TEST: ");
			AssertNoWarning(Dummy.Z0_DescriptionInfo, warningMessage);

			Dummy.Z0_Description = "";
			MandatoryValidationForTest.WarnIfNotEntered(Dummy.Z0_DescriptionInfo, "Ararar", "TEST: ");
			AssertHasWarning(Dummy.Z0_DescriptionInfo, warningMessage);
		}

		public void TestWarnIfIsEntered()
		{
			Dummy.Z0_Description = "";
			MandatoryValidationForTest.WarnIfIsEntered(Dummy.Z0_DescriptionInfo);
			AssertNoWarning(Dummy.Z0_DescriptionInfo, "Please do not enter a Description.");

			Dummy.Z0_Description = "rarar";
			MandatoryValidationForTest.WarnIfIsEntered(Dummy.Z0_DescriptionInfo);
			AssertHasWarning(Dummy.Z0_DescriptionInfo, "Please do not enter a Description.");

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			Dummy.Z0_Description = "";
			MandatoryValidationForTest.WarnIfIsEntered(Dummy.Z0_DescriptionInfo, "Ararar");
			AssertNoWarning(Dummy.Z0_DescriptionInfo, "Please do not enter an Ararar.");

			Dummy.Z0_Description = "rarar";
			MandatoryValidationForTest.WarnIfIsEntered(Dummy.Z0_DescriptionInfo, "Ararar");
			AssertHasWarning(Dummy.Z0_DescriptionInfo, "Please do not enter an Ararar.");

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			Dummy.Z0_Description = "";
			MandatoryValidationForTest.WarnIfIsEntered(Dummy.Z0_DescriptionInfo, "Ararar", "TEST: ");
			AssertNoWarning(Dummy.Z0_DescriptionInfo, "TEST: Please do not enter an Ararar.");

			Dummy.Z0_Description = "rarar";
			MandatoryValidationForTest.WarnIfIsEntered(Dummy.Z0_DescriptionInfo, "Ararar", "TEST: ");
			AssertHasWarning(Dummy.Z0_DescriptionInfo, "TEST: Please do not enter an Ararar.");
		}

		public void TestMessageErrorIfNotEntered()
		{
			AssertEquals(false, Dummy.HasMessageErrors);
			Dummy.Z0_Description = "rarar";
			MandatoryValidation.MessageErrorIfNotEntered(Dummy.Z0_DescriptionInfo);
			AssertEquals(false, Dummy.HasMessageErrors);

			AssertEquals(false, Dummy.HasMessageErrors);
			Dummy.Z0_Description = "";
			MandatoryValidation.MessageErrorIfNotEntered(Dummy.Z0_DescriptionInfo);
			AssertEquals(true, Dummy.HasMessageErrors);
			AssertEquals("Expecting Message Error.", "You have not entered a Description.", Dummy.Z0_DescriptionInfo.GetMessageErrors().GetFirstMessage());

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			MandatoryValidation.MessageErrorIfNotEntered(Dummy.Z0_DescriptionInfo, "Ararar");
			AssertEquals(true, Dummy.HasMessageErrors);
			AssertEquals("Expecting Message Error.", "You have not entered an Ararar.", Dummy.Z0_DescriptionInfo.GetMessageErrors().GetFirstMessage());
		}

		public void TestMessageErrorIfNotEntered_WithPrefix()
		{
			Dummy.Z0_Description = "blah";
			MandatoryValidation.MessageErrorIfNotEntered(Dummy.Z0_DescriptionInfo, messagePrefix: "[Rule123] ");
			AssertEquals(false, Dummy.HasMessageErrors);

			Dummy.Z0_Description = "";
			MandatoryValidation.MessageErrorIfNotEntered(Dummy.Z0_DescriptionInfo, messagePrefix: "[Rule123] ");
			AssertEquals(true, Dummy.HasMessageErrors);
			AssertEquals("Expecting Message Error.", "[Rule123] You have not entered a Description.", Dummy.Z0_DescriptionInfo.GetMessageErrors().GetFirstMessage());

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			MandatoryValidation.MessageErrorIfNotEntered(Dummy.Z0_DescriptionInfo, propertyDescription: "AlternativeDescription", messagePrefix: "[Rule456] ");
			AssertEquals(true, Dummy.HasMessageErrors);
			AssertEquals("Expecting Message Error.", "[Rule456] You have not entered an AlternativeDescription.", Dummy.Z0_DescriptionInfo.GetMessageErrors().GetFirstMessage());
		}

		public void TestMessageErrorIfNotEntered_WithNullPtyDescAndOrMsgPrefix()
		{
			Dummy.Z0_Description = "";
			MandatoryValidation.MessageErrorIfNotEntered(Dummy.Z0_DescriptionInfo, propertyDescription: null, messagePrefix: null);
			AssertEquals("Expecting Message Error.", "You have not entered a Description.", Dummy.Z0_DescriptionInfo.GetMessageErrors().GetFirstMessage());

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			MandatoryValidation.MessageErrorIfNotEntered(Dummy.Z0_DescriptionInfo, propertyDescription: "Better Description", messagePrefix: null);
			AssertEquals("Expecting Message Error.", "You have not entered a Better Description.", Dummy.Z0_DescriptionInfo.GetMessageErrors().GetFirstMessage());

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			MandatoryValidation.MessageErrorIfNotEntered(Dummy.Z0_DescriptionInfo, propertyDescription: null, messagePrefix: "Test Prefix - ");
			AssertEquals("Expecting Message Error.", "Test Prefix - You have not entered a Description.", Dummy.Z0_DescriptionInfo.GetMessageErrors().GetFirstMessage());
		}

		public void TestMessageErrorIfIsEntered()
		{
			Dummy.Z0_Description = "";
			MandatoryValidation.MessageErrorIfIsEntered(Dummy.Z0_DescriptionInfo);
			AssertNoMessageError(Dummy.Z0_DescriptionInfo, "Please do not enter a Description.");

			Dummy.Z0_Description = "rarar";
			MandatoryValidation.MessageErrorIfIsEntered(Dummy.Z0_DescriptionInfo);
			AssertHasMessageError(Dummy.Z0_DescriptionInfo, "Please do not enter a Description.");

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			Dummy.Z0_Description = "";
			MandatoryValidation.MessageErrorIfIsEntered(Dummy.Z0_DescriptionInfo, "Ararar");
			AssertNoMessageError(Dummy.Z0_DescriptionInfo, "Please do not enter an Ararar.");

			Dummy.Z0_Description = "rarar";
			MandatoryValidation.MessageErrorIfIsEntered(Dummy.Z0_DescriptionInfo, "Ararar");
			AssertHasMessageError(Dummy.Z0_DescriptionInfo, "Please do not enter an Ararar.");
		}

		public void TestCheckUnitEntered()
		{
			AssertEquals(false, Dummy.HasErrors);
			Dummy.Z0_Description = "";
			Dummy.Z0_Number = 0;
			MandatoryValidation.CheckUnitEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo);
			AssertEquals(false, Dummy.HasErrors);

			AssertEquals(false, Dummy.HasErrors);
			Dummy.Z0_Description = "rarar";
			Dummy.Z0_Number = 4;
			MandatoryValidation.CheckUnitEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo);
			AssertEquals(false, Dummy.HasErrors);

			AssertEquals(false, Dummy.HasErrors);
			Dummy.Z0_Description = "";
			Dummy.Z0_Number = 4;
			MandatoryValidation.CheckUnitEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo);
			AssertEquals(true, Dummy.HasErrors);
			AssertEquals("Expecting Error message.", "Please enter a Description.", Dummy.Z0_DescriptionInfo.GetErrors().GetFirstMessage());

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckUnitEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo, "Ararar");
			AssertEquals(true, Dummy.HasErrors);
			AssertEquals("Expecting Error message.", "Please enter an Ararar.", Dummy.Z0_DescriptionInfo.GetErrors().GetFirstMessage());
		}

		public void TestWarnIfUnitNotEntered()
		{
			Dummy.Z0_Description = "";
			Dummy.Z0_Number = 0;
			MandatoryValidation.WarnIfUnitNotEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo);
			AssertNoWarning(Dummy.Z0_DescriptionInfo, "You have not entered a Description.");

			Dummy.Z0_Number = 4;
			MandatoryValidation.WarnIfUnitNotEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo);
			AssertHasWarning(Dummy.Z0_DescriptionInfo, "You have not entered a Description.");

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			Dummy.Z0_Description = "bla";
			MandatoryValidation.WarnIfUnitNotEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo);
			AssertNoWarning(Dummy.Z0_DescriptionInfo, "You have not entered a Description.");

			Dummy.Z0_Description = "";
			MandatoryValidation.WarnIfUnitNotEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo, "Ararar");
			AssertHasWarning(Dummy.Z0_DescriptionInfo, "You have not entered an Ararar.");

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			Dummy.Z0_Description = "bla";
			MandatoryValidation.WarnIfUnitNotEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo, "Ararar");
			AssertNoWarning(Dummy.Z0_DescriptionInfo, "You have not entered a Description.");
		}

		public void TestMessageErrorIfUnitNotEntered()
		{
			Dummy.Z0_Description = "";
			Dummy.Z0_Number = 0;
			MandatoryValidation.MessageErrorIfUnitNotEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo);
			AssertNoMessageError(Dummy.Z0_DescriptionInfo, "You have not entered a Description.");

			Dummy.Z0_Number = 4;
			MandatoryValidation.MessageErrorIfUnitNotEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo);
			AssertHasMessageError(Dummy.Z0_DescriptionInfo, "You have not entered a Description.");

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			Dummy.Z0_Description = "bla";
			MandatoryValidation.MessageErrorIfUnitNotEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo);
			AssertNoMessageError(Dummy.Z0_DescriptionInfo, "You have not entered a Description.");

			Dummy.Z0_Description = "";
			MandatoryValidation.MessageErrorIfUnitNotEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo);
			AssertHasMessageError(Dummy.Z0_DescriptionInfo, "You have not entered a Description.");

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			Dummy.Z0_Description = "";
			Dummy.Z0_Number = 0;
			MandatoryValidation.MessageErrorIfUnitNotEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo, "Ararar");
			AssertNoMessageError(Dummy.Z0_DescriptionInfo, "You have not entered an Ararar.");

			Dummy.Z0_Number = 4;
			MandatoryValidation.MessageErrorIfUnitNotEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo, "Ararar");
			AssertHasMessageError(Dummy.Z0_DescriptionInfo, "You have not entered an Ararar.");

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			Dummy.Z0_Description = "bla";
			MandatoryValidation.MessageErrorIfUnitNotEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo, "Ararar");
			AssertNoMessageError(Dummy.Z0_DescriptionInfo, "You have not entered an Ararar.");

			Dummy.Z0_Description = "";
			MandatoryValidation.MessageErrorIfUnitNotEntered(Dummy.Z0_DescriptionInfo, Dummy.Z0_NumberInfo, "Ararar");
			AssertHasMessageError(Dummy.Z0_DescriptionInfo, "You have not entered an Ararar.");
		}

		public void TestCheckNotNegative()
		{
			Dummy.Z0_Number = 5;
			MandatoryValidation.CheckNotNegative(Dummy.Z0_NumberInfo);
			AssertNoError(Dummy.Z0_NumberInfo, "Number cannot be negative.");

			Dummy.Z0_Number = -1;
			MandatoryValidation.CheckNotNegative(Dummy.Z0_NumberInfo);
			AssertHasError(Dummy.Z0_NumberInfo, "Number cannot be negative.");

			Dummy.Z0_NumberInfo.ClearAllNotifications();
			Dummy.Z0_Number = 5;
			MandatoryValidation.CheckNotNegative(Dummy.Z0_NumberInfo, "Dummy Number");
			AssertNoError(Dummy.Z0_NumberInfo, "Dummy Number cannot be negative.");

			Dummy.Z0_Number = -1;
			MandatoryValidation.CheckNotNegative(Dummy.Z0_NumberInfo, "Dummy Number");
			AssertHasError(Dummy.Z0_NumberInfo, "Dummy Number cannot be negative.");
		}

		public void TestWarnIfIsNegative()
		{
			Dummy.Z0_Number = 5;
			MandatoryValidation.WarnIfIsNegative(Dummy.Z0_NumberInfo);
			AssertNoWarning(Dummy.Z0_NumberInfo, "Number cannot be negative.");

			Dummy.Z0_Number = -1;
			MandatoryValidation.WarnIfIsNegative(Dummy.Z0_NumberInfo);
			AssertHasWarning(Dummy.Z0_NumberInfo, "Number cannot be negative.");

			Dummy.Z0_NumberInfo.ClearAllNotifications();
			Dummy.Z0_Number = 5;
			MandatoryValidation.WarnIfIsNegative(Dummy.Z0_NumberInfo, "Dummy Number");
			AssertNoWarning(Dummy.Z0_NumberInfo, "Dummy Number cannot be negative.");

			Dummy.Z0_Number = -1;
			MandatoryValidation.WarnIfIsNegative(Dummy.Z0_NumberInfo, "Dummy Number");
			AssertHasWarning(Dummy.Z0_NumberInfo, "Dummy Number cannot be negative.");
		}

		public void TestMessageErrorIfIsNegative()
		{
			Dummy.Z0_Number = 5;
			MandatoryValidation.MessageErrorIfIsNegative(Dummy.Z0_NumberInfo);
			AssertNoMessageError(Dummy.Z0_NumberInfo, "Number cannot be negative.");

			Dummy.Z0_Number = -1;
			MandatoryValidation.MessageErrorIfIsNegative(Dummy.Z0_NumberInfo);
			AssertHasMessageError(Dummy.Z0_NumberInfo, "Number cannot be negative.");

			Dummy.Z0_NumberInfo.ClearAllNotifications();
			Dummy.Z0_Number = 5;
			MandatoryValidation.MessageErrorIfIsNegative(Dummy.Z0_NumberInfo, "Dummy Number");
			AssertNoMessageError(Dummy.Z0_NumberInfo, "Dummy Number cannot be negative.");

			Dummy.Z0_Number = -1;
			MandatoryValidation.MessageErrorIfIsNegative(Dummy.Z0_NumberInfo, "Dummy Number");
			AssertHasMessageError(Dummy.Z0_NumberInfo, "Dummy Number cannot be negative.");
		}

		public void TestCheckNotZero()
		{
			Dummy.Z0_Number = 5;
			MandatoryValidation.CheckNotZero(Dummy.Z0_NumberInfo);
			AssertNoError(Dummy.Z0_NumberInfo, "Number cannot be zero.");

			Dummy.Z0_Number = 0;
			MandatoryValidation.CheckNotZero(Dummy.Z0_NumberInfo);
			AssertHasError(Dummy.Z0_NumberInfo, "Number cannot be zero.");

			Dummy.Z0_NumberInfo.ClearAllNotifications();
			Dummy.Z0_Number = 5;
			MandatoryValidation.CheckNotZero(Dummy.Z0_NumberInfo, "Dummy Number");
			AssertNoError(Dummy.Z0_NumberInfo, "Dummy Number cannot be zero.");

			Dummy.Z0_Number = 0;
			MandatoryValidation.CheckNotZero(Dummy.Z0_NumberInfo, "Dummy Number");
			AssertHasError(Dummy.Z0_NumberInfo, "Dummy Number cannot be zero.");
		}

		public void TestWarnIfIsZero()
		{
			Dummy.Z0_Number = 5;
			MandatoryValidation.WarnIfIsZero(Dummy.Z0_NumberInfo);
			AssertNoWarning(Dummy.Z0_NumberInfo, "Number cannot be zero.");

			Dummy.Z0_Number = 0;
			MandatoryValidation.WarnIfIsZero(Dummy.Z0_NumberInfo);
			AssertHasWarning(Dummy.Z0_NumberInfo, "Number cannot be zero.");

			Dummy.Z0_NumberInfo.ClearAllNotifications();
			Dummy.Z0_Number = 5;
			MandatoryValidation.WarnIfIsZero(Dummy.Z0_NumberInfo, "Dummy Number");
			AssertNoWarning(Dummy.Z0_NumberInfo, "Dummy Number cannot be zero.");

			Dummy.Z0_Number = 0;
			MandatoryValidation.WarnIfIsZero(Dummy.Z0_NumberInfo, "Dummy Number");
			AssertHasWarning(Dummy.Z0_NumberInfo, "Dummy Number cannot be zero.");
		}

		public void TestMessageErrorIfIsZero()
		{
			Dummy.Z0_Number = 5;
			MandatoryValidation.MessageErrorIfIsZero(Dummy.Z0_NumberInfo);
			AssertNoMessageError(Dummy.Z0_NumberInfo, "Number cannot be zero.");

			Dummy.Z0_Number = 0;
			MandatoryValidation.MessageErrorIfIsZero(Dummy.Z0_NumberInfo);
			AssertHasMessageError(Dummy.Z0_NumberInfo, "Number cannot be zero.");

			Dummy.Z0_NumberInfo.ClearAllNotifications();
			Dummy.Z0_Number = 5;
			MandatoryValidation.MessageErrorIfIsZero(Dummy.Z0_NumberInfo, "Dummy Number");
			AssertNoMessageError(Dummy.Z0_NumberInfo, "Dummy Number cannot be zero.");

			Dummy.Z0_Number = 0;
			MandatoryValidation.MessageErrorIfIsZero(Dummy.Z0_NumberInfo, "Dummy Number");
			AssertHasMessageError(Dummy.Z0_NumberInfo, "Dummy Number cannot be zero.");
		}

		public void TestAddNotificationIfIsNegative_Long()
		{
			const string errorMessage = "value cannot be negative.";
			CombineAssertions(() =>
			{
				Dummy.Z0_Long = -12345;
				MandatoryValidation.CheckNotNegative(Dummy.Z0_LongInfo);
				AssertHasError("Negative", Dummy.Z0_LongInfo, errorMessage);

				Dummy.Z0_Long = 12345;
				MandatoryValidation.CheckNotNegative(Dummy.Z0_LongInfo);
				AssertNoError("Positive", Dummy.Z0_LongInfo, errorMessage);
			});
		}

		public void TestAddNotificationIfIsZero_Long()
		{
			const string errorMessage = "value cannot be zero.";
			CombineAssertions(() =>
			{
				Dummy.Z0_Long = 0;
				MandatoryValidation.CheckNotZero(Dummy.Z0_LongInfo);
				AssertHasError("Zero", Dummy.Z0_LongInfo, errorMessage);

				Dummy.Z0_Long = 123423;
				MandatoryValidation.CheckNotZero(Dummy.Z0_LongInfo);
				AssertNoError("Not Zero", Dummy.Z0_LongInfo, errorMessage);
			});
		}

		public void TestAddYouHaveNotEnteredMessage()
		{
			var dummy = Factory.New<DummyBusinessObjectForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest>();
			dummy.AddYouHaveNotEnteredMessage_ValidationEnabled = true;
			dummy.Z0_Description = ZString.Empty;
			AssertHasMessageError(dummy.Z0_DescriptionInfo, "You have not entered a Test Description.");

			dummy.Z0_DescriptionPropertyDescription = "Funny Description";
			dummy.Validation.ValidateZ0_Description();
			AssertHasMessageError(dummy.Z0_DescriptionInfo, "You have not entered a Funny Description.");
		}

		public void TestGetMessageErrorWhenPropertyNotEnteredAndOtherPropertyIsEntered()
		{
			var dummy = Factory.New<DummyBusinessObjectForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest>();
			AssertEquals("You have not entered a Test Description when Code is entered.", MandatoryValidation.GetMessageErrorWhenPropertyNotEnteredAndOtherPropertyIsEntered(dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo));
		}

		public void TestGetMessageErrorWhenPropertyNotEnteredAndOtherPropertyHasValue()
		{
			var dummy = Factory.New<DummyBusinessObjectForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest>();
			AssertEquals("You have not entered a Test Description when Code is AA.", MandatoryValidation.GetMessageErrorWhenPropertyNotEnteredAndOtherPropertyHasValue(dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo, new ZString("AA")));
		}

		public void TestAddMessageErrorIfNotEnteredAndOtherPropertyIsEntered()
		{
			var dummy = Factory.New<DummyBusinessObjectForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest>();
			dummy.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered_ValidationEnabled = true;
			AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo);
		}

		public void TestAddErrorIfNotEnteredAndOtherPropertyIsEntered()
		{
			var dummy = Factory.New<DummyBusinessObjectForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest>();
			dummy.AddErrorIfNotEnteredAndOtherPropertyIsEntered_ValidationEnabled = true;
			AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo);
		}

		public void TestGetErrorWhenPropertyNotEnteredAndOtherPropertyIsEntered()
		{
			var dummy = Factory.New<DummyBusinessObjectForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest>();
			AssertEquals("Please enter a Test Description when Code is entered.", MandatoryValidation.GetErrorWhenPropertyNotEnteredAndOtherPropertyIsEntered(dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo));
		}

		public void TestGetErrorWhenPropertyNotEnteredAndOtherPropertyHasValue()
		{
			var dummy = Factory.New<DummyBusinessObjectForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest>();
			AssertEquals("Please enter a Test Description when Code is AA.", MandatoryValidation.GetErrorWhenPropertyNotEnteredAndOtherPropertyHasValue(dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo, new ZString("AA")));
		}

		public void TestAddMessageErrorIfNotEnteredAndOtherPropertyHasValue()
		{
			var dummy = Factory.New<DummyBusinessObjectForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest>();
			dummy.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue_ValidationEnabled = true;
			AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo, new IZType[] { new ZString("AA"), new ZString("BB") });
		}

		public void TestAddMessageErrorIfNotEnteredAndCodeListIsNotEmpty() => CombineAssertions(() =>
		{
			const string messageError = "You have not entered a Test Code.";
			var dummy = Factory.New<DummyBusinessObjectForAddMessageErrorIfNotEnteredAndCodeListIsNotEmptyTest>();
			var codeList = new CodeDescriptionPairList();
			dummy.Lookups.Codes = codeList;

			dummy.Validation.ValidateZ0_Code();
			AssertNoMessageError("When code list is empty and value is empty", dummy.Z0_CodeInfo, messageError);

			codeList.AddPair("FOO", "Just another fake value");
			dummy.Validation.ValidateZ0_Code();
			AssertHasMessageError("When code list is not empty and value is empty", dummy.Z0_CodeInfo, messageError);

			dummy.Z0_Code = "BAR";
			dummy.Validation.ValidateZ0_Code();
			AssertNoMessageError("When code list is not empty and value is not empty", dummy.Z0_CodeInfo, messageError);
		});

		public void TestAddErrorIfNotEnteredAndOtherPropertyHasValue()
		{
			var dummy = Factory.New<DummyBusinessObjectForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest>();
			dummy.AddErrorIfNotEnteredAndOtherPropertyHasValue_ValidationEnabled = true;
			AssertErrorIfNotEnteredWhenOtherPropertyHasValue(dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo, new IZType[] { new ZString("AA"), new ZString("BB") });
		}

		static void AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(ZPropertyInfo propertyInfoToCheck, ZPropertyInfo otherPropertyInfo)
		{
			var error = MandatoryValidation.GetErrorWhenPropertyNotEnteredAndOtherPropertyIsEntered(propertyInfoToCheck, otherPropertyInfo);

			CombineAssertions(() =>
			{
				otherPropertyInfo.Value = new ZString("A");
				propertyInfoToCheck.Value = new ZString("A");
				AssertNoError("Both are entered", propertyInfoToCheck, error);

				otherPropertyInfo.Value = new ZString("A");
				propertyInfoToCheck.Value = ZString.Empty;
				AssertHasError($"{propertyInfoToCheck.HumanReadableName} is entered, so should the {propertyInfoToCheck.HumanReadableName}", propertyInfoToCheck, error);

				otherPropertyInfo.Value = ZString.Empty;
				propertyInfoToCheck.Value = ZString.Empty;
				AssertNoError("Neither is entered", propertyInfoToCheck, error);
			});
		}

		static void AssertErrorIfNotEnteredWhenOtherPropertyHasValue(ZPropertyInfo propertyInfoToCheck, ZPropertyInfo otherPropertyInfo, IEnumerable<IZType> valuesThatShouldTriggerValidation)
		{
			CombineAssertions(() =>
			{
				var valueThatShouldNotTriggerValidation = new ZString("xly");
				otherPropertyInfo.Value = new ZString(valueThatShouldNotTriggerValidation);
				propertyInfoToCheck.Value = ZString.Empty;
				AssertNoErrors("Other property is entered, but not with a value that should trigger validation", propertyInfoToCheck);

				foreach (var valueThatShouldTriggerValidation in valuesThatShouldTriggerValidation)
				{
					otherPropertyInfo.Value = new ZString(valueThatShouldTriggerValidation);
					var error = MandatoryValidation.GetErrorWhenPropertyNotEnteredAndOtherPropertyHasValue(propertyInfoToCheck, otherPropertyInfo, otherPropertyInfo.Value);
					propertyInfoToCheck.Value = ZString.Empty;
					AssertHasError($"{otherPropertyInfo.HumanReadableName} is entered, with a value that should trigger validation ({valueThatShouldTriggerValidation})", propertyInfoToCheck, error);
					propertyInfoToCheck.Value = new ZString("A");
					AssertNoError($"{otherPropertyInfo.HumanReadableName} is entered, with a value that should trigger validation ({valueThatShouldTriggerValidation}), however, {propertyInfoToCheck.Description} is entered", propertyInfoToCheck, error);
				}
			});
		}

		static void AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(ZPropertyInfo propertyInfoToCheck, ZPropertyInfo otherPropertyInfo)
		{
			var messageError = MandatoryValidation.GetMessageErrorWhenPropertyNotEnteredAndOtherPropertyIsEntered(propertyInfoToCheck, otherPropertyInfo);

			CombineAssertions(() =>
			{
				otherPropertyInfo.Value = new ZString("A");
				propertyInfoToCheck.Value = new ZString("A");
				AssertNoMessageError("Both are entered", propertyInfoToCheck, messageError);

				otherPropertyInfo.Value = new ZString("A");
				propertyInfoToCheck.Value = ZString.Empty;
				AssertHasMessageError($"{propertyInfoToCheck.HumanReadableName} is entered, so should the {propertyInfoToCheck.HumanReadableName}", propertyInfoToCheck, messageError);

				otherPropertyInfo.Value = ZString.Empty;
				propertyInfoToCheck.Value = ZString.Empty;
				AssertNoMessageError("Neither is entered", propertyInfoToCheck, messageError);
			});
		}

		static void AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(ZPropertyInfo propertyInfoToCheck, ZPropertyInfo otherPropertyInfo, IEnumerable<IZType> valuesThatShouldTriggerValidation)
		{
			CombineAssertions(() =>
			{
				var valueThatShouldNotTriggerValidation = new ZString("xly");
				otherPropertyInfo.Value = new ZString(valueThatShouldNotTriggerValidation);
				propertyInfoToCheck.Value = ZString.Empty;
				AssertNoMessageErrors("Other property is entered, but not with a value that should trigger validation", propertyInfoToCheck);

				foreach (var valueThatShouldTriggerValidation in valuesThatShouldTriggerValidation)
				{
					otherPropertyInfo.Value = new ZString(valueThatShouldTriggerValidation);
					var messageError = MandatoryValidation.GetMessageErrorWhenPropertyNotEnteredAndOtherPropertyHasValue(propertyInfoToCheck, otherPropertyInfo, otherPropertyInfo.Value);
					propertyInfoToCheck.Value = ZString.Empty;
					AssertHasMessageError($"{otherPropertyInfo.HumanReadableName} is entered, with a value that should trigger validation ({valueThatShouldTriggerValidation})", propertyInfoToCheck, messageError);
					propertyInfoToCheck.Value = new ZString("A");
					AssertNoMessageError($"{otherPropertyInfo.HumanReadableName} is entered, with a value that should trigger validation ({valueThatShouldTriggerValidation}), however, {propertyInfoToCheck.Description} is entered", propertyInfoToCheck, messageError);
				}
			});
		}

		sealed class DummyBusinessObjectForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest : DummyBusinessObject
		{
			public bool AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered_ValidationEnabled { get; set; }
			public bool AddMessageErrorIfNotEnteredAndOtherPropertyHasValue_ValidationEnabled { get; set; }
			public bool AddErrorIfNotEnteredAndOtherPropertyIsEntered_ValidationEnabled { get; set; }
			public bool AddErrorIfNotEnteredAndOtherPropertyHasValue_ValidationEnabled { get; set; }
			public bool AddYouHaveNotEnteredMessage_ValidationEnabled { get; set; }
			public string Z0_DescriptionPropertyDescription { get; set; }

			[ResourceStringData("DummyBusinessObjectForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest|Z0_Description", Caption = "Test Description")]
			public override ZString Z0_Description { get => base.Z0_Description; set => base.Z0_Description = value; }

			public DummyBusinessObjectForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override DummyBizoValidation GetNewValidation() => new DummyBizoValidationForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest(this);
		}

		sealed class DummyBizoValidationForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest : DummyBizoValidation
		{
			public DummyBizoValidationForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest(DummyBusinessObject bizO) : base(bizO)
			{
			}

			new DummyBusinessObjectForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest Parent => (DummyBusinessObjectForMessageErrorIfNotEnteredWhenOtherPropertyHasValueTest)base.Parent;

			protected override void CheckZ0_Description()
			{
				base.CheckZ0_Description();

				if (Parent.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered_ValidationEnabled)
				{
					MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.Z0_DescriptionInfo, Parent.Z0_CodeInfo);
				}

				if (Parent.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue_ValidationEnabled)
				{
					MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(Parent.Z0_DescriptionInfo, Parent.Z0_CodeInfo, new IZType[] { new ZString("AA"), new ZString("BB") });
				}

				if (Parent.AddErrorIfNotEnteredAndOtherPropertyIsEntered_ValidationEnabled)
				{
					MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.Z0_DescriptionInfo, Parent.Z0_CodeInfo);
				}

				if (Parent.AddErrorIfNotEnteredAndOtherPropertyHasValue_ValidationEnabled)
				{
					MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyHasValues(Parent.Z0_DescriptionInfo, Parent.Z0_CodeInfo, new IZType[] { new ZString("AA"), new ZString("BB") });
				}

				if (Parent.AddYouHaveNotEnteredMessage_ValidationEnabled)
				{
					MandatoryValidation.AddYouHaveNotEnteredMessage(Parent.Z0_DescriptionInfo, Parent.Z0_DescriptionPropertyDescription);
				}
			}
		}

		sealed class DummyBusinessObjectForAddMessageErrorIfNotEnteredAndCodeListIsNotEmptyTest : DummyBusinessObject
		{
			public DummyBusinessObjectForAddMessageErrorIfNotEnteredAndCodeListIsNotEmptyTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public DummyBusinessObjectLookupsForAddMessageErrorIfNotEnteredAndCodeListIsNotEmptyTest Lookups => fLookups ??= new(this);
			DummyBusinessObjectLookupsForAddMessageErrorIfNotEnteredAndCodeListIsNotEmptyTest fLookups;

			protected override DummyBizoValidation GetNewValidation() => new DummyBusinessObjectValidationForAddMessageErrorIfNotEnteredAndCodeListIsNotEmptyTest(this);

			[List(nameof(Lookups) + "." + nameof(Lookups.Codes))]
			[ResourceStringData("DummyBusinessObjectLookupsForAddMessageErrorIfNotEnteredAndCodeListIsNotEmptyTest|Z0_Code", Caption = "Test Code")]
			public override ZString Z0_Code { get; set; }
		}

		sealed class DummyBusinessObjectLookupsForAddMessageErrorIfNotEnteredAndCodeListIsNotEmptyTest : ZLookups
		{
			public DummyBusinessObjectLookupsForAddMessageErrorIfNotEnteredAndCodeListIsNotEmptyTest(BusinessObject parent) : base(parent)
			{
			}

			public ICodeDescriptionPairList Codes { get; set; }
		}

		sealed class DummyBusinessObjectValidationForAddMessageErrorIfNotEnteredAndCodeListIsNotEmptyTest : DummyBizoValidation
		{
			public DummyBusinessObjectValidationForAddMessageErrorIfNotEnteredAndCodeListIsNotEmptyTest(DummyBusinessObject parent) : base(parent)
			{
			}

			protected override void CheckZ0_Code()
			{
				base.CheckZ0_Code();
				MandatoryValidation.AddMessageErrorIfNotEnteredAndCodeListIsNotEmpty(Parent.Z0_CodeInfo);
			}
		}

		sealed class MandatoryValidationForTest : MandatoryValidation
		{
			public new static void WarnIfNotEntered(ZPropertyInfo propertyInfo, ZString propertyDescription, ZString errorMessagePrefix)
			{
				MandatoryValidation.WarnIfNotEntered(propertyInfo, propertyDescription, errorMessagePrefix);
			}

			public new static void WarnIfIsEntered(ZPropertyInfo propertyInfo, ZString propertyDescription, ZString errorMessagePrefix)
			{
				MandatoryValidation.WarnIfIsEntered(propertyInfo, propertyDescription, errorMessagePrefix);
			}
		}
	}
}
