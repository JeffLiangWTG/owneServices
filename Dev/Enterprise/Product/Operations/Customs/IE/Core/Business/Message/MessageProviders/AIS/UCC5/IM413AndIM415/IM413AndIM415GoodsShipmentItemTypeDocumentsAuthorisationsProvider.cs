using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415GoodsShipmentItemTypeDocumentsAuthorisationsProvider : IGoodsShipmentItemTypeDocumentsAuthorisations
	{
		public IM413AndIM415GoodsShipmentItemTypeDocumentsAuthorisationsProvider(CusEntryLine entryLine)
		{
			this.entryLine = entryLine;
		}
		readonly CusEntryLine entryLine;

		public IReadOnlyCollection<IGoodsShipmentItemTypeSimplifiedDeclarationDocumentsWritingOff> SimplifiedDeclarationDocuments => simplifiedDeclarationDocuments ??= entryLine.PreviousDocuments.Select(x => IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider.New(x)).ToArray();
		IReadOnlyCollection<IGoodsShipmentItemTypeSimplifiedDeclarationDocumentsWritingOff> simplifiedDeclarationDocuments;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ??= entryLine.AdditionalInfos.Where(x => x.IsAnAdditionalInformation).Select(AdditionalInformationProvider.New).ToArray();
		IReadOnlyCollection<IAdditionalInformation> additionalInformations;

		public IReadOnlyCollection<IGoodsShipmentItemTypeProducedDocumentsWritingOff> ProducedDocuments
		{
			get
			{
				if (producedDocuments == null)
				{
					var list = new List<IGoodsShipmentItemTypeProducedDocumentsWritingOff>();
					list.AddRange(entryLine.SupportingDocuments.Select(IM413AndIM415ProducedDocumentsWritingOffProvider.New));
					foreach (var invoiceLine in entryLine.InvoiceLines.Cast<JobComInvoiceLine>())
					{
						list.AddRange(invoiceLine.CusLineTariffDetails.Select(IM413AndIM415ProducedDocumentsWritingOffFromTariffProvider.New));
					}
					producedDocuments = list.ToArray();
				}
				return producedDocuments;
			}
		}
		IReadOnlyCollection<IGoodsShipmentItemTypeProducedDocumentsWritingOff> producedDocuments;
	}
}
