using System;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import008Person : IImport008Person
	{
		public string Name { get; set; }
		public string RelationshipToImporter { get; set; }
		public DateTime BirthDate { get; set; }
		public string PassportNumber { get; set; }
		public string JobCode { get; set; }
		public string EntryToKR_YN { get; set; }
		public DateTime ScheduledStartDateInKR { get; set; }
		public DateTime ScheduledEndDateInKR { get; set; }
		public string PhoneNumber { get; set; }
		public string Email { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string RoadNameCode { get; set; }
		public string BuildingNumber { get; set; }
		public string PostCode { get; set; }
		public string Nationality { get; set; }
		public string NationalityClassCode { get; set; }

		ZString IImport008Person.Name => Name;
		ZString IImport008Person.RelationshipToImporter => RelationshipToImporter;
		ZDate IImport008Person.BirthDate => new ZDate(BirthDate);
		ZString IImport008Person.PassportNumber => PassportNumber;
		ZString IImport008Person.JobCode => JobCode;
		ZString IImport008Person.EntryToKR_YN => EntryToKR_YN;
		ZDate IImport008Person.ScheduledStartDateInKR => new ZDate(ScheduledStartDateInKR);
		ZDate IImport008Person.ScheduledEndDateInKR => new ZDate(ScheduledEndDateInKR);
		ZString IImport008Person.PhoneNumber => PhoneNumber;
		ZString IImport008Person.Email => Email;
		ZString IImport008Person.AddressLine1 => AddressLine1;
		ZString IImport008Person.AddressLine2 => AddressLine2;
		ZString IImport008Person.RoadNameCode => RoadNameCode;
		ZString IImport008Person.BuildingNumber => BuildingNumber;
		ZString IImport008Person.PostCode => PostCode;
		ZString IImport008Person.Nationality => Nationality;
		ZString IImport008Person.NationalityClassCode => NationalityClassCode;
	}
}
