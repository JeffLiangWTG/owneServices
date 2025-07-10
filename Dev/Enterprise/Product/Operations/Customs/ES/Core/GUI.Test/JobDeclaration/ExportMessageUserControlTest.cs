using System.Windows.Forms;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class ExportMessageUserControlTest : MessageUserControlTest
	{
		public override void TestSetupEntryHeaderColumns()
		{
			base.TestSetupEntryHeaderColumns();
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new ZForm(declaration))
			using (var userControl = new ExportMessageUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				CombineAssertions(() =>
				{
					var limitDateofArrivalColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ZG_LimitDateOfArrival];
					AssertNotNull("User control should have Limit Date of Arrival column", limitDateofArrivalColumn);
					AssertEquals("Limit Date of Arrival column name is correct", "Limit Date of Arrival", limitDateofArrivalColumn.ColumnStyle.HeaderText);

					var clearanceResultColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.FormattedClearanceResult];
					AssertNotNull("User control should have Clearance Result column", clearanceResultColumn);
					AssertEquals("Clearance Result column name is correct", "Clearance Result", clearanceResultColumn.ColumnStyle.HeaderText);

					var eadPrintProcedureColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.FormattedEADPrint];
					AssertNotNull("User control should have EAD Print Procedure column", eadPrintProcedureColumn);
					AssertEquals("EAD Print Procedure column name is correct", "EAD Print Procedure", eadPrintProcedureColumn.ColumnStyle.HeaderText);

					var csvT2LColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ZG_CSVT2L];
					AssertNotNull("User control should have CSV T2L column", csvT2LColumn);
					AssertEquals("CSV T2L column name is correct", "CSV T2L", csvT2LColumn.ColumnStyle.HeaderText);

					var indirectExportColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.IndirectExport];
					AssertNotNull("User control should have Indirect Export column", indirectExportColumn);
					AssertEquals("Indirect Export column name is correct", "Indirect Export", indirectExportColumn.ColumnStyle.HeaderText);

					var csvExittCertificateColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ZG_CSVExitCertificate];
					AssertNotNull("User control should have CSV Exit Certificate column", csvExittCertificateColumn);
					AssertEquals("Indirect Export column name is correct", "CSV Exit Certificate", csvExittCertificateColumn.ColumnStyle.HeaderText);
				});
			}
		}

		public void TestEntryLineAdditionalDataUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new ZForm(declaration))
			using (var userControl = new ExportMessageUserControl())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();
				AssertType(typeof(ExportEntryLineAdditionalDataUserControl), userControl.FindSingle<EntryLineAdditionalDataUserControl>("EntryLineAdditionalDataUserControl"));
			}
		}

		protected override MessageUserControl GetControlToTest() => new ExportMessageUserControl();
	}
}
