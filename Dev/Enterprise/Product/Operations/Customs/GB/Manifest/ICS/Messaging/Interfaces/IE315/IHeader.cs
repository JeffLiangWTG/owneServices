using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging.IE315
{
	public interface IHeader
	{
		ZString ReferenceNumber { get; }
		ZString TransportModeAtBorder { get; }
		ZString IdentityOfMeansOfTransportCrossingBorder { get; }
		ZString IdentityOfMeansOfTransportCrossingBorderLNG { get; }
		ZString NationalityOfMeansOfTransportCrossingBorder { get; }
		ZInt TotalNumberOfItems { get; }
		ZInt TotalNumberOfPackages { get; }
		ZDecimal TotalGrossMass { get; }
		ZBool IsTotalGrossMassSpecified { get; }
		ZString DeclarationPlace { get; }
		ZString DeclarationPlaceLNG { get; }
		ZString SpecificCircumstanceIndicator { get; }
		ZString TransportChargesMethodOfPayment { get; }
		ZString CommercialReferenceNumber { get; }
		ZString ConveyanceReferenceNumber { get; }
		ZString PlaceOfLoading { get; }
		ZString PlaceOfLoadingLNG { get; }
		ZString PlaceOfUnloading { get; }
		ZString PlaceOfUnloadingLNG { get; }
		ZDateTime DeclarationDateAndTime { get; }
	}
}
