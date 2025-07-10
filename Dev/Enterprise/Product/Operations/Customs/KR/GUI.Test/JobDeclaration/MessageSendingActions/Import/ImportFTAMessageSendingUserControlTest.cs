using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class ImportFTAMessageSendingUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumn()
		{
			using (var userControl = new ImportFTAEntryLinesMessageSendingUserControl())
			{
				var grid = userControl.FTAEntryLinesGrid;

				var index = 0;
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.EntryLineNo));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.FTASequenceNumber));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.FormattedHSCode));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.Preference));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.TotalNetWeight));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.SplitOrder));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.NetWeightInKG));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.COONo));
				AssertEquals(((ZDateEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.COOIssuedDate));

				AssertEquals(false, grid.GetColumnStyle(nameof(MessageSendingEntryLineObject.CountryOfOrigin)).IsVisible);
				AssertEquals(false, grid.GetColumnStyle(nameof(MessageSendingEntryLineObject.TariffRate)).IsVisible);
				AssertEquals(false, grid.GetColumnStyle(nameof(MessageSendingEntryLineObject.COOProductType)).IsVisible);
				AssertEquals(false, grid.GetColumnStyle(nameof(MessageSendingEntryLineObject.ThirdCountryAdditionalInvoiceIssued)).IsVisible);
				AssertEquals(false, grid.GetColumnStyle(nameof(MessageSendingEntryLineObject.ThirdCountry)).IsVisible);
				AssertEquals(false, grid.GetColumnStyle(nameof(MessageSendingEntryLineObject.COOExporterNumber)).IsVisible);
				AssertEquals(false, grid.GetColumnStyle(nameof(MessageSendingEntryLineObject.AssociatedCOOIssuingCountryCode)).IsVisible);
				AssertEquals(false, grid.GetColumnStyle(nameof(MessageSendingEntryLineObject.COOIssuingAgencyType)).IsVisible);
				AssertEquals(false, grid.GetColumnStyle(nameof(MessageSendingEntryLineObject.COOAgencyName)).IsVisible);
				AssertEquals(false, grid.GetColumnStyle(nameof(MessageSendingEntryLineObject.COOSupportingDocType)).IsVisible);
				AssertEquals(false, grid.GetColumnStyle(nameof(MessageSendingEntryLineObject.COOIssuerType)).IsVisible);
			}
		}
	}
}
