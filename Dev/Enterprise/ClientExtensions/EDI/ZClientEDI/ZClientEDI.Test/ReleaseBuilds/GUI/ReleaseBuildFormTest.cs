using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.ReleaseBuilds.GUI
{
	[TestedType(typeof(ReleaseBuildForm))]
	class ReleaseBuildFormTest : ZFormBasherTest
	{
		public void TestRingClientsTabPageIsHiddenIfBuildIsSupersededOrAlpha()
		{
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_Superceded = false;
			build.HL_ReleaseStatus = ReleaseRings.Codes.GPR;
			using (ReleaseBuildForm form = new ReleaseBuildForm(build))
			{
				AssertEquals("ReleaseClientsTabPage should be visible.", true, form.InternalMainTabControl.TabPages.Contains(form.InternalReleaseClientsTabPage));
			}

			build.HL_Superceded = true;
			using (ReleaseBuildForm form = new ReleaseBuildForm(build))
			{
				AssertEquals("ReleaseClientsTabPage should be hidden.", false, form.InternalMainTabControl.TabPages.Contains(form.InternalReleaseClientsTabPage));
			}
		}

		public void TestDeleteBadBuild()
		{
			ReleaseBuild build = Factory.NewWithValidTestData<ReleaseBuild>();
			build.HL_Product = ProductTypes.Codes.Enterprise;
			build.HL_Superceded = false;
			Factory.Save();
			using (ReleaseBuildForm testForm = new ReleaseBuildForm(build))
			{
				testForm.Show();
				build.HL_Superceded = true;
				Factory.Save();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals("Do you want to delete this build from the Generic and Client Specific FTP folders, as well as cancelling any queued upgrades?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRemoveBuildAndStopQueue_FTPDeleteFails()
		{
			string testRootDir = Path.Combine(Env.TempPath, "ediEnterpriseUpgrades");
			string testClientDir = Path.Combine(testRootDir, "ClientSpecific");
			string testClientDirWithClient1 = Path.Combine(testClientDir, "SomeClient");
			string testClientDirWithClient2 = Path.Combine(testClientDir, "AnotherClient");
			string clientFile1 = testClientDirWithClient1 + "\\Package20041229_135600_1_2_3_4.txt";
			string clientFile2 = testClientDirWithClient2 + "\\Package20041229_135600_1_2_3_4.txt";
			try
			{
				Directory.CreateDirectory(testClientDir);
				Directory.CreateDirectory(testClientDirWithClient1);
				Directory.CreateDirectory(testClientDirWithClient2);
				CreateFile(clientFile1);
				CreateFile(clientFile2);
				Business.Test.MockReleaseBuild build = Factory.New<Business.Test.MockReleaseBuild>();
				build.HL_Product = ProductTypes.Codes.Enterprise;
				build.FtpDeleteShouldFail = true;
				build.HL_ExeVersionDate = ZDateTime.Now;
				build.HL_Superceded = false;
				Factory.Save();
				using (ReleaseBuildForm form = new ReleaseBuildForm(build))
				{
					build.HL_Superceded = true;
					Factory.Save();
					AssertEquals(string.Format(@"This release build could not be deleted from the following FTP locations.
Please review the list and delete the files manually.

  - {0} - Something happened
  - {1} - Something happened", clientFile2, clientFile1), UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				if (Directory.Exists(testRootDir))
				{
					Directory.Delete(testRootDir, true);
				}
			}
		}

		public void TestControls_RolledOutCheckBox()
		{
			ReleaseBuild build = Factory.NewWithValidTestData<ReleaseBuild>();
			build.HL_Product = ProductTypes.Codes.Enterprise;
			build.HL_Superceded = false;
			build.HL_IsRolledOut = true;
			Factory.Save();
			using (ReleaseBuildForm testForm = new ReleaseBuildForm(build))
			{
				testForm.Show();
				var rolledOutCheckBox = testForm.Controls.Find("RolledOutCheckBox", true).Single() as ZCheckBox;
				AssertEquals(true, rolledOutCheckBox.Checked);
				AssertEquals("This build has been rolled out to all CW Cloud servers", rolledOutCheckBox.Text);
			}
		}

		public void TestShowRelatedBusinessObjectForm()
		{
			ReleaseBuild build = Factory.NewWithValidTestData<ReleaseBuild>();
			Factory.Save();
			using (ReleaseBuildForm form = GetFormToBash())
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				var controller = ZControllerFactory.Create(Modules.ClientControllerRegistration.ReleaseBuild);
				form.ShowRelatedBusinessObjectForm(controller, build);
				using (IZForm lastShownForm = controller.LastShownForm)
				{
					AssertEquals("The controller should have displayed the form in View mode.", ODisplayMode.ReadOnly, lastShownForm.DisplayMode);
				}

				form.DisplayMode = ODisplayMode.Browse;
				controller = ZControllerFactory.Create(Modules.ClientControllerRegistration.ReleaseBuild);
				form.ShowRelatedBusinessObjectForm(controller, build);
				using (IZForm lastShownForm = controller.LastShownForm)
				{
					AssertEquals("The controller should have displayed the form in View mode.", ODisplayMode.Browse, lastShownForm.DisplayMode);
				}
			}
		}

		public void TestOpenIncident()
		{
			ReleaseBuild build = Factory.NewWithValidTestData<ReleaseBuild>();
			SupportIncident supportIncident = Factory.NewWithValidTestData<SupportIncident>();
			supportIncident.IM_HL_ClientReportedOnVersion = build.PK;
			Factory.Save();
			using (ReleaseBuildForm form = new ReleaseBuildForm(build))
			{
				form.Show();
				form.InternalTopLevelTabControl.SelectedTab = form.InternalIncidentsReportedTabPage;
				AssertEquals("IncidentsReportedGrid.List.Count", 1, form.InternalIncidentsReportedGrid.List.Count);
				form.InternalIncidentsReportedGrid.Select(0);
				var lastController = form.ShowSupportIncident(form.InternalIncidentsReportedGrid);
				using (IZForm lastShownForm = lastController.LastShownForm)
				{
					AssertEquals("lastController.LastShownForm.GetType()", typeof(SupportIncidentForm), lastShownForm.GetType());
				}
			}
		}

		public void TestReadOnlyWhenENTOrCWN()
		{
			var build = Factory.NewWithValidTestData<ReleaseBuild>();
			build.HL_Product = "ENT";
			Factory.Save();
			using (var testForm = new ReleaseBuildForm(build))
			{
				testForm.Show();
				AssertEquals(build.HL_Product_ReadOnly, true);
				AssertEquals(build.HL_ExeVersionDate_ReadOnly, true);
				AssertEquals(build.HL_ExeVersionDate_ReadOnly, true);
				AssertEquals(build.ExeVersion_ReadOnly, true);
			}

			build.HL_Product = "CWN";
			Factory.Save();
			using (var testForm = new ReleaseBuildForm(build))
			{
				testForm.Show();
				AssertEquals(build.HL_Product_ReadOnly, true);
				AssertEquals(build.HL_ExeVersionDate_ReadOnly, true);
				AssertEquals(build.HL_ExeVersionDate_ReadOnly, true);
				AssertEquals(build.ExeVersion_ReadOnly, true);
			}
		}

		public void TestProductValueChangedEvent()
		{
			var build = Factory.NewWithValidTestData<ReleaseBuild>();
			build.HL_Product = string.Empty;
			using (var testForm = new ReleaseBuildForm(build))
			{
				testForm.Show();

				UnitTestUserNotification.Instance.AddOKAnswer();
				build.HL_Product = "ENT";
				AssertEquals("Product should not be set to 'ENT' or 'CWN' or 'CGW'", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddOKAnswer();
				build.HL_Product = "CWN";
				AssertEquals("Product should not be set to 'ENT' or 'CWN' or 'CGW'", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDownloadLink()
		{
			var build = Factory.NewWithValidTestData<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(25, 1, 1, 1);

			using (var testForm = new ReleaseBuildForm(build))
			{
				testForm.Show();
				var downloadLinkLabel = testForm.downloadPackageLink;
				AssertEquals("Download link is not visible", false, downloadLinkLabel.Visible);
			}

			build.HL_PackagePath = @"\\somewhere\out\there";

			using (var testForm = new ReleaseBuildForm(build))
			{
				testForm.Show();
				var downloadLinkLabel = testForm.downloadPackageLink;
				AssertEquals("Download link is visible", true, downloadLinkLabel.Visible);
				AssertEquals("Download 25.1.1.1 Package", downloadLinkLabel.Text);

				downloadLinkLabel.OnLinkClicked_Exposed(new(new()));
				AssertEquals($"File does not exist at path '{build.HL_PackagePath}'", UnitTestUserNotification.Instance.LastMessage.Text);

				using var tempDirectory = new TempDirectory();
				var filePath = Path.Combine(tempDirectory.DirectoryName, "temp.txt");
				File.WriteAllText(filePath, string.Empty);
				build.HL_PackagePath = filePath;

				downloadLinkLabel.OnLinkClicked_Exposed(new(new()));
				AssertEquals("File should be opened", filePath, WebUrlLauncher.LastUrlLaunched);
			}
		}

		public void TestFormHasPlugIns()
		{
			var build = Factory.NewWithValidTestData<ReleaseBuild>();
			build.HL_Product = string.Empty;
			using (var testForm = new ReleaseBuildForm(build))
			{
				AssertEquals("Audit plugIn should be added", true, testForm.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		#region Implementation
		protected new ReleaseBuildForm GetFormToBash()
		{
			return (ReleaseBuildForm)base.GetFormToBash();
		}

		protected override Form GetFormToBashCore()
		{
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			return new ReleaseBuildForm(build);
		}

		void CreateFile(string fileName)
		{
			if (!File.Exists(fileName))
			{
				using (FileStream stream = File.Create(fileName))
				{
				}
			}
		}
		#endregion
	}
}
