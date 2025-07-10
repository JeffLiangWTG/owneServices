using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;
using IFilterControl = Enterprise.Integration.ZArchitecture.IFilterControl;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI.Testing;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class RNSColumnsAndFiltersProviderTest : ColumnsAndFiltersProviderTest
	{
		public void TestRNSReleaseStatus()
		{
			var filter = (ModuleTextFilter)filters["RNS Release Status"];
			AssertNotNull("RNS Release Status filter", filter);
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, EDIReleaseImportEntryStatusList.Codes.GoodsReleased, shipment3, shipment4);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, EDIReleaseImportEntryStatusList.Codes.GoodsReleased, shipment1, shipment2, shipment5, shipment6, shipment7);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, EDIReleaseImportEntryStatusList.Codes.GoodsRequiredForExamination, shipment5);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, EDIReleaseImportEntryStatusList.Codes.GoodsRequiredForExamination, shipment1, shipment2, shipment3, shipment4, shipment6, shipment7);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, EDIReleaseImportEntryStatusList.Codes.Error, shipment6);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, EDIReleaseImportEntryStatusList.Codes.Error, shipment1, shipment2, shipment3, shipment4, shipment5, shipment7);

			const string awaitingResponceStatus = "AWT";
			AssertStatusQuery(filter, SQLComparisonOperator.Equal, awaitingResponceStatus, shipment2, shipment7);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, awaitingResponceStatus, shipment1, shipment3, shipment4, shipment5, shipment6);

			const string notSentStatus = "NST";
			AssertStatusQuery(filter, SQLComparisonOperator.Equal, notSentStatus, shipment1);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, notSentStatus, shipment2, shipment3, shipment4, shipment5, shipment6, shipment7);
		}

		public void TestRNSReleaseDate()
		{
			var filter = (ModuleDateFilter)filters["RNS Release Date"];
			AssertNotNull("RNS Release Date filter", filter);

			AssertRNSReleaseDateQuery(filter, ModuleDateFilter.HasDateEntered, ZDateTime.Empty, ZDateTime.Empty, shipment3, shipment4, shipment5, shipment6);
			AssertRNSReleaseDateQuery(filter, ModuleDateFilter.HasNoDateEntered, ZDateTime.Empty, ZDateTime.Empty, shipment1, shipment2, shipment7);
			AssertRNSReleaseDateQuery(filter, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Now, ZDateTime.Now.AddDays(3), shipment3, shipment4, shipment6);
			AssertRNSReleaseDateQuery(filter, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Now.AddDays(4), ZDateTime.Now.AddDays(6), shipment5);
			AssertRNSReleaseDateQuery(filter, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Empty, ZDateTime.Now.AddDays(3), shipment3, shipment4, shipment6);
			AssertRNSReleaseDateQuery(filter, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Now.AddDays(4), ZDateTime.Empty, shipment5);
		}

		[TestDate(2011, 11, 23)]
		public void TestAddColumns()
		{
			using (var form = new ZForm())
			{
				var collection = new ForwardingShipmentCollection(Factory, new ZQuery(JobShipmentSchema.PK, shipment3.PK));
				collection.Load();
				EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
				var filterControl = new ZFilterStripControlForTesting(collection);
				new CAShipmentModuleColumnsAndFiltersProvider().AddColumns(filterControl);
				form.Controls.Add(filterControl);
				form.Show();

				ResourceStringData groupName = new ResourceStringData("", "Release Notifications (RNS)");
				AssertColumn(filterControl.FilteredGrid, "RNSReleaseStatus", groupName, "RNS Release Status", false, "CLR - Goods Released");
				AssertColumn(filterControl.FilteredGrid, "RNSReleaseDate", groupName, "RNS Release Date", false, "25-NOV-11");
			}
		}

		public void TestSetFetchForView()
		{
			using (var form = new ZForm())
			{
				var newfactory = new BusinessObjectFactory();
				var collection = new ForwardingModuleShipmentCollection(newfactory);
				collection.Load();
				EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
				var filterControl = new ZFilterStripControlForTesting(collection);
				new CAShipmentModuleColumnsAndFiltersProvider().AddColumns(filterControl);
				var filteredGrid = filterControl.FilteredGrid;
				filteredGrid.SetAllColumnsVisible(true);
				form.Controls.Add(filterControl);
				form.Show();

				/*
				 *	JobShipment: 1
				 *	StmData: 1
				 *	
				 *	Hits: 2/2
				 */
				AssertMaxDbHits(2, newfactory);

				filteredGrid.Refresh();

				/*
				 *	EDIMessage: 2
				 *	GenAddOnColumn: 1
				 *	JobDeclaration: 1
				 *	JobShipment: 1
				 *	StmNote: 1
				 *	
				 *	Hits: 6/6
				 */
				AssertMaxDbHits(6, newfactory);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var messaging2 = new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(shipment2));
			AddRNSRequestMessage(messaging2, ZDateTime.Now);

			shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var messaging3 = new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(shipment3));
			AddRNSRequestMessage(messaging3, ZDateTime.Now.AddDays(1));
			AddEDIReleaseMessage(messaging3, ZDateTime.Now.AddDays(2), "4");

			shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			var messaging4 = new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(shipment4));
			AddRNSRequestMessage(messaging4, ZDateTime.Now.AddDays(1));
			AddEDIReleaseMessage(messaging4, ZDateTime.Now.AddDays(2), "1");
			AddEDIReleaseMessage(messaging4, ZDateTime.Now.AddDays(3), "4");

			shipment5 = Factory.NewWithValidTestData<ForwardingShipment>();
			var messaging5 = new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(shipment5));
			AddRNSRequestMessage(messaging5, ZDateTime.Now.AddDays(1));
			AddEDIReleaseMessage(messaging5, ZDateTime.Now.AddDays(2), "14");
			AddRNSRequestMessage(messaging5, ZDateTime.Now.AddDays(4));
			AddRNSRequestMessage(messaging5, ZDateTime.Now.AddDays(5));
			AddEDIReleaseMessage(messaging5, ZDateTime.Now.AddDays(6), "5");
			AddRNSRequestMessage(messaging5, ZDateTime.Now.AddDays(7));
			AddEDIReleaseMessage(messaging5, ZDateTime.Now.AddDays(8), "2", false);

			shipment6 = Factory.NewWithValidTestData<ForwardingShipment>();
			var messaging6 = new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(shipment6));
			AddRNSRequestMessage(messaging6, ZDateTime.Now.AddDays(1));
			AddEDIReleaseMessage(messaging6, ZDateTime.Now.AddDays(2), "14");

			shipment7 = Factory.NewWithValidTestData<ForwardingShipment>();
			var messaging7 = new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(shipment7));
			AddRNSRequestMessage(messaging7, ZDateTime.Now);
			AddRNSRequestMessage(messaging7, ZDateTime.Now.AddDays(1));

			Factory.Save();

			filters = new ModuleFilterCollection();
			new CAShipmentModuleColumnsAndFiltersProvider().AddFilters(filters, Factory);
		}

		void AssertRNSReleaseDateQuery(ModuleDateFilter filter, ZString propertySearch, ZDateTime value1, ZDateTime value2, params ForwardingShipment[] expectedShipments)
		{
			filter.PropertySearch = propertySearch;
			filter.Property1 = value1;
			filter.Property2 = value2;
			AssertProperShipmentsLoaded(filter, expectedShipments);
		}

		static void AddRNSRequestMessage(RNSMessagingBO messaging, ZDateTime createTime)
		{
			var request = messaging.Messages.AddNew(typeof(RNSRequestMessage));
			request.EM_MessageSubType = RNSMessageTypes.Codes.StatusQuery;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_Status = EDIMessage.Status.Sent;
			request.EM_SystemCreateTimeUtc = createTime;
			request.EM_MessageText = RNSRequestMessage.MessageNumberPlaceHolder;
		}

		static void AddEDIReleaseMessage(RNSMessagingBO messaging, ZDateTime createTime, string processingIndicator, bool setReleasedate = true)
		{
			var response = (EDIReleaseMessage)messaging.Messages.AddNew(typeof(EDIReleaseMessage));
			response.EM_MessageSubType = EDIReleaseMessageTest.GetEntryStatusByProcessingIndicatorCoded(processingIndicator);
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_Status = EDIMessage.Status.Received;
			response.EM_SystemCreateTimeUtc = createTime;
			if (setReleasedate)
			{
				response.RNSReleaseDate = createTime;
			}

			response.EM_MessageText = string.Format(@"UNH+<<MSGNO PLACEHOLDER>>+CUSRES:D:96A:UN'BGM+:::125+12345000067897+11'DTM+58:201006221028:203'GIS+{0}'RFF+CN:CCN123456'UNT+6+1'", processingIndicator);
		}

		ForwardingShipment shipment1;
		ForwardingShipment shipment2;
		ForwardingShipment shipment3;
		ForwardingShipment shipment4;
		ForwardingShipment shipment5;
		ForwardingShipment shipment6;
		ForwardingShipment shipment7;
		ModuleFilterCollection filters;

		sealed class ZFilterStripControlForTesting : FilterStripControlTest.DummyZFilterStripControl, IFilterControl
		{
			internal ZFilterStripControlForTesting(IBusinessObjectCollection gridCollection)
				: base(gridCollection, new DummyFilterStripBusinessObject())
			{
			}
		}
	}
}
