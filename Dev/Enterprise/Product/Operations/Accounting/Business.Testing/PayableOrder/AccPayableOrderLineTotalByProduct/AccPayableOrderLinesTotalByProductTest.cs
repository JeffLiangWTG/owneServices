using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.PayableOrder.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(AccPayableOrderLinesTotalByProduct))]
	internal sealed class AccPayableOrderLinesTotalByProductTest : NonPersistentBusinessObjectTestCase
	{
		public void TestWithoutOrderLines()
		{
			var orderLinesTotalByProduct = new AccPayableOrderLinesTotalByProduct(Factory);
			AssertEquals("", orderLinesTotalByProduct.Product);
			AssertEquals("", orderLinesTotalByProduct.ProductDescription);
			AssertEquals(0M, orderLinesTotalByProduct.Quantity);
			AssertEquals(0M, orderLinesTotalByProduct.QuantityInvoiced);
			AssertEquals(0M, orderLinesTotalByProduct.QuantityReceived);
			AssertEquals(0M, orderLinesTotalByProduct.QuantityRemaining);
		}

		public void TestProductDescription()
		{
			var order = Factory.New<AccPayableOrderHeader>();
			List<AccPayableOrderLine> lines = new List<AccPayableOrderLine>();
			var orderLine = order.OrderLines.AddNew();
			lines.Add(orderLine);
			var orderLinesTotalByProduct = new AccPayableOrderLinesTotalByProduct(Factory);
			orderLinesTotalByProduct.OrderLines = lines;
			AssertEquals("", orderLinesTotalByProduct.ProductDescription);
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "111222";
			part.OP_Desc = "desc";
			var org = Factory.New<OrgHeader>();
			order.SupplierDocumentaryAddress.OrganisationPK = org.PK;
			part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Supplier);
			orderLine.APL_PartNo = part.OP_PartNum;
			orderLinesTotalByProduct = new AccPayableOrderLinesTotalByProduct(Factory);
			orderLinesTotalByProduct.OrderLines = lines;
			AssertEquals("desc", orderLinesTotalByProduct.ProductDescription);
			orderLine.APL_PartNo = "22-333";
			orderLine.APL_Desc = "STLTH 5 CS";
			orderLinesTotalByProduct = new AccPayableOrderLinesTotalByProduct(Factory);
			orderLinesTotalByProduct.OrderLines = lines;
			AssertEquals("Expected order line product description to be overriden by order line description", "STLTH 5 CS", orderLinesTotalByProduct.ProductDescription);
		}

		public void TestQuantities()
		{
			var order = Factory.New<AccPayableOrderHeader>();
			List<AccPayableOrderLine> lines = new List<AccPayableOrderLine>();
			var orderLine = order.OrderLines.AddNew();
			lines.Add(orderLine);
			orderLine.APL_Quantity = 3.00M;
			orderLine.APL_QtyInvoiced = 2.00M;
			orderLine.APL_QtyReceived = 1.00M;
			orderLine.APL_InnerPacks = 4;
			orderLine.APL_OuterPacks = 5;
			orderLine = order.OrderLines.AddNew();
			lines.Add(orderLine);
			orderLine.APL_Quantity = 4.50M;
			orderLine.APL_QtyInvoiced = 3.50M;
			orderLine.APL_QtyReceived = 1.50M;
			orderLine.APL_InnerPacks = 6;
			orderLine.APL_OuterPacks = 7;
			var orderLinesTotalByProduct = new AccPayableOrderLinesTotalByProduct(Factory);
			orderLinesTotalByProduct.OrderLines = lines;
			AssertEquals(7.5M, orderLinesTotalByProduct.Quantity);
			AssertEquals(5.5M, orderLinesTotalByProduct.QuantityInvoiced);
			AssertEquals(2.5M, orderLinesTotalByProduct.QuantityReceived);
			AssertEquals(5.0M, orderLinesTotalByProduct.QuantityRemaining);
			AssertEquals(10, orderLinesTotalByProduct.InnerPacks);
			AssertEquals(12, orderLinesTotalByProduct.OuterPacks);
			//orderLine.APL_Quantity = 5.50M;
			orderLine.APL_QtyInvoiced = 4.50M;
			orderLine.APL_QtyReceived = 2.50M;
			orderLine.APL_InnerPacks = 7;
			orderLine.APL_OuterPacks = 8;
			AssertEquals(7.5M, orderLinesTotalByProduct.Quantity);
			AssertEquals(5.5M, orderLinesTotalByProduct.QuantityInvoiced);
			AssertEquals(2.5M, orderLinesTotalByProduct.QuantityReceived);
			AssertEquals(5.0M, orderLinesTotalByProduct.QuantityRemaining);
			AssertEquals(10, orderLinesTotalByProduct.InnerPacks);
			AssertEquals(12, orderLinesTotalByProduct.OuterPacks);
			orderLinesTotalByProduct = new AccPayableOrderLinesTotalByProduct(Factory);
			orderLinesTotalByProduct.OrderLines = lines;
			AssertEquals(7.5M, orderLinesTotalByProduct.Quantity);
			AssertEquals(6.5M, orderLinesTotalByProduct.QuantityInvoiced);
			AssertEquals(3.5M, orderLinesTotalByProduct.QuantityReceived);
			AssertEquals(4.0M, orderLinesTotalByProduct.QuantityRemaining);
			AssertEquals(11, orderLinesTotalByProduct.InnerPacks);
			AssertEquals(13, orderLinesTotalByProduct.OuterPacks);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AccPayableOrderLinesTotalByProduct(Factory);
		}
	}
}
