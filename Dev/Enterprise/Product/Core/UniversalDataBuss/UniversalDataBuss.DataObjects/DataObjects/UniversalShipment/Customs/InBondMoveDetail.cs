using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Inner)]
	public partial class InBondMoveDetail : IDataObject,
		IAdditionalBillLinkParent,
		IAddInfoGroupCollectionParent,
		ICustomsReferenceCollectionParent
	{
		public InBondMoveDetail()
		{
		}

		public InBondMoveDetail(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public ZInt? AdditionalBillLink { get; set; }
		public CodeDescriptionPair CustomsStatus { get; set; }
		public ZInt? InBondQuantity { get; set; }
		public ZDateTime? PreviousInBondTransitDate { get; set; }
		public CodeDescriptionPair2Char PreviousInBondTransitType { get; set; }
		public CodeDescriptionPair4Char PreviousInBondTransitPortScheduleD { get; set; }
		[MaxLength(5)]
		public ZString? SequenceNumber { get; set; }
		public ZDecimal? MonetaryValue { get; set; }
		[MaxLength(5)]
		public ZString? ForeignDestPortScheduleK { get; set; }
		public ZDateTime? ExportDate { get; set; }
		[MaxLength(35)]
		public ZString? ExportVesselName { get; set; }
		public CodeDescriptionPair MessageStatus { get; set; }
		public CodeDescriptionPair DepartureStatus { get; set; }
		public CodeDescriptionPair ArrivalStatus { get; set; }
		public CodeDescriptionPair ExportationStatus { get; set; }
		public CodeDescriptionPair TransferOfLiabilityStatus { get; set; }
		public ZInt? Sequence { get; set; }
		public CodeDescriptionPair TransportPaymentMethod { get; set; }
		public CodeDescriptionPair2Char DispatchCountry { get; set; }
		public CodeDescriptionPair2Char DestinationCountry { get; set; }
		public ZDecimal? Weight { get; set; }
		public UnitOfWeight WeightUnit { get; set; }

		public List<AddInfoGroup> AddInfoGroupCollection { get; set; }
		public List<ContainerLink> ContainerLinkCollection { get; set; }
		public List<CustomsReference> CustomsReferenceCollection { get; set; }
		public List<Universal.EntryNumber> EntryNumberCollection { get; set; }
		public List<InBondMoveLineItem> InBondMoveLineItemCollection { get; set; }
		public List<InBondWarehouseDetail> WarehouseDetailCollection { get; set; }
		public List<TransportMeans> TransportMeansCollection { get; private set; }
	}
}
