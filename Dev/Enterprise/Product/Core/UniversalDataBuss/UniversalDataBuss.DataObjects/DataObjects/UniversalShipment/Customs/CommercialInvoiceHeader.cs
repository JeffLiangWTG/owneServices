using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Inner)]
	public partial class CommercialInvoiceHeader : IDataObject,
		IOrganizationAddressCollectionParent,
		IAddInfoCollectionParent,
		IAddInfoGroupCollectionParent,
		ICustomsReferenceCollectionParent,
		ICustomsSupportingInformationCollectionParent,
		ICustomizedFieldContainer,
		ITransportLogisticsCostCollectionParent
	{
		public CommercialInvoiceHeader()
		{
		}

		public CommercialInvoiceHeader(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[MaxLength(35), CandidateKey]
		public ZString? InvoiceNumber { get; set; }
		public OrganizationAddress Supplier { get; set; }
		public OrganizationAddress Buyer { get; set; }
		public ZDecimal? InvoiceAmount { get; set; }
		public Currency InvoiceCurrency { get; set; }
		public CodeDescriptionPair ExchangeRateType { get; set; }
		public ZDecimal? AgreedExchangeRate { get; set; }
		public ZDateTime? InvoiceDate { get; set; }
		public ZDateTime? ValuationDateOverride { get; set; }
		[MaxLength(35)]
		public ZString? AdditionalTerms { get; set; }
		[MaxLength(35)]
		public ZString? DeliveryTerms { get; set; }
		public CodeDescriptionPair IncoTerm { get; set; }
		public ZDecimal? Volume { get; set; }
		public UnitOfVolume VolumeUnit { get; set; }
		public ZDecimal? Weight { get; set; }
		public UnitOfWeight WeightUnit { get; set; }
		public ZDecimal? NetWeight { get; set; }
		public UnitOfWeight NetWeightUQ { get; set; }
		public ZDecimal? LandedCostExchangeRate { get; set; }
		public CodeDescriptionPair MessageStatus { get; set; }
		public ZDecimal? NoOfPacks { get; set; }
		[MaxLength(20)]
		public ZString? PaymentNumber { get; set; }
		public ZDecimal? PaymentAmount { get; set; }
		public ZDecimal? PaymentExchangeRate { get; set; }
		public ZDateTime? PaymentDate { get; set; }
		[MaxLength(35)]
		public ZString? BillNumber { get; set; }
		public WayBillType BillType { get; set; }
		public CodeDescriptionPair ValuationCode { get; set; }
		public CodeDescriptionPair RelatedIndicator { get; set; }

		public List<AddInfo> AddInfoCollection { get; set; }
		public List<AddInfoGroup> AddInfoGroupCollection { get; set; }
		public List<CommercialCharge> CommercialChargeCollection { get; set; }
		public DataObjectList<CommercialInvoiceLine> CommercialInvoiceLineCollection { get; private set; }
		public List<CustomsReference> CustomsReferenceCollection { get; set; }
		public List<CustomsSupportingInformation> CustomsSupportingInformationCollection { get; set; }
		public List<Note> NoteCollection { get; set; }
		public List<OrganizationAddress> OrganizationAddressCollection { get; set; }
		public List<CustomizedField> CustomizedFieldCollection { get; set; }
		public List<TransportLogisticsCost> TransportLogisticsCostCollection { get; set; }
		public List<PackingLink> PackingLinkCollection { get; set; }

		[MaxLength(int.MaxValue), AllowLineControlWhiteSpace]
		public ZString? MarksAndNumbers { get; set; }
	}
}

