using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RegistryServiceLevelCollection))]
	class RegistryServiceLevelCollectionTest : RegistryBusinessObjectCollectionTestCase<RegistryServiceLevelCollection>
	{
		public void TestAllowNew()
		{
			Assert(!Collection.AllowNew);
		}

		public void TestFindByRefServiceLevelPK()
		{
			ZGuid testPK = new ZGuid();
			RegistryServiceLevel level = Collection.Add(testPK, "TT1", (NoResString)"testDescription", true);
			Collection.Add(new ZGuid(), "TT2", (NoResString)"testDescription2", true);

			AssertEquals(level, Collection.FindByRefServiceLevelPK(testPK));
		}

		#region overrides

		protected override RegistryServiceLevelCollection GetCollectionToTest()
		{
			return new RegistryServiceLevelCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RegistryServiceLevel();
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
