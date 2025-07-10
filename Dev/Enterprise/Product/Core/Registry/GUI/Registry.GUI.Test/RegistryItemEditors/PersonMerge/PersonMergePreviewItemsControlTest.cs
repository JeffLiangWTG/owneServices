using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(PersonMergePreviewItemsControl))]
	sealed class PersonMergePreviewItemsControlTest : RegistryZUserControlTestCase
	{
		public void TestMoveUpAndMoveDownButton()
		{
			var collection = new PersonMergePreviewItemCollection()
			{
				new PersonMergePreviewItem() { FriendlyName = "Full Name", ColumnName = GlbPersonSchema.PER_FullName.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Friendly Name", ColumnName = GlbPersonSchema.PER_FriendlyName.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Legal Name", ColumnName = GlbPersonSchema.PER_LegalName.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Name Suffix", ColumnName = GlbPersonSchema.PER_NameSuffix.Name, Visibility = false },
			};

			using (var form = new ZForm(collection))
			using (var control = new PersonMergePreviewItemsControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(collection, null);

				AssertEquals("Grid has 4 rows.", 4, control.PersonMergePreviewItemsGrid.ListManager.Count);

				control.PersonMergePreviewItemsGrid.PerformMouseDownForTest(3, 1);
				AssertEquals(3, control.PersonMergePreviewItemsGrid.CurrentRowIndex);
				AssertEquals("Name Suffix", (control.PersonMergePreviewItemsGrid.GetCurrent() as PersonMergePreviewItem).FriendlyName);

				control.MoveUpButton.PerformClick();
				AssertEquals(2, control.PersonMergePreviewItemsGrid.CurrentRowIndex);
				AssertEquals("Name Suffix", (control.PersonMergePreviewItemsGrid.GetCurrent() as PersonMergePreviewItem).FriendlyName);

				control.MoveUpButton.PerformClick();
				AssertEquals(1, control.PersonMergePreviewItemsGrid.CurrentRowIndex);
				AssertEquals("Name Suffix", (control.PersonMergePreviewItemsGrid.GetCurrent() as PersonMergePreviewItem).FriendlyName);

				control.MoveUpButton.PerformClick();
				AssertEquals(0, control.PersonMergePreviewItemsGrid.CurrentRowIndex);
				AssertEquals("Name Suffix", (control.PersonMergePreviewItemsGrid.GetCurrent() as PersonMergePreviewItem).FriendlyName);

				control.MoveUpButton.PerformClick();
				AssertEquals("It's already in the first place and index will not change.", 0, control.PersonMergePreviewItemsGrid.CurrentRowIndex);
				AssertEquals("Name Suffix", (control.PersonMergePreviewItemsGrid.GetCurrent() as PersonMergePreviewItem).FriendlyName);

				control.MoveDownButton.PerformClick();
				AssertEquals(1, control.PersonMergePreviewItemsGrid.CurrentRowIndex);
				AssertEquals("Name Suffix", (control.PersonMergePreviewItemsGrid.GetCurrent() as PersonMergePreviewItem).FriendlyName);

				control.MoveDownButton.PerformClick();
				AssertEquals(2, control.PersonMergePreviewItemsGrid.CurrentRowIndex);
				AssertEquals("Name Suffix", (control.PersonMergePreviewItemsGrid.GetCurrent() as PersonMergePreviewItem).FriendlyName);

				control.MoveDownButton.PerformClick();
				AssertEquals(3, control.PersonMergePreviewItemsGrid.CurrentRowIndex);
				AssertEquals("Name Suffix", (control.PersonMergePreviewItemsGrid.GetCurrent() as PersonMergePreviewItem).FriendlyName);

				control.MoveDownButton.PerformClick();
				AssertEquals("It's already in the last place and index will not change.", 3, control.PersonMergePreviewItemsGrid.CurrentRowIndex);
				AssertEquals("Name Suffix", (control.PersonMergePreviewItemsGrid.GetCurrent() as PersonMergePreviewItem).FriendlyName);
			}
		}

		#region Implementation

		protected override CargoWise.EntityFramework.IBusiness GetNewBusinessEntity()
		{
			return new PersonMergePreviewItemCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((PersonMergePreviewItemsControl)control).PersonMergePreviewItemsGrid.ReadOnly &&
				   ((PersonMergePreviewItemsControl)control).MoveDownButton.ReadOnly &&
				   ((PersonMergePreviewItemsControl)control).MoveUpButton.ReadOnly;
		}

		#endregion

	}
}
