using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMControlCustomisationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIsSystemWide()
		{
			var customisation1 = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation1.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			var customisation2 = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation2.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			var customisation3 = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation3.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;

			customisation1.FM_IsSystemWide = true;
			customisation2.FM_IsSystemWide = true;
			customisation3.FM_IsSystemWide = true;

			AssertHasError(customisation2.FM_IsSystemWideInfo, "Only one system-wide customization may be made per Control Type.");
			AssertNoErrors(customisation3.FM_IsSystemWideInfo);
		}

		public void TestName()
		{
			var customisation1 = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation1.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			var customisation2 = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation2.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			var customisation3 = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation3.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;

			customisation1.FM_Name = "Boo";
			customisation2.FM_Name = "Bar";
			customisation3.FM_Name = "Boo";

			AssertNoErrors(customisation1.FM_NameInfo);
			AssertNoErrors(customisation2.FM_NameInfo);
			AssertNoErrors(customisation3.FM_NameInfo);

			customisation2.FM_Name = "Boo";
			AssertHasError(customisation2.FM_NameInfo, "The Name has been duplicated and must be unique.");

			customisation2.FM_Name = "Bar";
			AssertNoErrors(customisation2.FM_NameInfo);

			customisation2.FM_Name = "boo";
			AssertHasError(customisation2.FM_NameInfo, "The Name has been duplicated and must be unique.");
		}

		public void TestWidth()
		{
			var customisationDetailedCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisationDetailedCard.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			var customisationTaskCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisationTaskCard.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;

			customisationDetailedCard.Width = 24;
			customisationTaskCard.Width = 24;

			AssertHasError(customisationDetailedCard.WidthInfo, "Please enter a 'Width' within the range 25 to 800.");
			AssertHasError(customisationDetailedCard.WidthInfo, "Please enter a 'Width' within the range 25 to 800.");

			customisationDetailedCard.Width = 801;
			customisationTaskCard.Width = 801;

			AssertHasError(customisationDetailedCard.WidthInfo, "Please enter a 'Width' within the range 25 to 800.");
			AssertHasError(customisationDetailedCard.WidthInfo, "Please enter a 'Width' within the range 25 to 800.");

			customisationDetailedCard.Width = 25;
			customisationTaskCard.Width = 25;

			AssertNoErrors(customisationDetailedCard.WidthInfo);
			AssertNoErrors(customisationTaskCard.WidthInfo);

			customisationDetailedCard.Width = 800;
			customisationTaskCard.Width = 800;

			AssertNoErrors(customisationDetailedCard.WidthInfo);
			AssertNoErrors(customisationTaskCard.WidthInfo);
		}

		public void TestHeight()
		{
			var customisationDetailedCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisationDetailedCard.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			var customisationTaskCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisationTaskCard.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;

			customisationDetailedCard.Height = 24;
			customisationTaskCard.Height = 24;

			AssertHasError(customisationDetailedCard.HeightInfo, "Please enter a 'Height' within the range 25 to 600.");
			AssertHasError(customisationDetailedCard.HeightInfo, "Please enter a 'Height' within the range 25 to 600.");

			customisationDetailedCard.Height = 601;
			customisationTaskCard.Height = 601;

			AssertHasError(customisationDetailedCard.HeightInfo, "Please enter a 'Height' within the range 25 to 600.");
			AssertHasError(customisationDetailedCard.HeightInfo, "Please enter a 'Height' within the range 25 to 600.");

			customisationDetailedCard.Height = 25;
			customisationTaskCard.Height = 25;

			AssertNoErrors(customisationDetailedCard.HeightInfo);
			AssertNoErrors(customisationTaskCard.HeightInfo);

			customisationDetailedCard.Height = 600;
			customisationTaskCard.Height = 600;

			AssertNoErrors(customisationDetailedCard.HeightInfo);
			AssertNoErrors(customisationTaskCard.HeightInfo);
		}

		#region Status button behavior

		public void TestButtonBehavior_ForStatusButtonsLine()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.StatusButtons, string.Empty, 120, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			line.Validation.ValidateAll();
			AssertNoErrors(line.PlayButtonBehaviorInfo);
			AssertNoErrors(line.SuspendButtonBehaviorInfo);
			AssertNoErrors(line.CloseTaskButtonBehaviorInfo);

			line.PlayButtonBehavior = ZString.Empty;
			AssertHasError(line.PlayButtonBehaviorInfo, "Please enter a Play button behavior.");

			line.SuspendButtonBehavior = ZString.Empty;
			AssertHasError(line.SuspendButtonBehaviorInfo, "Please enter a Suspend button behavior.");

			line.CloseTaskButtonBehavior = ZString.Empty;
			AssertHasError(line.CloseTaskButtonBehaviorInfo, "Please enter a Close Task button behavior.");

			line.PlayButtonBehavior = "ZZZ";
			AssertHasError(line.PlayButtonBehaviorInfo, "Please select a valid Play button behavior.");

			line.SuspendButtonBehavior = "ZZZ";
			AssertHasError(line.SuspendButtonBehaviorInfo, "Please select a valid Suspend button behavior.");

			line.CloseTaskButtonBehavior = "ZZZ";
			AssertHasError(line.CloseTaskButtonBehaviorInfo, "Please select a valid Close Task button behavior.");
		}

		public void TestButtonBehavior_ForPlayButtonLine()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.WorkingStatusButton, string.Empty, 120, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			line.Validation.ValidateAll();
			AssertNoErrors(line.PlayButtonBehaviorInfo);
			AssertNoErrors(line.SuspendButtonBehaviorInfo);
			AssertNoErrors(line.CloseTaskButtonBehaviorInfo);

			line.PlayButtonBehavior = ZString.Empty;
			AssertHasError(line.PlayButtonBehaviorInfo, "Please enter a Play button behavior.");

			line.SuspendButtonBehavior = ZString.Empty;
			AssertNoErrors(line.SuspendButtonBehaviorInfo);

			line.CloseTaskButtonBehavior = ZString.Empty;
			AssertNoErrors(line.CloseTaskButtonBehaviorInfo);

			line.PlayButtonBehavior = "ZZZ";
			AssertHasError(line.PlayButtonBehaviorInfo, "Please select a valid Play button behavior.");

			line.SuspendButtonBehavior = "ZZZ";
			AssertHasError(line.SuspendButtonBehaviorInfo, "Please select a valid Suspend button behavior.");

			line.CloseTaskButtonBehavior = "ZZZ";
			AssertHasError(line.CloseTaskButtonBehaviorInfo, "Please select a valid Close Task button behavior.");
		}

		public void TestButtonBehavior_ForSuspendButtonLine()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.SuspendStatusButton, string.Empty, 120, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			line.Validation.ValidateAll();
			AssertNoErrors(line.PlayButtonBehaviorInfo);
			AssertNoErrors(line.SuspendButtonBehaviorInfo);
			AssertNoErrors(line.CloseTaskButtonBehaviorInfo);

			line.PlayButtonBehavior = ZString.Empty;
			AssertNoErrors(line.PlayButtonBehaviorInfo);

			line.SuspendButtonBehavior = ZString.Empty;
			AssertHasError(line.SuspendButtonBehaviorInfo, "Please enter a Suspend button behavior.");

			line.CloseTaskButtonBehavior = ZString.Empty;
			AssertNoErrors(line.CloseTaskButtonBehaviorInfo);

			line.PlayButtonBehavior = "ZZZ";
			AssertHasError(line.PlayButtonBehaviorInfo, "Please select a valid Play button behavior.");

			line.SuspendButtonBehavior = "ZZZ";
			AssertHasError(line.SuspendButtonBehaviorInfo, "Please select a valid Suspend button behavior.");

			line.CloseTaskButtonBehavior = "ZZZ";
			AssertHasError(line.CloseTaskButtonBehaviorInfo, "Please select a valid Close Task button behavior.");
		}

		public void TestButtonBehavior_ForCloseTaskButtonLine()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.CompletedStatusButton, string.Empty, 120, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			line.Validation.ValidateAll();
			AssertNoErrors(line.PlayButtonBehaviorInfo);
			AssertNoErrors(line.SuspendButtonBehaviorInfo);
			AssertNoErrors(line.CloseTaskButtonBehaviorInfo);

			line.PlayButtonBehavior = ZString.Empty;
			AssertNoErrors(line.PlayButtonBehaviorInfo);

			line.SuspendButtonBehavior = ZString.Empty;
			AssertNoErrors(line.SuspendButtonBehaviorInfo);

			line.CloseTaskButtonBehavior = ZString.Empty;
			AssertHasError(line.CloseTaskButtonBehaviorInfo, "Please enter a Close Task button behavior.");

			line.PlayButtonBehavior = "ZZZ";
			AssertHasError(line.PlayButtonBehaviorInfo, "Please select a valid Play button behavior.");

			line.SuspendButtonBehavior = "ZZZ";
			AssertHasError(line.SuspendButtonBehaviorInfo, "Please select a valid Suspend button behavior.");

			line.CloseTaskButtonBehavior = "ZZZ";
			AssertHasError(line.CloseTaskButtonBehaviorInfo, "Please select a valid Close Task button behavior.");
		}

		public void TestButtonBehavior_ForNonStatusButtonsLine()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.AttachedTagsIndicator, string.Empty, 120, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			line.Validation.ValidateAll();
			AssertNoErrors(line.PlayButtonBehaviorInfo);
			AssertNoErrors(line.SuspendButtonBehaviorInfo);
			AssertNoErrors(line.CloseTaskButtonBehaviorInfo);

			line.PlayButtonBehavior = ZString.Empty;
			AssertNoErrors(line.PlayButtonBehaviorInfo);

			line.SuspendButtonBehavior = ZString.Empty;
			AssertNoErrors(line.SuspendButtonBehaviorInfo);

			line.CloseTaskButtonBehavior = ZString.Empty;
			AssertNoErrors(line.CloseTaskButtonBehaviorInfo);

			line.PlayButtonBehavior = "ZZZ";
			AssertHasError(line.PlayButtonBehaviorInfo, "Please select a valid Play button behavior.");

			line.SuspendButtonBehavior = "ZZZ";
			AssertHasError(line.SuspendButtonBehaviorInfo, "Please select a valid Suspend button behavior.");

			line.CloseTaskButtonBehavior = "ZZZ";
			AssertHasError(line.CloseTaskButtonBehaviorInfo, "Please select a valid Close Task button behavior.");
		}

		#endregion
	}
}
