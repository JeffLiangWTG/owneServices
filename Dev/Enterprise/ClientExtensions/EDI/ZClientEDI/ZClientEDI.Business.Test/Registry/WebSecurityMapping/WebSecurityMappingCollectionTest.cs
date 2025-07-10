using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(WebSecurityMappingCollection))]
	internal sealed class WebSecurityMappingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<WebSecurityMappingCollection>
	{
		public void TestHasProduct()
		{
			Collection.AddNew("S1", "P1", "M1");
			Collection.AddNew("S1", "P2", "M1");

			AssertEquals(true, Collection.HasProduct("P1"));
			AssertEquals(true, Collection.HasProduct("P2"));
			AssertEquals(false, Collection.HasProduct("P3"));
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override WebSecurityMappingCollection GetCollectionToTest()
		{
			return new WebSecurityMappingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WebSecurityMapping();
		}

		#endregion
	}
}
