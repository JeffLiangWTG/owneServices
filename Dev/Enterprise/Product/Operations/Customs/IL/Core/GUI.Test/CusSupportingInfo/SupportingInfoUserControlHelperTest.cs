using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class SupportingInfoUserControlHelperTest : TestCaseWithFactory
	{
		public void TestChangeParentAndSetGridDetails_ISupportingInfoUserControls()
		{
			CombineAssertions(() =>
			{
				var jobDeclaration = Factory.New<JobDeclaration>();
				using (var form = new ZForm(jobDeclaration))
				using (var control = new PreviousDocumentsUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var grid = control.Controls.Find("PreviousDocumentsGrid", true).First() as ZGrid;
					AssertEquals("Initial DataMember", "CustomsEntryInstructions.PreviousDocuments", grid.DataMember);

					//TODO: test the following part in WI00885158
					//SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(control, "FilteredInvoiceLines", "INL");
					//AssertEquals("DataMember is updated after ChangeParentAndSetGridDetails", "FilteredInvoiceLines.PreviousDocuments", grid.DataMember);
				}
			});
		}
	}
}
