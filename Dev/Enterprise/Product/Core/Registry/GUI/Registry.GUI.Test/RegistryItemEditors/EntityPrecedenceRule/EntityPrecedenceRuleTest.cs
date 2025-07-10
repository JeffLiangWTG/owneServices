using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(EntityPrecedenceRuleControl))]
	sealed class EntityPrecedenceRuleTest : Testing.RegistryZUserControlTestCase
	{
		public void TestAddAndRemoveButtons()
		{
			var dataSource = (EntityPrecedenceRule)this.GetNewBusinessEntity();

			using (var dummyForm = new Form())
			using (var control = new EntityPrecedenceRuleControl())
			{
				control.SetDataBinding(dataSource, "");

				dummyForm.Controls.Add(control);
				dummyForm.Show();

				AssertEquals("0 items selected", 0, control.SelectedItemsGrid.List.Count);
				AssertEquals("3 items available", 3, control.AvailableItemsGrid.List.Count);

				control.AvailableItemsGrid.SelectAllElements();
				control.AddItemButton_Click(this, new EventArgs());

				AssertEquals("3 items selected", 3, control.SelectedItemsGrid.List.Count);
				AssertEquals("0 items available", 0, control.AvailableItemsGrid.List.Count);

				control.SelectedItemsGrid.Select(0);
				control.SelectedItemsGrid.Select(1);

				AssertEquals("2 items selected", 2, control.SelectedItemsGrid.SelectedElements.Length);

				control.RemoveItemButon_Click(this, new EventArgs());

				AssertEquals("2 items available", 2, control.AvailableItemsGrid.List.Count);
				AssertEquals("1 item selected", 1, control.SelectedItemsGrid.List.Count);

				AssertEquals("AAA", control.AvailableItemsGrid[0, 0]);
				AssertEquals("BBB", control.AvailableItemsGrid[1, 0]);
				AssertEquals("CCC", control.SelectedItemsGrid[0, 0]);
			}
		}

		public void TestMoveButtons()
		{
			var rule = (EntityPrecedenceRule)GetNewBusinessEntity();

			using (var dummyForm = new Form())
			using (var control = new EntityPrecedenceRuleControl())
			{
				control.SetDataBinding(rule, "");

				dummyForm.Controls.Add(control);
				dummyForm.Show();

				control.AvailableItemsGrid.SelectAllElements();
				control.AddItemButton_Click(this, new EventArgs());

				AssertEquals("Precondition 1", "AAA", control.SelectedItemsGrid[0, 0]);
				AssertEquals("Precondition 2", "BBB", control.SelectedItemsGrid[1, 0]);
				AssertEquals("Precondition 3", "CCC", control.SelectedItemsGrid[2, 0]);

				control.SelectedItemsGrid.Select(1);
				control.MoveDownButton_Click(this, new EventArgs());

				AssertEquals("AAA", control.SelectedItemsGrid[0, 0]);
				AssertEquals("CCC", control.SelectedItemsGrid[1, 0]);
				AssertEquals("BBB", control.SelectedItemsGrid[2, 0]);

				control.MoveDownButton_Click(this, new EventArgs());
				AssertEquals("AAA", control.SelectedItemsGrid[0, 0]);
				AssertEquals("CCC", control.SelectedItemsGrid[1, 0]);
				AssertEquals("BBB", control.SelectedItemsGrid[2, 0]);

				control.MoveUpButton_Click(this, new EventArgs());
				control.MoveUpButton_Click(this, new EventArgs());
				AssertEquals("BBB", control.SelectedItemsGrid[0, 0]);
				AssertEquals("AAA", control.SelectedItemsGrid[1, 0]);
				AssertEquals("CCC", control.SelectedItemsGrid[2, 0]);

				control.MoveUpButton_Click(this, new EventArgs());
				AssertEquals("BBB", control.SelectedItemsGrid[0, 0]);
				AssertEquals("AAA", control.SelectedItemsGrid[1, 0]);
				AssertEquals("CCC", control.SelectedItemsGrid[2, 0]);
			}
		}

		public void TestResetButton()
		{
			var rule = (EntityPrecedenceRule)GetNewBusinessEntity();

			using (var dummyForm = new Form())
			using (var control = new EntityPrecedenceRuleControl())
			{
				control.SetDataBinding(rule, "");

				dummyForm.Controls.Add(control);
				dummyForm.Show();

				control.AvailableItemsGrid.SelectAllElements();
				control.AddItemButton_Click(this, new EventArgs());

				AssertEquals("Precondition 1", "AAA", control.SelectedItemsGrid[0, 0]);
				AssertEquals("Precondition 2", "BBB", control.SelectedItemsGrid[1, 0]);
				AssertEquals("Precondition 3", "CCC", control.SelectedItemsGrid[2, 0]);

				control.ResetButton_Click(this, new EventArgs());

				AssertEquals("AAA", control.AvailableItemsGrid[0, 0]);
				AssertEquals("BBB", control.AvailableItemsGrid[1, 0]);
				AssertEquals("CCC", control.AvailableItemsGrid[2, 0]);
			}
		}

		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			var rule = new EntityPrecedenceRule();

			rule.DefaultItems.Add(new EntityPrecedenceRuleItem { Code = "AAA", Description = (NoResString)"ADesc", Bool = false });
			rule.DefaultItems.Add(new EntityPrecedenceRuleItem { Code = "BBB", Description = (NoResString)"BDesc", Bool = false });
			rule.DefaultItems.Add(new EntityPrecedenceRuleItem { Code = "CCC", Description = (NoResString)"CDesc", Bool = false });

			rule.AvailableItems.Add(new EntityPrecedenceRuleItem { Code = "AAA", Description = (NoResString)"ADesc" });
			rule.AvailableItems.Add(new EntityPrecedenceRuleItem { Code = "BBB", Description = (NoResString)"BDesc" });
			rule.AvailableItems.Add(new EntityPrecedenceRuleItem { Code = "CCC", Description = (NoResString)"CDesc" });
			return rule;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((EntityPrecedenceRuleControl)control).ReadOnly;
		}

		protected override void BashForDescriptionColumn(Control controlToBash) { }

		#endregion
	}
}
