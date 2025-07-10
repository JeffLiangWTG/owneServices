using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class EntryInstructionGuaranteeGridColumnsBagTest : TestCase
	{
		public void TestBondTypeDropEditColumn()
		{
			AssertNotNull(ColumnsBag.BondTypeDropEditColumn);

			var columnInfo = ColumnsBag.BondTypeDropEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("Mandatory", true, columnInfo.IsMandatory);
				AssertEquals("ColumnName", "PW_BondType", columnInfo.ColumnName);
			});
		}

		public void TestBondNumberMultiControlColumn()
		{
			AssertNotNull(ColumnsBag.BondNumberMultiControlColumn);
			var columnInfo = ColumnsBag.BondNumberMultiControlColumn.CreateGridColumnInfo() as ZMultiControlColumnStyleInfo;
			AssertNotNull(columnInfo);

			CombineAssertions("ZMultiControlColumnStyleInfo Configuration", () =>
			{
				AssertNull("BindToDecimalPlaces", columnInfo.BindToDecimalPlaces);
				AssertEquals("FieldTypeColumnName", "ReferenceNumberFieldType", columnInfo.FieldTypeColumnName);
				AssertEquals("ModuleID", Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Guarantees, columnInfo.ModuleID);
				AssertEquals("ColumnName", "PW_BondNumber", columnInfo.ColumnName);
			});
		}

		public void TestBondNumber2TextBoxColumn()
		{
			AssertNotNull(ColumnsBag.BondNumber2TextBoxColumn);
			var columnInfo = ColumnsBag.BondNumber2TextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "PW_BondNumber2", columnInfo.ColumnName);
		}

		public void TestPasswordBoxColumn()
		{
			AssertNotNull(ColumnsBag.PasswordBoxColumn);
			var columnInfo = ColumnsBag.PasswordBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions("PasswordBox Configuration", () =>
			{
				AssertEquals("ColumnName", "PW_Password", columnInfo.ColumnName);
				AssertEquals("PasswordChar", '*', columnInfo.PasswordChar);
				AssertEquals("TextAlign", HorizontalAlignment.Right, columnInfo.TextAlign);
			});
		}

		public void TestHolderIdentificationTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.HolderIdentificationTextBoxColumn);
			var columnInfo = ColumnsBag.HolderIdentificationTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "PW_HolderIdentification", columnInfo.ColumnName);
		}

		public void TestNkCurrencyCodeFindBoxColumn()
		{
			AssertNotNull(ColumnsBag.NkCurrencyCodeFindBoxColumn);
			var columnInfo = ColumnsBag.NkCurrencyCodeFindBoxColumn.CreateGridColumnInfo() as ZCodeFindBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "PW_RX_NKCurrency", columnInfo.ColumnName);
		}

		public void TestBondAmountCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.BondAmountCalcEditColumn);
			var columnInfo = ColumnsBag.BondAmountCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);

			CombineAssertions("ZCalcEditColumnStyleInfo configuration", () =>
			{
				AssertEquals("ColumnName", "PW_BondAmount", columnInfo.ColumnName);
				AssertEquals("Decimals", 2, columnInfo.Decimals);
				AssertEquals("MaxValue", 0m, columnInfo.MaxValue);
			});
		}

		public void TestBondFiledPortCodeFindBoxColumn()
		{
			AssertNotNull(ColumnsBag.BondFiledPortCodeFindBoxColumn);
			var columnInfo = ColumnsBag.BondFiledPortCodeFindBoxColumn.CreateGridColumnInfo() as ZCodeFindBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "PW_BondFiledPort", columnInfo.ColumnName);
		}

		public void TestSuretyCodeDropEditColumn()
		{
			AssertNotNull(ColumnsBag.SuretyCodeDropEditColumn);
			var columnInfo = ColumnsBag.SuretyCodeDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "PW_SuretyCode", columnInfo.ColumnName);
		}

		EntryInstructionGuaranteeGridColumnsBag ColumnsBag => EntryInstructionGuaranteeGridColumnsBag.Instance;
	}
}
