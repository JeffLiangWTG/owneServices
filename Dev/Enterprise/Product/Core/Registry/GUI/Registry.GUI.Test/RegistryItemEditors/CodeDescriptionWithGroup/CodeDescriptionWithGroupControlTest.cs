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
	[TestedType(typeof(CodeDescriptionWithGroupControl))]
	sealed class CodeDescriptionWithGroupControlTest : RegistryZUserControlTestCase
	{
		public void TestDescriptionColumn()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var key = "91A087B0-08AD-4951-9A2C-EF2C0B791906";
				mockChs.Put(key, new ResourceStringData(key, "描述"));
				var codeDescriptionWithGroupCollection = new CodeDescriptionWithGroupCollection();
				var codeDescriptionBool = codeDescriptionWithGroupCollection.AddNew();
				codeDescriptionBool.Description = ResString.GetMultilingualString(key, "description");
				codeDescriptionBool.EnglishDescription = "description";

				var control = new CodeDescriptionWithGroupControl(true);
				AssertDescriptionColumn(control, codeDescriptionWithGroupCollection, typeof(ResourceString), "描述");

				control = new CodeDescriptionWithGroupControl();
				AssertDescriptionColumn(control, codeDescriptionWithGroupCollection, typeof(ZString), "description");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule", Justification = "Testing")]
		void AssertDescriptionColumn(CodeDescriptionWithGroupControl control, object dataSource, Type columnDataType, string columnValue)
		{
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(dataSource, null);
				control.CodeDescriptionWithGroupGrid.Select(0);
				var selectedElement = ((BusinessObject)control.CodeDescriptionWithGroupGrid.ListManager.Current)[control.descriptionColumnStyleInfo.ColumnName];
				AssertEquals(columnDataType, selectedElement.GetType());

				AssertEquals(columnValue,
					columnDataType == typeof(ResourceString)
						? ((MultilingualString)selectedElement).ToString(Core.SharedConstants.Languages.ChineseSimplified)
						: selectedElement.ToString());
			}
		}

		public void TestGroupColumnVisible()
		{
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionWithGroupControl();
				control.SetupGroupColumn("GroupCaption", true);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 3, control.CodeDescriptionWithGroupGrid.Columns.Count);

				int expectedColumnWidth;
#if !WINZOR
				using (var graphic = Graphics.FromHwnd(control.CodeDescriptionWithGroupGrid.Handle))
#else
				using (var graphic = new BGraphics())
#endif
				{
					expectedColumnWidth = (int)graphic.MeasureString("GroupCaption", control.CodeDescriptionWithGroupGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
				}

				AssertEquals("CodeDescriptionBoolGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("GroupColumn Caption", "GroupCaption", control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.GroupColumnName].ColumnStyle.HeaderText);
				AssertEquals("GroupColumn Width", expectedColumnWidth, control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.GroupColumnName].ColumnStyle.Width);
			}
		}

		public void TestGroupColumnNotVisible()
		{
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionWithGroupControl();
				control.SetupGroupColumn("HiddenGroup", false);
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 2, control.CodeDescriptionWithGroupGrid.Columns.Count);

				AssertEquals("CodeDescriptionBoolGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionBoolGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.DescriptionColumnName].ColumnStyle.HeaderText);
			}
		}

		public void TestGridCannotBeSorted()
		{
			using (var control = new CodeDescriptionWithGroupControl())
			{
				AssertEquals("CodeDescriptionWithGroupGrid.AllowSorting", false, control.CodeDescriptionWithGroupGrid.AllowSorting);
			}
		}

		public void TestOnlyGroupColumnIsEditable()
		{
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionWithGroupControl();
				control.SetupGroupColumn("Group", true);
				control.SetupEditMode(true);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionBoolGrid.Columns.Count", 3, control.CodeDescriptionWithGroupGrid.Columns.Count);

				AssertEquals("CodeDescriptionWithGroupGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithGroupGrid.Columns['EnglishDescription'].ColumnStyle.ReadOnly", true, control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.DescriptionColumnName].ColumnStyle.ReadOnly);
				AssertEquals("CodeDescriptionWithGroupGrid.Columns['Code'].ColumnStyle.ReadOnly", true, control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.CodeColumnName].ColumnStyle.ReadOnly);
				AssertEquals("CodeDescriptionWithGroupGrid.Columns['Group'].ColumnStyle.HeaderText", "Group", control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.GroupColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithGroupGrid.Columns['Group'].ColumnStyle.ReadOnly", false, control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.GroupColumnName].ColumnStyle.ReadOnly);
				AssertEquals("CodeDescriptionWithGroupGrid.RemoveAction", RemoveAction.NoRemovePossible, control.CodeDescriptionWithGroupGrid.RemoveAction);
				AssertEquals("CodeDescriptionWithGroupGrid.Columns['Code'].IsVisible", true, control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.CodeColumnName].IsVisible);
				AssertEquals("CodeDescriptionWithGroupGrid.Columns['EnglishDescription'].IsVisible", true, control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.DescriptionColumnName].IsVisible);
				AssertEquals("CodeDescriptionWithGroupGrid.Columns['Group'].IsVisible", true, control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.GroupColumnName].IsVisible);

				control = new CodeDescriptionWithGroupControl();
				control.SetupGroupColumn("", false);
				control.SetupEditMode(true);
				form.Controls.Add(control);
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionWithGroupGrid.Columns.Count", 2, control.CodeDescriptionWithGroupGrid.Columns.Count);

				AssertEquals("CodeDescriptionWithGroupGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithGroupGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithGroupGrid.Columns['EnglishDescription'].ColumnStyle.ReadOnly", false, control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.DescriptionColumnName].ColumnStyle.ReadOnly);
				AssertEquals("CodeDescriptionWithGroupGrid.Columns['Code'].ColumnStyle.ReadOnly", false, control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.CodeColumnName].ColumnStyle.ReadOnly);
				AssertNotEquals("CodeDescriptionWithGroupGrid.RemoveAction", RemoveAction.NoRemovePossible, control.CodeDescriptionWithGroupGrid.RemoveAction);
			}
		}

		public void TestNoColumnsAreEditable()
		{
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionWithGroupControl();
				control.SetupGroupColumn("Group", true);
				control.SetupEditMode(false);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionWithGroupGrid.Columns.Count", 3, control.CodeDescriptionWithGroupGrid.Columns.Count);

				AssertEquals("CodeDescriptionWithGroupGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithGroupGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithGroupGrid.Columns['EnglishDescription'].ColumnStyle.ReadOnly", false, control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.DescriptionColumnName].ColumnStyle.ReadOnly);
				AssertEquals("CodeDescriptionWithGroupGrid.Columns['Group'].ColumnStyle.HeaderText", "Group", control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.GroupColumnName].ColumnStyle.HeaderText);
				AssertNotEquals("CodeDescriptionWithGroupGrid.RemoveAction", RemoveAction.NoRemovePossible, control.CodeDescriptionWithGroupGrid.RemoveAction);

				control = new CodeDescriptionWithGroupControl();
				control.SetupGroupColumn("", false);
				control.SetupEditMode(false);
				form.Controls.Add(control);
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionWithGroupGrid.Columns.Count", 2, control.CodeDescriptionWithGroupGrid.Columns.Count);

				AssertEquals("CodeDescriptionWithGroupGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithGroupGrid.Columns['EnglishDescription'].ColumnStyle.HeaderText", "Description", control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.DescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithGroupGrid.Columns['EnglishDescription'].ColumnStyle.ReadOnly", false, control.CodeDescriptionWithGroupGrid.Columns[CodeDescriptionWithGroupControl.DescriptionColumnName].ColumnStyle.ReadOnly);
				AssertNotEquals("CodeDescriptionWithGroupGrid.RemoveAction", RemoveAction.NoRemovePossible, control.CodeDescriptionWithGroupGrid.RemoveAction);
			}
		}

		public void TestHumanReadableName()
		{
			using (var control = new CodeDescriptionWithGroupControl())
			{
				control.SetupGroupColumn("My Caption for Group", true);
				control.SetupEditMode(false);
				var dataSource = new CodeDescriptionWithGroupCollection();
				dataSource.Add(new CodeDescriptionWithGroup());
				control.SetDataBinding(dataSource, null);
				control.CodeDescriptionWithGroupGrid_CurrentCellChanged(dataSource, new EventArgs());

				var row = dataSource[0];
				AssertEquals("Human readable name for column Group", "My Caption for Group", row.GroupInfo.HumanReadableName);
			}
		}

		#region Implementation

		protected override RegistryZUserControl GetNewControl()
		{
			return new CodeDescriptionWithGroupControl();
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new CodeDescriptionWithGroupCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CodeDescriptionWithGroupControl)control).CodeDescriptionWithGroupGrid.ReadOnly;
		}

		#endregion
	}
}
