using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.TaxFramework.GUI.Testing
{
	public class TaxTransactionsLinkedToJobChargeControlTest : TestCaseWithFactory
	{
		public void TestTaxRecordsGridVisibleColumns()
		{
			using (var form = new ZForm())
			using (var control = new TaxTransactionsLinkedToJobChargeControl())
			{
				form.Controls.Add(control);
				form.Show();

				var taxRecordsGrid = control.GetField("TaxRecordsGrid") as ZGrid;
				Assert("ReadOnly", taxRecordsGrid.ReadOnly);

				var expectedListOfColumns = new[]
				{
					$"{AccTaxRecordTransactionLinePivot.Schema.ATP_LocalTaxAmount} (ZCalcEditColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_Ledger} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_TaxSystemCode} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"TaxAuthorityCode (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_TaxAuthorityServiceCode} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"ATT_Rate (ZCalcEditColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_LocalTaxBaseAmount} (ZCalcEditColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_LocalTaxAmount} (ZCalcEditColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_AffectsSourceTransactionTotal} (ZCheckBoxColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_RX_NKOSTaxCurrency} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_OSTaxBaseAmount} (ZCalcEditColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_OSTaxAmount} (ZCalcEditColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_Basis} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_PostDate} (ZDateEditColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_RealisationDate} (ZDateEditColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_TaxDate} (ZDateEditColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_AT_TaxID} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_A9_TaxMessage} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_TaxAuthorityServiceCodeDescription} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"ATT_AH_MatchTransaction_ForBinding (ZTextBoxColumnStyleInfo) IsVisible:False",
					$"{AccTaxTransaction.Schema.ATT_TaxSuperType} (ZTextBoxColumnStyleInfo) IsVisible:False",
				};
				var realListOfColumns = taxRecordsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x.ToString()} IsVisible:{x.IsVisible}").ToArray();
				AssertArrayEqualsByElements(expectedListOfColumns, realListOfColumns);
			}
		}
	}
}
