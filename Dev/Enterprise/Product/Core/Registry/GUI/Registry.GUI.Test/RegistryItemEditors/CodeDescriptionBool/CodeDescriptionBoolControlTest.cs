using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionBoolControl))]
	sealed class CodeDescriptionBoolControlTest : RegistryZUserControlTestCase
	{
		public void TestCodeColumnCaption()
		{
			var codeCaption = "ZCodeCaption";
			var boolCaption = "BoolCaption";
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionBoolControl();
				control.SetupColumns(boolCaption, isBoolColumnVisible: true, isCodeColumnVisible: false);
				control.SetupCodeColumnCaption(codeCaption);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 2, control.CodeDescriptionBoolGrid.Columns.Count);
				AssertNull(control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.CodeColumnName]);
			}

			using (var form = new ZForm())
			{
				var control = new CodeDescriptionBoolControl();
				control.SetupColumns(boolCaption, isBoolColumnVisible: false, isCodeColumnVisible: true);
				control.SetupCodeColumnCaption(codeCaption);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 2, control.CodeDescriptionBoolGrid.Columns.Count);
				AssertEquals("Code Column Caption", codeCaption, control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.CodeColumnName].ColumnStyle.HeaderText);
				CombineAssertions("Column order", () =>
				{
					AssertEquals(codeCaption, control.CodeDescriptionBoolGrid.Columns[0].ColumnStyle.HeaderText);
					AssertEquals("Description", control.CodeDescriptionBoolGrid.Columns[1].ColumnStyle.HeaderText);
				});
			}

			using (var form = new ZForm())
			{
				var control = new CodeDescriptionBoolControl();
				control.SetupColumns(boolCaption, isBoolColumnVisible: true, isCodeColumnVisible: true);
				control.SetupCodeColumnCaption(codeCaption);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 3, control.CodeDescriptionBoolGrid.Columns.Count);
				AssertEquals("Code Column Caption", codeCaption, control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.CodeColumnName].ColumnStyle.HeaderText);
				CombineAssertions("Column order", () =>
				{
					AssertEquals(codeCaption, control.CodeDescriptionBoolGrid.Columns[0].ColumnStyle.HeaderText);
					AssertEquals("Description", control.CodeDescriptionBoolGrid.Columns[1].ColumnStyle.HeaderText);
					AssertEquals(boolCaption, control.CodeDescriptionBoolGrid.Columns[2].ColumnStyle.HeaderText);
				});
			}
		}

		public void TestDescriptionColumn()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var key = "91A087B0-08AD-4951-9A2C-EF2C0B791906";
				mockChs.Put(key, new ResourceStringData(key, "描述"));
				var codeDescriptionBoolCollection = new CodeDescriptionBoolCollection();
				var codeDescriptionBool = codeDescriptionBoolCollection.AddNew();
				codeDescriptionBool.Description = ResString.GetMultilingualString(key, "description");
				codeDescriptionBool.EnglishDescription = "description";

				var control = new CodeDescriptionBoolControl(true);
				AssertDescriptionColumn(control, codeDescriptionBoolCollection, typeof(ResourceString), "描述");

				control = new CodeDescriptionBoolControl();
				AssertDescriptionColumn(control, codeDescriptionBoolCollection, typeof(ZString), "description");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule", Justification = "Testing")]
		void AssertDescriptionColumn(CodeDescriptionBoolControl control, object dataSource, Type columnDataType, string columnValue)
		{
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(dataSource, null);
				control.CodeDescriptionBoolGrid.Select(0);
				var selectedElement = ((BusinessObject)control.CodeDescriptionBoolGrid.ListManager.Current)[control.descriptionColumnStyleInfo.ColumnName];
				AssertEquals(columnDataType, selectedElement.GetType());

				AssertEquals(columnValue,
					columnDataType == typeof(ResourceString)
						? ((MultilingualString)selectedElement).ToString(Core.SharedConstants.Languages.ChineseSimplified)
						: selectedElement.ToString());
			}
		}

		public void TestBoolColumnVisible()
		{
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionBoolControl();
				control.SetupColumns("BoolCaption", true, true);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 3, control.CodeDescriptionBoolGrid.Columns.Count);

				int expectedColumnWidth;
#if !WINZOR
				using (var graphic = Graphics.FromHwnd(control.CodeDescriptionBoolGrid.Handle))
#else
				using (var graphic = new BGraphics())
#endif
				{
					expectedColumnWidth = (int)graphic.MeasureString("BoolCaption", control.CodeDescriptionBoolGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
				}

				AssertEquals("CodeDescriptionBoolGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("BoolColumn Caption", "BoolCaption", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.BoolColumnName].ColumnStyle.HeaderText);
				AssertEquals("BoolColumn Width", expectedColumnWidth, control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.BoolColumnName].ColumnStyle.Width);
			}
		}

		[RequiresSTA]
		public void TestBoolColumnNotVisible()
		{
			using (var form = new ZForm())
			{
				CodeDescriptionBoolControl control = new CodeDescriptionBoolControl();
				control.SetupColumns("", false, true);
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 2, control.CodeDescriptionBoolGrid.Columns.Count);

				AssertEquals("CodeDescriptionBoolGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.DescriptionColumnName].ColumnStyle.HeaderText);
			}
		}

		public void TestCodeColumnVisible()
		{
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionBoolControl();

				control.SetupEditMode(true);
				control.SetupColumns((NoResString)"", true, false);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals(false, control.IsCodeColumnVisible);
				AssertNull("Code Column is hidden when IsCodeColumnVisible is false", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.CodeColumnName]);
			}

			using (var form = new ZForm())
			{
				var control = new CodeDescriptionBoolControl();

				control.SetupEditMode(true);
				control.SetupColumns((NoResString)"", true, true);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals(true, control.IsCodeColumnVisible);
				AssertNotNull("Code Column is visible if IsCodeColumnVisible is true", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.CodeColumnName]);
			}
		}

		public void TestGridCannotBeSorted()
		{
			using (var control = new CodeDescriptionBoolControl())
			{
				AssertEquals("CodeDescriptionBoolGrid.AllowSorting", false, control.CodeDescriptionBoolGrid.AllowSorting);
			}
		}

		public void TestOnlyBoolColumnIsEditable()
		{
			using (var form = new ZForm())
			{
				CodeDescriptionBoolControl control = new CodeDescriptionBoolControl();
				control.SetupColumns("Bool", true, false);
				control.SetupEditMode(true);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 2, control.CodeDescriptionBoolGrid.Columns.Count);

				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.ReadOnly", true, control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.DescriptionColumnName].ColumnStyle.ReadOnly);
				AssertEquals("CodeDescriptionBoolGrid.Columns['Bool'].ColumnStyle.HeaderText", "Bool", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.BoolColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.RemoveAction", RemoveAction.NoRemovePossible, control.CodeDescriptionBoolGrid.RemoveAction);

				control = new CodeDescriptionBoolControl();
				control.SetupColumns("", false, false);
				control.SetupEditMode(true);
				form.Controls.Add(control);
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 2, control.CodeDescriptionBoolGrid.Columns.Count);

				AssertEquals("CodeDescriptionBoolGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.ReadOnly", false, control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.DescriptionColumnName].ColumnStyle.ReadOnly);
				AssertNotEquals("CodeDescriptionBoolGrid.RemoveAction", RemoveAction.NoRemovePossible, control.CodeDescriptionBoolGrid.RemoveAction);
			}
		}

		public void TestNotOnlyBoolColumnIsEditable()
		{
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionBoolControl();
				control.SetupColumns("Bool", true, true);
				control.SetupEditMode(false);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 3, control.CodeDescriptionBoolGrid.Columns.Count);

				AssertEquals("CodeDescriptionBoolGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.ReadOnly", false, control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.DescriptionColumnName].ColumnStyle.ReadOnly);
				AssertEquals("CodeDescriptionBoolGrid.Columns['Bool'].ColumnStyle.HeaderText", "Bool", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.BoolColumnName].ColumnStyle.HeaderText);
				AssertNotEquals("CodeDescriptionBoolGrid.RemoveAction", RemoveAction.NoRemovePossible, control.CodeDescriptionBoolGrid.RemoveAction);

				control = new CodeDescriptionBoolControl();
				control.SetupColumns("", false, false);
				control.SetupEditMode(false);
				form.Controls.Add(control);
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 2, control.CodeDescriptionBoolGrid.Columns.Count);

				AssertEquals("CodeDescriptionBoolGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.ReadOnly", false, control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.DescriptionColumnName].ColumnStyle.ReadOnly);
				AssertNotEquals("CodeDescriptionBoolGrid.RemoveAction", RemoveAction.NoRemovePossible, control.CodeDescriptionBoolGrid.RemoveAction);
			}
		}

		public void TestForceDescriptionColumnReadOnly()
		{
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionBoolControl(true);

				control.SetupEditMode(true, true);
				control.SetupColumns((NoResString)"", true, false);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				Assert("Description Column is ReadOnly when ForceDescriptionColumnReadOnly is true", control.CodeDescriptionBoolGrid.Columns[CodeDescriptionBoolControl.DescriptionColumnName_Translatable].ColumnStyle.ReadOnly);
			}
		}

		#region Implementation

		protected override RegistryZUserControl GetNewControl()
		{
			return new CodeDescriptionBoolControl();
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new CodeDescriptionBoolCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CodeDescriptionBoolControl)control).CodeDescriptionBoolGrid.ReadOnly;
		}

		#endregion
	}
}
