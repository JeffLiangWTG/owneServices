using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Registry.Testing
{
	[TestedType(typeof(SendAcknowledgementsRegistryItemControl))]
	class SendAcknowledgementsRegistryItemControlTest : RegistryZUserControlTestCase
	{
		public void TestControls_AcknowledgementsGrid()
		{
			using (var form = new ZForm())
			using (var control = new SendAcknowledgementsRegistryItemControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var acknowledgementsGrid = control.FindSingle<ZGrid>("AcknowledgementsGrid");
					var columnNames = acknowledgementsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
					AssertSequencesEqual("Columns", new[] { SendAcknowledgementsRegistry.Schema.EBSCode, SendAcknowledgementsRegistry.Schema.SendGroupPK }, columnNames);
					AssertEquals("EBSCode", 105, acknowledgementsGrid.GetColumnStyle(SendAcknowledgementsRegistry.Schema.EBSCode).Width);
					AssertEquals("SendGroupPK", 120, acknowledgementsGrid.GetColumnStyle(SendAcknowledgementsRegistry.Schema.SendGroupPK).Width);
				});
			}
		}

		protected override IBusiness GetNewBusinessEntity() => new SendAcknowledgementsRegistryCollection();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly || businessEntity.IsReadOnly;
	}
}
