using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackProductWrapper))]
	sealed class PackProductWrapperTest : GenericWrapperTest
	{
		public void TestProductCode()
		{
			PackProduct product = Factory.New<PackProduct>();
			product.D2_ProductCode = "XXX";

			PackProductWrapper wrapper = new PackProductWrapper(product, Factory);
			AssertEquals("XXX", wrapper.ProductCode);
		}

		public override void TestWrapperMappingsEmpty()
		{
			PackProductWrapper wrapperEmpty = (PackProductWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.ToString()", ZString.Empty, wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.ProductCode", ZString.Empty, wrapperEmpty.ProductCode);
		}

		public void TestWrapperMappingFull()
		{
			PackProduct product = Factory.New<PackProduct>();
			product.D2_ProductCode = "XXX";

			PackProductWrapper wrapperFull = new PackProductWrapper(product, Factory);
			AssertEquals("wrapperFull.ToString()", "XXX", wrapperFull.ToString());
			AssertEquals("wrapperFull.ProductCode", "XXX", wrapperFull.ProductCode);
		}

		public void TestWrapperMappingSummary_WithOrderLines()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "ORD001";
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineSplitNumber = 1;

			AssertEquals("Pre-condition", "ORD001 - 1", orderLine.OrderAndOrderLineNumber);

			PackProduct product = Factory.NewWithValidTestData<PackProduct>();
			product.D2_JO = orderLine.PK;
			product.D2_ProductCode = "YYY";
			product.D2_ProductQuantity = 900.00m;
			product.D2_ProductUnitOfQty = "PLT";
			Factory.Save();

			PackProductWrapper wrapperFull = new PackProductWrapper(product, Factory);
			AssertEquals("YYY, 900 PLT, ORD001 - 1", wrapperFull.Summary);
		}

		public void TestWrapperMappingSummary_WithOrgSupplierParts()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "ZZZ";
			part.OP_Desc = "Sleeping Tablets";

			PackProduct product = Factory.New<PackProduct>();
			product.D2_ProductCode = part.OP_PartNum;
			product.D2_ProductQuantity = 20m;
			product.D2_ProductUnitOfQty = "BAS";

			PackProductWrapper wrapperFull = new PackProductWrapper(product, Factory);
			AssertEquals("ZZZ, 20 BAS, Sleeping Tablets", wrapperFull.Summary);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PackProductWrapper(null, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
PackProductWrapper                        (Default Field: ProductCode)
======================================================================
Name                                    Type
----------------------------------------------------------------------
ProductCode                             String
ProductDescription                      String
ProductOrderLineNo                      String
ProductQuantity                         Decimal
ProductQuantityUnit                     String
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
			PackProduct product = Factory.New<PackProduct>();
			product.D2_ProductCode = "XXX";
			return new PackProductWrapper(product, Factory);
		}
	}
}
