using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport008Person
	{
		ZString Name { get; }
		ZString RelationshipToImporter { get; }
		ZDate BirthDate { get; }
		ZString PassportNumber { get; }
		ZString JobCode { get; }
		ZString EntryToKR_YN { get; }
		ZDate ScheduledStartDateInKR { get; }
		ZDate ScheduledEndDateInKR { get; }
		ZString PhoneNumber { get; }
		ZString Email { get; }
		ZString AddressLine1 { get; }
		ZString RoadNameCode { get; }
		ZString BuildingNumber { get; }
		ZString PostCode { get; }
		ZString AddressLine2 { get; }
		ZString Nationality { get; }
		ZString NationalityClassCode { get; }
	}
}
