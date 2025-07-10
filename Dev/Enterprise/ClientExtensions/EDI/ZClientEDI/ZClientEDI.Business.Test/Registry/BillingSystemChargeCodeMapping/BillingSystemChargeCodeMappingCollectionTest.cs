using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(BillingSystemChargeCodeMappingCollection))]
	internal sealed class BillingSystemChargeCodeMappingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<BillingSystemChargeCodeMappingCollection>
	{
		#region GetSystems

		public void TestGetSystemCodes()
		{
			var collection = new BillingSystemChargeCodeMappingCollection();
			collection.AddNew("ENT", "ABM", "AAA");
			collection.AddNew("ENT", "ABM", "BBB");
			collection.AddNew("ENT", "USC", "");
			collection.AddNew("SPH", "", "");
			collection.AddNew("SPH", "ABM", "CCC");
			collection.AddNew("SPH", "ABM", "DDD");
			collection.AddNew("SPH", "USC", "");

			AssertContainsExactElementsInAnyOrder(new ZString[] { "ABM", "USC" }, collection.GetSystemCodes("ENT"));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "ABM", "USC" }, collection.GetSystemCodes("SPH"));
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<ZString>(), collection.GetSystemCodes("ZZZ"));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "ABM", "USC", "ABM", "USC" }, collection.GetSystemCodes());
		}

		#endregion

		#region GetSubModules

		public void TestGetSubModuleCodes()
		{
			var collection = new BillingSystemChargeCodeMappingCollection();
			collection.AddNew("ENT", "ABM", "AAA");
			collection.AddNew("ENT", "ABM", "BBB");
			collection.AddNew("ENT", "USC", "");
			collection.AddNew("SPH", "", "");
			collection.AddNew("SPH", "ABM", "BBB");
			collection.AddNew("SPH", "ABM", "CCC");
			collection.AddNew("SPH", "USC", "");

			AssertContainsExactElementsInAnyOrder(new ZString[] { "AAA", "BBB" }, collection.GetSubModuleCodes("ENT", "ABM"));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "BBB", "CCC" }, collection.GetSubModuleCodes("SPH", "ABM"));
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<ZString>(), collection.GetSubModuleCodes("ZZZ", "ZZZ"));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "AAA", "BBB", "BBB", "CCC" }, collection.GetSubModuleCodes());
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BillingSystemChargeCodeMappingCollection GetCollectionToTest()
		{
			return new BillingSystemChargeCodeMappingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BillingSystemChargeCodeMapping();
		}

		#endregion
	}
}
