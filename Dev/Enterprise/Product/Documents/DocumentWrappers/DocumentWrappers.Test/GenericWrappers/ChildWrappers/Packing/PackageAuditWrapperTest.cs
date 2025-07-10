using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageAuditWrapper))]
	sealed class PackageAuditWrapperTest : GenericWrapperTest
	{
		#region GenericWrapperOverride

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"Registry : (No Default Field Value Available on Registry)";
			}
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
PackageAuditWrapper                     (Default Field: CompletedTime)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Auditor                                 String
CompletedTime                           DateTimeOffset
DockDoorLocation                        String

Failures                                PackageAuditFailureWrapper Collection
";
			}
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = new PackageAuditWrapper(null, Factory);
			AssertEquals("Auditor", "", wrapperEmpty.Auditor);
			AssertEquals("CompletedTime", ZDateTimeOffset.Empty, wrapperEmpty.CompletedTime);
			AssertEquals("Failures", 0, wrapperEmpty.Failures.Count);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PackageAuditWrapper(null, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new PackageAuditWrapper(null, Factory);
		}

		#endregion

		#region TestConstructor

		public void TestConstructor()
		{
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, "Order", Data.Part1, 50m);
			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var audit = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m);
			var wrapper = new PackageAuditWrapper(audit, Factory);

			AssertEquals("Audit must be the one passed in.", audit, wrapper.WrappedObject);
		}

		#endregion

		#region TestProperties

		#region TestBasicProperties

		public void TestBasicProperties()
		{
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, "Order", Data.Part1, 50m);
			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var completeTime = ZDateTimeOffset.Now;

			var audit = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m, completeTime);
			audit.WPA_GS_NKAuditor = "USR";
			AssertNotNull("Precondition: ", audit);

			var wrapper = new PackageAuditWrapper(audit, Factory);
			CombineAssertions("Error testing basic properties of audit wrapper: ", () =>
			{
				AssertEquals("Auditor is incorrect", "USR", wrapper.Auditor);
				AssertEquals("CompletedTime is incorrect", completeTime, wrapper.CompletedTime);
				AssertEquals("DockDoorLocation is incorrect", completeTime, wrapper.CompletedTime);
			});
		}

		#endregion

		#region TestGetFailures

		public void TestGetFailuresForFailedAudit()
		{
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, "Order", Data.Part1, 50m);
			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var audit = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m);
			Helper.CreateWhsPackageAuditLineFailure(audit, Data.Part1, 50m, 49m);

			var wrapper = new PackageAuditWrapper(audit, Factory);
			AssertNotNull("Wrapper must have failures.", wrapper.Failures);
			AssertEquals("Wrapper must have 2 failures (2 lines).", 2, wrapper.Failures.Count);
			AssertContainsExactElementsInAnyOrder(audit.PackageAuditFailureLines, wrapper.Failures.Cast<PackageAuditFailureWrapper>().Select(w => w.WrappedObject));
		}

		public void TestGetFailuresForPassedAudit()
		{
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, "Order", Data.Part1, 50m);
			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var audit = Helper.CreateWhsPackageAudit(package);

			var wrapper = new PackageAuditWrapper(audit, Factory);
			AssertEquals("Wrapper must NOT have failures.", 0, wrapper.Failures.Count);
		}

		#endregion

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}
		PackingTestHelper packingHelper;

		TestDataSimpleEnvironment Data
		{
			get { return data ?? (data = new TestDataSimpleEnvironment(Factory, 2, 2)); }
		}
		TestDataSimpleEnvironment data;

		#endregion
	}
}
