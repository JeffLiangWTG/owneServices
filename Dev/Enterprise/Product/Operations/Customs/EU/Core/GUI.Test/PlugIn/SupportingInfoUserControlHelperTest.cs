using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	class SupportingInfoUserControlHelperTest : TestCaseWithFactory
	{
		public void TestChangeParentAndSetGridDetails_ISupportingInfoUserControls()
		{
			CombineAssertions(() =>
			{
				var jobDeclaration = Factory.New<JobDeclaration>();
				using (var form = new ZForm(jobDeclaration))
				using (var control = new AdditionalInfosUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var grid = control.Controls.Find("AdditionalInfosGrid", true).First() as ZGrid;
					AssertEquals("Initial DataMember", "FilteredInvoiceLines.AdditionalInfos", grid.DataMember);

					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(control, "Invoices", "INV");
					AssertEquals("DataMember is updated after ChangeParentAndSetGridDetails", "Invoices.AdditionalInfos", grid.DataMember);
				}
			});
		}

		public void TestChangeParentAndSetGridDetails_InvalidOperationException()
		{
			CombineAssertions(() =>
			{
				var jobDeclaration = Factory.New<JobDeclaration>();
				using (var form = new ZForm(jobDeclaration))
				using (var control = new TaxUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					AssertExceptionThrown<InvalidOperationException>("userControl doesn't implement ISupportingInfoUserControls",
						"ChangeParentAndSetGridDetails requires that userControl implement ISupportingInfoUserControls",
						() => SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(control, "Invoices", "INV"));
				}
			});
		}
	}
}
