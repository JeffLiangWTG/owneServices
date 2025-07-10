using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(CodeDescriptionEmailTemplate))]
	internal class CodeDescriptionEmailTemplateTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestEmailTemplate()
		{
			CodeDescriptionEmailTemplate template = new CodeDescriptionEmailTemplate();
			AssertEquals("", template.EmailTemplate.EmailSubject);
			AssertEquals("", template.EmailTemplate.EmailBody);

			NotificationEmailTemplate emailTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "Subject", "Body");
			template.EmailTemplate = emailTemplate;
			AssertEquals(typeof(DocSupportIncident), template.EmailTemplate.DocSourceType);
			AssertEquals("Subject", template.EmailTemplate.EmailSubject);
			AssertEquals("Body", template.EmailTemplate.EmailBody);
		}

		public void TestGetClone()
		{
			CodeDescriptionEmailTemplate template = NewPopulatedBusinessObject();
			CodeDescriptionEmailTemplate clone = template.Clone(template.CurrentFallbackLevel, template.Factory) as CodeDescriptionEmailTemplate;
			AssertEquals("TTT", clone.Code);
			AssertEquals((NoResString)"Test Template", clone.Description);
			AssertEquals(typeof(DocSupportIncident), clone.EmailTemplate.DocSourceType);
			AssertEquals("Test Subject TTT", clone.EmailTemplate.EmailSubject);
			AssertEquals("Test Body TTT", clone.EmailTemplate.EmailBody);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return NewPopulatedBusinessObject();
		}

		CodeDescriptionEmailTemplate NewPopulatedBusinessObject()
		{
			CodeDescriptionEmailTemplate result = new CodeDescriptionEmailTemplate(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
			result.Code = "TTT";
			result.Description = (NoResString)"Test Template";
			result.Bool = true;
			result.EmailTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "Test Subject TTT", "Test Body TTT");
			return result;
		}

		protected new CodeDescriptionEmailTemplate BizObj
		{
			get { return (CodeDescriptionEmailTemplate)base.BizObj; }
		}

		#endregion
	}
}
