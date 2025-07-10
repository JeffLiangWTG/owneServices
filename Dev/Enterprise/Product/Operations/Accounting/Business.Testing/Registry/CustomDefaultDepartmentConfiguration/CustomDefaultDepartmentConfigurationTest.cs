using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CustomDefaultDepartmentConfiguration))]
	public class CustomDefaultDepartmentConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Test HTML Properties

		public void TestHtmlProperty()
		{
			var config = (CustomDefaultDepartmentConfiguration)GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, config.Config);
			AssertEquals(ZBlob.Empty, config.Config_HTML);

			config.Config_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(config.Config.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", config.Config_HTML.ToUTF8());
		}

		public void TestHtmlFromTextProperty()
		{
			var config = (CustomDefaultDepartmentConfiguration)GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, config.Config);
			AssertEquals(ZBlob.Empty, config.Config_HTML);

			config.Config = ZBlob.FromUTF8("1234\r\n5678");

			AssertEquals("<p>1234</p><p>5678</p>", config.Config_HTML.ToUTF8());

			config.Config = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");

			AssertEquals("<p>rtf</p>", config.Config_HTML.ToUTF8());
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var fallbackLevel = (BizObj?.CurrentFallbackLevel) ?? NewFallbackLevel();
			return new CustomDefaultDepartmentConfiguration(fallbackLevel, Factory);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return (CustomDefaultDepartmentConfiguration)GetNewBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return (CustomDefaultDepartmentConfiguration)GetNewBusinessObject();
		}

		protected new CustomDefaultDepartmentConfiguration BizObj
		{
			get { return (CustomDefaultDepartmentConfiguration)base.BizObj; }
		}

		#endregion
	}
}
