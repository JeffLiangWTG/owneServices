using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(IncidentEmailTemplatePairRegistryItem))]
	class IncidentEmailTemplatePairRegistryItemTest : StronglyTypedRegistryItemTestCase<IncidentEmailTemplatePair>
	{
		protected override StronglyTypedRegistryItem<IncidentEmailTemplatePair, IncidentEmailTemplatePair> GetNewRegistryItem()
		{
			return new IncidentEmailTemplatePairRegistryItem("", null, null, null, RegistryStorageFlags.System, new IncidentEmailTemplatePair(typeof(DocSupportIncident)));
		}
	}

	[TestedType(typeof(IncidentEmailTemplatePairRegistryDataType))]
	class IncidentEmailTemplatePairRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<IncidentEmailTemplatePairRegistryDataType>
	{
		public void TestDeserialiseFromNotificationEmailTemplate()
		{
			var oldNotificationEmailTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "old subject", "old body");

			var defaultLegacyTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "default legacy subject", "default legacy body");
			var defaultERequestTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "default eRequest v2 subject", "default eRequest v2 body");
			var dataType = new IncidentEmailTemplatePairRegistryDataType(new IncidentEmailTemplatePair(typeof(DocSupportIncident), defaultLegacyTemplate, defaultERequestTemplate));

			var templatePair = dataType.Deserialise(new NotificationEmailTemplateRegistryDataType(typeof(DocSupportIncident)).Serialise(oldNotificationEmailTemplate));
			AssertEquals(typeof(DocSupportIncident), templatePair.DocSourceType);
			AssertEquals("Should use old notification email template", "old subject", templatePair.LegacyAndERequestV1EmailTemplate.EmailSubject);
			AssertEquals("Should use old notification email template", "old body", templatePair.LegacyAndERequestV1EmailTemplate.EmailBody);
			AssertEquals("Should use default eRequest v2 notification email template", "default eRequest v2 subject", templatePair.ERequestV2EmailTemplate.EmailSubject);
			AssertEquals("Should use default eRequest v2 notification email template", "default eRequest v2 body", templatePair.ERequestV2EmailTemplate.EmailBody);
		}

		protected override IncidentEmailTemplatePairRegistryDataType GetNewDataType()
		{
			return new IncidentEmailTemplatePairRegistryDataType(new IncidentEmailTemplatePair(typeof(DocSupportIncident)));
		}

		protected override string ExpectedEditorName
		{
			get { return "IncidentEmailTemplatePairRegistryEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample = new IncidentEmailTemplatePair(typeof(DocSupportIncident));
			sample.LegacyAndERequestV1EmailTemplate.EmailSubject = "Sample legacy subject";
			sample.LegacyAndERequestV1EmailTemplate.EmailBody = "Sample legacy body";
			sample.ERequestV2EmailTemplate.EmailSubject = "Sample eRequest v2 subject";
			sample.ERequestV2EmailTemplate.EmailSubject = "Sample eRequest v2 body";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample, new IncidentEmailTemplatePairRegistryDataType(sample).Serialise(sample))
			};
		}
	}
}
