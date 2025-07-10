using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	class EMCSInvoiceLinePackagesUserControlTest : TestCaseWithFactory
	{
		public void TestReferenceNumberTextBox_UpperLowerCase()
		{
			using (var form = new ZForm(packagePivot))
			using (var control = new EMCSInvoiceLinePackagesUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var referenceNumberTextBox = control.FindSingle<ZTextBox>("ReferenceNumberTextBox");
				AssertEquals("Seal Number", referenceNumberTextBox.Text);
			}
		}

		public void TestDescriptionTextBox_UpperLowerCase()
		{
			using (var form = new ZForm(packagePivot))
			using (var control = new EMCSInvoiceLinePackagesUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var descriptionTextBox = control.FindSingle<ZTextBox>("DescriptionTextBox");
				AssertEquals("Seal Comment", descriptionTextBox.Text);
			}
		}

		public void TestMarksAndNumbersTextBox_Binding()
		{
			using (var form = new ZForm(packagePivot))
			using (var control = new EMCSInvoiceLinePackagesUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var descriptionTextBox = control.FindSingle<LongTextControl>("MarksAndNumbersTextBox");
				AssertEquals("Shipping Marks", descriptionTextBox.CurrentDataItem);
			}
		}

		public void TestGridColumnSize()
		{
			using (var control = new EMCSInvoiceLinePackagesUserControl())
			{
				var packagesGrid = control.FindSingle<ZGrid>("PackagesGrid");
				CombineAssertions(() =>
				{
					AssertEquals("Is for invoice line?", 130, packagesGrid.GetColumnStyle(NonPersistentPackagePivot.Schema.IsForInvoiceLine).Width);
					AssertEquals("Quantity", 120, packagesGrid.GetColumnStyle(NonPersistentPackagePivot.Schema.UnitCount).Width);
					AssertEquals("Type", 40, packagesGrid.GetColumnStyle(NonPersistentPackagePivot.Schema.UnitType).Width);
					AssertEquals("Marks And Number", 325, packagesGrid.GetColumnStyle(NonPersistentPackagePivot.Schema.MarksAndNumbers).Width);
					AssertEquals("Seal Number", 120, packagesGrid.GetColumnStyle(NonPersistentPackagePivot.Schema.SealNumber).Width);
					AssertEquals("Seal Comment", 200, packagesGrid.GetColumnStyle(NonPersistentPackagePivot.Schema.SealComment).Width);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<EMCSJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var package = declaration.EMCSPackages.AddNew();
			package.B5_SealComment = "Seal Comment";
			package.B5_SealNumber = "Seal Number";
			package.B5_MarksAndNumbers = "Shipping Marks";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			packagePivot = invoiceLine.EMCSPackagePivots[0];
		}
		NonPersistentPackagePivot packagePivot;
	}
}
