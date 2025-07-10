using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class SendOrganizationDataToCertCaptureRegistryItem : StronglyTypedRegistryItem<SendOrganizationDataToCertCapture>
	{
		public SendOrganizationDataToCertCaptureRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions registryOption, SendOrganizationDataToCertCapture defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new SendOrganizationDataToCertCaptureDataType(), storage, registryOption, defaultValue))
		{
		}
	}
}
