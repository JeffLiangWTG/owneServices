using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsOrder))]
	public class WoolworthsOrderTest : EnterpriseBusinessObjectTestCase
	{
		#region Metadata
		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Enterprise.Client.Wow.Metadata.WoolworthsOrder);
			}
		}

		#endregion

		public void TestHasJobComInvoiceLineLink()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_JZ = invoice.PK;

			var order = Factory.NewWithValidTestData<TestWoolworthsOrder>(TestBusinessObjectKind.MinimumRequiredToSave);
			order.JD_OrderNumber = "TestOrder";
			invoiceLine.JI_OrderNumber = "TestOrder";
			Assert("InvoiceLine is not linked to declaration", !order.HasJobComInvoiceLineLink);

			var dec = Factory.New<JobDeclaration>();
			invoice.JZ_JE = dec.PK;
			Assert("InvoiceLine is linked to declaration", order.HasJobComInvoiceLineLink);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				order.JD_OrderNumber = "TestOrder2";

				var invoiceNZ = Factory.New<BaseJobComInvoiceHeader>();
				var invoiceLineNZ = invoiceNZ.InvoiceLines.AddNew();
				invoiceLineNZ.JI_OrderNumber = "TestOrder2";

				var decNZ = Factory.New<BaseJobDeclaration>();
				invoiceNZ.JZ_JE = decNZ.PK;
			}
			Assert("HasJobComInvoiceLineLink returns false if order is linked to non-AU declaration", !order.HasJobComInvoiceLineLink);
		}

		public void TestShouldSendToEdiTrack()
		{
			var order = (WoolworthsOrder)GetNewBusinessObject();
			order.JD_OrderNumber = "00000001";
			var orderLine = (WoolworthsOrderLine)order.OrderLines.AddNew();
			var delivery = (WoolworthsOrderLineDelivery)orderLine.Deliveries.AddNew();
			var container = (WoolworthsOrderLineDeliverContainer)delivery.Containers.AddNew();
			var declaration = (WoolworthsJobDeclaration)Factory.New(typeof(JobDeclaration));
			var invoiceHeader = (WoolworthsJobComInvoiceHeader)declaration.Invoices.AddNew();
			var invoiceLine = (WoolworthsJobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_OrderNumber = "00000001";
			Assert(!order.ShouldSendToEdiTrack());
			order.JD_OrderStatus = Constants.OrderStatus.Incomplete;
			Assert(!order.ShouldSendToEdiTrack());
			order.OrderLines[0].Delete();
			Assert(!order.ShouldSendToEdiTrack());
			invoiceLine.JI_OrderNumber = "16362648";
			order.JD_OrderStatus = Constants.OrderStatus.Confirmed;
			Assert(order.ShouldSendToEdiTrack());
		}

		public void TestPopulateDeliverPointsOnAllLines()
		{
			var order = (WoolworthsOrder)GetNewBusinessObject();
			var line1 = order.OrderLines.AddNew();
			var line2 = order.OrderLines.AddNew();
			var delivery1 = line1.Deliveries.AddNew();
			var delivery2 = line2.Deliveries.AddNew();
			var delivery3 = line2.Deliveries.AddNew();
			var delivery4 = line2.Deliveries.AddNew();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = Factory.New<OrgAddress>();
			address1.OA_OH = org.PK;
			address1.OA_Code = "1234";
			var address2 = Factory.New<OrgAddress>();
			address2.OA_OH = org.PK;
			address2.OA_Code = "1111";
			var address3 = Factory.New<OrgAddress>();
			address3.OA_OH = org.PK;
			address3.OA_Code = "1111";
			delivery1.J4_CustomAttribute2 = "999";
			delivery2.J4_CustomAttribute2 = "1234";
			delivery3.J4_CustomAttribute2 = "1111";
			delivery4.J4_CustomAttribute2 = "1111";
			order.BuyerPK = org.PK;
			AssertEquals("Can't find address", ZString.Empty, delivery1.J4_OA_NKDeliveryPoint);
			AssertEquals("Address match found", address1.OA_Code, delivery2.J4_OA_NKDeliveryPoint);
			AssertEquals("Duplicate addresses", ZString.Empty, delivery3.J4_OA_NKDeliveryPoint);
			AssertEquals("Duplicate addresses", ZString.Empty, delivery4.J4_OA_NKDeliveryPoint);
			AssertEquals(2, order.JD_OA_BuyerAddressInfo.GetWarnings().GetUniqueMessageList().Length);
			List<INotification> buyerWarnings = new List<INotification>(order.JD_OA_BuyerAddressInfo.GetWarnings());
			Assert("not found warning", buyerWarnings[0].Message.ToLower().IndexOf("could not find") != -1);
			Assert("duplicate warning", buyerWarnings[1].Message.ToLower().IndexOf("more than 1") != -1);
		}

		public void TestNewProperties()
		{
			WoolworthsOrder order = (WoolworthsOrder)GetNewBusinessObject();
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = Factory.LoadTop1<RefCountry>(new ZQuery()).RN_Code;
			order.JD_CustomAttrib2 = "OTHERBUYER";
			order.SupplierPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, order.JD_RN_CountryOfOrigin);
			order.SupplierPK = supplier.PK;
			AssertNotNull(supplier.MiscServ.EXDefaultCntryOfOrigin);
			AssertEquals(supplier.MiscServ.EXDefaultCntryOfOrigin.PK, order.JD_RN_CountryOfOrigin);
		}

		public void TestMatchBuyerOnOrderLineProductsDuringSave()
		{
			WoolworthsProduct part = Factory.New<WoolworthsProduct>();
			part.OP_PartNum = "partno";
			Factory.Save();
			WoolworthsOrder order = Factory.New<WoolworthsOrder>();
			OrgHeader testOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
			order.BuyerPK = testOrg.PK;
			order.SupplierPK = testOrg.PK;
			order.JD_OrderNumber = "order";
			WoolworthsOrderLine line = (WoolworthsOrderLine)order.OrderLines.AddNew();
			line.JO_Partno = "partno";
			line.JO_LineNo = 2;
			Factory.Save();
			AssertNotNull("Part matched to buyer", part.RelatedOrganisations.FindByOrganisationPKAndRelationship(testOrg.PK, OrgPartRelation.RelationshipTypes.Owner));
			AssertEquals("Order should be marked as matched now so matching doesn't happen on that order again", true, order[WoolworthsOrder.HasMatchedProductBuyersProperty.Name]);
		}

		public void TestPopulateOrderNumber()
		{
			string nextOrderNumber = Env.NumberFountains.OrderNumber.PeekPreliminaryFormatted(Factory);
			WowDataRegistry.Instance.EnableOrderNumberFountain = true;
			Order order = (Order)GetNewBusinessObject();
			order.JD_OrderNumber = "";
			Factory.Save();
			AssertEquals("OrderNumber populated from number fountain", nextOrderNumber, order.JD_OrderNumber);
		}

		public void TestUpdateOrderStatusesForReport()
		{
			WoolworthsOrder order = Factory.New<WoolworthsOrder>();
			order.JD_OrderStatus = Constants.OrderStatus.Incomplete;
			order.JD_OrderNumber = "testorder";
			order.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			Factory.Save();
			TestFilterField field = new TestFilterField(Factory);
			WoolworthsOrder retrievedOrder = Factory.Load<WoolworthsOrder>(order.PK);
			AssertEquals("testorder", retrievedOrder.JD_OrderNumber);
		}

		#region MatchBuyerOnOrderLineProducts
		public void TestMatchBuyerOnOrderLineProducts_WhenNoOrgPartRelations()
		{
			WoolworthsProduct part = Factory.New<WoolworthsProduct>();
			part.OP_PartNum = "part";
			ZGuid buyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			Factory.Save();
			WoolworthsOrder order = (WoolworthsOrder)GetNewBusinessObject();
			order.BuyerPK = buyerPK;
			order.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			WoolworthsOrderLine line1 = (WoolworthsOrderLine)order.OrderLines.AddNew();
			line1.JO_Partno = part.OP_PartNum;
			WoolworthsOrderLine line2 = (WoolworthsOrderLine)order.OrderLines.AddNew();
			line2.JO_Partno = part.OP_PartNum;
			Factory.Save();
			NotificationBuffer buffer = new NotificationBuffer(null);
			order.MatchBuyerOnOrderLineProducts(buffer);
			AssertEquals("No errors should occur", false, buffer.ContainsNotificationType(WowErrorType.MissingPKFromNK));
			AssertNotNull("Part should have a buyer that is the order buyer", part.RelatedOrganisations.FindByOrganisationPKAndRelationship(buyerPK, OrgPartRelation.RelationshipTypes.Owner));
		}

		public void TestMatchBuyerOnOrderLineProducts_WhenSomeOtherBuyerAttached()
		{
			OrgHeader someRandomBuyer = Factory.New<OrgHeader>();
			someRandomBuyer.OH_FullName = "buyer1";
			someRandomBuyer.OH_Code = "buy1";
			someRandomBuyer.MainAddress.OA_Address1 = "addr1";
			OrgHeader buyerForOrder = Factory.New<OrgHeader>();
			buyerForOrder.OH_FullName = "buyer2";
			buyerForOrder.OH_Code = "buy2";
			buyerForOrder.MainAddress.OA_Address1 = "addr2";
			WoolworthsProduct part = Factory.New<WoolworthsProduct>();
			part.OP_PartNum = "part";
			part.RelatedOrganisations.AddOrganisationIfNotExist(someRandomBuyer.PK, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();
			WoolworthsOrder order = (WoolworthsOrder)GetNewBusinessObject();
			order.BuyerPK = buyerForOrder.PK;
			order.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			WoolworthsOrderLine line1 = (WoolworthsOrderLine)order.OrderLines.AddNew();
			line1.JO_Partno = part.OP_PartNum;
			WoolworthsOrderLine line2 = (WoolworthsOrderLine)order.OrderLines.AddNew();
			line2.JO_Partno = part.OP_PartNum;
			Factory.Save();
			NotificationBuffer buffer = new NotificationBuffer(null);
			order.MatchBuyerOnOrderLineProducts(buffer);
			AssertEquals("No errors should occur", false, buffer.HasErrors);
			AssertNotNull("Buyer on order should be assigned to the part now", part.RelatedOrganisations.FindByOrganisationPKAndRelationship(buyerForOrder.PK, OrgPartRelation.RelationshipTypes.Owner));
		}

		public void TestMatchBuyerOnOrderLineProducts_AmbiguousMatch()
		{
			WoolworthsProduct product1 = Factory.New<WoolworthsProduct>();
			product1.OP_PartNum = "part";
			WoolworthsProduct product2 = Factory.New<WoolworthsProduct>();
			product2.OP_PartNum = "part";
			Factory.Save();
			WoolworthsOrder order = (WoolworthsOrder)GetNewBusinessObject();
			order.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			WoolworthsOrderLine line1 = (WoolworthsOrderLine)order.OrderLines.AddNew();
			line1.JO_Partno = "part";
			WoolworthsOrderLine line2 = (WoolworthsOrderLine)order.OrderLines.AddNew();
			line2.JO_Partno = "part";
			Factory.Save();
			NotificationBuffer buffer = new NotificationBuffer(null);
			order.MatchBuyerOnOrderLineProducts(buffer);
			AssertEquals("More than 1 part number should be detected", true, buffer.ContainsNotificationType(WowErrorType.MoreThan1PartNumber));
			AssertEquals(0, product1.RelatedOrganisations.Count);
			AssertEquals(0, product2.RelatedOrganisations.Count);
		}

		public void TestMatchBuyerOnOrderLineProducts_WhenNoProductFound()
		{
			WoolworthsOrderLine orderLine = Factory.NewWithValidTestData<WoolworthsOrderLine>();
			orderLine.JO_Partno = "product";
			Factory.Save();
			NotificationBuffer buffer = new NotificationBuffer(null);
			((WoolworthsOrder)orderLine.Order).MatchBuyerOnOrderLineProducts(buffer);
			AssertEquals("Although no part was found, no error should be shown to the user because Woolworths import their product file at a later stage", false, buffer.HasErrors);
		}

		#endregion
		#region Test Classes

		class TestWoolworthsOrder : WoolworthsOrder
		{
			public TestWoolworthsOrder(BusinessObjectFactory factory, DataRow row) : base(factory, row)
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

		[Enterprise.Core.NonSerializedClass]
		class TestFilterField : FilterField
		{
			public TestFilterField(BusinessObjectFactory factory) : base(factory)
			{
			}

			public override object ValueAsObject
			{
				get
				{
					return null;
				}
			}

			protected override string NonEmptyWhereClause()
			{
				return "OrderNumber=@OrderNumber";
			}

			protected override SqlParameterList GetSqlParametersCore()
			{
				var result = new SqlParameterList();
				result.Add(new SqlParameter("OrderNumber", "testorder"));
				return result;
			}

			public override bool IsEmpty
			{
				get
				{
					return false;
				}
			}

			public override void FillFilterData(ReportFilterData reportFilterData)
			{
				//Do nothing
			}

			public override void SetFilterValue(ReportFilterData reportFilterData)
			{
				//Do nothing
			}

			public override FilterFieldSuggestedUserControlType SuggestedUserControlType
			{
				get
				{
					return FilterFieldSuggestedUserControlType.None;
				}
			}

			public override void SafeCopyValuesFrom(IFilter source)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public override void ClearValues()
			{
				throw new Exception("The method or operation is not implemented.");
			}

			protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
			{
				throw new NotImplementedException();
			}
		}

		#endregion
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			WoolworthsOrder result = Factory.New<WoolworthsOrder>();
			result.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			result.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			result.JD_OrderNumber = "myiord";
			return result;
		}
		#endregion
	}
}
