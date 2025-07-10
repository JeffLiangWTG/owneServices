using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CYDDeliveryWrapper))]
	sealed class CYDDeliveryWrapperTest : GenericWrapperTest
	{
		public void TestWrapperMappings()
		{
			var wrapper = (CYDDeliveryWrapper)GetSetupWrapperForDefaultFormatting();
			CombineAssertions(() =>
			{
				AssertEquals("wrapper.TransportReference", "TEST123", wrapper.TransportReference);
			});
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapper = (CYDDeliveryWrapper)GetNewDocumentWrapper();
			CombineAssertions(() =>
			{
				AssertEquals("wrapper.TransportReference", "", wrapper.TransportReference);
			});
		}

		protected override ZString ExpectedDefaultFormatting => @"
Registry : (No Default Field Value Available on Registry)";

		protected override string ExpectedFieldMap => @"
CYDDelivery
======================================================================
Name                                    Type
----------------------------------------------------------------------
TransportReference                      String";

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CYDDeliveryWrapper(null, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var delivery = GetNewDelivery();
			return new CYDDeliveryWrapper(delivery, Factory);
		}

		CYDDelivery GetNewDelivery()
		{
			var delivery = Factory.New<CYDDelivery>();
			var user = Factory.New<GlbStaff>();
			user.GS_FullName = "ABC";
			user.GS_Code = "ABC";
			delivery.YDL_TransportReference = "TEST123";
			delivery.YDL_GS_NKUnloadUser = "ABC";
			return delivery;
		}
	}
}
