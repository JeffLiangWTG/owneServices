using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Inner)]
	public partial class InBondMoveLineItem : IDataObject,
		ICustomsReferenceCollectionParent,
		IOrganizationAddressCollectionParent,
		ICustomsSupportingInformationCollectionParent
	{
		public ZShort? PrintingSequenceNo { get; set; }
		[MaxLength(512), AllowLineControlWhiteSpace]
		public ZString? MarksAndNumbers { get; set; }
		[MaxLength(1000), AllowLineControlWhiteSpace]
		public ZString? DescriptionAndQuantityOfMerchandise { get; set; }
		public ZDecimal? Weight { get; set; }
		[MaxLength(3)]
		public CodeDescriptionPair WeightUnit { get; set; }
		public ZDecimal? LinePrice { get; set; }
		public CodeDescriptionPair LinePriceCurrency { get; set; }
		public ZDecimal? MonetaryValue { get; set; }
		public CodeDescriptionPair MonetaryValueCurrency { get; set; }
		[MaxLength(100)]
		public ZString? RateComment { get; set; }
		[MaxLength(100)]
		public ZString? DutyComment { get; set; }
		public ZBool? IsMonetaryValueEstimated { get; set; }
		public ZInt? LineNumber { get; set; }
		public ZDecimal? NetWeight { get; set; }
		public UnitOfWeight NetWeightUnit { get; set; }
		[MaxLength(22)]
		public ZString? TariffCode { get; set; }
		public ZDecimal? CustomsSecondQuantity { get; set; }
		public CodeDescriptionPair4Char CustomsSecondQuantityUnit { get; set; }
		public CodeDescriptionPair DeclarationType { get; set; }
		public CodeDescriptionPair2Char CountryOfDispatch { get; set; }
		public CodeDescriptionPair2Char CountryOfDestination { get; set; }
		public CodeDescriptionPair2Char CountryOfOrigin { get; set; }
		[MaxLength(70)]
		public ZString? ReferenceNumber { get; set; }
		public CodeDescriptionPair TransportPaymentMethod { get; set; }
		public HazardousMaterial HazardousMaterial { get; set; }
		public ZDecimal? CustomsFirstQuantity { get; set; }
		public UnitOfWeight CustomsFirstQuantityUnit { get; set; }
		public ZDecimal? CustomsThirdQuantity { get; set; }
		public CodeDescriptionPair4Char CustomsThirdQuantityUnit { get; set; }
		public ZDecimal? CustomsFourthQuantity { get; set; }
		public CodeDescriptionPair4Char CustomsFourthQuantityUnit { get; set; }
		public CodeDescriptionPair TaxType { get; set; }
		public List<CustomsSupportingInformation> CustomsSupportingInformationCollection { get; set; }
		public List<CustomsReference> CustomsReferenceCollection { get; set; }
		public List<OrganizationAddress> OrganizationAddressCollection { get; private set; }
		public ZInt? Link { get; set; }
	}
}
