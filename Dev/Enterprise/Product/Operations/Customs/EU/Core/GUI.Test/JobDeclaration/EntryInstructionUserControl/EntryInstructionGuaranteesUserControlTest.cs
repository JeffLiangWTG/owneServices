using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class EntryInstructionGuaranteesUserControlTest : TestCaseWithFactory
	{
		public void TestEntryInstructionGuaranteesGrid()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			using (var form = new ZForm(declaration))
			using (var userControl = new EntryInstructionGuaranteesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				declaration.JE_MessageType = "EXP";
				entryInstruction.CEI_Style = "H1";
				var guaranteesGrid = userControl.FindSingle<ZGrid>("GuaranteesGrid");

				AssertNotNull("GuaranteesGrid", guaranteesGrid);
				AssertEquals("Visible ColumnStyles Count", 9, GetVisibleColumnsCount());

				CombineAssertions(() =>
				{
					AssertColumnOrder<ZDropEditColumnStyleInfo>(CusBondDetail.Schema.PW_BondType, 0);
					AssertColumnOrder<ZMultiControlColumnStyleInfo>(CusBondDetail.Schema.PW_BondNumber, 1);
					AssertColumnOrder<ZTextBoxColumnStyleInfo>(CusBondDetail.Schema.PW_BondNumber2, 2);
					AssertColumnOrder<ZTextBoxColumnStyleInfo>(CusBondDetail.Schema.PW_Password, 3);
					AssertColumnOrder<ZTextBoxColumnStyleInfo>(CusBondDetail.Schema.PW_HolderIdentification, 4);
					AssertColumnOrder<ZCodeFindBoxColumnStyleInfo>(CusBondDetail.Schema.PW_RX_NKCurrency, 5);
					AssertColumnOrder<ZCalcEditColumnStyleInfo>(CusBondDetail.Schema.PW_BondAmount, 6);
					AssertColumnOrder<ZCodeFindBoxColumnStyleInfo>(CusBondDetail.Schema.PW_BondFiledPort, 7);
					AssertColumnOrder<ZDropEditColumnStyleInfo>(CusBondDetail.Schema.PW_SuretyCode, 8);
				});

				int GetVisibleColumnsCount() => guaranteesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Count(x => x.IsVisible);

				void AssertColumnOrder<T>(ZString columnName, ZInt index)
					where T : ZGridColumnInfo
				{
					var columnStyleList = guaranteesGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
					var columnStyle = columnStyleList.SingleOrDefault(x => x.ColumnName == columnName);
					AssertNotNull($"{columnName} not null", columnStyle);
					Assert($"{columnName}", columnStyle.IsVisible);
					AssertEquals(index, Array.IndexOf(columnStyleList, columnStyle));
				}
			}
		}

		public void TestCheckPW_PasswordColumnPasswordChar()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			using (var form = new ZForm(declaration))
			using (var userControl = new EntryInstructionGuaranteesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				entryInstruction.CEI_Style = "H1";
				var guaranteesGrid = userControl.FindSingleOrDefault<ZGrid>("GuaranteesGrid");
				AssertNotNull("GuaranteesGrid", guaranteesGrid);

				var passwordColumn = guaranteesGrid.GetColumnStyle(CusBondDetail.Schema.PW_Password) as ZTextBoxColumnStyleInfo;
				AssertNotNull($"Password column", passwordColumn);

				AssertEquals("Password Char", '*', passwordColumn.PasswordChar);
			}
		}

		[RequiresSTA]
		public void TestCheckPW_BondAmountColumnDecimals()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			using (var form = new ZForm(declaration))
			using (var userControl = new EntryInstructionGuaranteesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				entryInstruction.CEI_Style = "H1";
				var guaranteesGrid = userControl.FindSingleOrDefault<ZGrid>("GuaranteesGrid");
				AssertNotNull("GuaranteesGrid", guaranteesGrid);

				var bondAmountColumn = guaranteesGrid.GetColumnStyle(CusBondDetail.Schema.PW_BondAmount) as ZCalcEditColumnStyleInfo;
				AssertNotNull("BondAmount column", bondAmountColumn);
				AssertEquals("BondAmount Decimals", 2, bondAmountColumn.Decimals);
				AssertEquals("BondAmount BindToDecimalPlaces", "GuaranteeBondAmountDecimalPlaces", bondAmountColumn.BindToDecimalPlaces);
			}
		}

		public void TestGridColumnLayoutProvider()
		{
			using (var control = new EntryInstructionGuaranteesUserControlForTest())
			{
				AssertType<EntryInstructionGuaranteeGridColumnsLayout>(control.GetGridColumnLayoutProvider_Exposed());
			}
		}

		sealed class EntryInstructionGuaranteesUserControlForTest : EntryInstructionGuaranteesUserControl
		{
			internal IGridColumnLayoutProvider GetGridColumnLayoutProvider_Exposed() => GetGridColumnLayoutProvider();
		}
	}
}
