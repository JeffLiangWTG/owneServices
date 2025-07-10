using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperCollection))]
	sealed class FreightWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<FreightWrapperCollection>
	{
		protected override FreightWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new FreightWrapperCollection((ForwardingConsol)null, Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			return new FreightWrapperFromShipment(shipment, Factory);
		}

		public void TestLoadFromConsolWithDeliveryAgent()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_IsForwardRegistered = true;
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_IsForwardRegistered = true;
			ForwardingShipment shipment3 = consol.Shipments.AddNew();
			shipment3.JS_IsForwardRegistered = true;

			DeliveryAgentOrgHeader deliveryAgent1 = Factory.New<DeliveryAgentOrgHeader>();
			shipment1.JS_OH_DeliveryAgent = deliveryAgent1.PK;
			shipment2.JS_OH_DeliveryAgent = deliveryAgent1.PK;
			DeliveryAgentOrgHeader deliveryAgent2 = Factory.New<DeliveryAgentOrgHeader>();
			shipment3.JS_OH_DeliveryAgent = deliveryAgent2.PK;

			FreightWrapperCollection freightWrappers = new FreightWrapperCollection(consol, deliveryAgent1, Factory);
			AssertEquals("freightWrappers.Count", 2, freightWrappers.Count);
			AssertEquals("freightWrappers[0].WrappedObject", shipment1, freightWrappers[0].WrappedObject);
			AssertEquals("freightWrappers[1].WrappedObject", shipment2, freightWrappers[1].WrappedObject);
			freightWrappers = new FreightWrapperCollection(consol, deliveryAgent2, Factory);
			AssertEquals("freightWrappers.Count", 1, freightWrappers.Count);
			AssertEquals("freightWrappers[0].WrappedObject", shipment3, freightWrappers[0].WrappedObject);
		}

		public void TestLoadFromConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HBL1234TEST";
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HBL9876TEST";

			FreightWrapperCollection freightWrappers = new FreightWrapperCollection(consol, Factory);
			AssertEquals("Collection Count is 2", 2, freightWrappers.Count);
			AssertEquals("First object is correct", shipment1, freightWrappers[0].WrappedObject);
			AssertEquals("First shipment housebill is correct", "HBL1234TEST", freightWrappers[0].HouseBill);
			AssertEquals("Second object is correct", shipment2, freightWrappers[1].WrappedObject);
			AssertEquals("Second shipment housebill is correct", "HBL9876TEST", freightWrappers[1].HouseBill);
		}

		public void TestLoadFromLoadList()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			CFSShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HBL1234TEST";
			CFSShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HBL9876TEST";

			FreightWrapperCollection freightWrappers = new FreightWrapperCollection(consol, Factory);
			AssertEquals("Collection Count is 2", 2, freightWrappers.Count);
			AssertEquals("First object is correct", shipment1, freightWrappers[0].WrappedObject);
			AssertEquals("First shipment housebill is correct", "HBL1234TEST", freightWrappers[0].HouseBill);
			AssertEquals("Second object is correct", shipment2, freightWrappers[1].WrappedObject);
			AssertEquals("Second shipment housebill is correct", "HBL9876TEST", freightWrappers[1].HouseBill);
		}

		public void TestLoadFromShipmentCoLoads()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CommonShipment coloadShipment = shipment.CoLoadShipments.AddNew();
			coloadShipment.JS_HouseBill = "HBLTEST234";
			CommonShipment coloadShipment2 = shipment.CoLoadShipments.AddNew();
			coloadShipment2.JS_HouseBill = "HBL2TEST84";

			FreightWrapperCollection freightWrappers = new FreightWrapperCollection(shipment, RoutingLevel.Shipment, Factory);
			AssertEquals("Collection Count is 2", 2, freightWrappers.Count);
			AssertEquals("freightWrappers[0].HouseBill", "HBLTEST234", freightWrappers[0].HouseBill);
			AssertEquals("freightWrappers[1].HouseBill", "HBL2TEST84", freightWrappers[1].HouseBill);
		}

		public void TestLoadFromCFSShipment()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			CommonShipment coloadShipment = shipment.CoLoadShipments.AddNew();
			coloadShipment.JS_HouseBill = "HBLTEST234";
			CommonShipment coloadShipment2 = shipment.CoLoadShipments.AddNew();
			coloadShipment2.JS_HouseBill = "HBL2TEST84";

			FreightWrapperCollection freightWrappers = new FreightWrapperCollection(shipment, RoutingLevel.Shipment, Factory);
			AssertEquals("Collection Count is 2", 2, freightWrappers.Count);
			AssertEquals("freightWrappers[0].HouseBill", "HBLTEST234", freightWrappers[0].HouseBill);
			AssertEquals("freightWrappers[1].HouseBill", "HBL2TEST84", freightWrappers[1].HouseBill);
		}

		public void TestLoadFromShipmentConsolidations()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol = shipment.Consols.AddNew();
			consol.JK_BookingReference = "BOOK REF 1";
			ForwardingConsol consol1 = shipment.Consols.AddNew();
			consol1.JK_BookingReference = "BOOK REF 2";

			FreightWrapperCollection freightWrappers = new FreightWrapperCollection(shipment, RoutingLevel.Consol, Factory);
			AssertEquals("Collection Count is 2", 2, freightWrappers.Count);
			AssertEquals("freightWrappers[0].BookingReference", "BOOK REF 1", freightWrappers[0].BookingReference);
			AssertEquals("freightWrappers[1].BookingReference", "BOOK REF 2", freightWrappers[1].BookingReference);
		}

		public void TestLoadFromOrder()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL0908954";

			ForwardingConsol consol = shipment.Consols.AddNew();
			consol.JK_BookingReference = "THE FIRST BOOK REF WOW";
			ForwardingConsol consol1 = shipment.Consols.AddNew();
			consol1.JK_BookingReference = "THE SECOND BOOK REF GREAT";

			Order order = Factory.New<Order>();
			order.JD_JS = shipment.PK;

			FreightWrapperCollection shipmentWrappers = new FreightWrapperCollection(order, RoutingLevel.Shipment, Factory);
			AssertEquals("Collection Count is 1", 1, shipmentWrappers.Count);
			AssertEquals("freightWrappers[0].HouseBill", "HBL0908954", shipmentWrappers[0].HouseBill);

			FreightWrapperCollection consolWrappers = new FreightWrapperCollection(order, RoutingLevel.Consol, Factory);
			AssertEquals("Collection Count is 2", 2, consolWrappers.Count);
			AssertEquals("consolWrappers[0].BookingReference", "THE FIRST BOOK REF WOW", consolWrappers[0].BookingReference);
			AssertEquals("consolWrappers[1].BookingReference", "THE SECOND BOOK REF GREAT", consolWrappers[1].BookingReference);
		}

		public void TestLoadFromCartage()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL23412412";
			ForwardingConsol consol = shipment.Consols.AddNew();
			consol.JK_BookingReference = "CONSOL BOOK REF 1";
			ForwardingConsol consol1 = shipment.Consols.AddNew();
			consol1.JK_BookingReference = "CONSOL BOOK REF 2";

			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cartage.JJ_ParentID = shipment.PK;

			FreightWrapperCollection shipmentWrappers = new FreightWrapperCollection(cartage, RoutingLevel.Shipment, Factory);
			AssertEquals("Collection Count is 1", 1, shipmentWrappers.Count);
			AssertEquals("freightWrappers[0].HouseBill", "HBL23412412", shipmentWrappers[0].HouseBill);

			FreightWrapperCollection consolWrappers = new FreightWrapperCollection(cartage, RoutingLevel.Consol, Factory);
			AssertEquals("Collection Count is 2", 2, consolWrappers.Count);
			AssertEquals("consolWrappers[0].BookingReference", "CONSOL BOOK REF 1", consolWrappers[0].BookingReference);
			AssertEquals("consolWrappers[1].BookingReference", "CONSOL BOOK REF 2", consolWrappers[1].BookingReference);
		}

		public void TestLoadFromDeclaration()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "MBL983924JSHF";
			ForwardingConsol consol = shipment.Consols.AddNew();
			consol.JK_BookingReference = "CRAP I HAVE RUN OUT OF OTHER CRAP";
			ForwardingConsol consol1 = shipment.Consols.AddNew();
			consol1.JK_BookingReference = "MORE CRAP IM BORED";

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			FreightWrapperCollection shipmentWrappers = new FreightWrapperCollection(declaration, RoutingLevel.Shipment, Factory);
			AssertEquals("Collection Count is 1", 1, shipmentWrappers.Count);
			AssertEquals("freightWrappers[0].HouseBill", "MBL983924JSHF", shipmentWrappers[0].HouseBill);

			FreightWrapperCollection consolWrappers = new FreightWrapperCollection(declaration, RoutingLevel.Consol, Factory);
			AssertEquals("Collection Count is 2", 2, consolWrappers.Count);
			AssertEquals("consolWrappers[0].BookingReference", "CRAP I HAVE RUN OUT OF OTHER CRAP", consolWrappers[0].BookingReference);
			AssertEquals("consolWrappers[1].BookingReference", "MORE CRAP IM BORED", consolWrappers[1].BookingReference);
		}

		public void TestLoadFromOrgOpportunity()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Some Enquiry Org";
			org.MainAddress.OA_Address1 = "1 Street";
			org.OH_RL_NKClosestPort = "AUSYD";

			var opportunity = org.SalesOpportunities.AddNew();
			var pivots1 = opportunity.RelatedChildActivityPivotCollection;

			var rating3 = Factory.New<Quote>();
			rating3.TH_QuoteNumber = "003";
			rating3.TH_OH = org.PK;
			rating3.TH_OneTimeQuote = true;
			var pivot3 = pivots1.AddNew();
			pivot3.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot3.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot3.RAP_ChildActivityID = rating3.PK;

			var rating2 = Factory.New<Quote>();
			rating2.TH_QuoteNumber = "002";
			rating2.TH_OH = org.PK;
			rating2.TH_OneTimeQuote = true;
			var pivot2 = pivots1.AddNew();
			pivot2.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot2.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot2.RAP_ChildActivityID = rating2.PK;

			var rating1 = Factory.New<Quote>();
			rating1.TH_QuoteNumber = "001";
			rating1.TH_OH = org.PK;
			rating1.TH_OneTimeQuote = true;
			var pivot1 = pivots1.AddNew();
			pivot1.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot1.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot1.RAP_ChildActivityID = rating1.PK;

			Factory.Save();

			var wrapper = FreightWrapper.New(opportunity, Factory);
			var resultList = wrapper[0].SalesRelations.OneOffQuotes;

			AssertEquals("First node should be 001", (resultList[0].WrappedObject as IRatingHeader).TH_QuoteNumber, "001/A");
			AssertEquals("First node should be 002", (resultList[1].WrappedObject as IRatingHeader).TH_QuoteNumber, "002/A");
			AssertEquals("First node should be 003", (resultList[2].WrappedObject as IRatingHeader).TH_QuoteNumber, "003/A");
		}

		public void TestExcludeRatingsFromOtherCompanies()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Some Enquiry Org";
			org.MainAddress.OA_Address1 = "1 Street";
			org.OH_RL_NKClosestPort = "AUSYD";
			var otherCompany = Factory.New<GlbCompany>();
			var opportunity = org.SalesOpportunities.AddNew();
			var pivots1 = opportunity.RelatedChildActivityPivotCollection;

			var rating3 = Factory.New<Quote>();
			rating3.TH_QuoteNumber = "003";
			rating3.TH_OH = org.PK;
			rating3.TH_OneTimeQuote = true;
			rating3.TH_GC = Env.CurrentCompanyPK;
			var pivot3 = pivots1.AddNew();
			pivot3.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot3.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot3.RAP_ChildActivityID = rating3.PK;

			var rating2 = Factory.New<Quote>();
			rating2.TH_QuoteNumber = "002";
			rating2.TH_OH = org.PK;
			rating2.TH_OneTimeQuote = true;
			rating2.TH_GC = otherCompany.PK;
			var pivot2 = pivots1.AddNew();
			pivot2.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot2.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot2.RAP_ChildActivityID = rating2.PK;

			var rating1 = Factory.New<Quote>();
			rating1.TH_QuoteNumber = "001";
			rating1.TH_OH = org.PK;
			rating1.TH_OneTimeQuote = true;
			rating1.TH_GC = otherCompany.PK;
			var pivot1 = pivots1.AddNew();
			pivot1.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot1.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot1.RAP_ChildActivityID = rating1.PK;

			Factory.Save();

			var wrapper = FreightWrapper.New(opportunity, Factory);
			var resultList = wrapper[0].SalesRelations.OneOffQuotes;

			AssertEquals("Only 1 node can be found", resultList.Count, 1);
			AssertEquals("First node should be 003", (resultList[0].WrappedObject as IRatingHeader).TH_QuoteNumber, "003/A");
		}

		public void TestFromReceiveTransportationUnit()
		{
			var rtu = new List<WhsItemReceiveTransportationUnit>
			{
				Factory.New<WhsItemReceiveTransportationUnit>()
			};
			var wrapper = new FreightWrapperCollection(rtu, Factory);
			AssertEquals("Collection Count is 1", 1, wrapper.Count);
		}
	}
}
