using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CommodityWrapperCollection))]
	sealed class CommodityWrapperCollectionTest : GenericWrapperCollectionTest<CommodityWrapperCollection>
	{
		public void TestLoadFromCommonContainer()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingContainer container = consol.Containers.AddNew();
			ForwardingShipment shipment = consol.Shipments.AddNew();

			CommodityWrapperCollection collection = new CommodityWrapperCollection(container, Factory);
			AssertEquals("collection.Count", 0, collection.Count);

			container.JC_RH_NKContainerCommodityCode = "MTHZ";

			collection = new CommodityWrapperCollection(container, Factory);
			AssertEquals("collection.Count", 1, collection.Count);
			AssertEquals("MTHZ", collection[0].Code);

			PackLine line1 = shipment.OuterPackLines.AddNew();
			line1.JL_JC = container.PK;
			line1.JL_RH_NKCommodityCode = "HAZ";
			PackLine line2 = shipment.OuterPackLines.AddNew();
			line2.JL_JC = container.PK;
			line2.JL_RH_NKCommodityCode = "GEN";
			PackLine line3 = shipment.OuterPackLines.AddNew();
			line3.JL_JC = container.PK;
			line3.JL_RH_NKCommodityCode = "HAZ";

			collection = new CommodityWrapperCollection(container, Factory);
			AssertEquals("collection.Count", 3, collection.Count);
			AssertEquals("GEN", collection[0].Code);
			AssertEquals("HAZ", collection[1].Code);
			AssertEquals("MTHZ", collection[2].Code);
			AssertEquals("MTHZ", collection["coNtainEr"].Code);
		}

		public void TestLoadFromRateOneOffContainer()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);

			var container = quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_ContainerCount = 5;
			container.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			Factory.Save();

			var collection = new CommodityWrapperCollection(container, Factory);
			AssertEquals("Expected no commodity codes", 0, collection.Count);

			quote.CurrentOneOffQuote.TT_RH_NKCommodity = "IRON";

			collection = new CommodityWrapperCollection(container, Factory);
			AssertEquals("Expected only one code as rate one off shipment will set the code for all containers", 1, collection.Count);
			AssertEquals("IRON", collection[0].Code);
			AssertEquals("IRON", collection["container"].Code);

			quote.CurrentOneOffQuote.TT_RH_NKCommodity = "1X1X";

			collection = new CommodityWrapperCollection(container, Factory);
			AssertEquals("Should add to the collection if commodity code is invalid", 0, collection.Count);

			quote.CurrentOneOffQuote.TT_RH_NKCommodity = "OCHM";

			collection = new CommodityWrapperCollection(container, Factory);
			AssertEquals("collection.Count", 1, collection.Count);
			AssertEquals("OCHM", collection[0].Code);
			AssertEquals("OCHM", collection["container"].Code);
		}

		#region Implementation

		protected override CommodityWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new CommodityWrapperCollection(Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new CommodityWrapper(null, Factory);
		}

		#endregion
	}
}
