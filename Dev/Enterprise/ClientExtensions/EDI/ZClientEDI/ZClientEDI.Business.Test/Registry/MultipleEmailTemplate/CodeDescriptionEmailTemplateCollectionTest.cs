using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(CodeDescriptionEmailTemplateCollection))]
	internal sealed class CodeDescriptionEmailTemplateCollectionTest : RegistryBusinessObjectCollectionTestCase<CodeDescriptionEmailTemplateCollection>
	{
		public void TestGetEmailTemplate()
		{
			CodeDescriptionEmailTemplateCollection collection = new CodeDescriptionEmailTemplateCollection(typeof(DocSupportIncident));

			CodeDescriptionEmailTemplate template1 = collection.AddNew();
			template1.Code = "AAA";
			template1.Description = (NoResString)"Template One";
			template1.EmailTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "Incident subject one", "Email body template one");
			CodeDescriptionEmailTemplate template2 = collection.AddNew();
			template2.Code = "BBB";
			template2.Description = (NoResString)"Template Two";
			template2.EmailTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "Incident subject two", "Email body template two");

			AssertEquals("Incident subject one", collection.GetEmailTemplate("AAA").EmailSubject);
			AssertEquals("Email body template one", collection.GetEmailTemplate("AAA").EmailBody);
			AssertEquals("Incident subject two", collection.GetEmailTemplate("BBB").EmailSubject);
			AssertEquals("Email body template two", collection.GetEmailTemplate("BBB").EmailBody);
			AssertEquals("", collection.GetEmailTemplate("CCC").EmailSubject);
			AssertEquals("", collection.GetEmailTemplate("CCC").EmailBody);
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

		protected override CodeDescriptionEmailTemplateCollection GetCollectionToTest()
		{
			return new CodeDescriptionEmailTemplateCollection(typeof(DocSupportIncident), NewFallbackLevel());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CodeDescriptionEmailTemplate(NewFallbackLevel());
		}

		#endregion
	}
}
