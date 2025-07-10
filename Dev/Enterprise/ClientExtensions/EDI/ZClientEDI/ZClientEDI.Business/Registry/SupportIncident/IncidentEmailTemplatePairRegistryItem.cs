using System;
using System.Text;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class IncidentEmailTemplatePairRegistryItem : StronglyTypedRegistryItem<IncidentEmailTemplatePair>
	{
		public IncidentEmailTemplatePairRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, IncidentEmailTemplatePair defaultTemplatePair)
			: base(new RegistryItemImpl(name, category, caption, hint, new IncidentEmailTemplatePairRegistryDataType(defaultTemplatePair), null, storage, RegistryOptions.Default, defaultTemplatePair, false))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.IncidentEmailTemplatePairRegistryEditor, ZClientEDI")]
	public class IncidentEmailTemplatePairRegistryDataType : NonPersistentBusinessObjectRegistryDataType<IncidentEmailTemplatePair>
	{
		public IncidentEmailTemplatePairRegistryDataType(IncidentEmailTemplatePair defaultTemplatePair)
			: base(new IncidentEmailTemplatePair())
		{
			this.docSourceType = defaultTemplatePair.DocSourceType;
			this.defaultTemplatePair = defaultTemplatePair;
		}

		readonly Type docSourceType;
		readonly IncidentEmailTemplatePair defaultTemplatePair;

		protected override IncidentEmailTemplatePair DeserialiseCore(byte[] value)
		{
			IncidentEmailTemplatePair result;

			string xml = Encoding.Unicode.GetString(value);
			if (xml.StartsWith(@"<?xml version=""1.0"" encoding=""utf-16""?><NotificationEmailTemplate", StringComparison.Ordinal))
			{
				var legacyEmailTemplate = new NotificationEmailTemplateRegistryDataType(docSourceType).Deserialise(value);
				result = new IncidentEmailTemplatePair(docSourceType, legacyEmailTemplate, defaultTemplatePair.ERequestV2EmailTemplate);
			}
			else
			{
				result = base.DeserialiseCore(value);
				result.DocSourceType = docSourceType;
				result.LegacyAndERequestV1EmailTemplate.DocSourceType = docSourceType;
				result.ERequestV2EmailTemplate.DocSourceType = docSourceType;
			}

			return result;
		}
	}
}

