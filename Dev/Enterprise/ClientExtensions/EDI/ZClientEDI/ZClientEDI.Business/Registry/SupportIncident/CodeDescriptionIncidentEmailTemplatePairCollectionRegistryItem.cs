using System;
using System.Text;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem : StronglyTypedRegistryItem<CodeDescriptionIncidentEmailTemplatePairCollection>
	{
		public CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CodeDescriptionIncidentEmailTemplatePairCollection defaultTemplateCollection)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryDataType(defaultTemplateCollection), null, storage, RegistryOptions.Default, defaultTemplateCollection, false))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.CodeDescriptionIncidentEmailTemplatePairCollectionRegistryEditor, ZClientEDI")]
	public class CodeDescriptionIncidentEmailTemplatePairCollectionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CodeDescriptionIncidentEmailTemplatePairCollection>
	{
		public CodeDescriptionIncidentEmailTemplatePairCollectionRegistryDataType(CodeDescriptionIncidentEmailTemplatePairCollection defaultTemplateCollection)
			: base()
		{
			this.docSourceType = defaultTemplateCollection.DocSourceType;
			this.defaultTemplateCollection = defaultTemplateCollection;
		}

		readonly Type docSourceType;
		readonly CodeDescriptionIncidentEmailTemplatePairCollection defaultTemplateCollection;

		protected override CodeDescriptionIncidentEmailTemplatePairCollection DeserialiseCore(byte[] value)
		{
			CodeDescriptionIncidentEmailTemplatePairCollection result;

			string xml = Encoding.Unicode.GetString(value);
			if (xml.StartsWith(@"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfCodeDescriptionEmailTemplate", StringComparison.Ordinal))
			{
				var legacyTemplateCollection = new MultipleEmailTemplatesRegistryDataType(docSourceType).Deserialise(value);
				result = GetEmailTemplatePairCollection(legacyTemplateCollection);
			}
			else if (xml.StartsWith(@"<?xml version=""1.0"" encoding=""utf-16""?><IncidentEmailTemplatePair", StringComparison.Ordinal))
			{
				var legacyTemplatePair = new IncidentEmailTemplatePairRegistryDataType(defaultTemplateCollection[0].EmailTemplates).Deserialise(value);
				result = GetEmailTemplatePairCollection(legacyTemplatePair);
			}
			else if (xml.StartsWith(@"<?xml version=""1.0"" encoding=""utf-16""?><NotificationEmailTemplate", StringComparison.Ordinal))
			{
				var legacyTemplate = new NotificationEmailTemplateRegistryDataType(docSourceType).Deserialise(value);
				result = GetEmailTemplatePairCollection(legacyTemplate);
			}
			else
			{
				result = base.DeserialiseCore(value);
				result.DocSourceType = docSourceType;
			}

			return result;
		}

		CodeDescriptionIncidentEmailTemplatePairCollection GetEmailTemplatePairCollection(CodeDescriptionEmailTemplateCollection emailTemplateCollection)
		{
			var result = new CodeDescriptionIncidentEmailTemplatePairCollection(docSourceType);
			foreach (CodeDescriptionEmailTemplate legacyTemplate in emailTemplateCollection)
			{
				var templatePair = result.AddNew();
				templatePair.Code = legacyTemplate.Code;
				templatePair.Description = legacyTemplate.Description;

				var eRequestV2Template = defaultTemplateCollection.ContainsCode(legacyTemplate.Code) ? defaultTemplateCollection.GetEmailTemplate(true, legacyTemplate.Code, "") : legacyTemplate.EmailTemplate;
				if (eRequestV2Template != null)
				{
					templatePair.EmailTemplates = new IncidentEmailTemplatePair(docSourceType, legacyTemplate.EmailTemplate, eRequestV2Template);
				}
				else
				{
					templatePair.EmailTemplates = new IncidentEmailTemplatePair(docSourceType, legacyTemplate.EmailTemplate, new NotificationEmailTemplate());
				}
			}
			return result;
		}

		CodeDescriptionIncidentEmailTemplatePairCollection GetEmailTemplatePairCollection(IncidentEmailTemplatePair emailTemplatePair)
		{
			var result = new CodeDescriptionIncidentEmailTemplatePairCollection(docSourceType);
			var defaultValue = defaultTemplateCollection[0];

			var templatePair = result.AddNew();
			templatePair.Code = defaultValue.Code;
			templatePair.Description = defaultValue.Description;
			templatePair.EmailTemplates = new IncidentEmailTemplatePair(docSourceType, emailTemplatePair.LegacyAndERequestV1EmailTemplate, emailTemplatePair.ERequestV2EmailTemplate);
			return result;
		}

		CodeDescriptionIncidentEmailTemplatePairCollection GetEmailTemplatePairCollection(NotificationEmailTemplate emailTemplate)
		{
			var result = new CodeDescriptionIncidentEmailTemplatePairCollection(docSourceType);
			var defaultValue = defaultTemplateCollection[0];

			var templatePair = result.AddNew();
			templatePair.Code = defaultValue.Code;
			templatePair.Description = defaultValue.Description;
			templatePair.EmailTemplates = new IncidentEmailTemplatePair(docSourceType, emailTemplate, defaultValue.EmailTemplates.ERequestV2EmailTemplate);
			return result;
		}
	}

	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.CodeDescriptionIncidentEmailTemplatePairCollectionRegistryEditor, ZClientEDI")]
	public class CodeDescriptionIncidentEmailTemplatePairCollectionRegistryEditorInfo : IRegistryEditorInfo
	{
		public Type BaseDataTypeToBeEdited => typeof(CodeDescriptionIncidentEmailTemplatePairCollection);
		public bool IsNeedUpgradeColumnVisible { get; set; }
	}
}

