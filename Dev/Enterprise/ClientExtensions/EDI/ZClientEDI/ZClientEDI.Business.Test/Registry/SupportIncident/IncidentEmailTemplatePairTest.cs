using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(IncidentEmailTemplatePair))]
	class IncidentEmailTemplatePairTest : RegistryBusinessObjectTemplateTestCase<IncidentEmailTemplatePair>
	{
		public void TestDocumentFields()
		{
			var template = new IncidentEmailTemplatePair(typeof(DocSupportIncident));
			AssertNotNull("The document field collection should not be null", template.DocumentFields);

			IDocumentFieldDefinitionCollection availableFields = new DocumentFieldAttributeFinder().FindProperties(typeof(DocSupportIncident));
			AssertEquals("The document field collection should contain all DocumentField properties from the Document source", availableFields, template.DocumentFields);
		}

		public void TestGetClone()
		{
			var template = NewPopulatedBusinessObject();
			var clone = template.Clone(template.CurrentFallbackLevel, template.Factory) as IncidentEmailTemplatePair;
			AssertEquals(typeof(DocSupportIncident), clone.LegacyAndERequestV1EmailTemplate.DocSourceType);
			AssertEquals("Test Subject", clone.LegacyAndERequestV1EmailTemplate.EmailSubject);
			AssertEquals("Test Body", clone.LegacyAndERequestV1EmailTemplate.EmailBody);

			AssertEquals(typeof(DocSupportIncident), clone.ERequestV2EmailTemplate.DocSourceType);
			AssertEquals("Test Subject 2", clone.ERequestV2EmailTemplate.EmailSubject);
			AssertEquals("Test Body 2", clone.ERequestV2EmailTemplate.EmailBody);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return NewPopulatedBusinessObject();
		}

		protected override IncidentEmailTemplatePair GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override IncidentEmailTemplatePair GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		IncidentEmailTemplatePair NewPopulatedBusinessObject()
		{
			var legacyTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "Test Subject", "Test Body");
			var eRequestV2Template = new NotificationEmailTemplate(typeof(DocSupportIncident), "Test Subject 2", "Test Body 2");
			var result = new IncidentEmailTemplatePair(typeof(DocSupportIncident), legacyTemplate, eRequestV2Template);
			return result;
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
