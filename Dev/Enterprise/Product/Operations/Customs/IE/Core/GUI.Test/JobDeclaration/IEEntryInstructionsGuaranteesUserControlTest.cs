using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class IEEntryInstructionsGuaranteesUserControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			using (var form = new ZForm())
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.Show();

				var grid = userControl.GuaranteesGrid;
				var index = 0;
				CombineAssertions(() =>
				{
					AssertColumn(grid, "PW_CPH_Guarantee", true, index++);
					AssertColumn(grid, "PW_BondType", true, index++);
					AssertColumn(grid, "PW_BondNumber", true, index++);
					AssertColumn(grid, "PW_GuaranteeDescription", true, index++);
					AssertColumn(grid, "PW_Password", true, index++);
					AssertColumn(grid, "PW_BondAmount", true, index++);
					AssertColumn(grid, "PW_RX_NKCurrency", true, index++);
					AssertColumn(grid, "PW_BondFiledPort", true, index++);
					AssertColumn(grid, "PW_RN_NKCountryOfIssue", true, index++);
					AssertColumn(grid, "PW_BondNumber2", false);
					AssertColumn(grid, "PW_HolderIdentification", false);
					AssertColumn(grid, "PW_SuretyCode", false);
				});
			}

			using (EUCustomsDataRegistry.Instance.EnableCentralizedClearanceForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				userControl = new IEEntryInstructionGuaranteesUserControl();
				using (var form = new ZForm())
				{
					form.Controls.Add(userControl);
					form.Show();
					userControl.Show();

					var grid = userControl.GuaranteesGrid;
					AssertColumn(grid, "PW_RN_NKCountryOfIssue", false);
				}
			}
		}

		public void TestColumnStyles()
		{
			var grid = userControl.GuaranteesGrid;
			CombineAssertions(() =>
			{
				AssertColumnStyle<ZGuidFindBoxColumnStyleInfo>("PW_CPH_Guarantee");
				AssertColumnStyle<ZDropEditColumnStyleInfo>("PW_BondType");
				AssertColumnStyle<ZTextBoxColumnStyleInfo>("PW_BondNumber");
				AssertColumnStyle<ZTextBoxColumnStyleInfo>("PW_GuaranteeDescription");
				AssertColumnStyle<ZTextBoxColumnStyleInfo>("PW_Password");
				AssertColumnStyle<ZCalcEditColumnStyleInfo>("PW_BondAmount");
				AssertColumnStyle<ZDropEditColumnStyleInfo>("PW_RX_NKCurrency");
				AssertColumnStyle<ZCodeFindBoxColumnStyleInfo>("PW_BondFiledPort");
				AssertColumnStyle<ZDropEditColumnStyleInfo>("PW_RN_NKCountryOfIssue");
			});

			void AssertColumnStyle<T>(string name) where T : ZGridColumnInfo => AssertType<T>(name, grid.GetColumnStyle(name));
		}

		void AssertColumn(ZGrid lineGrid, string columnName, bool shouldBeAdded, int index = -1)
		{
			var column = lineGrid.GetColumnStyle(columnName);
			AssertEquals($"Column {columnName} added.", !shouldBeAdded, column == null);
			if (index >= 0)
			{
				AssertEquals($"Column {columnName} should be at index {index}", index,
					lineGrid.ColumnStyles.IndexOf(lineGrid.GetColumnStyle(columnName)));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			using (EUCustomsDataRegistry.Instance.EnableCentralizedClearanceForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				userControl = new IEEntryInstructionGuaranteesUserControl();
			}
		}
		IEEntryInstructionGuaranteesUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
