using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class EntryInstructionGridUserControlTest : TestCaseWithFactory
{
	public void TestEntryInstructionsGridColumns()
	{
		using (var control = new EntryInstructionGridUserControl())
		{
			var entryInstructionGridControl = (ZGrid)control.Controls.Find("EntryInstructionsGrid", true).First();
			var entryInstructionGridColumsStyles = entryInstructionGridControl.ColumnStyles.Cast<ZGridColumnInfo>();
			Assert("ZG_TempProcLimitDate column should be visible", entryInstructionGridColumsStyles.First(x => x.ColumnName == CusEntryInstruction.Schema.ZG_TempProcLimitDate).IsVisible);
			Assert("CEI_Procedure column should be visible", entryInstructionGridColumsStyles.First(x => x.ColumnName == CusEntryInstruction.Schema.CEI_Procedure).IsVisible);
			var dateForDutyColumn = (ZDateEditColumnStyleInfo)entryInstructionGridColumsStyles.First(x => x.ColumnName == CusEntryInstruction.Schema.CEI_DateForDuty);
			AssertNotNull("Date for duty column should not be null", dateForDutyColumn);
			CombineAssertions("Date for duty column should be", () =>
			{
				Assert("Is Visible =>", dateForDutyColumn.IsVisible);
				AssertEquals("DateTime Format =>", ZArchitecture.Core.ZDateTimePickerFormat.Short, dateForDutyColumn.DateTimeFormat);
			});
			Assert("Incoterm column visible", entryInstructionGridColumsStyles.First(x => x.ColumnName == CusEntryInstruction.Schema.Incoterm).IsVisible);
			Assert("Valuation code column visible", entryInstructionGridColumsStyles.First(x => x.ColumnName == CusEntryInstruction.Schema.ValuationCode).IsVisible);
			Assert("Currency column visible", entryInstructionGridColumsStyles.First(x => x.ColumnName == CusEntryInstruction.Schema.Currency).IsVisible);
			Assert("WarehouseIDFor27 column should be visible", entryInstructionGridColumsStyles.First(x => x.ColumnName == "WarehouseIDFor27").IsVisible);
		}
	}

	public void TestEntryInstructionsGridColumnsOrder()
	{
		using (var control = new EntryInstructionGridUserControl())
		{
			var entryInstructionGridControl = (ZGrid)control.Controls.Find("EntryInstructionsGrid", true).First();
			var columnStyles = entryInstructionGridControl.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				AssertColumnOrder(columnStyles, "CEI_Style", 0);
				AssertColumnOrder(columnStyles, "CEI_SubStyle", 1);
				AssertColumnOrder(columnStyles, "CEI_Description", 2);
				AssertColumnOrder(columnStyles, "CEI_DateForDuty", 3);
				AssertColumnOrder(columnStyles, "WarehouseIDFor27", 4);
				AssertColumnOrder(columnStyles, "FromWarehouseCode", 5);
				AssertColumnOrder(columnStyles, "CEI_Procedure", 6);
				AssertColumnOrder(columnStyles, "ZG_TempProcLimitDate", 7);
				AssertColumnOrder(columnStyles, "ZG_ParticipantType", 8);
				AssertColumnOrder(columnStyles, "Incoterm", 9);
				AssertColumnOrder(columnStyles, "ValuationCode", 10);
				AssertColumnOrder(columnStyles, "Currency", 11);
			});
		}

		void AssertColumnOrder(ZGridColumnInfo[] columnStyleList, string columnName, int expectedIndex)
		{
			var columnStyle = columnStyleList.Single(x => x.ColumnName == columnName);
			AssertEquals(columnName, expectedIndex, Array.IndexOf(columnStyleList, columnStyle));
		}
	}

	public void TestZG_ParticipantTypeGridColumnVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryInstructions.AddNew();

		using (var form = new ZForm(declaration))
		using (var userControl = new EntryInstructionGridUserControl())
		{
			form.SetDataBinding(declaration, ".");
			form.Controls.Add(userControl);
			form.Show();

			var entryInstructionGridControl = (ZGrid)userControl.Controls.Find("EntryInstructionsGrid", true).First();
			var columnStyles = entryInstructionGridControl.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertColumnVisibility(columnStyles, "JE_MessageType = EXP", false);

				declaration.JE_MessageType = "XXX";
				AssertColumnVisibility(columnStyles, "JE_MessageType IS NOT EXP", true);

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					AssertColumnVisibility(columnStyles, "UCC6, JE_MessageType = EXP", true);
				}
			});
		}

		void AssertColumnVisibility(ZGridColumnInfo[] columnStyleList, ZString assertionMessage, ZBool expectedUnavailable)
		{
			AssertEquals(assertionMessage, expectedUnavailable, columnStyleList.Single(x => x.ColumnName == CusEntryInstruction.Schema.ZG_ParticipantType).IsUnavailable);
		}
	}

	public void TestShowRequestedProcedure()
	{
		using (var userControl = new EntryInstructionGridUserControlForTesting())
		{
			AssertEquals(true, userControl.ShowRequestedProcedureExposed);
		}
	}
}

sealed class EntryInstructionGridUserControlForTesting : EntryInstructionGridUserControl
{
	internal bool ShowRequestedProcedureExposed => ShowRequestedProcedure;
}
