using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI.Testing
{
	class SupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumnStyleProperties()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			using (var control = new SupportingDocumentsUserControl())
			{
				var grid = control.SupportingDocumentsGrid;
				control.Show();
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Code).CharacterCasing);
					AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber).CharacterCasing);
					AssertEquals("CSI_Actions: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Actions).CharacterCasing);
					AssertEquals("CSI_Availability: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Availability).CharacterCasing);
					AssertEquals("CSI_SubType: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_SubType).CharacterCasing);
					AssertEquals("CSI_DateOfIssue: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_DateOfIssue).CharacterCasing);
					AssertEquals("CSI_DateOfExpiry: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_DateOfExpiry).CharacterCasing);
					AssertEquals("CSI_Description: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Description).CharacterCasing);
					AssertEquals("CSI_ReferenceNumber2: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber2).CharacterCasing);
				});
			}
		}

		public void TestLayout()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			using (var form = new ZForm(bill))
			using (var control = new SupportingDocumentsUserControl())
			{
				control.SetDataBinding(bill, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("SupportingDocumentsGrid");
				Assert("Grid should be visible", grid.Visible);

				var panel = control.FindSingle<ZPanel>("SupportingDocumentsPanel");
				Assert("Additional Details Panel should be visible", panel.Visible);
			}
		}
	}
}
