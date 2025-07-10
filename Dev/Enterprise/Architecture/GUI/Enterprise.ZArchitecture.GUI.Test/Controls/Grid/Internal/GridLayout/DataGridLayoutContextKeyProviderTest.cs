using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DataGridLayoutContextKeyProviderTest : TestCaseWithDummy
	{
		public void TestGridLayoutForZModuleButtonGridsPersistedSeparately()
		{
			using (var form = new ZModuleButtonGridNonPersistentTestForm(Dummy))
			{
				form.Show();

				form.Grid.InnerGrid.Columns[DummyBizoSchema.Z0_Number.Name].IsVisible = false;
				form.Grid.InnerGrid.Columns[DummyBizoSchema.Z0_Description.Name].IsVisible = true;

				form.Grid.InnerGrid.Columns.HasLayoutChanged = true;
			}

			using (var differentForm = new ZModuleButtonGridTestForm(Dummy))
			{
				differentForm.Show();

				differentForm.Grid.InnerGrid.Columns[DummyBizoSchema.Z0_Number.Name].IsVisible = true;
				differentForm.Grid.InnerGrid.Columns[DummyBizoSchema.Z0_Description.Name].IsVisible = false;

				differentForm.Grid.InnerGrid.Columns.HasLayoutChanged = true;
			}

			new DataGridLayoutDataAccessor().ClearGridColumnSettingsCacheForTesting();

			using (var form = new ZModuleButtonGridNonPersistentTestForm(Dummy))
			{
				form.Show();
				AssertEquals("Number should be invisible", false, form.Grid.InnerGrid.Columns[DummyBizoSchema.Z0_Number.Name].IsVisible);
				AssertEquals("Description should be visible", true, form.Grid.InnerGrid.Columns[DummyBizoSchema.Z0_Description.Name].IsVisible);
			}

			using (var differentForm = new ZModuleButtonGridTestForm(Dummy))
			{
				differentForm.Show();
				AssertEquals("Number should be visible", true, differentForm.Grid.InnerGrid.Columns[DummyBizoSchema.Z0_Number.Name].IsVisible);
				AssertEquals("Description should be invisible", false, differentForm.Grid.InnerGrid.Columns[DummyBizoSchema.Z0_Description.Name].IsVisible);
			}
		}

		public void TestContextKeyTakesParentIdRoot()
		{
			Dummy.Z0_Code = "AAA";//Grid layout key has this code part of it
			using (var form = new ZTestGridFormAsDataGridLayoutIdRoot(Dummy))
			{
				form.Show();
				form.TabGrid.Columns[DummyBizoSchema.Z0_Number.Name].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Z0_Description.Name].IsVisible = true;
				form.TabGrid.Columns.HasLayoutChanged = true;
			}

			Dummy.Z0_Code = "BBB";
			using (var form = new ZTestGridFormAsDataGridLayoutIdRoot(Dummy))
			{
				form.Show();
				form.TabGrid.Columns[DummyBizoSchema.Z0_Number.Name].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Z0_Description.Name].IsVisible = false;
				form.TabGrid.Columns.HasLayoutChanged = true;
			}

			Dummy.Z0_Code = "AAA";
			using (var form = new ZTestGridFormAsDataGridLayoutIdRoot(Dummy))
			{
				form.Show();
				AssertEquals("Number should be invisible", false, form.TabGrid.Columns[DummyBizoSchema.Z0_Number.Name].IsVisible);
				AssertEquals("Description should be visible", true, form.TabGrid.Columns[DummyBizoSchema.Z0_Description.Name].IsVisible);
			}

			Dummy.Z0_Code = "BBB";
			using (var form = new ZTestGridFormAsDataGridLayoutIdRoot(Dummy))
			{
				form.Show();
				AssertEquals("Number should be visible", true, form.TabGrid.Columns[DummyBizoSchema.Z0_Number.Name].IsVisible);
				AssertEquals("Description should be invisible", false, form.TabGrid.Columns[DummyBizoSchema.Z0_Description.Name].IsVisible);
			}
		}

		public void TestNewDataGridLayoutContextKeyProviderImp()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var gridID = form.TabGrid.GridId;
				AssertEquals("NewKey", GUI.DataGridLayoutContextKeyProvider.Prefix + "|" + gridID, new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter);
			}
		}

		public void TestDataGridLayoutContextKeyProviderWithLayoutCategoryPK()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				form.TabGrid.LayoutCategoryPK = Guid.Empty;
				AssertEquals("Key for StmModuleFilter", new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter, new DataGridLayoutContextKeyProvider(form.TabGrid, true).ContextKeyForStmModuleFilter);
				AssertEquals("Key for StmData", new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmData, new DataGridLayoutContextKeyProvider(form.TabGrid, true).ContextKeyForStmData);

				form.TabGrid.LayoutCategoryPK = Guid.NewGuid();
				AssertEquals("Key for StmModuleFilter", new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter + "|" + form.TabGrid.LayoutCategoryPK, new DataGridLayoutContextKeyProvider(form.TabGrid, true).ContextKeyForStmModuleFilter);
				AssertEquals("Key for StmData", new DataGridLayoutContextKeyProvider(form.TabGrid, true).ContextKeyForStmModuleFilter, new DataGridLayoutContextKeyProvider(form.TabGrid, true).ContextKeyForStmData);
			}
		}
	}
}
