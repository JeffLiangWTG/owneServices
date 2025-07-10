using NUnit.Framework;

namespace Enterprise.Accounting.Web.WebService.Testing
{
	public class SecuritySOAPHeaderTestCase : TestCase
	{
		#region Test Cases

		public void TestConstructors()
		{
			AssertNotNull(SecurityHeader);
			AssertEquals("", SecurityHeader.UserName);
			AssertNotEquals(SecurityHeader, new SecuritySOAPHeader());
			AssertEquals("", SecurityHeader.Password);
		}

		public void TestUserName()
		{
			AssertEquals("", SecurityHeader.UserName);

			SecurityHeader.UserName = "1234";
			AssertEquals("1234", SecurityHeader.UserName);

			SecurityHeader.UserName = "4321";
			AssertEquals("4321", SecurityHeader.UserName);
		}

		public void TestPassword()
		{
			AssertEquals("", SecurityHeader.Password);

			SecurityHeader.Password = "1234";
			AssertEquals("1234", SecurityHeader.Password);

			SecurityHeader.Password = "4321";
			AssertEquals("4321", SecurityHeader.Password);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			SecurityHeader = new SecuritySOAPHeader();
		}

		SecuritySOAPHeader SecurityHeader;

		#endregion
	}
}
