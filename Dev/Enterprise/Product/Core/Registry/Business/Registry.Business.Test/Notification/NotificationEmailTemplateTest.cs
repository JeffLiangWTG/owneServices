using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(NotificationEmailTemplate))]
	sealed class NotificationEmailTemplateTest : RegistryBusinessObjectTemplateTestCase<NotificationEmailTemplate>
	{
		public void TestProperties()
		{
			NotificationEmailTemplate template = new NotificationEmailTemplate();
			Assert("By default email subject should be empty", template.RawEmailBody.IsEmpty);
			Assert("By default email body should be empty", template.RawEmailBody.IsEmpty);

			template = new NotificationEmailTemplate(typeof(BusinessObject), "some subject", "test setting email template through constructor");
			AssertEquals("some subject", template.EmailSubject);
			AssertEquals("test setting email template through constructor", template.RawEmailBody);

			template.EmailSubject = "new subject";
			template.EmailBody = "new body";
			AssertEquals("new subject", template.EmailSubject);
			AssertEquals("new body", template.RawEmailBody);
		}

		// Do not trim these properties due to following scenario:
		// User has inputted in Email(Subject|Body) "Dear " and then double-clicked on a macro on NotificationEmailTemplateControl.
		// NotificationEmailTemplateControl's cursorPosBeforeLosingFocus will be set to 5. But propertyToInsertDocumentFieldTo.Value would be 4.
		// It would result in failing the condition within ZString.Insert(), and if the condition isn't placed 
		// the end result would be "Dear(*macro*)" instead of "Dear (*macro*)".
		public void TestPropertiesDoNotGetTrimmed()
		{
			var template = new NotificationEmailTemplate(typeof(BusinessObject), "some subject with trailing character ", "test setting email template through constructor with trailing character ");
			AssertEquals("some subject with trailing character ", template.EmailSubject);
			AssertEquals("test setting email template through constructor with trailing character ", template.RawEmailBody);

			template.EmailSubject = "new subject with trailing character ";
			template.EmailBody = "new body with trailing character ";
			AssertEquals("new subject with trailing character ", template.EmailSubject);
			AssertEquals("new body with trailing character ", template.RawEmailBody);
		}

		public void TestEnglishProperties()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.EnglishBritish))
			using (var mockRes = Res.UseMockData())
			using (var britishRes = Res.GetLanguageInstance(Core.SharedConstants.Languages.EnglishBritish).UseMockData())
			{
				mockRes.Put("Subject", new ResourceStringData("Subject", "Subject"));
				mockRes.Put("Body", new ResourceStringData("Body", "Body"));
				britishRes.Put("Subject", new ResourceStringData("Subject", "British English Subject"));
				britishRes.Put("Body", new ResourceStringData("Body", "British English Body"));

				var template = new NotificationEmailTemplate();
				template.RawEmailSubject = (NoResString)"Subject";
				template.RawEmailBody = (NoResString)"Body";

				AssertEquals("Subject", template.EnglishEmailSubject);
				AssertEquals("Body", template.EnglishEmailBody);
			}
		}

		public void TestDocumentFields()
		{
			NotificationEmailTemplate template = new NotificationEmailTemplate(typeof(BusinessObject));
			AssertNotNull("The document field collection should not be null", template.DocumentFields);

			IDocumentFieldDefinitionCollection availableFields = ObjectFactory.Get<IDocumentFieldAttributeFinder>().FindProperties(typeof(BusinessObject));
			AssertEquals("The document field collection should contain all DocumentField properties from the Document source", availableFields, template.DocumentFields);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			NotificationEmailTemplate result = new NotificationEmailTemplate();
			result.DocSourceType = typeof(BusinessObject);
			return result;
		}

		protected override NotificationEmailTemplate GetBusinessObjectToClone()
		{
			NotificationEmailTemplate result = new NotificationEmailTemplate();
			result.DocSourceType = typeof(BusinessObject);
			result.EmailSubject = "test email subject";
			result.EmailBody = "test email body";
			return result;
		}

		protected override NotificationEmailTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
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
