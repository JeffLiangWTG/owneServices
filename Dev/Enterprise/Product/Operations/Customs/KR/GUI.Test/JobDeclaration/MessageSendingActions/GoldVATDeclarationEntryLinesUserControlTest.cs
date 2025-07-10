using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class GoldVATDeclarationEntryLinesUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumn()
		{
			using (var userControl = new GoldVATDeclarationEntryLinesUserControl())
			{
				var grid = userControl.EntryLinesGrid;

				var index = 0;
				AssertEquals(nameof(MessageSendingEntryLineObject.EntryLineNo), ((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName);
				AssertEquals(nameof(MessageSendingEntryLineObject.IsGoldOrItsProduct), ((ZCheckBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName);
				AssertEquals(nameof(MessageSendingEntryLineObject.HSCode), ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName);
				AssertEquals(nameof(MessageSendingEntryLineObject.HSDescription), ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName);
				AssertEquals(nameof(MessageSendingEntryLineObject.NetWeightInKG), ((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName);
				AssertEquals(nameof(MessageSendingEntryLineObject.NetWeightUnit), ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName);
				AssertEquals(nameof(MessageSendingEntryLineObject.ValueForVAT), ((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName);
				AssertEquals(nameof(MessageSendingEntryLineObject.VAT), ((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName);
			}
		}
	}
}
