using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUImportMessageUserControlTest : TestCaseWithFactory
	{
		public void TestTestSetupEntryHeaderColumns()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			using (var form = new ZForm(testDec))
			using (var userControl = new AUImportMessageUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();
				CombineAssertions(() =>
				{
					AssertNotNull("User control should have AQISServicePaymentAmount column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.AQISServicePaymentAmount]);
					AssertNotNull("User control should have TotalAmountPayable column", userControl.EntriesBoundGrid.Columns[Customs.Business.CusEntryHeader.Schema.TotalAmountPayable]);
					AssertNotNull("User control should have WoodLevy column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.WoodLevy]);
					AssertNotNull("User control should have TAndI column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.TAndI]);
					AssertNotNull("User control should have GSTAmount column", userControl.EntriesBoundGrid.Columns[Customs.Business.CusEntryHeader.Schema.GSTAmount]);
					AssertNotNull("User control should have LCTAmount column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.LCTAmount]);
					AssertNotNull("User control should have WETAmount column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.WETAmount]);
					AssertNotNull("User control should have CustomsFactor column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CustomsFactor]);
					AssertNotNull("User control should have ImportEntryAdvice column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ImportEntryAdvice]);
					AssertNotNull("User control should have AQISContainerCharges column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.AQISContainerCharges]);
					AssertNotNull("User control should have AQISProcessingCharge column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.AQISProcessingCharge]);
					AssertNotNull("User control should have DeclarationProcessingCharge column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.DeclarationProcessingCharge]);
					AssertNotNull("User control should have TotalPayableAdmin column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.TotalPayableAdmin]);
					AssertNotNull("User control should have OtherEntryCharges column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.OtherEntryCharge]);
				});
			}
		}
	}
}
