using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MailManager.Business.Testing
{
	[TestedType(typeof(MailItemTemplate))]
	sealed class MailItemTemplateTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<MailItemTemplate>();
		}

		#endregion

		public void TestHumanReadableName()
		{
			var template = Factory.New<MailItemTemplate>();
			AssertEquals("Email Template", template.HumanReadableName);
		}

		public void TestHumanReadableShortcutNameCore()
		{
			var template = Factory.New<MailItemTemplate>();
			AssertEquals("Email Template", template.HumanReadableShortcutName);
			template.MIT_Name = "Project 01";
			AssertEquals("Email Template - Project 01", template.HumanReadableShortcutName);
			template.MIT_Category = "AAA";
			AssertEquals("Email Template - AAA_Project 01", template.HumanReadableShortcutName);
		}

		public void TestTemplateID()
		{
			var template = Factory.New<MailItemTemplate>();
			AssertEquals(string.Empty, template.TemplateID);
			template.MIT_Category = "AAA";
			template.MIT_Name = "Project 01";
			AssertEquals("AAA_Project 01", template.TemplateID);
		}

		public void TestSetDefaultValues()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Japanese))
			{
				var template = Factory.New<MailItemTemplate>();
				AssertEquals(Core.SharedConstants.Languages.Japanese, template.MIT_Language);
			}
		}
	}
}
