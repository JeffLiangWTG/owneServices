using System.Drawing;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GLPresentationJournalCategoryControl))]
	internal class GLPresentationJournalCategoryControlTest : RegistryZUserControlTestCase
	{
		public void TestBoolColumnVisible()
		{
			using (var form = new ZForm())
			{
				var control = new GLPresentationJournalCategoryControl();
				control.SetupBoolColumn("BoolCaption", "Bool2Caption", "Bool3Caption", "Bool4Caption");

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns.Count", 7, control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns.Count);

				int expectedBoolColumnWidth;
				int expectedBool2ColumnWidth;
				int expectedBool3ColumnWidth;
				int expectedBool4ColumnWidth;

				using (Graphics graphic = Graphics.FromHwnd(control.GLPresentationJournalCategoryGrid_ForTestOnly.Handle))
				{
					expectedBoolColumnWidth = (int)graphic.MeasureString("BoolCaption", control.GLPresentationJournalCategoryGrid_ForTestOnly.Font).Width + 5;
					expectedBool2ColumnWidth = (int)graphic.MeasureString("Bool2Caption", control.GLPresentationJournalCategoryGrid_ForTestOnly.Font).Width + 5;
					expectedBool3ColumnWidth = (int)graphic.MeasureString("Bool3Caption", control.GLPresentationJournalCategoryGrid_ForTestOnly.Font).Width + 5;
					expectedBool4ColumnWidth = (int)graphic.MeasureString("Bool4Caption", control.GLPresentationJournalCategoryGrid_ForTestOnly.Font).Width + 5;
				}

				AssertEquals("CodeDescriptionBoolGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['Parent'].ColumnStyle.HeaderText", "Parent", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.ParentCodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("BoolColumn Caption", "BoolCaption", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.BoolColumnName].ColumnStyle.HeaderText);
				AssertEquals("BoolColumn Width", expectedBoolColumnWidth, control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.BoolColumnName].ColumnStyle.Width);
				AssertEquals("Bool2Column Caption", "Bool2Caption", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.Bool2ColumnName].ColumnStyle.HeaderText);
				AssertEquals("Bool2Column Width", expectedBool2ColumnWidth, control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.Bool2ColumnName].ColumnStyle.Width);
				AssertEquals("Bool3Column Caption", "Bool3Caption", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.Bool3ColumnName].ColumnStyle.HeaderText);
				AssertEquals("Bool3Column Width", expectedBool3ColumnWidth, control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.Bool3ColumnName].ColumnStyle.Width);
				AssertEquals("Bool4Column Caption", "Bool4Caption", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.Bool4ColumnName].ColumnStyle.HeaderText);
				AssertEquals("Bool4Column Width", expectedBool4ColumnWidth, control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.Bool4ColumnName].ColumnStyle.Width);
			}
		}

		public void TestGridCannotBeSorted()
		{
			using (var control = new GLPresentationJournalCategoryControl())
			{
				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.AllowSorting", false, control.GLPresentationJournalCategoryGrid_ForTestOnly.AllowSorting);
			}
		}

		public void TestOnlyBoolColumnIsEditable()
		{
			using (var form = new ZForm())
			{
				var control = new GLPresentationJournalCategoryControl();
				control.SetupBoolColumn("Bool", "Bool2", "Bool3", "Bool4");
				control.SetupEditMode(true);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns.Count", 5, control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns.Count);

				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns['EnglishDescription'].ColumnStyle.ReadOnly", true, control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.DescriptionColumnName].ColumnStyle.ReadOnly);
				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns['Bool'].ColumnStyle.HeaderText", "Bool", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.BoolColumnName].ColumnStyle.HeaderText);
				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns['Bool2'].ColumnStyle.HeaderText", "Bool2", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.Bool2ColumnName].ColumnStyle.HeaderText);
				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns['Bool3'].ColumnStyle.HeaderText", "Bool3", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.Bool3ColumnName].ColumnStyle.HeaderText);
				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns['Bool4'].ColumnStyle.HeaderText", "Bool4", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.Bool4ColumnName].ColumnStyle.HeaderText);
				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.RemoveAction", RemoveAction.NoRemovePossible, control.GLPresentationJournalCategoryGrid_ForTestOnly.RemoveAction);
			}
		}

		public void TestNotOnlyBoolColumnIsEditable()
		{
			using (var form = new ZForm())
			{
				var control = new GLPresentationJournalCategoryControl();
				control.SetupBoolColumn("Bool", "Bool2", "Bool3", "Bool4");
				control.SetupEditMode(false);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns.Count", 7, control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns.Count);

				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns['Code'].ColumnStyle.HeaderText", "Code", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns['Parent'].ColumnStyle.HeaderText", "Parent", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.ParentCodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns['EnglishDescription'].ColumnStyle.ReadOnly", false, control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.DescriptionColumnName].ColumnStyle.ReadOnly);
				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns['Bool'].ColumnStyle.HeaderText", "Bool", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.BoolColumnName].ColumnStyle.HeaderText);
				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns['Bool2'].ColumnStyle.HeaderText", "Bool2", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.Bool2ColumnName].ColumnStyle.HeaderText);
				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns['Bool3'].ColumnStyle.HeaderText", "Bool3", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.Bool3ColumnName].ColumnStyle.HeaderText);
				AssertEquals("GLPresentationJournalCategoryGrid_ForTestOnly.Columns['Bool4'].ColumnStyle.HeaderText", "Bool4", control.GLPresentationJournalCategoryGrid_ForTestOnly.Columns[GLPresentationJournalCategoryControl.Bool4ColumnName].ColumnStyle.HeaderText);
				AssertNotEquals("GLPresentationJournalCategoryGrid_ForTestOnly.RemoveAction", RemoveAction.NoRemovePossible, control.GLPresentationJournalCategoryGrid_ForTestOnly.RemoveAction);
			}
		}

		#region Implementation

		protected override RegistryZUserControl GetNewControl()
		{
			return new GLPresentationJournalCategoryControl();
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new GLPresentationJournalCategoryCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((GLPresentationJournalCategoryControl)control).GLPresentationJournalCategoryGrid_ForTestOnly.ReadOnly;
		}

		#endregion
	}
}
