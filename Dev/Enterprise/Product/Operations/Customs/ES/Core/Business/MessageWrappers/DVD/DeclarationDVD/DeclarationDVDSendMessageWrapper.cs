using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationDVDSendMessageWrapper : DVDCommonSendMessageWrapper, IDeclarationDVDMessageDataProvider
	{
		public DeclarationDVDSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
			this.shouldDeclareUCRInLines = ShouldDeclareUCRInLines();
			this.shouldDeclareAddSupplyActorsInLines = ShouldDeclareAddSupplyActorsInLines();
			this.shouldDeclareCountryOfDestinationInLines = ShouldDeclareCountryOfDestinationInLines();
			this.shouldDeclareCountryOfExportInLines = ShouldDeclareCountryOfExportInLines();
			this.shouldDeclarePreviousDocumentsInLines = ShouldDeclarePreviousDocumentsInLines();
		}

		readonly ZBool shouldDeclareUCRInLines;
		readonly ZBool shouldDeclareAddSupplyActorsInLines;
		readonly ZBool shouldDeclareCountryOfDestinationInLines;
		readonly ZBool shouldDeclareCountryOfExportInLines;
		readonly ZBool shouldDeclarePreviousDocumentsInLines;

		public IDeclarationDVDHeader Header => header ?? (header = new DeclarationDVDHeaderWrapper(entryHeader, !shouldDeclareUCRInLines, !shouldDeclareAddSupplyActorsInLines, !shouldDeclareCountryOfDestinationInLines, !shouldDeclareCountryOfExportInLines, !shouldDeclarePreviousDocumentsInLines));
		DeclarationDVDHeaderWrapper header;

		public IReadOnlyCollection<IDeclarationDVDLine> Lines => lines ?? (lines = entryHeader.MergedLines.Cast<CusEntryLine>().Select(x => new DeclarationDVDLineWrapper(x, shouldDeclareUCRInLines, shouldDeclareAddSupplyActorsInLines, shouldDeclareCountryOfDestinationInLines, shouldDeclareCountryOfExportInLines, shouldDeclarePreviousDocumentsInLines)).ToList().AsReadOnly());
		IReadOnlyCollection<DeclarationDVDLineWrapper> lines;

		ZBool ShouldDeclareUCRInLines()
		{
			var ucrInLines = entryHeader.MergedLines
				.SelectMany(x => x.InvoiceLines
				.Select(y => ((JobComInvoiceLine)y).ZG_CommercialReference))
				.Distinct();

			return ucrInLines.Count() != 1;
		}

		ZBool ShouldDeclareAddSupplyActorsInLines()
		{
			var supplyChainActorInLines = entryHeader.MergedLines
				.Any(x => x.InvoiceLines
				.Any(y => ((JobComInvoiceLine)y).CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>().Any()));

			return supplyChainActorInLines;
		}

		ZBool ShouldDeclareCountryOfDestinationInLines()
		{
			var countryOfDestinationHeader = declaration.JE_GoodsDestination;
			var countryOfDestinationInLines = entryHeader.MergedLines
				.Any(x => x.InvoiceLines
				.Cast<JobComInvoiceLine>()
				.Any(y => !y.ZG_CountryOfDestination.IsEmpty && y.ZG_CountryOfDestination != countryOfDestinationHeader));

			return countryOfDestinationInLines;
		}

		ZBool ShouldDeclareCountryOfExportInLines()
		{
			var countryOfExportHeader = declaration.JE_GoodsOrigin;
			var countryOfExportInLines = entryHeader.MergedLines
				.Any(x => x.InvoiceLines
				.Cast<JobComInvoiceLine>()
				.Any(y => !y.ZG_CountryOfSupply.IsEmpty && y.ZG_CountryOfSupply != countryOfExportHeader));

			return countryOfExportInLines;
		}

		ZBool ShouldDeclarePreviousDocumentsInLines()
		{
			var prevDocsInLines = entryHeader.MergedLines
				.Any(x => x.InvoiceLines
				.Any(y => ((JobComInvoiceLine)y).PreviousDocuments.Any()));

			return prevDocsInLines;
		}
	}
}
