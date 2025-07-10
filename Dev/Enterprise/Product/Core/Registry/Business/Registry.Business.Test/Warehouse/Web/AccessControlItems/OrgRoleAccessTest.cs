using Enterprise.Registry.Business.Web;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrgsRoleAccess))]
	sealed class OrgRoleAccessTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestConstructor()
		{
			OrgsRoleAccess testObject = new OrgsRoleAccess("testRole", new string[5]);
			AssertEquals("testRole", testObject.Role);
			AssertEquals(5, testObject.LastUsedPropertyNum);
		}

		#region Overrides

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new OrgsRoleAccess();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
