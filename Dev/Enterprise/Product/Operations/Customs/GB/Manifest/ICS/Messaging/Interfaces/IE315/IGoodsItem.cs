using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging.IE315
{
	public interface IGoodsItem
	{
		ZShort ItemNumber { get; }
		ZString GoodsDescription { get; }
		ZString GoodsDescriptionLNG { get; }
		ZDecimal GrossMass { get; }
		ZBool IsGrossMassSpecified { get; }
		ZString TransportChargesOrMethodOfPayment { get; }
		ZString CommercialReferenceNumber { get; }
		ZString UNDangerousGoodsCode { get; }
		ZString PlaceOfLoading { get; }
		ZString PlaceOfLoadingLNG { get; }
		ZString PlaceOfUnloading { get; }
		ZString PlaceOfUnloadingLNG { get; }
		IEnumerable<IProducedDocument> ProducedDocuments { get; }
		IEnumerable<ISpecialMention> SpecialMentions { get; }
		ITrader Consignor { get; }
		ICommodity Commodity { get; }
		ITrader Consignee { get; }
		IEnumerable<IContainer> Containers { get; }
		IEnumerable<IMeansOfTransportIdentity> MeansOfTransportIdentities { get; }
		IEnumerable<IPackage> Packages { get; }
		ITrader NotifyParty { get; }
	}
}
