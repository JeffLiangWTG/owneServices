using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class ENettRegistrationRegistryItem : StronglyTypedRegistryItem<EnettRegistrationCode>
	{
		public ENettRegistrationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new ENettRegistrationRegistryDataType(), storage))
		{
		}

		public bool CheckRegistrationCodeIsConfigured(Guid companyPK)
		{
			return !this.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty).RegistrationCode.IsEmpty;
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ENettRegistrationRegistryItemEditor, Enterprise.Accounting.GUI")]
	#if DEBUG
	public
	#endif
	class ENettRegistrationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EnettRegistrationCode>
	{
		public ENettRegistrationRegistryDataType()
		{
		}
	}
}
