using System;
using Enterprise.Customs.DataRegistry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	class SettingsTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestURI()
		{
			CustomsWareRegistry.Instance.URI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Uri");
			NUnit.Framework.Assert.That(Settings.Instance.Uri, Is.EqualTo("Uri"));
		}

		[ExpectNoExceptions]
		public void TestUserName()
		{
			CustomsWareRegistry.Instance.UserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "UserName");
			NUnit.Framework.Assert.That(Settings.Instance.UserName, Is.EqualTo("UserName"));
		}

		[ExpectNoExceptions]
		public void TestPassword()
		{
			CustomsWareRegistry.Instance.Password.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Password");
			NUnit.Framework.Assert.That(Settings.Instance.Password, Is.EqualTo("Password"));
		}

		[ExpectNoExceptions]
		public void TestCompany()
		{
			CustomsDataRegistry.Instance.CustomsWareCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Company");
			NUnit.Framework.Assert.That(Settings.Instance.Company, Is.EqualTo("Company"));
		}

		[ExpectNoExceptions]
		public void TestApplicationID()
		{
			CustomsWareRegistry.Instance.ApplicationID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ApplicationID");
			NUnit.Framework.Assert.That(Settings.Instance.ApplicationID, Is.EqualTo("ApplicationID"));
		}
	}
}
