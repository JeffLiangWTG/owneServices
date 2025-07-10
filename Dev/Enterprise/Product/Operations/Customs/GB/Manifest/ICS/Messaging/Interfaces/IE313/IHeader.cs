using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging.IE313
{
	public interface IHeader
	{
		ZString DocumentReferenceNumber { get; }
		ZString TransportModeAtBorder { get; }
		ZString IdentityOfMeansOfTransportCrossingBorder { get; }
		ZString IdentityOfMeansOfTransportCrossingBorderLNG { get; }
		ZString NationalityOfMeansOfTransportCrossingBorder { get; }
		ZInt TotalNumberOfItems { get; }
		ZInt TotalNumberOfPackages { get; }
		ZDecimal TotalGrossMass { get; }
		ZString AmendmentPlace { get; }
		ZString AmendmentPlaceLNG { get; }
		ZString SpecificCircumstanceIndicator { get; }
		ZString TransportChargesMethodOfPayment { get; }
		ZString CommercialReferenceNumber { get; }
		ZString ConveyanceReferenceNumber { get; }
		ZString PlaceOfLoading { get; }
		ZString PlaceOfLoadingLNG { get; }
		ZString PlaceOfUnloading { get; }
		ZString PlaceOfUnloadingLNG { get; }
		ZDateTime AmendmentDateTime { get; }
	}
}
