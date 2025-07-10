using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Inner)]
	public partial class CommercialInvoiceLine : IDataObject,
		ICustomizedFieldContainer,
		IOrganizationAddressCollectionParent,
		IAddInfoCollectionParent,
		IAddInfoGroupCollectionParent,
		ICustomsReferenceCollectionParent,
		ICustomsSupportingInformationCollectionParent,
		ITransportLogisticsCostCollectionParent,
		ITaxOrFeeCollectionParent,
		IAdditionalLineTariffDetailParent,
		IVehicleCollectionParent
	{
		public CommercialInvoiceLine()
		{
		}

		public CommercialInvoiceLine(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[CandidateKey]
		public ZInt? LineNo { get; set; }
		[MaxLength(525), AllowLineControlWhiteSpace]
		public ZString? Description { get; set; }
		[MaxLength(38)]
		public ZString? DataImportMatchingKey { get; set; }
		[MaxLength(35)]
		public ZString? HarmonisedCode { get; set; }
		public ZDecimal? InvoiceQuantity { get; set; }
		public CodeDescriptionPair InvoiceQuantityUnit { get; set; }
		public ZInt? Link { get; set; }
		public ZInt? OrderLineLink { get; set; }
		public ZDecimal? LinePrice { get; set; }
		public ZDecimal? UnitPrice { get; set; }
		[MaxLength(35)]
		public ZString? PartNo { get; set; }
		public ZDecimal? Volume { get; set; }
		public UnitOfVolume VolumeUnit { get; set; }
		public ZDecimal? Weight { get; set; }
		public UnitOfWeight WeightUnit { get; set; }
		public ZDecimal? BondedWarehouseQuantity { get; set; }
		public CodeDescriptionPair BondedWarehouseQuantityUnit { get; set; }
		[MaxLength(35)]
		public ZString? BondedWarehouseRemarks { get; set; }
		[MaxLength(35)]
		public ZString? BondedWHSOrderNumber { get; set; }
		public ZShort? BondedWHSOrderLineNumber { get; set; }
		[MaxLength(35)]
		public ZString? ClassificationCode { get; set; }
		public Commodity Commodity { get; set; }
		[MaxLength(15)]
		public ZString? ConcessionOrder { get; set; }
		public ContainerMode ContainerMode { get; set; }
		[MaxLength(20)]
		public ZString? ContainerNumber { get; set; }
		public Country CountryOfExport { get; set; }
		public Country CountryOfOrigin { get; set; }
		public State StateOfOrigin { get; set; }
		public ZDecimal? CustomsQuantity { get; set; }
		public CodeDescriptionPair6Char CustomsQuantityUnit { get; set; }
		public ZDecimal? CustomsSecondQuantity { get; set; }
		public CodeDescriptionPair6Char CustomsSecondQuantityUnit { get; set; }
		public ZDecimal? CustomsThirdQuantity { get; set; }
		public CodeDescriptionPair6Char CustomsThirdQuantityUnit { get; set; }
		public ZDecimal? CustomsFourthQuantity { get; set; }
		public CodeDescriptionPair6Char CustomsFourthQuantityUnit { get; set; }
		public ZDecimal? CustomsFifthQuantity { get; set; }
		public CodeDescriptionPair6Char CustomsFifthQuantityUnit { get; set; }
		public ZShort? EntryLineNumber { get; set; }
		[MaxLength(35)]
		public ZString? EntryNumber { get; set; }
		public ZDecimal? NetWeight { get; set; }
		public UnitOfWeight NetWeightUnit { get; set; }
		[MaxLength(25)]
		public ZString? OrderNumber { get; set; }
		public HazardousMaterial HazardousMaterial { get; set; }
		public ZInt? ParentLineNo { get; set; }
		public ZDecimal? CustomsValue { get; set; }
		public LandedCostDetail LandedCostDetail { get; set; }
		public ZInt? EntryInstructionLink { get; set; }
		[MaxLength(7)]
		public ZString? Procedure { get; set; }
		[MaxLength(10)]
		public ZString? PrimaryPreference { get; set; }
		[MaxLength(10)]
		public ZString? SecondaryPreference { get; set; }
		public CodeDescriptionPair ValuationCode { get; set; }
		public CodeDescriptionPair RelatedIndicator { get; set; }
		[MaxLength(50)]
		public ZString? BrandName { get; set; }
		[MaxLength(50)]
		public ZString? Model { get; set; }
		[MaxLength(525), AllowLineControlWhiteSpace]
		public ZString? LocalDescription { get; set; }
		[MaxLength(35)]
		public ZString? PreviousEntryNumber { get; set; }
		public ZShort? PreviousEntryLineNumber { get; set; }
		public CodeDescriptionPair4Char TaxType { get; set; }
		public ZDecimal? ValuationMarkup { get; set; }
		[MaxLength(200)]
		public ZString? ClassUsageComment { get; set; }
		public Staff ClassUsageCommentStaff { get; set; }

		public List<AddInfo> AddInfoCollection { get; set; }
		public List<CommercialCharge> CommercialChargeCollection { get; set; }
		public List<CustomizedField> CustomizedFieldCollection { get; set; }
		public List<CustomsSupportingInformation> CustomsSupportingInformationCollection { get; set; }
		public List<AddInfoGroup> AddInfoGroupCollection { get; set; }
		public List<CustomsReference> CustomsReferenceCollection { get; set; }
		public List<OrganizationAddress> OrganizationAddressCollection { get; set; }
		public List<EntryReference> EntryReferenceCollection { get; set; }
		public List<TransportLogisticsCost> TransportLogisticsCostCollection { get; set; }
		public List<AdditionalLineTariffDetail> AdditionalLineTariffDetailCollection { get; set; }
		public List<TaxOrFee> TaxOrFeeCollection { get; set; }
		public List<CustomAttribute> CustomAttributeCollection { get; set; }
		public List<Vehicle> VehicleCollection { get; set; }

		[MaxLength(2147483646), AllowLineControlWhiteSpace]
		public ZString? DetailedDescription { get; set; }

		[MaxLength(3)]
		public ZString? EntryStatus { get; set; }

		public ZDateTime? EntryReleaseDate { get; set; }

		[MaxLength(35)]
		public ZString? FormattedTariff { get; set; }
	}
}
