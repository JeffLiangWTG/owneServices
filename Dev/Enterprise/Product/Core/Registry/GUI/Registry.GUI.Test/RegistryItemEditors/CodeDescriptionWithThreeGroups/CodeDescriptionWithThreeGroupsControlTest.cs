using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionWithThreeGroupsControl))]
	sealed class CodeDescriptionWithThreeGroupsControlTest : RegistryZUserControlTestCase
	{
		// AWU: MainDescription is not translatable because it is set from the description related to the value in Group / Group2 field,
		// see GetEnglishDescription() method in "CodeDescriptionWithThreeGroups.cs"
		// Similar logic applies to Group3 and ExtraDescription.
		//public void TestMainDescriptionColumn(){ }

		[RequiresSTA]
		public void TestGroupColumnVisible()
		{
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionWithThreeGroupsControl();
				control.SetupGroupColumns("GroupCaption", "Group2Caption", "Group3Caption", true);
				control.SetupMainDescriptionColumn("MainDescription");
				control.SetupExtraDescriptionColumn("ExtraDescription");

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns.Count", 6, control.CodeDescriptionWithThreeGroupsGrid.Columns.Count);

				int expectedColumnWidth;
				int expectedColumn2Width;
				int expectedColumn3Width;
#if !WINZOR
				using (var graphic = Graphics.FromHwnd(control.CodeDescriptionWithThreeGroupsGrid.Handle))
#else
				using (var graphic = new BGraphics())
#endif
				{
					expectedColumnWidth = (int)graphic.MeasureString("GroupCaption", control.CodeDescriptionWithThreeGroupsGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
					expectedColumn2Width = (int)graphic.MeasureString("Group2Caption", control.CodeDescriptionWithThreeGroupsGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
					expectedColumn3Width = (int)graphic.MeasureString("Group3Caption", control.CodeDescriptionWithThreeGroupsGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
				}

				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['MainDescription'].ColumnStyle.HeaderText", "MainDescription", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.MainDescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['ExtraDescription'].ColumnStyle.HeaderText", "ExtraDescription", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.ExtraDescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("GroupColumn Caption", "GroupCaption", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.GroupColumnName].ColumnStyle.HeaderText);
				AssertEquals("GroupColumn Width", expectedColumnWidth, control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.GroupColumnName].ColumnStyle.Width);
				AssertEquals("Group2Column Caption", "Group2Caption", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.Group2ColumnName].ColumnStyle.HeaderText);
				AssertEquals("Group2Column Width", expectedColumn2Width, control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.Group2ColumnName].ColumnStyle.Width);
				AssertEquals("Group3Column Caption", "Group3Caption", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.Group3ColumnName].ColumnStyle.HeaderText);
				AssertEquals("Group3Column Width", expectedColumn3Width, control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.Group3ColumnName].ColumnStyle.Width);
			}
		}

		public void TestGroupColumnNotVisible()
		{
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionWithThreeGroupsControl();
				control.SetupGroupColumns("HiddenGroup", "HiddenGroup", "HiddenGroup3", false);
				control.SetupMainDescriptionColumn("MainDescription");
				control.SetupExtraDescriptionColumn("ExtraDescription");
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns.Count", 3, control.CodeDescriptionWithThreeGroupsGrid.Columns.Count);

				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['MainDescription'].ColumnStyle.HeaderText", "MainDescription", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.MainDescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['ExtraDescription'].ColumnStyle.HeaderText", "ExtraDescription", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.ExtraDescriptionColumnName].ColumnStyle.HeaderText);
			}
		}

		public void TestGridCannotBeSorted()
		{
			using (var control = new CodeDescriptionWithThreeGroupsControl())
			{
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.AllowSorting", false, control.CodeDescriptionWithThreeGroupsGrid.AllowSorting);
			}
		}

		public void TestOnlyGroupColumnsAreEditable()
		{
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionWithThreeGroupsControl();
				control.SetupGroupColumns("Group", "Group2", "Group3", true);
				control.SetupMainDescriptionColumn("MainDescription");
				control.SetupExtraDescriptionColumn("ExtraDescription");
				control.SetupEditMode(true);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns.Count", 6, control.CodeDescriptionWithThreeGroupsGrid.Columns.Count);

				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['MainDescription'].ColumnStyle.HeaderText", "MainDescription", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.MainDescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['MainDescription'].ColumnStyle.ReadOnly", true, control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.MainDescriptionColumnName].ColumnStyle.ReadOnly);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['ExtraDescription'].ColumnStyle.HeaderText", "ExtraDescription", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.ExtraDescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['ExtraDescription'].ColumnStyle.ReadOnly", true, control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.ExtraDescriptionColumnName].ColumnStyle.ReadOnly);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['Group'].ColumnStyle.HeaderText", "Group", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.GroupColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['Group2'].ColumnStyle.HeaderText", "Group2", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.Group2ColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['Group3'].ColumnStyle.HeaderText", "Group3", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.Group3ColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.RemoveAction", RemoveAction.RemoveAndDelete, control.CodeDescriptionWithThreeGroupsGrid.RemoveAction);

				control = new CodeDescriptionWithThreeGroupsControl();
				control.SetupGroupColumns("Group", "Group2", "Group3", false);
				control.SetupMainDescriptionColumn("MainDescription");
				control.SetupExtraDescriptionColumn("ExtraDescription");
				control.SetupEditMode(true);
				form.Controls.Add(control);
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns.Count", 3, control.CodeDescriptionWithThreeGroupsGrid.Columns.Count);

				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['MainDescription'].ColumnStyle.HeaderText", "MainDescription", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.MainDescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['MainDescription'].ColumnStyle.ReadOnly", false, control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.MainDescriptionColumnName].ColumnStyle.ReadOnly);
				AssertNotEquals("CodeDescriptionWithThreeGroupsGrid.RemoveAction", RemoveAction.NoRemovePossible, control.CodeDescriptionWithThreeGroupsGrid.RemoveAction);
			}
		}

		[RequiresSTA]
		public void TestNoColumnsAreEditable()
		{
			using (var form = new ZForm())
			{
				var control = new CodeDescriptionWithThreeGroupsControl();
				control.SetupGroupColumns("Group", "Group2", "Group3", true);
				control.SetupMainDescriptionColumn("MainDescription");
				control.SetupExtraDescriptionColumn("ExtraDescription");
				control.SetupEditMode(false);

				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns.Count", 6, control.CodeDescriptionWithThreeGroupsGrid.Columns.Count);

				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['MainDescription'].ColumnStyle.HeaderText", "MainDescription", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.MainDescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['MainDescription'].ColumnStyle.ReadOnly", false, control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.MainDescriptionColumnName].ColumnStyle.ReadOnly);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['Group'].ColumnStyle.HeaderText", "Group", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.GroupColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['Group2'].ColumnStyle.HeaderText", "Group2", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.Group2ColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['Group3'].ColumnStyle.HeaderText", "Group3", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.Group3ColumnName].ColumnStyle.HeaderText);
				AssertNotEquals("CodeDescriptionWithThreeGroupsGrid.RemoveAction", RemoveAction.NoRemovePossible, control.CodeDescriptionWithThreeGroupsGrid.RemoveAction);

				control = new CodeDescriptionWithThreeGroupsControl();
				control.SetupGroupColumns("Group", "Group2", "Group3", false);
				control.SetupMainDescriptionColumn("MainDescription");
				control.SetupExtraDescriptionColumn("ExtraDescription");
				control.SetupEditMode(false);
				form.Controls.Add(control);
				control.SetDataBinding(GetNewBusinessEntity(), null);

				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns.Count", 3, control.CodeDescriptionWithThreeGroupsGrid.Columns.Count);

				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['Code'].ColumnStyle.HeaderText", "Code", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.CodeColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['MainDescription'].ColumnStyle.HeaderText", "MainDescription", control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.MainDescriptionColumnName].ColumnStyle.HeaderText);
				AssertEquals("CodeDescriptionWithThreeGroupsGrid.Columns['MainDescription'].ColumnStyle.ReadOnly", false, control.CodeDescriptionWithThreeGroupsGrid.Columns[CodeDescriptionWithThreeGroupsControl.MainDescriptionColumnName].ColumnStyle.ReadOnly);
				AssertNotEquals("CodeDescriptionWithThreeGroupsGrid.RemoveAction", RemoveAction.NoRemovePossible, control.CodeDescriptionWithThreeGroupsGrid.RemoveAction);
			}
		}

		public void TestHumanReadableNames()
		{
			using (var control = new CodeDescriptionWithThreeGroupsControl())
			{
				control.SetupGroupColumns("My Caption for Group1", "My Caption for Group2", "My Caption for Group3", true);
				control.SetupEditMode(false);
				var dataSource = new CodeDescriptionWithThreeGroupsCollection();
				dataSource.Add(new CodeDescriptionWithThreeGroups());
				control.SetDataBinding(dataSource, null);
				control.CodeDescriptionWithThreeGroupsGrid_CurrentCellChanged(dataSource, new EventArgs());

				var row = dataSource[0];
				AssertEquals("Human readable name for column Group", "My Caption for Group1", row.GroupInfo.HumanReadableName);
				AssertEquals("Human readable name for column Group 2", "My Caption for Group2", row.Group2Info.HumanReadableName);
				AssertEquals("Human readable name for column Group 3", "My Caption for Group3", row.Group3Info.HumanReadableName);
			}
		}

		#region Implementation

		protected override RegistryZUserControl GetNewControl()
		{
			return new CodeDescriptionWithThreeGroupsControl();
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new CodeDescriptionWithThreeGroupsCollection(17);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CodeDescriptionWithThreeGroupsControl)control).CodeDescriptionWithThreeGroupsGrid.ReadOnly;
		}

		#endregion
	}
}
