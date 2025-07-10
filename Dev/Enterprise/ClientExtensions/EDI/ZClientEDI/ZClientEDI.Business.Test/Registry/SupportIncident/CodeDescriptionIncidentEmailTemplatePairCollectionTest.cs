using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(CodeDescriptionIncidentEmailTemplatePairCollection))]
	class CodeDescriptionIncidentEmailTemplatePairCollectionTest : RegistryBusinessObjectCollectionTestCase<CodeDescriptionIncidentEmailTemplatePairCollection>
	{
		public void TestGetEmailTemplate()
		{
			var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));

			var legacyNotificationTemplate1 = new NotificationEmailTemplate(typeof(DocSupportIncident), "Legacy subject AAA", "Legacy body AAA");
			var eRequestV2NotificationTemplate1 = new NotificationEmailTemplate(typeof(DocSupportIncident), "eRequest v2 subject AAA", "eRequest v2 body AAA");
			var codeDescriptionTemplatePair1 = collection.AddNew();
			codeDescriptionTemplatePair1.Code = "AAA";
			codeDescriptionTemplatePair1.Description = (NoResString)"Template AAA";
			codeDescriptionTemplatePair1.EmailTemplates = new IncidentEmailTemplatePair(typeof(DocSupportIncident), legacyNotificationTemplate1, eRequestV2NotificationTemplate1);

			var legacyNotificationTemplate2 = new NotificationEmailTemplate(typeof(DocSupportIncident), "Legacy subject BBB", "Legacy body BBB");
			var eRequestV2NotificationTemplate2 = new NotificationEmailTemplate(typeof(DocSupportIncident), "eRequest v2 subject BBB", "eRequest v2 body BBB");
			var codeDescriptionTemplatePair2 = collection.AddNew();
			codeDescriptionTemplatePair2.Code = "BBB";
			codeDescriptionTemplatePair2.Description = (NoResString)"Template BBB";
			codeDescriptionTemplatePair2.EmailTemplates = new IncidentEmailTemplatePair(typeof(DocSupportIncident), legacyNotificationTemplate2, eRequestV2NotificationTemplate2);

			var productCollection = new SystemProductCollection();
			var product = productCollection.AddNew();
			product.Code = "SAP";
			product.Description = "SAP desc";
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productCollection);

			var legacyNotificationTemplate31 = new NotificationEmailTemplate(typeof(DocSupportIncident), "Legacy subject DDD", "Legacy body DDD");
			var eRequestV2NotificationTemplate31 = new NotificationEmailTemplate(typeof(DocSupportIncident), "eRequest v2 subject DDD", "eRequest v2 body DDD");
			var codeDescriptionTemplatePair31 = collection.AddNew();
			codeDescriptionTemplatePair31.Code = "DDD";
			codeDescriptionTemplatePair31.Description = (NoResString)"Template DDD";
			codeDescriptionTemplatePair31.EmailTemplates = new IncidentEmailTemplatePair(typeof(DocSupportIncident), legacyNotificationTemplate31, eRequestV2NotificationTemplate31);

			var legacyNotificationTemplate32 = new NotificationEmailTemplate(typeof(DocSupportIncident), "Legacy subject DDD 2", "Legacy body DDD 2");
			var eRequestV2NotificationTemplate32 = new NotificationEmailTemplate(typeof(DocSupportIncident), "eRequest v2 subject DDD 2", "eRequest v2 body DDD 2");
			var codeDescriptionTemplatePair32 = collection.AddNew();
			codeDescriptionTemplatePair32.Code = "DDD";
			codeDescriptionTemplatePair32.Description = (NoResString)"Template DDD 2";
			codeDescriptionTemplatePair32.Product = "SAP";
			codeDescriptionTemplatePair32.EmailTemplates = new IncidentEmailTemplatePair(typeof(DocSupportIncident), legacyNotificationTemplate32, eRequestV2NotificationTemplate32);

			var legacyNotificationTemplate33 = new NotificationEmailTemplate(typeof(DocSupportIncident), "Legacy subject DDD 2", "Legacy body DDD 2 - NeedUpgrade:true");
			var eRequestV2NotificationTemplate33 = new NotificationEmailTemplate(typeof(DocSupportIncident), "eRequest v2 subject DDD 2", "eRequest v2 body DDD 2 - NeedUpgrade:true");
			var codeDescriptionTemplatePair33 = collection.AddNew();
			codeDescriptionTemplatePair33.Code = "DDD";
			codeDescriptionTemplatePair33.Description = (NoResString)"Template DDD 2";
			codeDescriptionTemplatePair33.Product = "SAP";
			codeDescriptionTemplatePair33.NeedUpgrade = true;
			codeDescriptionTemplatePair33.EmailTemplates = new IncidentEmailTemplatePair(typeof(DocSupportIncident), legacyNotificationTemplate33, eRequestV2NotificationTemplate33);

			AssertEquals("Legacy subject AAA", collection.GetEmailTemplate(false, "AAA", "").EmailSubject);
			AssertEquals("Legacy body AAA", collection.GetEmailTemplate(false, "AAA", "").EmailBody);
			AssertEquals("eRequest v2 subject AAA", collection.GetEmailTemplate(true, "AAA", "").EmailSubject);
			AssertEquals("eRequest v2 body AAA", collection.GetEmailTemplate(true, "AAA", "").EmailBody);

			AssertEquals("Legacy subject BBB", collection.GetEmailTemplate(false, "BBB", "").EmailSubject);
			AssertEquals("Legacy body BBB", collection.GetEmailTemplate(false, "BBB", "").EmailBody);
			AssertEquals("eRequest v2 subject BBB", collection.GetEmailTemplate(true, "BBB", "").EmailSubject);
			AssertEquals("eRequest v2 body BBB", collection.GetEmailTemplate(true, "BBB", "").EmailBody);

			AssertNull(collection.GetEmailTemplate(false, "CCC", ""));
			AssertNull(collection.GetEmailTemplate(true, "CCC", ""));

			AssertEquals("Legacy subject DDD", collection.GetEmailTemplate(false, "DDD", "").EmailSubject);
			AssertEquals("Legacy body DDD", collection.GetEmailTemplate(false, "DDD", "").EmailBody);
			AssertEquals("eRequest v2 subject DDD", collection.GetEmailTemplate(true, "DDD", "").EmailSubject);
			AssertEquals("eRequest v2 body DDD", collection.GetEmailTemplate(true, "DDD", "").EmailBody);

			AssertEquals("Legacy subject DDD", collection.GetEmailTemplate(false, "DDD", "ENT").EmailSubject);
			AssertEquals("Legacy body DDD", collection.GetEmailTemplate(false, "DDD", "ENT").EmailBody);
			AssertEquals("eRequest v2 subject DDD", collection.GetEmailTemplate(true, "DDD", "ENT").EmailSubject);
			AssertEquals("eRequest v2 body DDD", collection.GetEmailTemplate(true, "DDD", "ENT").EmailBody);

			AssertEquals("Legacy subject DDD 2", collection.GetEmailTemplate(false, "DDD", "SAP").EmailSubject);
			AssertEquals("Legacy body DDD 2", collection.GetEmailTemplate(false, "DDD", "SAP").EmailBody);
			AssertEquals("eRequest v2 subject DDD 2", collection.GetEmailTemplate(true, "DDD", "SAP").EmailSubject);
			AssertEquals("eRequest v2 body DDD 2", collection.GetEmailTemplate(true, "DDD", "SAP").EmailBody);

			var template = collection.GetEmailTemplate(true, "DDD", "SAP", true);
			AssertEquals("eRequest v2 subject DDD 2", template.EmailSubject);
			AssertEquals("eRequest v2 body DDD 2 - NeedUpgrade:true", template.EmailBody);
			template = collection.GetEmailTemplate(true, "SAP", true);
			AssertEquals("eRequest v2 subject DDD 2", template.EmailSubject);
			AssertEquals("eRequest v2 body DDD 2 - NeedUpgrade:true", template.EmailBody);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CodeDescriptionIncidentEmailTemplatePairCollection GetCollectionToTest()
		{
			return new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CodeDescriptionIncidentEmailTemplatePair(typeof(DocSupportIncident));
		}

		#endregion
	}
}
