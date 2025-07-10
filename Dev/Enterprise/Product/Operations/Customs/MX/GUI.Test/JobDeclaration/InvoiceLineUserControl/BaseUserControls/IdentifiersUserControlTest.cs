using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI.Testing
{
	class IdentifiersUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new IdentifiersUserControl())
			{
				AssertEquals("DataSourceType", typeof(JobComInvoiceLine), control.DataSourceType);
			}
		}

		public void TestComponents()
		{
			using (var control = new IdentifiersUserControl())
			{
				AssertType<ZGrid>("IdentifiersGrid must be ZGrid", control.IdentifiersGrid);
				AssertType<ZGroupBox>("IdentifiersGroupBox must be ZGroupBox", control.IdentifiersGroupBox);
			}
		}

		public void TestColumnsTypes()
		{
			using (var form = new ZForm(Factory.New<JobComInvoiceLine>()))
			using (var control = new IdentifiersUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("CSI_Code must be ZDropEdit", typeof(ZDropEditColumnStyle), control.IdentifiersGrid.Columns[Identifier.Schema.CSI_Code].ColumnStyle.GetType());
				AssertEquals("Applicability must be ZTextBoxColumnStyle", typeof(ZTextBoxColumnStyle), control.IdentifiersGrid.Columns[Identifier.Schema.Applicability].ColumnStyle.GetType());
				AssertEquals("CSI_ReferenceNumber must be ZMultiControlColumnStyle", typeof(ZMultiControlColumnStyle), control.IdentifiersGrid.Columns[Identifier.Schema.CSI_ReferenceNumber].ColumnStyle.GetType());
				AssertEquals("FillGuidance1 must be ZTextBoxColumnStyle", typeof(ZTextBoxColumnStyle), control.IdentifiersGrid.Columns[Identifier.Schema.FillGuidance1].ColumnStyle.GetType());
				AssertEquals("CSI_ReferenceNumber2 must be ZMultiControlColumnStyle", typeof(ZMultiControlColumnStyle), control.IdentifiersGrid.Columns[Identifier.Schema.CSI_ReferenceNumber2].ColumnStyle.GetType());
				AssertEquals("FillGuidance2 must be ZTextBoxColumnStyle", typeof(ZTextBoxColumnStyle), control.IdentifiersGrid.Columns[Identifier.Schema.FillGuidance2].ColumnStyle.GetType());
				AssertEquals("CSI_Description must be ZMultiControlColumnStyle", typeof(ZMultiControlColumnStyle), control.IdentifiersGrid.Columns[Identifier.Schema.CSI_Description].ColumnStyle.GetType());
				AssertEquals("FillGuidance3 must be ZTextBoxColumnStyle", typeof(ZTextBoxColumnStyle), control.IdentifiersGrid.Columns[Identifier.Schema.FillGuidance3].ColumnStyle.GetType());
			}
		}
	}
}
