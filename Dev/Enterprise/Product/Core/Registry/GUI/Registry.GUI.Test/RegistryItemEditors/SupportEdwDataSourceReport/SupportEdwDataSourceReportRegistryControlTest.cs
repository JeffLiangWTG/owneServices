using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(SupportEdwDataSourceReportRegistryControl))]
	public class SupportEdwDataSourceReportRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new SupportEdwDataSourceReportCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((SupportEdwDataSourceReportRegistryControl)control).ReadOnly;
		}

		public void TestControlColumns()
		{
			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new SupportEdwDataSourceReportRegistryControl())
			{
				var columns = creditReportsRegistryControl.ReportListGrid.ColumnStyles;
				CombineAssertions(() =>
				{
					AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[0].GetType());
					AssertEquals("ReportName", ((ZTextBoxColumnStyleInfo)columns[0]).ColumnName);
					AssertContains("Report Name", ((ZTextBoxColumnStyleInfo)columns[0]).CaptionResourceString.ToString());

					AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[1].GetType());
					AssertEquals("BusinessContext", ((ZTextBoxColumnStyleInfo)columns[1]).ColumnName);
					AssertContains("Report Business Context", ((ZTextBoxColumnStyleInfo)columns[1]).CaptionResourceString.ToString());
				});
			}
		}
	}
}
