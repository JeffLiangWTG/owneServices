using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	class IE513And515ConsignmentProvider : IIE513And515Consignment, ILocationOfGoods
	{
		public IE513And515ConsignmentProvider(EntryHeaderWrapper entryHeaderWrapper, bool isSubStyle_B_C_E_F)
		{
			this.entryHeaderWrapper = entryHeaderWrapper;
			entryHeader = entryHeaderWrapper.EntryHeader;
			declaration = entryHeaderWrapper.Declaration;
			this.isSubStyle_B_C_E_F = isSubStyle_B_C_E_F;
		}
		internal readonly EntryHeaderWrapper entryHeaderWrapper;
		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly bool isSubStyle_B_C_E_F;

		#region IAESConsignment Members
		public string ContainerIndicator => isSubStyle_B_C_E_F ? string.Empty : AESFlagCodeList.GetContainerIndicator(declaration);

		public string InlandTransportMode => isSubStyle_B_C_E_F ? ZString.Empty : declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland);

		public string BorderModeOfTransport => isSubStyle_B_C_E_F ? ZString.Empty : declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportMode);

		public decimal GrossMass => entryHeader.TotalInvoiceLinesGrossWeightInKG;

		public ICarrier Carrier => CachedValueHelper.GetValue(ref carrierCached, () => CarrierProvider.New(declaration.ShippingLine));
		CachedValue<ICarrier> carrierCached;

		public IReadOnlyCollection<ITransportEquipmentWithSeals> TransportEquipment => transportEquipment ??= isSubStyle_B_C_E_F ? Array.Empty<ITransportEquipmentWithSeals>() : TransportEquipmentWithSealsProvider.GetEquipments(entryHeader);
		IReadOnlyCollection<ITransportEquipmentWithSeals> transportEquipment;

		public ILocationOfGoods LocationOfGoods => this;

		public IReadOnlyCollection<ITransportMeans> DepartureTransportMeans => departureTransportMeans ??= DepartureTransportMeansProvider.CreateCollection(declaration);
		IReadOnlyCollection<ITransportMeans> departureTransportMeans;

		public IReadOnlyCollection<string> CountryOfRoutingConsignment => countryOfRoutingConsignmentCache ??= GetCountryOfRoutingConsignment(declaration);
		IReadOnlyCollection<string> countryOfRoutingConsignmentCache;

		public ITransportMeans ActiveTransportMeans => CachedValueHelper.GetValue(ref activeTransportMeansCached, () => declaration.JE_TransportMode.IsEmpty || string.Equals(ExportDeclarationTypeList.Codes.C1, entryHeader.EntryInstruction?.CEI_Style) ? null : new ActiveTransportMeansProvider(declaration));
		CachedValue<ITransportMeans> activeTransportMeansCached;

		public IReadOnlyCollection<IDocument> TransportDocuments => transportDocument ??= declaration.IsTransitionPeriodAES30 ? null
			: EU.Business.Extensions.GetAggregatedData(MessageProviderHelper.GetTransportDocumentKeys(), MessageProviderHelper.FindAdditionalInfos(entryHeader, AdditionalInfoSubTypeList.Codes.TransportDocument)).Select(x => new AdditionalReferenceProvider(x)).ToArray<IDocument>();
		IReadOnlyCollection<IDocument> transportDocument;
		#endregion

		#region ILocationOfGoods Members;
		public string LocationCodeType => declaration.JE_LocationOtherInformation;
		public string UNLocode => declaration.JE_LocationOfGoods;
		#endregion

		IReadOnlyCollection<string> GetCountryOfRoutingConsignment(JobDeclaration declaration)
		{
			var routingCountries = new List<string>();
			var instruction = entryHeader.EntryInstruction;
			if (instruction != null && (instruction.IsB1Declaration || instruction.CEI_Style == ExportDeclarationTypeList.Codes.B2 || instruction.CEI_Style == ExportDeclarationTypeList.Codes.C1))
			{
				var itineraryCountries = declaration.ItineraryCountries;
				routingCountries.AddRange(itineraryCountries.Cast<ItineraryCountry>().Where(p => !string.IsNullOrEmpty(p.CY_Code)).OrderBy(p => p.SequenceNumber).Select(p => (string)declaration.GetDefaultTerritory(p.CY_Code)));
				var portOfArrival = declaration.GetDefaultTerritoryForPortOfArrival();
				if (!string.IsNullOrWhiteSpace(portOfArrival) && !routingCountries.Contains(portOfArrival))
				{
					routingCountries.Add(portOfArrival);
				}
			}
			return routingCountries;
		}
	}
}
