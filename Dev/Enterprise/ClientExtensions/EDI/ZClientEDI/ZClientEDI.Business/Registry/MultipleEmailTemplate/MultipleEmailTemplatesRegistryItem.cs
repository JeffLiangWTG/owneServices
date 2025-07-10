using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class MultipleEmailTemplatesRegistryItem : StronglyTypedRegistryItem<CodeDescriptionEmailTemplateCollection, CodeDescriptionEmailTemplateCollection>
	{
		public MultipleEmailTemplatesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, Type docSourceType)
			: this(name, category, caption, hint, storage, docSourceType, new CodeDescriptionEmailTemplateCollection(docSourceType))
		{
		}

		public MultipleEmailTemplatesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, Type docSourceType, CodeDescriptionEmailTemplateCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new MultipleEmailTemplatesRegistryDataType(docSourceType), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.MultipleEmailTemplatesRegistryEditor, ZClientEDI")]
	public class MultipleEmailTemplatesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CodeDescriptionEmailTemplateCollection>
	{
		public MultipleEmailTemplatesRegistryDataType(Type docSourceType)
			: base()
		{
			this.docSourceType = docSourceType;
		}

		readonly Type docSourceType;

		protected override CodeDescriptionEmailTemplateCollection DeserialiseCore(byte[] value)
		{
			CodeDescriptionEmailTemplateCollection result = base.DeserialiseCore(value);
			foreach (CodeDescriptionEmailTemplate codeDescriptionEmailTemplate in result)
			{
				codeDescriptionEmailTemplate.EmailTemplate.DocSourceType = docSourceType;
			}
			return result;
		}
	}
}

