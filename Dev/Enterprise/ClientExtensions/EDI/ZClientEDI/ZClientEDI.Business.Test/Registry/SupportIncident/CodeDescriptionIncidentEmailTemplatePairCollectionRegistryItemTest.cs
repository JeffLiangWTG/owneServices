using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem))]
	class CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<CodeDescriptionIncidentEmailTemplatePairCollection>
	{
		protected override StronglyTypedRegistryItem<CodeDescriptionIncidentEmailTemplatePairCollection, CodeDescriptionIncidentEmailTemplatePairCollection> GetNewRegistryItem()
		{
			return new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident)));
		}
	}

	[TestedType(typeof(CodeDescriptionIncidentEmailTemplatePairCollectionRegistryDataType))]
	class CodeDescriptionIncidentEmailTemplatePairCollectionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CodeDescriptionIncidentEmailTemplatePairCollectionRegistryDataType>
	{
		public void TestDeserialise_FromCodeDescriptionEmailTemplateCollection()
		{
			var oldCodeDescriptionTemplateCollection = new CodeDescriptionEmailTemplateCollection(typeof(DocSupportIncident));
			var oldCodeDescriptionTemplate1 = oldCodeDescriptionTemplateCollection.AddNew();
			oldCodeDescriptionTemplate1.Code = "AAA";
			oldCodeDescriptionTemplate1.Description = (NoResString)"AAA Template";
			oldCodeDescriptionTemplate1.EmailTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "old subject 1", "old body 1");

			var oldCodeDescriptionTemplate2 = oldCodeDescriptionTemplateCollection.AddNew();
			oldCodeDescriptionTemplate2.Code = "BBB";
			oldCodeDescriptionTemplate2.Description = (NoResString)"BBB Template";
			oldCodeDescriptionTemplate2.EmailTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "old subject 2", "old body 2");

			var defaultCodeDescriptionTemplatePairCollection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
			var defaultCodeDescriptionTemplatePair = defaultCodeDescriptionTemplatePairCollection.AddNew();
			defaultCodeDescriptionTemplatePair.Code = "AAA";
			defaultCodeDescriptionTemplatePair.Description = (NoResString)"Default AAA Template Description";
			var defaultTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "default subject 1", "default body 1");
			defaultCodeDescriptionTemplatePair.EmailTemplates = new IncidentEmailTemplatePair(typeof(DocSupportIncident), defaultTemplate, defaultTemplate);

			var registryDataType = new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryDataType(defaultCodeDescriptionTemplatePairCollection);
			var templatePairCollection = registryDataType.Deserialise(new MultipleEmailTemplatesRegistryDataType(typeof(DocSupportIncident)).Serialise(oldCodeDescriptionTemplateCollection));

			AssertEquals(typeof(DocSupportIncident), templatePairCollection.DocSourceType);
			AssertEquals(2, templatePairCollection.Count);
			AssertEquals("Should use old code", "AAA", templatePairCollection[0].Code);
			AssertEquals("Should use old description", "AAA Template", templatePairCollection[0].Description);
			AssertEquals("Should use old subject for legacy", "old subject 1", templatePairCollection[0].EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject);
			AssertEquals("Should use old body for legacy", "old body 1", templatePairCollection[0].EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody);
			AssertEquals("Should use default eRequest v2 subject", "default subject 1", templatePairCollection[0].EmailTemplates.ERequestV2EmailTemplate.EmailSubject);
			AssertEquals("Should use default eRequest v2 body", "default body 1", templatePairCollection[0].EmailTemplates.ERequestV2EmailTemplate.EmailBody);

			AssertEquals("Should use old code", "BBB", templatePairCollection[1].Code);
			AssertEquals("Should use old description", "BBB Template", templatePairCollection[1].Description);
			AssertEquals("Should use old subject for legacy", "old subject 2", templatePairCollection[1].EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject);
			AssertEquals("Should use old body for legacy", "old body 2", templatePairCollection[1].EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody);
			AssertEquals("Should use old subject when there is no default", "old subject 2", templatePairCollection[1].EmailTemplates.ERequestV2EmailTemplate.EmailSubject);
			AssertEquals("Should use old body when there is no default", "old body 2", templatePairCollection[1].EmailTemplates.ERequestV2EmailTemplate.EmailBody);
		}

		public void TestDeserialise_FromIncidentEmailTemplatePair()
		{
			var v1Template = new NotificationEmailTemplate(typeof(DocSupportIncident), "old subject 1", "old body 1");
			var v2Template = new NotificationEmailTemplate(typeof(DocSupportIncident), "old subject 2", "old body 2");
			var emailTemplatePair = new IncidentEmailTemplatePair(typeof(DocSupportIncident), v1Template, v2Template);

			var defaultCodeDescriptionTemplatePairCollection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
			var defaultCodeDescriptionTemplatePair = defaultCodeDescriptionTemplatePairCollection.AddNew();
			defaultCodeDescriptionTemplatePair.Code = "DEF";
			defaultCodeDescriptionTemplatePair.Description = (NoResString)"Default Email Template";
			var defaultTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "default subject 1", "default body 1");
			defaultCodeDescriptionTemplatePair.EmailTemplates = new IncidentEmailTemplatePair(typeof(DocSupportIncident), defaultTemplate, defaultTemplate);

			var registryDataType = new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryDataType(defaultCodeDescriptionTemplatePairCollection);
			var templatePairCollection = registryDataType.Deserialise(new IncidentEmailTemplatePairRegistryDataType(defaultCodeDescriptionTemplatePair.EmailTemplates).Serialise(emailTemplatePair));

			AssertEquals(typeof(DocSupportIncident), templatePairCollection.DocSourceType);
			AssertEquals(1, templatePairCollection.Count);
			AssertEquals("Should use default code", "DEF", templatePairCollection[0].Code);
			AssertEquals("Should use default description", "Default Email Template", templatePairCollection[0].Description);
			AssertEquals("Should use old subject for legacy", "old subject 1", templatePairCollection[0].EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject);
			AssertEquals("Should use old body for legacy", "old body 1", templatePairCollection[0].EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody);
			AssertEquals("Should use old eRequest v2 subject", "old subject 2", templatePairCollection[0].EmailTemplates.ERequestV2EmailTemplate.EmailSubject);
			AssertEquals("Should use old eRequest v2 body", "old body 2", templatePairCollection[0].EmailTemplates.ERequestV2EmailTemplate.EmailBody);
		}

		public void TestDeserialise_FromNotificationEmailTemplate()
		{
			var emailTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "old subject 1", "old body 1");

			var defaultCodeDescriptionTemplatePairCollection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
			var defaultCodeDescriptionTemplatePair = defaultCodeDescriptionTemplatePairCollection.AddNew();
			defaultCodeDescriptionTemplatePair.Code = "DEF";
			defaultCodeDescriptionTemplatePair.Description = (NoResString)"Default Email Template";
			var defaultTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "default subject 1", "default body 1");
			defaultCodeDescriptionTemplatePair.EmailTemplates = new IncidentEmailTemplatePair(typeof(DocSupportIncident), defaultTemplate, defaultTemplate);

			var registryDataType = new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryDataType(defaultCodeDescriptionTemplatePairCollection);
			var templatePairCollection = registryDataType.Deserialise(new NotificationEmailTemplateRegistryDataType(typeof(DocSupportIncident)).Serialise(emailTemplate));

			AssertEquals(typeof(DocSupportIncident), templatePairCollection.DocSourceType);
			AssertEquals(1, templatePairCollection.Count);
			AssertEquals("Should use default code", "DEF", templatePairCollection[0].Code);
			AssertEquals("Should use default description", "Default Email Template", templatePairCollection[0].Description);
			AssertEquals("Should use old subject for legacy", "old subject 1", templatePairCollection[0].EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject);
			AssertEquals("Should use old body for legacy", "old body 1", templatePairCollection[0].EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody);
			AssertEquals("Should use default eRequest v2 subject", "default subject 1", templatePairCollection[0].EmailTemplates.ERequestV2EmailTemplate.EmailSubject);
			AssertEquals("Should use default eRequest v2 body", "default body 1", templatePairCollection[0].EmailTemplates.ERequestV2EmailTemplate.EmailBody);
		}

		protected override CodeDescriptionIncidentEmailTemplatePairCollectionRegistryDataType GetNewDataType()
		{
			return new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryDataType(new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident)));
		}

		protected override string ExpectedEditorName
		{
			get { return "CodeDescriptionIncidentEmailTemplatePairCollectionRegistryEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
			var sampleTemplatePair = sample.AddNew();
			sampleTemplatePair.Code = "AAA";
			sampleTemplatePair.Description = (NoResString)"Sample Template Pair AAA";
			sampleTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Sample legacy subject";
			sampleTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "Sample legacy body";
			sampleTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = "Sample eRequest v2 subject";
			sampleTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = "Sample eRequest v2 body";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample, new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryDataType(sample).Serialise(sample))
			};
		}
	}
}
