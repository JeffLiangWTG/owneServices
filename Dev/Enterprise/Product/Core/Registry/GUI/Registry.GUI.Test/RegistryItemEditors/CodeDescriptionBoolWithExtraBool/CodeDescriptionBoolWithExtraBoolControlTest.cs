using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionBoolWithExtraBoolControl))]
	sealed class CodeDescriptionBoolWithExtraBoolControlTest : RegistryZUserControlTestCase
	{
		[RequiresSTA]
		public void TestBoolColumnVisible()
		{
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionBoolWithExtraBoolControl();
				control.SetupBoolColumn("BoolCaption", true, "Bool2Caption", true);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 4, control.CodeDescriptionBoolGrid.Columns.Count);

				int expectedBoolColumnWidth;
				int expectedBool2ColumnWidth;
#if !WINZOR
				using (var graphic = Graphics.FromHwnd(control.CodeDescriptionBoolGrid.Handle))
#else
				using (var graphic = new BGraphics())
#endif
				{
					expectedBoolColumnWidth = (int)graphic.MeasureString("BoolCaption", control.CodeDescriptionBoolGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
					expectedBool2ColumnWidth = (int)graphic.MeasureString("Bool2Caption", control.CodeDescriptionBoolGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
				}

				AssertEquals("CodeDescriptionBoolGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("BoolColumn Caption", "BoolCaption", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.BoolColumnName].ColumnStyle.HeaderText);
				AssertEquals("BoolColumn Width", expectedBoolColumnWidth, control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.BoolColumnName].ColumnStyle.Width);
				AssertEquals("Bool2Column Caption", "Bool2Caption", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.Bool2ColumnName].ColumnStyle.HeaderText);
				AssertEquals("Bool2Column Width", expectedBool2ColumnWidth, control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.Bool2ColumnName].ColumnStyle.Width);
			}
		}

		public void TestBoolColumnNotVisible()
		{
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionBoolWithExtraBoolControl();
				control.SetupBoolColumn("", false, "", false);
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 2, control.CodeDescriptionBoolGrid.Columns.Count);
				AssertEquals("CodeDescriptionBoolGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.DescriptionColumnName].ColumnStyle.HeaderText);
			}
		}

		public void TestGridCannotBeSorted()
		{
			using (var control = new CodeDescriptionBoolWithExtraBoolControl())
			{
				AssertEquals("CodeDescriptionBoolGrid.AllowSorting", false, control.CodeDescriptionBoolGrid.AllowSorting);
			}
		}

		public void TestOnlyBoolColumnIsEditable()
		{
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionBoolWithExtraBoolControl();
				control.SetupBoolColumn("Bool", true, "Bool2", true);
				control.SetupEditMode(true);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 3, control.CodeDescriptionBoolGrid.Columns.Count);

				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.ReadOnly", true, control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.DescriptionColumnName].ColumnStyle.ReadOnly);
				AssertEquals("CodeDescriptionBoolGrid.Columns['Bool'].ColumnStyle.HeaderText", "Bool", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.BoolColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['Bool2'].ColumnStyle.HeaderText", "Bool2", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.Bool2ColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.RemoveAction", RemoveAction.NoRemovePossible, control.CodeDescriptionBoolGrid.RemoveAction);

				control = new CodeDescriptionBoolWithExtraBoolControl();
				control.SetupBoolColumn("", false, "", false);
				control.SetupEditMode(true);
				form.Controls.Add(control);
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 2, control.CodeDescriptionBoolGrid.Columns.Count);

				AssertEquals("CodeDescriptionBoolGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.ReadOnly", false, control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.DescriptionColumnName].ColumnStyle.ReadOnly);
				AssertNotEquals("CodeDescriptionBoolGrid.RemoveAction", RemoveAction.NoRemovePossible, control.CodeDescriptionBoolGrid.RemoveAction);
			}
		}

		public void TestNotOnlyBoolColumnIsEditable()
		{
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionBoolWithExtraBoolControl();
				control.SetupBoolColumn("Bool", true, "Bool2", true);
				control.SetupEditMode(false);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 4, control.CodeDescriptionBoolGrid.Columns.Count);

				AssertEquals("CodeDescriptionBoolGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.ReadOnly", false, control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.DescriptionColumnName].ColumnStyle.ReadOnly);
				AssertEquals("CodeDescriptionBoolGrid.Columns['Bool'].ColumnStyle.HeaderText", "Bool", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.BoolColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['Bool2'].ColumnStyle.HeaderText", "Bool2", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.Bool2ColumnName].ColumnStyle.HeaderText);
				AssertNotEquals("CodeDescriptionBoolGrid.RemoveAction", RemoveAction.NoRemovePossible, control.CodeDescriptionBoolGrid.RemoveAction);

				control = new CodeDescriptionBoolWithExtraBoolControl();
				control.SetupBoolColumn("", false, "", false);
				control.SetupEditMode(false);
				form.Controls.Add(control);
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 2, control.CodeDescriptionBoolGrid.Columns.Count);

				AssertEquals("CodeDescriptionBoolGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['Code'].ColumnStyle.ReadOnly", false, control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolWithExtraBoolControl.CodeColumnName].ColumnStyle.ReadOnly);
				AssertNotEquals("CodeDescriptionBoolGrid.RemoveAction", RemoveAction.NoRemovePossible, control.CodeDescriptionBoolGrid.RemoveAction);
			}
		}

		#region Implementation

		protected override RegistryZUserControl GetNewControl()
		{
			return new CodeDescriptionBoolWithExtraBoolControl();
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new CodeDescriptionBoolWithExtraBoolCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CodeDescriptionBoolWithExtraBoolControl)control).CodeDescriptionBoolGrid.ReadOnly;
		}

		#endregion
	}
}
