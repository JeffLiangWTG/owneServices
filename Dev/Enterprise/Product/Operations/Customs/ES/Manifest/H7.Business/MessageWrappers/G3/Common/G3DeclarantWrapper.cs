using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3DeclarantWrapper : IG3Declarant
	{
		public static G3DeclarantWrapper New(OrgAddress declarant) => declarant == null ? null : new G3DeclarantWrapper(declarant);

		G3DeclarantWrapper(OrgAddress declarant)
		{
			this.declarant = declarant;
		}

		readonly OrgAddress declarant;

		public ZString IdNumber => declarant.GetEOROrNIFCode();

		public ZString Name => IdNumber.IsEmpty ? declarant.Header.OH_FullName : ZString.Empty;

		public IG3FullAddress FullAddress => CachedValueHelper.GetValue(ref fullAddress, () => G3FullAddressWrapper.New(declarant));
		CachedValue<IG3FullAddress> fullAddress;

		public IG3Communication Communication => CachedValueHelper.GetValue(ref communication, () => G3CommunicationWrapper.New(declarant.Header.AllocatedContacts?.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS)));
		CachedValue<IG3Communication> communication;
	}
}
