using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces.PBN;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class PBNContactDetailsProvider : IPBNContactDetails
	{
		public static PBNContactDetailsProvider New(AsycudaManifestHeader pbn)
		{
			if (pbn == null || pbn.Persons.Count == 0)
			{
				return null;
			}
			var firstCreatedContact = pbn.Persons.Cast<CusPerson>().OrderBy(person => person.CPN_SystemCreateTimeUtc).First();
			return new PBNContactDetailsProvider(firstCreatedContact);
		}

		PBNContactDetailsProvider(CusPerson person)
		{
			this.person = person.Person;
		}
		readonly GlbPerson person;

		public string Email => person.PER_EmailAddress;
		public string MobileNum1 => person.PER_MobilePhone;
		public string MobileNum2 => person.PER_HomePhone;
	}
}
