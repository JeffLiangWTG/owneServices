using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business.Test
{
	class TagMagnitudeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCode_ShouldWarnWhenAnotherMagnitudeExistsWithSameCode()
		{
			var tagGroup1 = BMSTestHelper.CreateTagDefinition(Factory, "WIZ", "You're a wizard, Harry.");
			var tagGroup2 = BMSTestHelper.CreateTagDefinition(Factory, "NOT", "I'm not a wizard, I'm Harry.");

			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup1, "HRY", "Snarry");
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup2, "HRY", "Snarry");

			AssertHasWarning(tag2.TGM_CodeInfo, "The code HRY is already in use in Tag Group [You're a wizard, Harry.]. Consider changing the code to avoid the wrong tag being applied mistakenly.");

			tag2.TGM_Code = "HAG";

			AssertNoWarnings(tag2.TGM_CodeInfo);
		}

		public void TestCode_ForSystemDefinedTags_ShouldBeAllowedToHaveConflictingCodes()
		{
			var tagGroup1 = BMSTestHelper.CreateTagDefinition(Factory, "WIZ", "You're a wizard, Harry.", isSystem: true);
			var tagGroup2 = BMSTestHelper.CreateTagDefinition(Factory, "NOT", "I'm not a wizard, I'm Harry.", isSystem: true);

			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup1, "HRY", "Snarry");
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup2, "HRY", "Snarry");

			AssertNoWarnings(tag2.TGM_CodeInfo);

			tag2.TGM_Code = "HAG";

			AssertNoWarnings(tag2.TGM_CodeInfo);
		}

		public void TestGGOwnerGroup_InvalidPK()
		{
			var definition = Factory.New<TagDefinition>();
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "JIM", "Jimmys Tag Magnitude");

			magnitude.TGM_GG_OwnerGroup = ZGuid.Invalid;

			AssertHasError(magnitude.TGM_GG_OwnerGroupInfo, "Enter a valid Owner Group.");
		}

		public void TestGGOwnerGroup_ValidPK()
		{
			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "JIM", "Jimmys Tag Magnitude");
			var newGroup = Factory.NewWithValidTestData<GlbGroup>();
			newGroup.GG_Code = "GRP";

			AssertNoErrors(magnitude.TGM_GG_OwnerGroupInfo);

			Factory.Save();

			magnitude.TGM_GG_OwnerGroup = newGroup.PK;

			AssertNoErrors(magnitude.TGM_GG_OwnerGroupInfo);
		}

		public void TestColor()
		{
			var definition = Factory.New<TagDefinition>();
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "Dan", "Daniel Keogh is a pretty cool guy.");
			AssertNoErrors(magnitude.ColorInfo);

			magnitude.Color = "I'm not a color";
			magnitude.Validation.ValidateAll();
			AssertHasError(magnitude.ColorInfo, "Enter a valid Color.");

			magnitude.Color = ColorList.NameFromColor(Color.Aqua);
			AssertNoErrors(magnitude.ColorInfo);
		}

		public void TestColor_MandatoryWhenBackgroundChecked()
		{
			var definition = Factory.New<TagDefinition>();
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "WHE", "When can I put down this fish?");
			AssertNoErrors(magnitude.ColorInfo);

			magnitude.Color = ZString.Empty;
			magnitude.Validation.ValidateAll();
			AssertNoErrors(magnitude.ColorInfo);

			magnitude.ApplyColorToBackground = true;

			magnitude.Validation.ValidateAll();
			AssertHasError(magnitude.ColorInfo, "A Color is required when Apply Color to Ticket Background is selected.");

			magnitude.Color = ColorList.NameFromColor(Color.Aqua);
			AssertNoErrors(magnitude.ColorInfo);
		}

		public void TestColor_MandatoryWhenBorderChecked()
		{
			var definition = Factory.New<TagDefinition>();
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "WHY", "Is this fish so heavy?");
			AssertNoErrors(magnitude.ColorInfo);

			magnitude.Color = ZString.Empty;
			magnitude.Validation.ValidateAll();
			AssertNoErrors(magnitude.ColorInfo);

			magnitude.ApplyColorToBorder = true;

			magnitude.Validation.ValidateAll();
			AssertHasError(magnitude.ColorInfo, "A Color is required when Apply Color to Ticket Border is selected.");

			magnitude.Color = ColorList.NameFromColor(Color.Aqua);
			AssertNoErrors(magnitude.ColorInfo);
		}

		public void TestBorderStyle()
		{
			var definition = Factory.New<TagDefinition>();
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "HOW", "Can I find a lighter fish?");
			AssertNoErrors(magnitude.ColorInfo);

			magnitude.BorderStyle = ZString.Empty;
			magnitude.Validation.ValidateAll();
			AssertNoErrors(magnitude.BorderStyleInfo);

			magnitude.BorderStyle = "I'm invalid!";
			AssertHasError(magnitude.BorderStyleInfo, "Enter a valid Border Style.");

			magnitude.BorderStyle = new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Inset, false).ToString();
			AssertNoErrors(magnitude.BorderStyleInfo);
		}

		public void TestCode()
		{
			var magnitude = Factory.New<TagMagnitude>();

			magnitude.TGM_Code = "";
			AssertHasErrors(magnitude.TGM_CodeInfo);

			magnitude.TGM_Code = "WOW";
			AssertNoErrors(magnitude.TGM_CodeInfo);

			magnitude.TGM_Code = "";
			AssertHasErrors(magnitude.TGM_CodeInfo);
		}

		public void TestCodeUnique()
		{
			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var mag1 = BMSTestHelper.CreateTagMagnitude(definition, "WOW");
			var mag2 = BMSTestHelper.CreateTagMagnitude(definition, "WOW");

			mag1.Validation.ValidateAll();
			AssertHasErrors(mag1.TGM_CodeInfo);
			mag2.Validation.ValidateAll();
			AssertHasErrors(mag2.TGM_CodeInfo);

			mag1.TGM_Code = "WO1";

			mag1.Validation.ValidateAll();
			AssertNoErrors(mag1.TGM_CodeInfo);
			mag2.Validation.ValidateAll();
			AssertNoErrors(mag2.TGM_CodeInfo);
		}
	}
}
