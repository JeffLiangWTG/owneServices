using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Web;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrgsRoleAccessCollection))]
	sealed class OrgsRoleAccessCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<OrgsRoleAccessCollection>
	{
		public void TestAllowNew()
		{
			Assert(!Collection.AllowNew);
		}

		#region Overrides

		protected override OrgsRoleAccessCollection GetCollectionToTest()
		{
			return new OrgsRoleAccessCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgsRoleAccess();
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
