using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.UserPortal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.CustomerService.GUI.Testing
{
	[TestedType(typeof(ELearningNoticeForm))]
	internal sealed class ELearningNoticeFormBasherTest : ZFormBasherTest
	{
		public void TestRaiseTrainingIncidentLinkLabel()
		{
			using (var form = new ELearningNoticeFormForTesting())
			{
				form.Show();
				form.GetRaiseTrainingIncidentCheckBoxForTesting().Checked = true;
				((IButtonControl)form.GetRaiseTrainingIncidentLinkLabelForTesting()).PerformClick();

				AssertEquals(true, form.IsDisposed);
			}
		}

		public void TestOpenELearningPortal()
		{
			using (var form = new ELearningNoticeFormForTesting())
			{
				form.Show();

				form.GetELearningPortalButtonForTesting().PerformClick();
				AssertEquals(1, form.UserPortalLauncherForTesting.Log.Count);
				AssertEquals("pageName:[Wise Learning] url:[http://www.cargowise.com/eLearning.aspx]", form.UserPortalLauncherForTesting.Log[0]);

				form.GetWiseLearningButtonForTesting().PerformClick();
				AssertEquals(2, form.UserPortalLauncherForTesting.Log.Count);
				AssertEquals("pageName:[Wise Learning] url:[http://www.cargowise.com/eLearning.aspx]", form.UserPortalLauncherForTesting.Log[1]);
			}
		}

		public void TestCloseFormConfirmationMessage()
		{
			using (var form = new ELearningNoticeFormForTesting())
			{
				form.Show();
				form.Close();

				AssertEquals("Please tick the box 'I searched the WiseLearning portal and did not find any relevant content' if you want to proceed with the incident.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOpenHowToDocumentERequestIncident()
		{
			using (var form = new ELearningNoticeFormForTesting())
			{
				form.Show();

				((IButtonControl)form.GetHowToDocumentERequestIncidentLinkLabelnForTesting()).PerformClick();
				AssertEquals(1, form.UserPortalLauncherForTesting.Log.Count);
				AssertEquals("pageName:[How to Document eRequest Incident] url:[http://www.cargowise.com/Documents/UserGuides/HowTo/How-To%20document%20your%20eRequest%20Incident.pdf]", form.UserPortalLauncherForTesting.Log[0]);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new ELearningNoticeForm();
		}

		class ELearningNoticeFormForTesting : ELearningNoticeForm
		{
			public ZLinkLabel GetRaiseTrainingIncidentLinkLabelForTesting()
			{
				return raiseTrainingIncidentLinkLabel;
			}

			public ZButton GetELearningPortalButtonForTesting()
			{
				return eLearningPortalButton;
			}

			public ZButton GetWiseLearningButtonForTesting()
			{
				return wiseLearningButton;
			}

			public ZCheckBox GetRaiseTrainingIncidentCheckBoxForTesting()
			{
				return raiseTrainingIncidentCheckBox;
			}

			public ZLinkLabel GetHowToDocumentERequestIncidentLinkLabelnForTesting()
			{
				return howToDocumentERequestIncidentLinkLabel;
			}

			public DummyUserPortalLauncher UserPortalLauncherForTesting;
			protected override UserPortalLauncher UserPortalLauncher
			{
				get { return UserPortalLauncherForTesting ?? (UserPortalLauncherForTesting = new DummyUserPortalLauncher()); }
			}

			internal class DummyUserPortalLauncher : UserPortalLauncher
			{
				protected override void GoToCargoWiseUrl(string pageName, string url)
				{
					Log.Add(string.Format("pageName:[{0}] url:[{1}]", pageName, url));
				}

				public IList<string> Log = new List<string>();
			}
		}

		#endregion
	}
}
