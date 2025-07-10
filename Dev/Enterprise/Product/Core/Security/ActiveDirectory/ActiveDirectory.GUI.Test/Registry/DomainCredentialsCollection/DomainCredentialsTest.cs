using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	[TestedType(typeof(DomainCredentials))]
	class DomainCredentialsTest : RegistryBusinessObjectTemplateTestCase<DomainCredentials>
	{
		#region Password Encryption Tests

		[GuiTest]
		public void TestMaskTheReturnValueWhenUsePasswordMacros()
		{
			var domainCredentials = GetTestBizo();

			var template = this.Factory.New<StmNoteTemplate>();
			template.S8_TemplateText = "<DomainUserPassword>";

			using (var form = new TextTemplateForm(template, new BusinessObject[] { domainCredentials }, true))
			{
				form.Show();
				form.ClickPreviewButton_ForTest();
				AssertEquals("macro <DomainUserPassword> return Empty", "***", form.PreviewFormTextBoxText_ForTest);
			}

			template.S8_TemplateText = "<DefaultPassword>";

			using (var form = new TextTemplateForm(template, new BusinessObject[] { domainCredentials }, true))
			{
				form.Show();
				form.ClickPreviewButton_ForTest();
				AssertEquals("macro <DefaultPassword> return empty", "***", form.PreviewFormTextBoxText_ForTest);
			}
		}

		DomainCredentials GetTestBizo()
		{
			return new DomainCredentials
			{
				DomainName = TestConstants.Domain,
				DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
				DomainUserPassword = TestConstants.ADTestUserAccount.Password,
				IsDefaultDomain = true,
				UserOrganisationalUnit = TestConstants.ValidOU,
				GroupOrganisationalUnit = TestConstants.ValidOU,
				DefaultPassword = "Changeme1234"
			};
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override DomainCredentials GetBusinessObjectToSerialise() => GetTestBizo();

		protected override DomainCredentials GetBusinessObjectToClone() => GetTestBizo();

		#endregion
	}
}
