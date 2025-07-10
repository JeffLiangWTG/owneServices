using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(REXCertificateSelectionForm))]
	sealed class REXCertificateSelectionFormTest : ZFormBasherTest
	{
		public void TestOKButton()
		{
			var certificateNumbers = new CodeDescriptionPairList();
			certificateNumbers.AddPair("AU1234567", "25-JUN-2021 14:54:00");

			var certificateReissueHeader = new CertificateReissueHeader(Factory, certificateNumbers);
			var reissueRequest = certificateReissueHeader.CertificateReissueRequests.AddNew();
			reissueRequest.CertificateNumber = "AU1234567";

			using (var form = new REXCertificateSelectionForm(certificateReissueHeader))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FindSingle<ZButton>("OKButton").PerformClick();
				AssertEquals("Reports an error when ReasonText is empty.", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.None, form.DialogResult);
			}

			reissueRequest.ReissueReason = "I need a new one.";

			using (var form = new REXCertificateSelectionForm(certificateReissueHeader))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FindSingle<ZButton>("OKButton").PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestCancelButton()
		{
			var certificateReissueHeader = new CertificateReissueHeader(Factory, null);
			using (var form = new REXCertificateSelectionForm(certificateReissueHeader))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FindSingle<ZButton>("Cancel_Button").PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var certificateReissueHeader = new CertificateReissueHeader(Factory, null);
			return new REXCertificateSelectionForm(certificateReissueHeader);
		}
	}
}
