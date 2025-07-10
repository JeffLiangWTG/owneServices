using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using BaseWarehouseCustomsFallbackDetailWithEntryInstruction = Enterprise.Customs.DataTransfer.Universal.WarehouseCustomsFallbackDetailWithEntryInstruction;

namespace Enterprise.Customs.DE.DataTransfer.Universal
{
	public class WarehouseCustomsLineDetailsProvider : EU.DataTransfer.Universal.WarehouseCustomsLineDetailsProvider
	{
		public WarehouseCustomsLineDetailsProvider(Shipment shipment) : base(shipment)
		{
		}

		protected override WarehouseCustomsFallbackDetail GetFallbackDetail(CommercialInvoiceHeader invoice)
		{
			var fallbackDetails = WarehouseCustomsFallbackDetailWithEntryInstruction.CloneBaseProperties((EU.DataTransfer.Universal.WarehouseCustomsFallbackDetailWithEntryInstruction)base.GetFallbackDetail(invoice));
			var firstInvoice = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			fallbackDetails.InvoiceNumber = invoice.InvoiceNumber ?? ZString.Empty;
			fallbackDetails.InvoiceDate = invoice.InvoiceDate ?? ZDateTime.Empty;
			fallbackDetails.IncotermCode = firstInvoice.IncoTerm?.Code ?? ZString.Empty;
			fallbackDetails.IncotermPlace = invoice.AdditionalTerms ?? ZString.Empty;
			fallbackDetails.ValuationCode = firstInvoice.ValuationCode?.Code ?? ZString.Empty;
			fallbackDetails.ImporterAddress = shipment.OrganizationAddressCollection.FindBestImporterMatch();
			fallbackDetails.BuyerAddress = shipment.OrganizationAddressCollection.FirstOrDefault(Customs.DataTransfer.Universal.Constants.AddressTypes.UltimateConsignee);
			fallbackDetails.SellerAddress = shipment.OrganizationAddressCollection.FirstOrDefault(Customs.DataTransfer.Universal.Constants.AddressTypes.Seller);
			fallbackDetails.PortOfLoading = shipment.PortOfLoading?.Code ?? ZString.Empty;
			fallbackDetails.PortOfFirstEUArrival = shipment.PortOfFirstArrival?.Code ?? ZString.Empty;
			fallbackDetails.TransportMode = shipment.TransportMode?.Code ?? ZString.Empty;
			fallbackDetails.supportingInfos = invoice.CustomsSupportingInformationCollection;
			return fallbackDetails;
		}

		protected override Dictionary<ZInt, ZString> GetNewEntryInstructionProcedureMap()
		{
			var result = new Dictionary<ZInt, ZString>();
			if (shipment.EntryInstructionCollection != null)
			{
				foreach (var entryInstruction in shipment.EntryInstructionCollection)
				{
					var link = entryInstruction.Link.GetValueOrDefault();
					if (link > ZInt.Zero && !result.ContainsKey(link))
					{
						result.Add(link, entryInstruction.Procedure.GetValueOrDefault());
					}
				}
			}

			return result;
		}

		protected override WarehouseCustomsLineDetailsWithEntryInstruction GetNewLineDetail(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, BaseWarehouseCustomsFallbackDetailWithEntryInstruction fallbackDetail)
			=> new WarehouseCustomsLineDetails(factory, invoiceLine, (WarehouseCustomsFallbackDetailWithEntryInstruction)fallbackDetail, shipment);
	}
}
