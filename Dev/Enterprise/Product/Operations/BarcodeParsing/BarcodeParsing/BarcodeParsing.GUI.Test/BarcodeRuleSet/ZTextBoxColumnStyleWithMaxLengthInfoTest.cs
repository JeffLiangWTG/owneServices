using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BarcodeParsing.Business;
using Enterprise.BarcodeParsing.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BarcodeParsing.GUI.Testing
{
	class ZTextBoxColumnStyleWithMaxLengthInfoTest : BarcodeParsingTestCase
	{
		#region TestColumnStyle

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Prevent change to base class as we would lose context of the actual schema class")]
		public void TestColumnStyle()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.Rules.AddNew();

			using (var form = new ZForm(ruleSet))
			{
				var grid = new ZGrid();
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(grid, "Rules");

				var info = new ZTextBoxColumnStyleWithMaxLengthInfo();
				info.ColumnName = BarcodeRule.Schema.BRU_Name;
				info.MaxLength = 5;
				grid.ColumnStyles.Add(info);

				form.Controls.Add(grid);

				form.Show();

				grid.Focus();
				grid.CurrentCell = new DataGridCell(1, 1);

				var columnStyle = (ZTextBoxColumnStyleWithMaxLength)grid.Columns[BarcodeRule.Schema.BRU_Name].ColumnStyle;
				KeySender.SendKeyPress(columnStyle.TextBox, Keys.N); // Send key to trigger 'Edit' on column Style, needs to be currently focused cell
				AssertEquals(5, columnStyle.TextBox.MaxLength);
			}
		}

		#endregion

		#region TestColumnStyleType

		public void TestColumnStyleType()
		{
			AssertEquals(typeof(ZTextBoxColumnStyleWithMaxLength), new ZTextBoxColumnStyleWithMaxLengthInfo().ColumnStyleType);
		}

		#endregion

		#region TestMaxLength

		public void TestMaxLength()
		{
			var info = new ZTextBoxColumnStyleWithMaxLengthInfo();
			AssertEquals(-1, info.MaxLength);

			info.MaxLength = 10;
			AssertEquals(10, info.MaxLength);
		}

		#endregion
	}
}
