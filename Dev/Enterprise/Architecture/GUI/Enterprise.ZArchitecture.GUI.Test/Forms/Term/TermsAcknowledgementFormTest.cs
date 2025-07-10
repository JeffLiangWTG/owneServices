using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.Term
{
	[TestedType(typeof(TermsAcknowledgementForm))]
	class TermsAcknowledgementFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = new TermsAcknowledgementForm(new TermsForTest());
			MissingResourceStringChecker.ExcludeFromTest(form.Controls.Find("TitleLabel", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(form.Controls.Find("TermsContentRichTextBox", true).Single());
			return form;
		}

		public void TestContinueButton_Click()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			using (var form = new TermsAcknowledgementFormForTest(new TermsForTest()))
			{
				form.Show();
				form.ContinueButton_Click_Exposed(this, null);
				AssertEquals("Enterprise.ZArchitecture.GUI.TermsProgressForm", ZFormModaliser.LastFormShownForTest.GetType().FullName);
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestForbiddenButton_Click()
		{
			using (var form = new TermsAcknowledgementFormForTest(new TermsForTest()))
			{
				form.Show();
				var forbiddenButton = form.Controls.Find("ForbiddenButton", true).Single() as ZButton;
				forbiddenButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestTermsAcknowledgementFormMinimumSize()
		{
			using (var form = new TermsAcknowledgementForm(new TermsForTest()))
			{
				form.Show();
				var expectedWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(543);
				var expectedHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(725);
				AssertEquals(expectedWidth, form.MinimumSize.Width);
				AssertEquals(expectedHeight, form.MinimumSize.Height);
			}
		}
	}

	#region Implementation

	class TermsForTest : BaseTermsAgreement
	{
		public override string Type => string.Empty;

		public override bool IsCurrentUserAllowedToAcknowledgeAgreement => true;

		public override string ErrorMessageForAcknowledgementNotAllowed => null;

		protected override bool IsLocalDisplayConditionSatisfied() => true;
		public override async Task<bool> TryLoadTerm() => await Task.FromResult(true);

		public override async Task TryPostAcknowledgement() => await Task.FromResult(true);
	}

	class TermsAcknowledgementFormForTest : TermsAcknowledgementForm
	{
		public TermsAcknowledgementFormForTest(BaseTermsAgreement term) : base(term)
		{
		}

		public void ContinueButton_Click_Exposed(object sender, EventArgs e)
		{
			ContinueButton_Click(sender, e);
		}
	}

	#endregion
}
