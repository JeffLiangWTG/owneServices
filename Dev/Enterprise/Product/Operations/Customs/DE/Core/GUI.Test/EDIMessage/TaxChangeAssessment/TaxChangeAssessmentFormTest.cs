using System.Windows.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(TaxChangeAssessmentForm))]
	class TaxChangeAssessmentFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (var form = new TaxChangeAssessmentForm(taxChangeAssessment))
			{
				AssertEquals("Tax Change Assessment", form.FormCaption);
			}
		}

		public void TestControls()
		{
			using (var form = new TaxChangeAssessmentForm(taxChangeAssessment))
			{
				var mainTabControl = form.FindSingle<ZTabControl>("MainTabControl", 1);
				var assessmentTabPage = mainTabControl.FindSingle<ZTabPage>("MainTabPage");
				var logsTabPage = mainTabControl.FindSingleOrDefault<ZTabPage>("LogsTabPage");
				CombineAssertions(() =>
				{
					var taxChangeAssessmentGroupBox = assessmentTabPage.FindSingle<ZGroupBox>("TaxChangeAssessmentGroupBox");
					AssertEquals("TypeTextBox", CharacterCasing.Normal, taxChangeAssessmentGroupBox.FindSingle<ZTextBox>("TypeTextBox").CharacterCasing);
					AssertEquals("ReferenceTextBox", CharacterCasing.Normal, taxChangeAssessmentGroupBox.FindSingle<ZTextBox>("ReferenceTextBox").CharacterCasing);
					AssertEquals("LRNTextBox", CharacterCasing.Normal, taxChangeAssessmentGroupBox.FindSingle<ZTextBox>("LRNTextBox").CharacterCasing);

					AssertNoExceptionThrown("IssueDateEdit", () => taxChangeAssessmentGroupBox.FindSingle<ZDateEdit>("IssueDateEdit"));
					AssertNoExceptionThrown("MaturityDateEdit", () => taxChangeAssessmentGroupBox.FindSingle<ZDateEdit>("MaturityDateEdit"));
					AssertNoExceptionThrown("StatusDropEdit", () => taxChangeAssessmentGroupBox.FindSingle<ZDropEdit>("StatusDropEdit"));

					AssertNull("LogsTabPage", logsTabPage);
				});
			}
		}

		public void TestAllowNew()
		{
			var taxChangeAssessment = Factory.New<TaxChangeAssessment>();
			using (var form = new TaxChangeAssessmentFormForTest(taxChangeAssessment))
			{
				AssertEquals(false, form.AllowNewExposed);
			}
		}

		public void TestShowNotesTab()
		{
			var taxChangeAssessment = Factory.New<TaxChangeAssessment>();
			using (var form = new TaxChangeAssessmentFormForTest(taxChangeAssessment))
			{
				AssertEquals(false, form.ShowNotesTabExposed);
			}
		}

		protected override Form GetFormToBashCore() => new TaxChangeAssessmentForm(taxChangeAssessment);

		protected override void SetUp()
		{
			base.SetUp();
			taxChangeAssessment = Factory.New<TaxChangeAssessment>();
		}
		TaxChangeAssessment taxChangeAssessment;

		class TaxChangeAssessmentFormForTest : TaxChangeAssessmentForm
		{
			public TaxChangeAssessmentFormForTest(TaxChangeAssessment taxChangeAssessment) : base(taxChangeAssessment)
			{
				InitializeComponent();
			}

			public bool AllowNewExposed => AllowNew;

			public bool ShowNotesTabExposed => ShowNotesTab;
		}
	}
}
