using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class H7PartyNameWrapper : IPartyNameProvider
	{
		public H7PartyNameWrapper(OrgAddress address)
		{
			this.address = address;
		}

		readonly OrgAddress address;

		public ZString Id => address.GetEOROrNIFCode();

		public ZString Name => address?.Header != null ? address.Header.OH_FullName : string.Empty;
	}
}
