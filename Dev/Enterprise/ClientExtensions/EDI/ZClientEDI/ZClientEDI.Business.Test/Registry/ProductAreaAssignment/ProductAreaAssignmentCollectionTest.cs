using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ProductAreaAssignmentCollection))]
	internal sealed class ProductAreaAssignmentCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ProductAreaAssignmentCollection>
	{
		public void TestGetAssignedStaffCodeByProductArea()
		{
			ProductAreaAssignmentCollection collection = new ProductAreaAssignmentCollection();
			collection.AddNew("ARC", "SCW");
			collection.AddNew("CUS", "TST");
			collection.AddNew("BLK", "COO");

			AssertEquals("SCW", collection.GetAssignedStaffCodeByProductArea("ARC"));
			AssertEquals("TST", collection.GetAssignedStaffCodeByProductArea("CUS"));
			AssertEquals("", collection.GetAssignedStaffCodeByProductArea("INT"));
			AssertEquals("COO", collection.GetAssignedStaffCodeByProductArea(""));
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ProductAreaAssignmentCollection GetCollectionToTest()
		{
			return new ProductAreaAssignmentCollection(NewFallbackLevel(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ProductAreaAssignment(NewFallbackLevel(), Factory);
		}

		#endregion
	}
}
