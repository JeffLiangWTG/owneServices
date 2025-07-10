using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(DiscountSuspensionPolicyCollection))]
	internal sealed class DiscountSuspensionPolicyCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DiscountSuspensionPolicyCollection>
	{
		public void TestGetPolicyByProduct()
		{
			var collection = GetCollectionToTest();
			AssertNull(collection.GetPolicyByProduct(""));
			AssertNull(collection.GetPolicyByProduct("CW1"));

			collection.AddNew("CW1", "NVR");
			collection.AddNew("ENT", "ALW");
			AssertNull(collection.GetPolicyByProduct(""));
			AssertNull(collection.GetPolicyByProduct("BOR"));
			AssertEquals("NVR", collection.GetPolicyByProduct("CW1").PolicyCode);
			AssertEquals("ALW", collection.GetPolicyByProduct("ENT").PolicyCode);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override DiscountSuspensionPolicyCollection GetCollectionToTest()
		{
			return new DiscountSuspensionPolicyCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DiscountSuspensionPolicy();
		}
	}
}
