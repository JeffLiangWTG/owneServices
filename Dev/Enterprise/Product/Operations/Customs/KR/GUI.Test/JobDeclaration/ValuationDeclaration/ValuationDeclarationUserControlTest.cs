using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class ValuationDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestValuationLineGrid()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._5SM;
			using (var form = new MiscDeclarationForm(declaration))
			{
				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var valuationTab = tabControl.FindSingle<ZTabPage>("ValuationTabPage");

				using (var control = valuationTab.Controls[0])
				{
					var valuationLineGroupBox = control.FindSingle<ZGroupBox>("ValuationLineGroupBox");
					var grid = valuationLineGroupBox.FindSingle<ZGrid>("ValuationLineGrid");

					var index = 0;
					AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, JobComInvoiceLine.Schema.JI_LineNo);
					AssertEquals(((ZCodeFindBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, JobComInvoiceLine.Schema.JI_PartNo);
					AssertEquals(((Universal.GUI.TariffColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, JobComInvoiceLine.Schema.JI_FormattedTariff);
					AssertEquals(((ZMultiLineTextBoxColumnInfo)grid.ColumnStyles[index++]).ColumnName, JobComInvoiceLine.Schema.JI_Description);
					AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, JobComInvoiceLine.Schema.JI_Model);
					AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, JobComInvoiceLine.Schema.JI_BrandName);
					AssertEquals(((ZMultiLineTextBoxColumnInfo)grid.ColumnStyles[index++]).ColumnName, nameof(JobComInvoiceLine.JI_Ingredient));
				}
			}
		}
	}
}
