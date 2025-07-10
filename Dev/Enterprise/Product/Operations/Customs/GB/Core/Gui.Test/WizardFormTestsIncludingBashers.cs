using System.Windows.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.GUI.Wizards;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Testing
{
	[TestedType(typeof(SuppDecWizardForm))]
	class SuppDecBasher : ZFormBasherTest
	{
		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.UnitedKingdom; }
		}

		protected override Form GetFormToBashCore()
		{
			return new SuppDecWizardForm();
		}
	}

	[TestedType(typeof(EidrWizardForm))]
	class EidrBasherTest : ZFormBasherTest
	{
		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.UnitedKingdom; }
		}

		protected override Form GetFormToBashCore()
		{
			return new EidrWizardForm();
		}
	}

	[TestedType(typeof(CdsFsdWizardForm))]
	class CdsFsdBasherTest : ZFormBasherTest
	{
		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.UnitedKingdom; }
		}

		protected override Form GetFormToBashCore()
		{
			return new CdsFsdWizardForm();
		}
	}

	[TestedType(typeof(JobDeclarationWizardForm))]
	class JobDeclarationWizardBasherTest : ZFormBasherTest
	{
		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.UnitedKingdom; }
		}

		protected override Form GetFormToBashCore()
		{
			return new JobDeclarationWizardForm();
		}

		public void TestForm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var declarationWizardManager = new DeclarationWizardManager(declaration);

			using (var form = new JobDeclarationWizardFormForTest(declarationWizardManager))
			{
				form.Show();
				//Question 1
				Assert("Previous button should be disabled", !form.PreviousButtonForTest().Enabled);
				Assert("Next button should be enabled", form.NextButtonForTest().Enabled);
				Assert("Finish button should not be visible", !form.FinishButtonForTest().Visible);
				AssertEquals("Question label showing incorrect value", "Where are your goods coming from/to?", form.QuestionLabelForTest().Text);
				AssertEquals("Group box contains incorrect number of options", 3, form.OptionGroupBoxForTest().Controls.Count);

				//Question 2
				form.NextButtonForTest().PerformClick();
				Assert("Previous button should be enabled", form.PreviousButtonForTest().Enabled);
				Assert("Next button should be enabled", form.NextButtonForTest().Enabled);
				Assert("Finish button should not be visible", !form.FinishButtonForTest().Visible);
				AssertEquals("Question label showing incorrect value", "What kind of declaration are you doing?", form.QuestionLabelForTest().Text);
				AssertEquals("Group box contains incorrect number of options", 4, form.OptionGroupBoxForTest().Controls.Count);

				//Question 3
				form.NextButtonForTest().PerformClick();
				Assert("Previous button should be enabled", form.PreviousButtonForTest().Enabled);
				Assert("Next button should be enabled", form.NextButtonForTest().Enabled);
				Assert("Finish button should not be visible", !form.FinishButtonForTest().Visible);
				AssertEquals("What options for this declaration type?", form.QuestionLabelForTest().Text);
				AssertEquals("Group box contains incorrect number of options", 6, form.OptionGroupBoxForTest().Controls.Count);

				//Question 4
				form.NextButtonForTest().PerformClick();
				Assert("Previous button should be enabled", form.PreviousButtonForTest().Enabled);
				Assert("Next button should be enabled", form.NextButtonForTest().Enabled);
				Assert("Finish button should not be visible", !form.FinishButtonForTest().Visible);
				AssertEquals("Have the goods arrived or not?", form.QuestionLabelForTest().Text);
				AssertEquals("Group box contains incorrect number of options", 2, form.OptionGroupBoxForTest().Controls.Count);

				//Back to Question 3
				form.PreviousButtonForTest().PerformClick();
				Assert("Previous button should be enabled", form.PreviousButtonForTest().Enabled);
				Assert("Next button should be enabled", form.NextButtonForTest().Enabled);
				Assert("Finish button should not be visible", !form.FinishButtonForTest().Visible);
				AssertEquals("What options for this declaration type?", form.QuestionLabelForTest().Text);
				AssertEquals("Group box contains incorrect number of options", 6, form.OptionGroupBoxForTest().Controls.Count);

				//Question 5
				form.NextButtonForTest().PerformClick();
				form.NextButtonForTest().PerformClick();
				Assert("Previous button should be enabled", form.PreviousButtonForTest().Enabled);
				Assert("Next button should be enabled", form.NextButtonForTest().Enabled);
				AssertEquals("Create an additional entry instruction for the result?", form.QuestionLabelForTest().Text);
				AssertEquals("Group box contains incorrect number of options", 2, form.OptionGroupBoxForTest().Controls.Count);

				//Question 6
				form.NextButtonForTest().PerformClick();
				Assert("Previous button should be enabled", form.PreviousButtonForTest().Enabled);
				Assert("Next button should not be visible", !form.NextButtonForTest().Visible);
				Assert("Finish button should be visible", form.FinishButtonForTest().Visible);
				AssertEquals("Is your declaration full, supplementary or some other variety?", form.QuestionLabelForTest().Text);
				AssertEquals("Group box contains incorrect number of options", 6, form.OptionGroupBoxForTest().Controls.Count);

				//Finish
				form.FinishButtonForTest().PerformClick();
				AssertEquals("Declaration Type", "H1", declaration.JE_DeclarationType);
				AssertEquals("Message Type", "IMP", declaration.JE_MessageType);
				AssertEquals("Entry Style", "IM", declaration.JE_EntryStyle);
				AssertEquals("Entry Sub Style", "A", declaration.JE_EntrySubStyle);
				AssertEquals("JI Procedure", "4000000", declaration.Invoices[0]?.InvoiceLines[0]?.JI_Procedure);
			}
		}

		class JobDeclarationWizardFormForTest(IDeclarationWizardManager manager) : JobDeclarationWizardForm(manager)
		{
			public ZArchitecture.GUI.ZButton PreviousButtonForTest() => ButtonPrevious;
			public ZArchitecture.GUI.ZButton NextButtonForTest() => ButtonNext;
			public ZArchitecture.GUI.ZButton FinishButtonForTest() => ButtonFinish;
			public ZArchitecture.ZLabel QuestionLabelForTest() => LabelQuestion;
			public ZArchitecture.GUI.ZGroupBox OptionGroupBoxForTest() => GroupBoxOptions;
		}

		protected override void SetUp()
		{
			base.SetUp();
			GB.CDS.Testing.DeclarationWizardTestHelper.SetUpRefCusCodeListData(Factory);
		}
	}
}
