using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsOrderLine))]
	public class WoolworthsOrderLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHasJobComInvoiceLineLink()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_JZ = invoice.PK;

			var order = Factory.New<WoolworthsOrder>();
			order.JD_OrderNumber = "TestOrder";

			var orderLine = Factory.New<TestWoolworthsOrderLine>();
			orderLine.JO_JD = order.PK;
			orderLine.JO_Partno = "PENCIL";
			orderLine.JO_LineNo = 1;

			invoiceLine.JI_OrderNumber = "TestOrder";
			invoiceLine.JI_PartNo = "PENCIL";
			invoiceLine.JI_CustomDecimal1 = 1;

			Assert("InvoiceLine is not linked to declaration", !orderLine.HasJobComInvoiceLineLink);

			var dec = Factory.New<JobDeclaration>();
			invoice.JZ_JE = dec.PK;
			Assert("InvoiceLine is linked to declaration", orderLine.HasJobComInvoiceLineLink);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				order.JD_OrderNumber = "TestOrder2";

				var invoiceNZ = Factory.New<BaseJobComInvoiceHeader>();
				var invoiceLineNZ = invoiceNZ.InvoiceLines.AddNew();
				invoiceLineNZ.JI_OrderNumber = "TestOrder2";
				invoiceLineNZ.JI_OrderNumber = "TestOrder";
				invoiceLineNZ.JI_PartNo = "PENCIL";
				invoiceLineNZ.JI_CustomDecimal1 = 1;

				var decNZ = Factory.New<BaseJobDeclaration>();
				invoiceNZ.JZ_JE = decNZ.PK;
			}
			Assert("HasJobComInvoiceLineLink returns false if order is linked to non-AU declaration", !orderLine.HasJobComInvoiceLineLink);
		}

		public void TestPartProxyProperties()
		{
			WoolworthsOrderLine bO = (WoolworthsOrderLine)GetNewBusinessObject();
			bO.JO_Partno = "somepart";
			AssertEquals("No part, no part department", ZString.Empty, bO.JO_PartDepartment);
			AssertEquals("No part, no part prefix", ZString.Empty, bO.JO_PartDivision);
			WoolworthsProduct part = Factory.New<WoolworthsProduct>();
			part.OP_PartNum = "somepart";
			part.RelatedOrganisations.AddOrganisationIfNotExist(bO.Order.BuyerPK, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();
			part.OP_Department = "dep";
			Factory.Save();
			AssertEquals("JO_Department", "dep", bO.JO_PartDepartment);
			part.OP_Division = "prefix";
			Factory.Save();
			AssertEquals("JO_PartDivision", "prefix", bO.JO_PartDivision);
		}

		public void TestCustomAttrib3SetOnProductFirstUsed()
		{
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "splaty";
			order.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			WoolworthsOrderLine line1 = (WoolworthsOrderLine)order.OrderLines.AddNew();
			line1.JO_LineNo = 1;
			WoolworthsOrderLine line2 = (WoolworthsOrderLine)order.OrderLines.AddNew();
			line2.JO_LineNo = 2;
			var part = Factory.New<Customs.Business.OrgSupplierPart>();
			part.OP_PartNum = "tst9";
			part.RelatedOrganisations.AddOrganisationIfNotExist(order.Buyer.PK, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();
			line1.JO_Partno = "tst9";
			AssertEquals("Part first used on Order line", true, line1.JO_ProductFirstUsedHere);
			line2.JO_Partno = "tst9";
			AssertEquals("Part already used, no flag", false, line2.JO_ProductFirstUsedHere);
			line1.JO_Partno = "tst5";
			AssertEquals("Part set to something else, no flag", false, line1.JO_ProductFirstUsedHere);
			line2.JO_Partno = "tst9";
			AssertEquals("Part is no longer used by anything except this now", true, line2.JO_ProductFirstUsedHere);
		}

		#region Implementation
		class TestWoolworthsOrderLine : WoolworthsOrderLine
		{
			public TestWoolworthsOrderLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new bool HasJobComInvoiceLineLink
			{
				get
				{
					return base.HasJobComInvoiceLineLink;
				}
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			WoolworthsOrder order = Factory.New<WoolworthsOrder>();
			order.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.JD_OrderNumber = "ordnum";
			WoolworthsOrderLine result = (WoolworthsOrderLine)order.OrderLines.AddNew();
			result.JO_LineNo = 2;
			return result;
		}
		#endregion

	}
}
