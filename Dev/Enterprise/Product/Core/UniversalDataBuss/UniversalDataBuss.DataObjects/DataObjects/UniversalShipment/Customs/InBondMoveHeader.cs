using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Inner)]
	public partial class InBondMoveHeader : IDataObject,
		IOrganizationAddressCollectionParent,
		ICustomsReferenceCollectionParent,
		IAddInfoCollectionParent,
		ICustomsSupportingInformationCollectionParent
	{
		public InBondMoveHeader()
		{
		}

		public InBondMoveHeader(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[MaxLength(500), AllowLineControlWhiteSpace]
		public ZString? AdditionalText { get; set; }
		[MaxLength(1)]
		public ZString? BioterrorismActIndicator { get; set; }
		public Staff CustomsAgent { get; set; }
		public CodeDescriptionPair CustomsStatus { get; set; }
		public CodeDescriptionPair PhaseStatus { get; set; }
		public CodeDescriptionPair MessagingStatus { get; set; }
		public CodeDescriptionPair4Char DestinationPortScheduleD { get; set; }
		public CodeDescriptionPair9Char EntryType { get; set; }
		public CodeDescriptionPair1Char AdditionalEntryType { get; set; }
		public CodeDescriptionPair PaymentMethod { get; set; }
		public ContainerMode ExportContainerMode { get; set; }
		public CodeDescriptionPair ExportTransportMode { get; set; }
		[MaxLength(35)]
		public ZString? ExportVesselName { get; set; }
		public CodeDescriptionPair5Char ForeignDestinationPortScheduleK { get; set; }
		public UNLOCO ForeignDestinationPortUNLOCO { get; set; }
		[MaxLength(12)]
		public ZString? InBondCarrierID { get; set; }
		[MaxLength(4)]
		public ZString? InBondCarrierSCAC { get; set; }
		public CodeDescriptionPair5Char LastForeignPortScheduleK { get; set; }
		public ZDecimal? MonetaryValue { get; set; }
		public CodeDescriptionPair1Char MoveToFTZ { get; set; }
		public CodeDescriptionPair4Char PortOfPresentationScheduleD { get; set; }
		[MaxLength(20)]
		public ZString? Seals { get; set; }
		[MaxLength(4)]
		public ZString? TransferOfLiabilityCarrierCode { get; set; }
		[MaxLength(27)]
		public ZString? TransferOfLiabilityCarrierID { get; set; }
		[MaxLength(19)]
		public ZString? TransferOfLiabilityCityName { get; set; }
		public CodeDescriptionPair2Char TransferOfLiabilityStateCode { get; set; }
		public CodeDescriptionPair MessagingApplicationCode { get; set; }
		[MaxLength(6)]
		public ZString? SequenceNumber { get; set; }

		public DataObjectList<AdditionalReference> AdditionalReferenceCollection { get; set; }
		public List<Date> DateCollection { get; set; }
		public List<Universal.EntryNumber> EntryNumberCollection { get; set; }
		public List<InBondMoveDetail> InBondMoveDetailCollection { get; set; }
		public List<OrganizationAddress> OrganizationAddressCollection { get; set; }

		public UNLOCO PortOfOrigin { get; set; }
		public UNLOCO PortOfDestination { get; set; }
		public UNLOCO PortOfLoading { get; set; }
		public UNLOCO PortOfDischarge { get; set; }
		public ZDecimal? GrossWeight { get; set; }
		public UnitOfWeight GrossWeightUnit { get; set; }

		public List<TransportMeans> TransportMeansCollection { get; private set; }
		public List<Guarantee> GuaranteeCollection { get; private set; }
		public List<LocationOfGoods> LocationOfGoodsCollection { get; private set; }
		public List<AddInfo> AddInfoCollection { get; private set; }
		public List<CustomsReference> CustomsReferenceCollection { get; private set; }
		public List<CustomsSupportingInformation> CustomsSupportingInformationCollection { get; private set; }
	}
}
