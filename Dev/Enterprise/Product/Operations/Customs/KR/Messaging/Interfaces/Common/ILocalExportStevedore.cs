using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface ILocalExportStevedore
	{
		ZInt SequenceNo { get; }
		ZString CompanyName { get; }
		ZDate Birthday { get; }
		ZString FullName { get; }
		ZString RoadNameCode { get; }
		ZString BuildingNumber { get; }
		ZString Postcode { get; }
		ZString AddressLine1 { get; }
		ZString AddressLine2 { get; }
		ZString PhoneNumber { get; }
		ZString MobileNumber { get; }
	}
}
