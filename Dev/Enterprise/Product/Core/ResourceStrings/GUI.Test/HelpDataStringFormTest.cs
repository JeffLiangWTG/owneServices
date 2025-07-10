using System;
using System.Windows.Forms;
using CargoWise.Tools.SpellCheck.GUI;
using CargoWise.Tools.SpellCheck.TestFramework;
using Enterprise.ResourceStrings.Business;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.GUI
{
	[TestedType(typeof(HelpDataStringForm))]
	sealed class HelpDataStringFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		#region TestSpellCheck

		public void TestSpellCheck()
		{
			bizO = CreateBizO(false, "Organisation XY&Z", "Organisation, org. XYZ", "Or&g.", "Org");
			using (HelpDataStringForm form = (HelpDataStringForm)GetFormToBash())
			{
				form.Show();
				using (SpellCheckFormTestHelper spellCheckTester = new SpellCheckFormTestHelper(SP1_1))
				{
					form.FireSaveButton();
					AssertEquals(3, spellCheckTester.DisplayCount);
					AssertEquals("Org.", bizO.HD_ShortCaption);
					AssertEquals("Or&g.", bizO.HD_MidCaption);
					AssertEquals("Organization XY&Z", bizO.HD_Caption);
					AssertEquals("Organization, organization XYZ", bizO.HD_FullDescription);
				}
			}

			bizO = CreateBizO(false, "", "Organisation Organisation Organisation", "", "");
			using (HelpDataStringForm form = (HelpDataStringForm)GetFormToBash())
			{
				form.Show();
				using (SpellCheckFormTestHelper spellCheckTester = new SpellCheckFormTestHelper(SP2_1))
				{
					form.FireSaveButton();
					AssertEquals(2, spellCheckTester.DisplayCount);
					AssertEquals("Organization Organisation Organisation", bizO.HD_FullDescription);
				}
			}

			bizO = CreateBizO(false, "Organisation", "Organisation", "Organisation", "Organisation");
			using (HelpDataStringForm form = (HelpDataStringForm)GetFormToBash())
			{
				form.Show();
				using (SpellCheckFormTestHelper spellCheckTester = new SpellCheckFormTestHelper(SP3_1))
				{
					form.FireSaveButton();
					AssertEquals(1, spellCheckTester.DisplayCount);
					AssertEquals("Organization", bizO.HD_Caption);
					AssertEquals("Organization", bizO.HD_FullDescription);
					AssertEquals("Organization", bizO.HD_ShortCaption);
					AssertEquals("Organization", bizO.HD_MidCaption);
				}
			}

			bizO = CreateBizO(false, "Organisation XY&Z", "Organisation, org. XYZ", "Or&g.", "Org");
			bizO.HD_Language = Core.SharedConstants.Languages.ChineseSimplified;
			using (HelpDataStringForm form = (HelpDataStringForm)GetFormToBash())
			{
				form.Show();
				using (SpellCheckFormTestHelper spellCheckTester = new SpellCheckFormTestHelper(SPVOID))
				{
					form.FireSaveButton();
					AssertEquals(0, spellCheckTester.DisplayCount);
				}
			}
		}

		public void TestNewButtonNotDisplayed()
		{
			using (var form = new HelpDataStringForm())
			{
				AssertEquals("This form should not have 'New' button", false, form.TestAllowNew);
			}
		}

		SpellCheckFormAction SPVOID(ISpellCheckerForm form)
		{
			return null;
		}

		SpellCheckFormAction SP1_1(ISpellCheckerForm form)
		{
			AssertEquals("Org", form.CurrentError.SpellingError.Word);
			AssertEquals("ShortCaptionTextBox", ((ControlSpellCheckError)form.CurrentError).Control.Name);
			AssertEquals("Org.", form.CurrentError.SpellingError.Suggestions[0]);
			return new SpellCheckFormAction(SpellCheckerFormResult.Change, SP1_2);
		}

		SpellCheckFormAction SP1_2(ISpellCheckerForm form)
		{
			AssertEquals("Organisation", form.CurrentError.SpellingError.Word);
			AssertEquals("CaptionTextBox", ((ControlSpellCheckError)form.CurrentError).Control.Name);
			return new SpellCheckFormAction(SpellCheckerFormResult.ChangeAll, form.CurrentError.SpellingError.Suggestions.IndexOf("Organization"), SP1_3);
		}

		SpellCheckFormAction SP1_3(ISpellCheckerForm form)
		{
			AssertEquals("org.", form.CurrentError.SpellingError.Word);
			AssertEquals("organization", form.CurrentError.SpellingError.Suggestions[0]);
			AssertEquals("DescriptionTextBox", ((ControlSpellCheckError)form.CurrentError).Control.Name);
			return new SpellCheckFormAction(SpellCheckerFormResult.Change, 0, SPVOID);
		}

		SpellCheckFormAction SP2_1(ISpellCheckerForm form)
		{
			AssertEquals("Organisation", form.CurrentError.SpellingError.Word);
			AssertEquals("DescriptionTextBox", ((ControlSpellCheckError)form.CurrentError).Control.Name);
			return new SpellCheckFormAction(SpellCheckerFormResult.Change, form.CurrentError.SpellingError.Suggestions.IndexOf("Organization"), SP2_2);
		}

		SpellCheckFormAction SP2_2(ISpellCheckerForm form)
		{
			return new SpellCheckFormAction(SpellCheckerFormResult.Cancel, SPVOID);
		}

		SpellCheckFormAction SP3_1(ISpellCheckerForm form)
		{
			AssertEquals("Organisation", form.CurrentError.SpellingError.Word);
			AssertEquals("ShortCaptionTextBox", ((ControlSpellCheckError)form.CurrentError).Control.Name);
			return new SpellCheckFormAction(SpellCheckerFormResult.ChangeAll, form.CurrentError.SpellingError.Suggestions.IndexOf("Organization"), SP2_2);
		}

		#endregion

		public void TestFixRN()
		{
			AssertEquals("\r\n", HelpDataStringForm.FixRN("\n"));
			AssertEquals("\r\n", HelpDataStringForm.FixRN("\r"));
			AssertEquals("\r\n", HelpDataStringForm.FixRN("\r\n"));
			AssertEquals("\r\n", HelpDataStringForm.FixRN("\n\r"));
			AssertEquals("\r\n\r\n", HelpDataStringForm.FixRN("\n\r\n\r"));
			AssertEquals("\r\n", HelpDataStringForm.FixRN("\n\r\n"));
			AssertEquals("\r\n\r\n", HelpDataStringForm.FixRN("\r\n\n"));
			AssertEquals("\r\n\r\nSome text\r\nwith different\r\nline\r\n\r\n\r\nendings.\r\n\r\n\r\n",
				HelpDataStringForm.FixRN("\r\n\r\nSome text\nwith different\rline\n\n\nendings.\n\r\n\r\n\r"));
		}

		#region Implementation

		HelpDataString bizO;

		protected override Form GetFormToBashCore()
		{
			return new HelpDataStringForm(bizO ?? CreateBizO(false));
		}

		public override void TestBashingForm()
		{
			Assert(true);
		}

		HelpDataString CreateBizO(bool checkedOut)
		{
			return CreateBizO(checkedOut, "Caption", "FooDescription", "", "");
		}

		HelpDataString CreateBizO(bool checkedOut, string caption, string fullDescription, string midCaption, string shortCaption)
		{
			var dataString = new HelpDataString();
			dataString.HD_IsCheckedOut = checkedOut;
			dataString.HD_Language = Core.SharedConstants.Languages.English;
			dataString.HD_Caption = caption;
			dataString.HD_FullDescription = fullDescription;
			dataString.HD_MidCaption = midCaption;
			dataString.HD_ShortCaption = shortCaption;
			dataString.HasChanges = false;

			return dataString;
		}

		protected override void SetUp()
		{
			mockSources = ResourceStringsFactory.MockSources();
			base.SetUp();
		}

		protected override void TearDown()
		{
			if (mockSources != null)
			{
				mockSources.Dispose();
			}
			base.TearDown();
		}

		IDisposable mockSources;

		#endregion
	}
}
