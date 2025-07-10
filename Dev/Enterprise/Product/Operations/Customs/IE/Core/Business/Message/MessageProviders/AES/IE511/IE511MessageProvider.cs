using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public class IE511MessageProvider : EntryHeaderMessageProvider, IIE511Header, IIE511Consignment, IIE511GoodsShipment, IIE511ExportOperation, ILocationOfGoods
	{
		public IE511MessageProvider(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public IIE511ExportOperation ExportOperation => this;

		public string LRN => entryHeader.CH_BGMReference;

		public string MRN => entryHeader.MovementReferenceNumber;

		public string PresentationOffice => declaration.PresentationCustomsOffice;

		public string ExportOffice => declaration.JE_CustomsOffice;

		public IIE511GoodsShipment GoodsShipment => this;

		public IParty Declarant => CachedValueHelper.GetValue(ref declarantCached, () => PartyProvider.New(declaration.Declarant));
		CachedValue<IParty> declarantCached;

		public IRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => RepresentativeProvider.New(declaration.Representative, declaration.JE_DeclarantType));
		CachedValue<IRepresentative> representativeCached;

		public string ContainerIndicator => AESFlagCodeList.GetContainerIndicator(declaration);

		public string InlandTransportMode => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland);

		public IReadOnlyCollection<ITransportEquipmentWithSeals> TransportEquipment => transportEquipment ?? (transportEquipment = TransportEquipmentWithSealsProvider.GetEquipments(entryHeader));
		IReadOnlyCollection<ITransportEquipmentWithSeals> transportEquipment;

		public ILocationOfGoods LocationOfGoods => this;

		public IReadOnlyCollection<ITransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = DepartureTransportMeansProvider.CreateCollection(declaration));
		IReadOnlyCollection<ITransportMeans> departureTransportMeans;

		public IIE511Consignment Consignment => this;

		#region ILocationOfGoods Members;
		public string LocationCodeType => declaration.JE_LocationOtherInformation;
		public string UNLocode => declaration.JE_LocationOfGoods;
		#endregion
	}
}
