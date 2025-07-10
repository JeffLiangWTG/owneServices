using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(CodeDescriptionIncidentEmailTemplatePair))]
	class CodeDescriptionIncidentEmailTemplatePairTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestDocumentFields()
		{
			var template = new CodeDescriptionIncidentEmailTemplatePair(typeof(DocSupportIncident));
			AssertNotNull("The document field collection should not be null", template.DocumentFields);

			IDocumentFieldDefinitionCollection availableFields = new DocumentFieldAttributeFinder().FindProperties(typeof(DocSupportIncident));
			AssertEquals("The document field collection should contain all DocumentField properties from the Document source", availableFields, template.DocumentFields);
		}

		public void TestProduct()
		{
			var productCollection = new SystemProductCollection();
			var product = productCollection.AddNew();
			product.Code = "SAP";
			product.Description = "SAP desc";
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productCollection);

			var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
			var template1 = collection.AddNew();
			template1.Code = "DDD";
			AssertEquals(false, template1.ProductInfo.HasErrors());

			template1.Product = "AAA";
			AssertEquals(true, template1.ProductInfo.HasErrors());

			template1.Product = "ENT";
			AssertEquals(false, template1.ProductInfo.HasErrors());

			template1.Product = "SAP";
			AssertEquals(false, template1.ProductInfo.HasErrors());

			template1.Product = "";
			var template2 = collection.AddNew();
			template2.Code = "DDD";
			AssertEquals(true, template2.CodeInfo.HasErrors());

			template2.Product = "ENT";
			AssertEquals(false, template2.CodeInfo.HasErrors());

			template2.Product = "";
			template2.NeedUpgrade = false;
			template2.Code = "DDD";
			AssertEquals(true, template2.CodeInfo.HasErrors());
			template2.NeedUpgrade = true;
			template2.Code = "DDD";
			AssertEquals(false, template2.CodeInfo.HasErrors());
		}

		public void TestGetClone()
		{
			var template = NewPopulatedBusinessObject();
			var clone = template.Clone(template.CurrentFallbackLevel, template.Factory) as CodeDescriptionIncidentEmailTemplatePair;
			AssertEquals("TST", clone.Code);
			AssertEquals((NoResString)"Test Template", clone.Description);
			AssertEquals("ENT", clone.Product);
			AssertEquals(true, clone.NeedUpgrade);
			AssertEquals(typeof(DocSupportIncident), clone.EmailTemplates.LegacyAndERequestV1EmailTemplate.DocSourceType);
			AssertEquals("Test Subject", clone.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject);
			AssertEquals("Test Body", clone.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody);

			AssertEquals(typeof(DocSupportIncident), clone.EmailTemplates.ERequestV2EmailTemplate.DocSourceType);
			AssertEquals("Test Subject 2", clone.EmailTemplates.ERequestV2EmailTemplate.EmailSubject);
			AssertEquals("Test Body 2", clone.EmailTemplates.ERequestV2EmailTemplate.EmailBody);
		}

		public void TestValidateNeedUpgrade()
		{
			var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
			var template1 = collection.AddNew();
			template1.Product = "ENT";
			template1.NeedUpgrade = true;
			AssertEquals(false, template1.NeedUpgradeInfo.HasErrors());

			template1.Product = "BOR";
			template1.NeedUpgrade = true;
			AssertHasError(template1.NeedUpgradeInfo, $"Need Upgrade option is only valid for {ProductTypes.Codes.Enterprise}.");

			template1.Product = "ENT";
			template1.NeedUpgrade = true;
			AssertEquals(false, template1.NeedUpgradeInfo.HasErrors());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return NewPopulatedBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		CodeDescriptionIncidentEmailTemplatePair NewPopulatedBusinessObject()
		{
			var legacyTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "Test Subject", "Test Body");
			var eRequestV2Template = new NotificationEmailTemplate(typeof(DocSupportIncident), "Test Subject 2", "Test Body 2");
			var result = new CodeDescriptionIncidentEmailTemplatePair(typeof(DocSupportIncident), legacyTemplate, eRequestV2Template);
			result.Code = "TST";
			result.Description = (NoResString)"Test Template";
			result.Product = "ENT";
			result.NeedUpgrade = true;
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

		protected override bool IsCodeUniqueInCollection
		{
			get { return false; }
		}

		#endregion
	}
}
