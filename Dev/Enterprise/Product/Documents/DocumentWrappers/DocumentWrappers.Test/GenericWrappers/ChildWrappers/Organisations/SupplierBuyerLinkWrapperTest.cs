using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(SupplierBuyerLinkWrapper))]
	sealed class SupplierBuyerLinkWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var emptyWrapper = new SupplierBuyerLinkWrapper(null, Factory);
			AssertEquals("VendorID", ZString.Empty, emptyWrapper.VendorID);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var supplierBuyerLink = Factory.New<OrgSupplierBuyerLink>();
			return new SupplierBuyerLinkWrapper(supplierBuyerLink, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
SupplierBuyerLink
======================================================================
Name                                    Type
----------------------------------------------------------------------
VendorID                                String";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var supplierBuyerLink = Factory.New<OrgSupplierBuyerLink>();
			return new SupplierBuyerLinkWrapper(supplierBuyerLink, Factory);
		}
	}
}
