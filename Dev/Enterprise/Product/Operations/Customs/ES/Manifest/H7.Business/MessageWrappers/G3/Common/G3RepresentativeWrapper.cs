using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3RepresentativeWrapper : IG3Representative
	{
		public static G3RepresentativeWrapper New(OrgHeader representative) => representative == null ? null : new G3RepresentativeWrapper(representative);

		G3RepresentativeWrapper(OrgHeader representative)
		{
			this.representative = representative;
		}

		readonly OrgHeader representative;

		public ZString IdNumber => representative.GetEOROrNIFCode();

		public ZString Status => RepresentativeStatus;

		public ZString Name => Contact?.OC_ContactName ?? string.Empty;

		public IG3Communication Communication => CachedValueHelper.GetValue(ref communication, () => G3CommunicationWrapper.New(Contact));
		CachedValue<IG3Communication> communication;

		OrgContact Contact
		{
			get
			{
				if (contact == null)
				{
					contact = representative.AllocatedContacts?.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS);
				}

				return contact;
			}
		}

		OrgContact contact;

		const string RepresentativeStatus = "2";
	}
}
