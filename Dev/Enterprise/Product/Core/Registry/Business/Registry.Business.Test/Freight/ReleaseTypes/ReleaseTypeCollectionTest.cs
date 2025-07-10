using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ReleaseTypeCollection))]
	sealed class ReleaseTypeCollectionTest : RegistryBusinessObjectCollectionTestCase<ReleaseTypeCollection>
	{
		public void TestParent()
		{
			ReleaseTypeCollection coll = new ReleaseTypeCollection();
			ReleaseType rt = coll.AddNew();
			AssertEquals(0, rt.OriginalsNumber);
			AssertEquals(0, rt.CopiesNumber);

			ReleaseTypes relTypes = new ReleaseTypes();
			relTypes.OriginalsNumber = 4;
			relTypes.CopiesNumber = 5;
			coll.Parent = relTypes;
			rt = coll.AddNew();
			AssertEquals(4, rt.OriginalsNumber);
			AssertEquals(5, rt.CopiesNumber);
		}

		#region Implementation

		protected override ReleaseTypeCollection GetCollectionToTest()
		{
			return new ReleaseTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ReleaseType();
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
