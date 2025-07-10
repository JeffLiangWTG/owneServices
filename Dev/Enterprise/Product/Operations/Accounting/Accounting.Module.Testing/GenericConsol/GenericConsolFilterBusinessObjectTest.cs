using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GenericConsol;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GenericConsolFilterBusinessObject))]
	public class GenericConsolFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		public void TestInvoiceConsolXMLDataHaveTheSameFieldsAsFilters()
		{
			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			var invoiceConsolXMLDataFieldsList = universalTransactionWrapper.GetConsolDataFieldNameList_ForTestOnly();

			var invoiceConsolXMLDataFiltersToExcludeList = new[]
					{
						"Consol Target #"
					};

			var operationsFilters = FilterBizO.ModuleFilters.Select(x => x.MultilingualDescription.ToString());

			var excludedFiltersList = new[]
					{
						"House Bill", //Excluded as it’s not a consol property. It’s a shipment property.
						"Shipment #", //Excluded as it’s not a consol property. It’s a shipment property.
						"Consignment Run-sheet Number" //Can be added later by request. This is for TransportConsignment Data Object with DtbConsignmentDataObjectReader and looks like it’s maybe only imported and not exported by CW. I couldn’t find any references on Run Sheet there.
					};

			AssertContainsExactElementsInAnyOrder(
@"If new filter is added please consider to add it in invoice ConsolXMLData list to help users to have more information to find matched consol.
If it's decided to not include it, please add in to excludedFiltersList in this test with a reason in comment.",
				operationsFilters.Except(excludedFiltersList), invoiceConsolXMLDataFieldsList.Except(invoiceConsolXMLDataFiltersToExcludeList));
		}

		#region Numbers

		public void TestBookingReferenceFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol3 = Factory.New<ForwardingConsol>();

			consol1.JK_BookingReference = "1234567890";
			consol1.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
			consol2.JK_BookingReference = "0987654321";
			consol2.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
			consol3.JK_BookingReference = "1234567890";
			consol3.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			Factory.Save();

			ModuleNumberFilter consolFilter = (ModuleNumberFilter)FilterBizO["Booking Reference #"];
			GenericConsolCollection results = new GenericConsolCollection(Factory, FilterBizO.Filter);

			AssertEquals("Prefix should be B", "B", consolFilter.Prefix);

			consolFilter.IsActive = true;
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Not filtered - should be included", results.GetPKs().Contains(consol1.PK));
			Assert("Not filtered - should be included", results.GetPKs().Contains(consol2.PK));
			Assert("Not filtered - should be included", results.GetPKs().Contains(consol3.PK));

			consolFilter.Property = "1234567890";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Should include Consol1", results.GetPKs().Contains(consol1.PK));
			Assert("Should NOT include Consol2", !results.GetPKs().Contains(consol2.PK));
			Assert("Should include Consol3", results.GetPKs().Contains(consol3.PK));

			consolFilter.Property = "0987";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Should NOT include Consol1", !results.GetPKs().Contains(consol1.PK));
			Assert("Should include Consol2", results.GetPKs().Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.GetPKs().Contains(consol3.PK));
		}

		public void TestCoLoadMasterBillFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol3 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol4 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol5 = Factory.New<ForwardingConsol>();

			consol1.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = "1234567890";
			consol2.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol2.JK_CoLoadMasterBill = "123-4567890";
			consol3.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol3.JK_CoLoadMasterBill = "0987654321";
			consol4.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol4.JK_CoLoadMasterBill = "123-456-7890";
			consol5.JK_BookingReference = "1234567890";
			consol5.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
			Factory.Save();

			ModuleNumberFilter consolFilter = (ModuleNumberFilter)FilterBizO["Co-Load Master Bill #"];

			AssertEquals("Prefix should be L", "L", consolFilter.Prefix);

			GenericConsolCollection results = new GenericConsolCollection(Factory, FilterBizO.Filter);

			consolFilter.IsActive = true;
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Not filtered - should be included", results.GetPKs().Contains(consol1.PK));
			Assert("Not filtered - should be included", results.GetPKs().Contains(consol2.PK));
			Assert("Not filtered - should be included", results.GetPKs().Contains(consol3.PK));
			Assert("Not filtered - should be included", results.GetPKs().Contains(consol4.PK));
			Assert("Not filtered - should be included", results.GetPKs().Contains(consol5.PK));

			consolFilter.Property = "123-4567890";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Should include Consol1", results.GetPKs().Contains(consol1.PK));
			Assert("Should include Consol2", results.GetPKs().Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.GetPKs().Contains(consol3.PK));
			Assert("Should NOT include Consol4", !results.GetPKs().Contains(consol4.PK));
			Assert("Should NOT include Consol5", !results.GetPKs().Contains(consol5.PK));

			consolFilter.Property = "1234567890";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Should include Consol1", results.GetPKs().Contains(consol1.PK));
			Assert("Should NOT include Consol2", !results.GetPKs().Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.GetPKs().Contains(consol3.PK));
			Assert("Should NOT include Consol4", !results.GetPKs().Contains(consol4.PK));
			Assert("Should NOT include Consol5", !results.GetPKs().Contains(consol5.PK));

			consolFilter.Property = "123-456-7890";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Should include Consol1", results.GetPKs().Contains(consol1.PK));
			Assert("Should NOT include Consol2", !results.GetPKs().Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.GetPKs().Contains(consol3.PK));
			Assert("Should include Consol4", results.GetPKs().Contains(consol4.PK));
			Assert("Should NOT include Consol5", !results.GetPKs().Contains(consol5.PK));

			consolFilter.Property = "123";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Should include Consol1", results.GetPKs().Contains(consol1.PK));
			Assert("Should include Consol2", results.GetPKs().Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.GetPKs().Contains(consol3.PK));
			Assert("Should include Consol4", results.GetPKs().Contains(consol4.PK));
			Assert("Should NOT include Consol5", !results.GetPKs().Contains(consol5.PK));
		}

		public void TestContainerNumberFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingContainer container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "AAA";

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingContainer container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "AAB";

			Factory.Save();

			ModuleTextFilter consolFilter = (ModuleTextFilter)FilterBizO["Container #"];
			AssertEquals("Prefix should be T", "T", consolFilter.Prefix);
			GenericConsolCollection results = new GenericConsolCollection(Factory, FilterBizO.Filter);

			consolFilter.IsActive = true;
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Not filtered", results.GetPKs().Contains(consol1.PK));
			Assert("Not filtered", results.GetPKs().Contains(consol2.PK));

			consolFilter.Property = "AA";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Both consols have containers starting with AA", results.GetPKs().Contains(consol1.PK));
			Assert("Both consols have containers starting with AA", results.GetPKs().Contains(consol2.PK));

			consolFilter.Property = "AAB";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Container in this consol has other number", !results.GetPKs().Contains(consol1.PK));
			Assert("Its container from this consol", results.GetPKs().Contains(consol2.PK));
		}

		public void TestHouseBillNumberFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_HouseBill = "HB6666666";
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_HouseBill = "HB6666777";
			Factory.Save();

			ModuleNumberFilter consolFilter = (ModuleNumberFilter)FilterBizO["House Bill"];
			AssertEquals("Prefix should be H", "H", consolFilter.Prefix);
			GenericConsolCollection results = new GenericConsolCollection(Factory, FilterBizO.Filter);

			consolFilter.IsActive = true;
			Assert("Not filtered - should be included", results.GetPKs().Contains(consol1.PK));
			Assert("Not filtered - should be included", results.GetPKs().Contains(consol2.PK));

			consolFilter.Property = "HB6";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Should include Consol1", results.GetPKs().Contains(consol1.PK));
			Assert("Should include Consol2", results.GetPKs().Contains(consol2.PK));

			consolFilter.Property = "HB6666777";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Should not include Consol1", !results.GetPKs().Contains(consol1.PK));
			Assert("Should include Consol2", results.GetPKs().Contains(consol2.PK));
		}

		public void TestMasterBillNumberFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol3 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol4 = Factory.New<ForwardingConsol>();
			consol1.JK_MasterBillNum = "1234567890";
			consol2.JK_MasterBillNum = "123-4567890";
			consol3.JK_MasterBillNum = "0987654321";
			consol4.JK_MasterBillNum = "123-456-7890";
			Factory.Save();

			ModuleNumberFilter consolFilter = (ModuleNumberFilter)FilterBizO["Master Bill"];
			AssertEquals("Prefix should be M", "M", consolFilter.Prefix);
			GenericConsolCollection results = new GenericConsolCollection(Factory, FilterBizO.Filter);

			consolFilter.IsActive = true;
			Assert("Not filtered - should be included", results.GetPKs().Contains(consol1.PK));
			Assert("Not filtered - should be included", results.GetPKs().Contains(consol2.PK));
			Assert("Not filtered - should be included", results.GetPKs().Contains(consol3.PK));
			Assert("Not filtered - should be included", results.GetPKs().Contains(consol4.PK));

			consolFilter.Property = "123-4567890";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Should include Consol1", results.GetPKs().Contains(consol1.PK));
			Assert("Should include Consol2", results.GetPKs().Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.GetPKs().Contains(consol3.PK));
			Assert("Should NOT include Consol4", !results.GetPKs().Contains(consol4.PK));

			consolFilter.Property = "1234567890";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Should include Consol1", results.GetPKs().Contains(consol1.PK));
			Assert("Should NOT include Consol2", !results.GetPKs().Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.GetPKs().Contains(consol3.PK));
			Assert("Should NOT include Consol4", !results.GetPKs().Contains(consol4.PK));

			consolFilter.Property = "123-456-7890";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Should include Consol1", results.GetPKs().Contains(consol1.PK));
			Assert("Should NOT include Consol2", !results.GetPKs().Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.GetPKs().Contains(consol3.PK));
			Assert("Should include Consol4", results.GetPKs().Contains(consol4.PK));

			consolFilter.Property = "123";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Should include Consol1", results.GetPKs().Contains(consol1.PK));
			Assert("Should include Consol2", results.GetPKs().Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.GetPKs().Contains(consol3.PK));
			Assert("Should include Consol4", results.GetPKs().Contains(consol4.PK));
		}

		public void TestShipmentNumberFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S66666666";
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S66666777";
			Factory.Save();

			ModuleNumberFilter consolFilter = (ModuleNumberFilter)FilterBizO["Shipment #"];
			AssertEquals("Prefix should be S", "S", consolFilter.Prefix);
			GenericConsolCollection results = new GenericConsolCollection(Factory, FilterBizO.Filter);

			consolFilter.IsActive = true;
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Not filtered - should be included", results.GetPKs().Contains(consol1.PK));
			Assert("Not filtered - should be included", results.GetPKs().Contains(consol2.PK));

			consolFilter.Property = "S66";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Should include Consol1", results.GetPKs().Contains(consol1.PK));
			Assert("Should include Consol2", results.GetPKs().Contains(consol2.PK));

			consolFilter.Property = "S66666777";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Should not include Consol1", !results.GetPKs().Contains(consol1.PK));
			Assert("Should include Consol2", results.GetPKs().Contains(consol2.PK));
		}

		public void TestVoyageVesselFilter()
		{
			ForwardingConsol consol1 = NewConsol("tvfvf1", false, "AUMEL", "SGSIN");
			Transport transport1 = consol1.Transports[0];
			transport1.JW_Vessel = Factory.NewWithValidTestData<RefVessel>().RV_FK;
			transport1.JW_VoyageFlight = "2222";

			ForwardingConsol consol2 = NewConsol("tvfvf2", false, "SGSIN", "NZAKL");
			Transport transport2 = consol2.Transports[0];
			transport2.JW_Vessel = Factory.NewWithValidTestData<RefVessel>().RV_FK;
			transport2.JW_VoyageFlight = "2233";
			Factory.Save();

			ModuleTextAndNkFilter consolFilter = (ModuleTextAndNkFilter)FilterBizO["Flight/Voyage # and Vessel"];
			AssertEquals("Prefix should be V", "V", consolFilter.Prefix);
			GenericConsolCollection results = new GenericConsolCollection(Factory, FilterBizO.Filter);

			consolFilter.IsActive = true;
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Consol1 should be in collection", results.GetPKs().Contains(consol1.PK));
			Assert("Consol2 should be in collection", results.GetPKs().Contains(consol2.PK));

			consolFilter.Property = "22";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Consol should be in collection", results.GetPKs().Contains(consol1.PK));
			Assert("Consol2 should be in collection", results.GetPKs().Contains(consol2.PK));

			consolFilter.Property = "2233";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Consol should not be in collection", !results.GetPKs().Contains(consol1.PK));
			Assert("Consol2 should be in collection", results.GetPKs().Contains(consol2.PK));

			consolFilter.Property = "";
			consolFilter.NkProperty = transport1.JW_Vessel;
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Consol1 should be in collection", results.GetPKs().Contains(consol1.PK));
			Assert("Consol2 should not be in collection", !results.GetPKs().Contains(consol2.PK));

			consolFilter.NkProperty = transport2.JW_Vessel;
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			AssertEquals("Consol1 should not be in the collection", true, !results.GetPKs().Contains(consol1.PK));
			AssertEquals("Consol2 should be in the collection", true, results.GetPKs().Contains(consol2.PK));
		}

		public void TestRunSheetNumberFilter()
		{
			var runSheet1 = (IDtbConsignmentRunSheet)Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbConsignmentRunSheet>());
			runSheet1.KG_RunSheetNumber = "CR1234567890";
			var runSheet2 = (IDtbConsignmentRunSheet)Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbConsignmentRunSheet>());
			runSheet2.KG_RunSheetNumber = "CR123-4567890";
			var runSheet3 = (IDtbConsignmentRunSheet)Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbConsignmentRunSheet>());
			runSheet3.KG_RunSheetNumber = "CR0987654321";

			Factory.Save();

			var results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			var consolFilter = (ModuleNumberFilter)FilterBizO["RunSheetNumber"];
			AssertEquals("Prefix should be CR", "CR", consolFilter.Prefix);
			consolFilter.IsActive = true;
			AssertContainsExactElementsInAnyOrder("When RunSheetNumber filter is empty, all DtbConsignmentRunSheets should be found.", new ZGuid[] { runSheet1.PK, runSheet2.PK, runSheet3.PK }, results.GetPKs());

			consolFilter.Property = "CR1234567890";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Should include runSheet1", results.GetPKs().Contains(runSheet1.PK));
			Assert("Should NOT include runSheet2", !results.GetPKs().Contains(runSheet2.PK));
			Assert("Should NOT include runSheet3", !results.GetPKs().Contains(runSheet3.PK));

			consolFilter.Property = "CR123";
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Should include runSheet1", results.GetPKs().Contains(runSheet1.PK));
			Assert("Should include runSheet2", results.GetPKs().Contains(runSheet2.PK));
			Assert("Should NOT include runSheet3", !results.GetPKs().Contains(runSheet3.PK));
		}

		#endregion

		#region Date Tests

		public void TestATAFilter()
		{
			TestDateFilter(JobConsolTransportSchema.JW_ATA.Name, "ATA");
		}

		public void TestATDFilter()
		{
			TestDateFilter(JobConsolTransportSchema.JW_ATD.Name, "ATD");
		}

		public void TestETAFilter()
		{
			TestDateFilter(JobConsolTransportSchema.JW_ETA.Name, "ETA");
		}

		public void TestETDFilter()
		{
			TestDateFilter(JobConsolTransportSchema.JW_ETD.Name, "ETD");
		}

		void TestDateFilter(ZString dateProperty, ZString filterProperty)
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			ForwardingConsol consol1 = NewConsol("tedf1", false, "USNYC", "SGSIN", "AUSYD", "NZAKA");
			ForwardingConsol consol2 = NewConsol("tedf2", false, "USNYC", "SGSIN", "AUSYD");
			consol1.Transports[0][dateProperty] = new ZDateTime(2007, 1, 1);
			consol1.Transports[1][dateProperty] = new ZDateTime(2007, 1, 3);
			consol1.Transports[2][dateProperty] = new ZDateTime(2007, 1, 5);

			consol2.Transports[0][dateProperty] = new ZDateTime(2007, 1, 2);
			consol2.Transports[1][dateProperty] = new ZDateTime(2007, 1, 8);
			Factory.Save();

			ModuleDateFilter consolFilter = (ModuleDateFilter)FilterBizO[filterProperty];
			GenericConsolCollection results = new GenericConsolCollection(Factory, FilterBizO.Filter);

			consolFilter.IsActive = true;
			consolFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Assert("Not filtered - should be in collection", results.GetPKs().Contains(consol1.PK));
			Assert("Not filtered - should be in collection", results.GetPKs().Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2007, 1, 1);
			consolFilter.Property2 = new ZDateTime(2007, 1, 2);
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Consol1 " + filterProperty + " fall in range", results.GetPKs().Contains(consol1.PK));
			Assert("Consol2 " + filterProperty + " fall in range", results.GetPKs().Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2007, 1, 6);
			consolFilter.Property2 = new ZDateTime(2007, 1, 7);
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Consol1 " + filterProperty + " not fall in range", !results.GetPKs().Contains(consol1.PK));
			Assert("Consol2 " + filterProperty + " not fall in range", !results.GetPKs().Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2007, 1, 5);
			consolFilter.Property2 = new ZDateTime(2007, 1, 7);
			results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			Assert("Consol1 " + filterProperty + " fall in range", results.GetPKs().Contains(consol1.PK));
			Assert("Consol2 " + filterProperty + " not fall in range", !results.GetPKs().Contains(consol2.PK));
		}

		#endregion

		#region TestFilter_ForwardingConsol

		public void TestFilter_ForwardingConsol()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();

			Factory.Save();

			var results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("When no filters set, all Forwarding Consols should be found.", new ZGuid[] { consol1.PK, consol2.PK, consol3.PK }, results.GetPKs());

			foreach (var filter in FilterBizO)
			{
				if (filter.Description == GenericConsolFilterBusinessObject.FilterConstants.RunSheetNumber)
				{
					var message = string.Format("Filter ({0}) is no return result filter for Transport Booking Consol.", filter.Description);
					filter.IsActive = true;

					if (filter is ModuleNumberFilter)
					{
						((ModuleNumberFilter)filter).Property = "ABC";
					}
					else if (filter is ModuleTextAndNkFilter)
					{
						((ModuleTextAndNkFilter)filter).Property = "ABC";
					}
					else if (filter is ModuleDateFilter)
					{
						((ModuleDateFilter)filter).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
						((ModuleDateFilter)filter).Property1 = ZDateTime.Today.AddYears(-1);
						((ModuleDateFilter)filter).Property2 = ZDateTime.Today.AddYears(1);
					}

					var consols = new GenericConsolCollection(Factory, FilterBizO.Filter);
					AssertEquals(message + " So no results expected.", 0, consols.Count);
					AssertEquals(message + " So no mentioning of JobConsol tables should exist in Filters.", false, FilterBizO.Filter.LiteralTextADOFormatted.Contains(JobConsolSchema.Constants.TableName));

					filter.IsActive = false; // clean up.
				}
			}
		}

		#endregion

		#region TestFilter_TransportBookingConsol

		public void TestFilter_TransportBookingConsol()
		{
			var dtbBookingConsolType = ObjectFactory.GetType<IDtbBookingConsolidation>();
			var bookingConsolidation1 = Factory.NewWithValidTestData(dtbBookingConsolType);
			var bookingConsolidation2 = Factory.NewWithValidTestData(dtbBookingConsolType);
			var singleBookingConsolidation = Factory.NewWithValidTestData(dtbBookingConsolType);
			bookingConsolidation1[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			bookingConsolidation2[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			singleBookingConsolidation[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.Booking;

			Factory.Save();

			var results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("When no filters set, all DtbBookingConsolidation should be found.", new ZGuid[] { bookingConsolidation1.PK, bookingConsolidation2.PK }, results.GetPKs());

			foreach (var filter in FilterBizO)
			{
				var message = string.Format("Filter ({0}) is no return result filter for Transport Booking Consol.", filter.Description);
				filter.IsActive = true;

				if (filter is ModuleNumberFilter)
				{
					((ModuleNumberFilter)filter).Property = "ABC";
				}
				else if (filter is ModuleTextAndNkFilter)
				{
					((ModuleTextAndNkFilter)filter).Property = "ABC";
				}
				else if (filter is ModuleDateFilter)
				{
					((ModuleDateFilter)filter).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
					((ModuleDateFilter)filter).Property1 = ZDateTime.Today.AddYears(-1);
					((ModuleDateFilter)filter).Property2 = ZDateTime.Today.AddYears(1);
				}

				var consols = new GenericConsolCollection(Factory, FilterBizO.Filter);
				AssertEquals(message + " So no results expected.", 0, consols.Count);
				AssertEquals(message + " So no mentioning of Transport Booking tables should exist in Filters.", false, FilterBizO.Filter.LiteralTextADOFormatted.Contains(DtbBookingConsolidationSchema.Constants.TableName));

				filter.IsActive = false; // clean up.
			}
		}

		#endregion

		#region TestFilter_RunSheetConsol

		public void TestFilter_RunSheetConsol()
		{
			var runSheet1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbConsignmentRunSheet>());
			var runSheet2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbConsignmentRunSheet>());
			var runSheet3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbConsignmentRunSheet>());

			Factory.Save();

			var results = new GenericConsolCollection(Factory, FilterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("When no filters set, all DtbConsignmentRunSheets should be found.", new ZGuid[] { runSheet1.PK, runSheet2.PK, runSheet3.PK }, results.GetPKs());

			foreach (var filter in FilterBizO)
			{
				if (filter.Description != GenericConsolFilterBusinessObject.FilterConstants.RunSheetNumber)
				{
					var message = string.Format("Filter ({0}) is no return result filter for Run Sheet Consol.", filter.Description);
					filter.IsActive = true;

					if (filter is ModuleNumberFilter)
					{
						((ModuleNumberFilter)filter).Property = "ABC";
					}
					else if (filter is ModuleTextAndNkFilter)
					{
						((ModuleTextAndNkFilter)filter).Property = "ABC";
					}
					else if (filter is ModuleDateFilter)
					{
						((ModuleDateFilter)filter).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
						((ModuleDateFilter)filter).Property1 = ZDateTime.Today.AddYears(-1);
						((ModuleDateFilter)filter).Property2 = ZDateTime.Today.AddYears(1);
					}

					var consols = new GenericConsolCollection(Factory, FilterBizO.Filter);
					AssertEquals(message + " So no results expected.", 0, consols.Count);
					AssertEquals(message + " So no mentioning of Transport Booking tables should exist in Filters.", false, FilterBizO.Filter.LiteralTextADOFormatted.Contains(DtbConsignmentRunSheetSchema.Constants.TableName));

					filter.IsActive = false; // clean up.
				}
			}
		}

		#endregion

		#region NewConsol

		ForwardingConsol NewConsol(string name, bool isLinked, string port1, string port2, params string[] otherports)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.JK_UniqueConsignRef = name;
			consol.JK_RL_NKLoadPort = port1;
			consol.JK_RL_NKDischargePort = (otherports.Length == 0 ? port2 : otherports[otherports.Length - 1]);

			Transport lastTransport = consol.Transports[0];
			lastTransport.JW_RL_NKLoadPort = port1;
			lastTransport.JW_RL_NKDiscPort = port2;
			lastTransport.JW_IsLinked = isLinked;

			foreach (string nextPort in otherports)
			{
				string lastPort = lastTransport.JW_RL_NKDiscPort;
				lastTransport = consol.Transports.AddNew();
				lastTransport.JW_IsLinked = isLinked;
				lastTransport.JW_RL_NKLoadPort = lastPort;
				lastTransport.JW_RL_NKDiscPort = nextPort;
			}

			return consol;
		}

		#endregion

		#region Implementation

		protected GenericConsolFilterBusinessObject FilterBizO
		{
			get { return CachedBusinessObject as GenericConsolFilterBusinessObject; }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GenericConsolFilterBusinessObject();
		}

		#endregion
	}
}
