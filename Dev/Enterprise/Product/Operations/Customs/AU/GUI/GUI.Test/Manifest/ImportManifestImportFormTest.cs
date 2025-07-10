using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(ImportManifestImportForm))]
	sealed class ImportManifestImportFormTest : ZFormBasherTest
	{
		public void TestShouldSaveOnSaveButtonClick()
		{
			AssertShouldSave(true, () => Form.SaveButton.PerformClick());
		}

		public void TestShouldntSaveOnCancelButtonClick()
		{
			AssertShouldSave(false, () => Form.CancellButton.PerformClick());
		}

		public void TestShouldntSaveOnFormClose()
		{
			AssertShouldSave(false, () => Form.Close());
		}

		delegate void DoFormAction();
		void AssertShouldSave(bool expectedHolderIsInDatabase, DoFormAction formAction)
		{
			bool formClosed = false;
			Form.FormClosed += (object sender, FormClosedEventArgs args) => formClosed = true;
			Form.Show();
			AssertEquals("precondition", false, formClosed);
			AssertEquals("precondition", false, manifest.IsInDatabase);
			formAction();
			AssertEquals("form has been closed after action", true, formClosed);
			AssertEquals("holder was saved", expectedHolderIsInDatabase, manifest.IsInDatabase);
		}

		#region Form Basher Overrides
		protected override Form GetFormToBashCore()
		{
			return Form;
		}

		#endregion
		#region Implementation
		protected override void TearDown()
		{
			if (form != null)
			{
				form.Dispose();
			}

			base.TearDown();
		}

		ImportManifestImportForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ImportManifestImportForm(ManifestCollection);
				}

				return form;
			}
		}

		ImportManifestImportForm form;
		ImportManifestItemCollection ManifestCollection
		{
			get
			{
				if (manifestCollection == null)
				{
					manifest = Factory.New<CusSeaManTranHead>();
					VoyageOrigin voyOrigin = Factory.New<VoyageOrigin>();
					JobVoyage voyage = Factory.New<JobVoyage>();
					VoyageDestination voyDestination = Factory.New<VoyageDestination>();
					JobSailing sailing = Factory.New<JobSailing>();
					sailing.JX_JA = voyOrigin.PK;
					sailing.JX_JB = voyDestination.PK;
					voyOrigin.JA_JV = voyage.PK;
					voyDestination.JB_JV = voyage.PK;
					manifestCollection = new ImportManifestItemCollection(manifest, sailing, false);
				}

				return manifestCollection;
			}
		}

		ImportManifestItemCollection manifestCollection;
		CusSeaManTranHead manifest;
		#endregion
	}
}
