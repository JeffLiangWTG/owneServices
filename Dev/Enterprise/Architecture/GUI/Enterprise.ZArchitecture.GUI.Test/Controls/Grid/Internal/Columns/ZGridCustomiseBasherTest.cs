using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	[TestedType(typeof(ZGridCustomise))]
	sealed class ZGridCustomiseBasherTest : ZFormBasherTest
	{
		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "columnSearchBox" || control.Name == "CustomColumnsCheckBox" || base.ShouldIgnoreMissingBindingMember(control);
		}

		#region Overrides of ZFormBasherTest

		protected override Form GetFormToBashCore()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var form = new ZForm(dummy);

			var grid = new ZGrid { Location = new Point(0, 0), Size = new Size(200, 200) };
			form.Controls.Add(grid);

			var info = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code };
			grid.ColumnStyles.Add(info);

			grid.SetDataBinding(dummy, "Collection", dummy.Collection.GetType().Name);

			var keyProvider = new LegacyDataGridLayoutContextKeyProvider(grid);
			var customiseBizObj = new ZGridCustomiseBizObj(new string[] { keyProvider.ContextKeyForStmModuleFilter }, new string[] { keyProvider.ContextKeyForStmData }, null, ZGuid.Empty);

			var gridCustomise = new ZGridCustomiseForTest(grid.Columns, grid.Columns, true, customiseBizObj);
			gridCustomise.Disposed += ((sender, e) => form.Dispose());
			gridCustomise.MoveUpButtonExposed.Enabled = false;
			gridCustomise.MoveDownButtonExposed.Enabled = false;

			return gridCustomise;
		}

		public class ZGridCustomiseForTest : ZGridCustomise
		{
			public ZGridCustomiseForTest(ZGridColumns currentColumns, ZGridColumns defaultColumns, bool shouldToolStripBeAvailable, ZGridCustomiseBizObj customiseBizObj) : base(currentColumns, defaultColumns, shouldToolStripBeAvailable, customiseBizObj)
			{
			}

			public ZButton MoveUpButtonExposed
			{
				get { return MoveUpButton; }
			}

			public ZButton MoveDownButtonExposed
			{
				get { return MoveDownButton; }
			}
		}

		#endregion
	}
}
