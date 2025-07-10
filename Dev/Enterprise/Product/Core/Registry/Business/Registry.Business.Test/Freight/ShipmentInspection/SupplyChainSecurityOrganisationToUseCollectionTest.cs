using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SupplyChainSecurityOrganisationToUseCollection))]
	sealed class SupplyChainSecurityOrganisationToUseCollectionTest : RegistryBusinessObjectCollectionTestCase<SupplyChainSecurityOrganisationToUseCollection>
	{
		public void TestAllowNewAndRemove()
		{
			var col = new SupplyChainSecurityOrganisationToUseCollection();
			AssertEquals(false, col.AllowNew);
			AssertEquals(false, col.AllowRemove);
		}

		public void TestLookupByKey()
		{
			var col = new SupplyChainSecurityOrganisationToUseCollection();
			col.Add(new SupplyChainSecurityOrganisationToUse() { Code = "ABC", Description = (NoResString)"ABC Type", ValidationCode = "YES" });
			col.Add(new SupplyChainSecurityOrganisationToUse() { Code = "XYZ", Description = (NoResString)"XYZ Type", ValidationCode = "NO" });

			AssertEquals("ABC Type", col["ABC"].Description);
			AssertEquals("XYZ Type", col["XYZ"].Description);
			AssertNull(col["DEF"]);
		}

		#region Implementation

		protected override SupplyChainSecurityOrganisationToUseCollection GetCollectionToTest()
		{
			return new SupplyChainSecurityOrganisationToUseCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SupplyChainSecurityOrganisationToUse();
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		#endregion
	}
}
