using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CYDPickupWrapper))]
	sealed class CYDPickupWrapperTest : GenericWrapperTest
	{
		public void TestWrapperMappings()
		{
			var wrapper = (CYDPickupWrapper)GetSetupWrapperForDefaultFormatting();
			CombineAssertions(() =>
			{
				AssertEquals("wrapper.TransportReference", "TEST123", wrapper.TransportReference);
			});
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapper = (CYDPickupWrapper)GetNewDocumentWrapper();
			CombineAssertions(() =>
			{
				AssertEquals("wrapper.TransportReference", "", wrapper.TransportReference);
			});
		}

		protected override ZString ExpectedDefaultFormatting => @"
Registry : (No Default Field Value Available on Registry)";

		protected override string ExpectedFieldMap => @"
CYDPickup
======================================================================
Name                                    Type
----------------------------------------------------------------------
TransportReference                      String";

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CYDPickupWrapper(null, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var pickup = GetNewPickup();
			return new CYDPickupWrapper(pickup, Factory);
		}

		CYDPickup GetNewPickup()
		{
			var pickup = Factory.New<CYDPickup>();
			var user = Factory.New<GlbStaff>();
			user.GS_FullName = "ABC";
			user.GS_Code = "ABC";
			pickup.YPL_TransportReference = "TEST123";
			pickup.YPL_GS_NKLoadUser = "ABC";
			return pickup;
		}
	}
}
