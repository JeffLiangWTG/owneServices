using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class ImporterDeliveryAddressWrapper : OrganisationWrapper
	{
		public ImporterDeliveryAddressWrapper(CusEntryHeader entryHeader)
			: base(entryHeader, entryHeader.Declaration.ImporterDeliveryAddress.Address, entryHeader.Declaration.ImporterDeliveryAddress.Organisation)
		{ }
	}
}
