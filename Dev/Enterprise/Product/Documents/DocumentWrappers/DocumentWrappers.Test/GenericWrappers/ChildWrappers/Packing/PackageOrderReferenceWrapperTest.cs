using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Packing.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageOrderReferenceWrapper))]
	sealed class PackageOrderReferenceWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = (PackageOrderReferenceWrapper)GetNewDocumentWrapper();
			AssertEquals(ZString.Empty, wrapperEmpty.ToString());
			AssertEquals(ZString.Empty, wrapperEmpty.Summary);
			AssertEquals(ZString.Empty, wrapperEmpty.BatchNumber);
			AssertEquals(ZString.Empty, wrapperEmpty.CommercialInvoiceNumber);
			AssertEquals(ZString.Empty, wrapperEmpty.ExpiryDate.ToString());
			AssertEquals(ZString.Empty, wrapperEmpty.LineReference);
			AssertEquals(ZString.Empty, wrapperEmpty.OrderNumber);
			AssertEquals(ZString.Empty, wrapperEmpty.SerialNumber);
			AssertEquals(ZString.Empty, wrapperEmpty.SKUPartNumber);
		}

		public void TestWrapperMappingFull()
		{
			var packageOrderReference = CreateTestPackageOrderReference();

			var wrapperFull = new PackageOrderReferenceWrapper(packageOrderReference, Factory);
			AssertEquals("KPO001", wrapperFull.BatchNumber);
			AssertEquals("CIN001", wrapperFull.CommercialInvoiceNumber);
			AssertEquals("07-Jan-24", wrapperFull.ExpiryDate.ToString());
			AssertEquals("LR001", wrapperFull.LineReference);
			AssertEquals("ON001", wrapperFull.OrderNumber);
			AssertEquals("SN001", wrapperFull.SerialNumber);
			AssertEquals("SKU001", wrapperFull.SKUPartNumber);
			AssertEquals("KPO001, CIN001, 07-Jan-24, LR001, ON001, SN001, SKU001", wrapperFull.Summary);
			AssertEquals("KPO001, CIN001, 07-Jan-24, LR001, ON001, SN001, SKU001", wrapperFull.ToString());
		}

		PkgPackageOrderReference CreateTestPackageOrderReference()
		{
			var packageOrderReference = Factory.New<PkgPackageOrderReference>();
			packageOrderReference.KPO_BatchNumber = "KPO001";
			packageOrderReference.KPO_CommercialInvoiceNumber = "CIN001";
			packageOrderReference.KPO_ExpiryDate = new ZDate(2024, 1, 7);
			packageOrderReference.KPO_KP_Package = new ZGuid();
			packageOrderReference.KPO_LineReference = "LR001";
			packageOrderReference.KPO_OrderNumber = "ON001";
			packageOrderReference.KPO_SerialNumber = "SN001";
			packageOrderReference.KPO_SKUPartNumber = "SKU001";
			return packageOrderReference;
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PackageOrderReferenceWrapper(null, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
PackageOrderReferenceWrapper                  (Default Field: Summary)
======================================================================
Name                                    Type
----------------------------------------------------------------------
BatchNumber                             String
CommercialInvoiceNumber                 String
ExpiryDate                              Date
LineReference                           String
OrderNumber                             String
SerialNumber                            String
SKUPartNumber                           String
Summary                                 String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var packageOrderReference = Factory.New<PkgPackageOrderReference>();
			packageOrderReference.KPO_OrderNumber = "XXX";
			return new PackageOrderReferenceWrapper(packageOrderReference, Factory);
		}
	}
}
