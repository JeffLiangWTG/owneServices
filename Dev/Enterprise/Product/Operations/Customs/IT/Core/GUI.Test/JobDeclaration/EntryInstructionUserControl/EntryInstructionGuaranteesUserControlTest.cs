using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class EntryInstructionGuaranteesUserControlTest : TestCaseWithFactory
{
	public void TestGridColumns()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		using (var form = new ZForm(declaration))
		using (var userControl = new EntryInstructionGuaranteesUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			entryInstruction.CEI_Style = ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeAmmissioneTemporaneaH3;

			var guaranteesGrid = userControl.FindSingle<ZGrid>("GuaranteesGrid");
			AssertNotNull("GuaranteesGrid", guaranteesGrid);
			AssertEquals("Visible ColumnStyles Count", 9, GetVisibleColumnsCount());

			CombineAssertions("When CEI_Style=H3", () =>
			{
				AssertColumnOrder<ZDropEditColumnStyleInfo>(CusBondDetail.Schema.PW_BondType, 0);
				AssertColumnOrder<ZMultiControlColumnStyleInfo>(CusBondDetail.Schema.PW_BondNumber, 1);
				AssertColumnOrder<ZMultiControlColumnStyleInfo>(CusBondDetail.Schema.PW_BondNumber2, 2);
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
				AssertType<T>($"Column type is different than expected for column {columnName}", columnStyle);
			}
		}
	}

	public void TestGridColumnLayoutProvider()
	{
		using (var control = new EntryInstructionGuaranteesUserControlForTest())
		{
			AssertType<EntryInstructionGuaranteeGridColumnsLayout>(control.GetGridColumnLayoutProvider_Exposed());
		}
	}

	class EntryInstructionGuaranteesUserControlForTest : EntryInstructionGuaranteesUserControl
	{
		internal IGridColumnLayoutProvider GetGridColumnLayoutProvider_Exposed() => GetGridColumnLayoutProvider();
	}
}
