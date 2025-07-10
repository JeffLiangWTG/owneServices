using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSContactPersonProvider : INCTSContactPerson
	{
		public static NCTSContactPersonProvider NewOrNull(JobDocAddress address) => address != null ? new NCTSContactPersonProvider(address) : null;

		protected NCTSContactPersonProvider(JobDocAddress docAddress)
		{
			this.docAddress = Argument.NotNull(docAddress, nameof(docAddress));
		}

		public string Name => docAddress.E2_Contact.ValueOrNullIfEmpty();

		public string PhoneNumber => docAddress.E2_Phone.ValueOrNullIfEmpty();

		public string MailAddress => docAddress.E2_Email.ValueOrNullIfEmpty();

		readonly JobDocAddress docAddress;
	}
}
