using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(SupportEdwDataSourceReportRegistryItemEditor))]
	public class SupportEdwDataSourceReportRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new SupportEdwDataSourceReportRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, SupportEdwDataSourceReportCollection.GetDefault());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new SupportEdwDataSourceReportRegistryItemEditor(null, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(SupportEdwDataSourceReportRegistryControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var reports = new SupportEdwDataSourceReportCollection();
			var supportEdwDataSourceReport = reports.AddNew();
			var report = Factory.LoadTop1<StmMenuItem>(new ZQuery());
			supportEdwDataSourceReport.ReportName = report.SU_MenuName;
			supportEdwDataSourceReport.BusinessContext = report.SU_BusinessContext;

			return new[] { reports };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((SupportEdwDataSourceReportRegistryControl)editorPane).ReadOnly;
		}
	}
}
