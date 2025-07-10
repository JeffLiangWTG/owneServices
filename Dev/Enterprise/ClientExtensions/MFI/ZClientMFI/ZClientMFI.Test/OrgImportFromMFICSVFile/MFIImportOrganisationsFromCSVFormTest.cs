using System.IO;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.MFI.OrgImportFromMFICSVFile.Testing
{
	[TestedType(typeof(MFIImportOrganisationsFromCSVForm))]
	class MFIImportOrganisationsFromCSVFormTest : DataLoaderFormTestCase
	{
		public void TestFormHeadingAndConfirmLoadData()
		{
			ZString fileName = Env.GetTempFileName();
			try
			{
				using (MFIImportOrganisationsFromCSVFormForTest testForm = new MFIImportOrganisationsFromCSVFormForTest())
				{
					AssertEquals("form heading", MFIImportOrganisationsFromCSVForm.formHeading, testForm.FormHeading);
					testForm.Show();
					testForm.FileNameTextBox.Text = fileName;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					testForm.StartButton.PerformClick();
					AssertNotNull("PreCondition: Message Shown", UnitTestUserNotification.Instance.LastMessage);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("Please Note: Existing Organisations will be updated.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				File.Delete(fileName);
			}
		}

		protected override ImportFromCSVForm GetNewImportFromCSVFormCore()
		{
			return new MFIImportOrganisationsFromCSVForm();
		}

		class MFIImportOrganisationsFromCSVFormForTest : MFIImportOrganisationsFromCSVForm
		{
			public new string FormHeading
			{
				get
				{
					return base.FormHeading;
				}
			}

			public new ZTextBox FileNameTextBox
			{
				get
				{
					return base.FileNameTextBox;
				}
			}

			public new ZButton StartButton
			{
				get
				{
					return base.StartButton;
				}
			}
		}
	}
}
