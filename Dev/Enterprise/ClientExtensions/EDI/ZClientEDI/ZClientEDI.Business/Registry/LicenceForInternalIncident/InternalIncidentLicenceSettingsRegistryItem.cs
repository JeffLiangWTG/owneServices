using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class InternalIncidentLicenceSettingsRegistryItem : StronglyTypedRegistryItem<InternalIncidentLicenceSettings, InternalIncidentLicenceSettings>
	{
		public InternalIncidentLicenceSettingsRegistryItem(string category)
			: base(new RegistryItemImpl(
				"InternalIncidentLicenceSettings",
				(NoResString)category,
				ResString.GetMultilingualString("08189fce-2345-414e-9f8c-c291551a939a", "Internal Incident License Settings"),
				ResString.GetMultilingualString("fa9691be-972c-4d77-aaad-1ec0960e9df1", "Please specify the licenses for reporting internal incidents on different releases. The enterprise code list restricts the license codes to be chosen for internal incidents."),
				new InternalIncidentLicenceSettingsDataType(),
				RegistryStorageFlags.System,
				InternalIncidentLicenceSettings.GetDefaultValue()))
		{
		}
	}
}

