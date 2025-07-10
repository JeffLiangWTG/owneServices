using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class TermsAndConditionsRegistryItem : StronglyTypedRegistryItem<NotificationEmailTemplate>
	{
		public TermsAndConditionsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, Type docSourceType)
			: base(new RegistryItemImpl(name, category, caption, hint, new TermsAndConditionsRegistryDataType(docSourceType), null, storage, RegistryOptions.Default, new NotificationEmailTemplate(docSourceType), false))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.TermsAndConditionsRegistryEditor, ZClientEDI")]
	public class TermsAndConditionsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<NotificationEmailTemplate>
	{
		public TermsAndConditionsRegistryDataType(Type docSourceType)
			: base(new NotificationEmailTemplate(docSourceType))
		{
			this.docSourceType = docSourceType;
		}

		protected override NotificationEmailTemplate DeserialiseCore(byte[] value)
		{
			NotificationEmailTemplate result = base.DeserialiseCore(value);
			result.DocSourceType = docSourceType;
			return result;
		}

		readonly Type docSourceType;
	}
}

