using System.Linq;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageAuditFailureWrapperCollection))]
	sealed class PackageAuditFailureWrapperCollectionTest : GenericWrapperCollectionTest<PackageAuditFailureWrapperCollection>
	{
		#region TestCollectionItems 

		public void TestCollectionItems_NoAudit()
		{
			var wrapper = new PackageAuditFailureWrapperCollection(Factory);
			AssertEquals("Collection must be empty", 0, wrapper.Count);
		}

		public void TestCollectionItems_SuccessfulAudit()
		{
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, Data.Part1, 50m);
			var audit = Helper.CreateWhsPackageAudit(order, "PAK1");
			AssertNotNull("Precondition", audit);

			var wrapper = new PackageAuditFailureWrapperCollection(audit, Factory);
			AssertEquals("Collection must be empty", 0, wrapper.Count);
		}

		public void TestCollectionItems_AuditWithFailures()
		{
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, Data.Part1, 50m);
			var audit = Helper.CreateWhsPackageAudit(order, "PAK1");
			var failure1 = helper.CreateWhsPackageAuditLineFailure(audit, Data.Part1, 10m, 9m);
			var failure2 = helper.CreateWhsPackageAuditLineFailure(audit, Data.Part2, 20m, 21m);
			AssertEquals("Precondition: Audit must have 2 failures.", 2, audit.PackageAuditFailureLines.Count);

			var collection = new PackageAuditFailureWrapperCollection(audit, Factory);
			var wrappedAuditFailures = collection.Cast<PackageAuditFailureWrapper>().Select(w => (WhsPackageAuditLineFailure)w.WrappedObject);
			AssertEquals("Collection must have 2 items.", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { failure1, failure2 }, wrappedAuditFailures);
		}

		#endregion

		#region GenericWrapperCollectionOverride

		protected override PackageAuditFailureWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new PackageAuditFailureWrapperCollection(Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new PackageAuditFailureWrapper(null, Factory);
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		TestDataSimpleEnvironment Data
		{
			get { return data ?? (data = new TestDataSimpleEnvironment(Factory, 2, 2)); }
		}
		TestDataSimpleEnvironment data;

		#endregion
	}
}
