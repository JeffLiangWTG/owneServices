using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.MasterFiles.Business;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;

namespace Enterprise.Customs.IE.H7.Business
{
	public class MRepresentativeProvider : IMRepresentative, CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1.IRepresentative
	{
		MRepresentativeProvider(AsycudaManifestHeader header)
		{
			this.header = header;
			address = header.Representative;
		}

		public static MRepresentativeProvider NewOrNull(AsycudaManifestHeader header)
		{
			var address = header.Representative;
			return address == null ? null : new MRepresentativeProvider(header);
		}

		readonly OrgAddress address;
		readonly AsycudaManifestHeader header;

		public string Id => CachedValueHelper.GetValue(ref idCached, () => { var id = address?.GetEORI() ?? ZString.Empty; if (id.IsEmpty) { return null; } else { return id; } });
		CachedValue<string> idCached;

		public IContact ContactPerson
		{
			get
			{
				if (contactPersonCached == null)
				{
					var contact = address.Header?.AllocatedContacts?.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS);
					if (contact != null)
					{
						contactPersonCached = new ContactProvider(contact.OC_ContactName, contact.OC_Phone, contact.Email);
					}
				}
				return contactPersonCached;
			}
		}
		IContact contactPersonCached;

		public string Status
		{
			get
			{
				switch (header.AMA_AgentType)
				{
					case RepresentationTypeList.Codes._2Direct:
						return DirectRepresentaton;
					case RepresentationTypeList.Codes._3Indirect:
						return IndirectRepresentation;
					default:
						return string.Empty;
				}
			}
		}

		const string DirectRepresentaton = "2";

		const string IndirectRepresentation = "3";
	}
}
