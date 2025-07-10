using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Integration.TransportBooking;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module.Testing
{
	public class GenericConsolToForwardingConsolFilterHelperTest : TestCaseWithFactory
	{
		#region TestMapFilters_CustomAttributesCopiedBetweenFilters

		public void TestMapFilters_CustomAttributesCopiedBetweenFilters()
		{
			var genericConsolFilter = new GenericConsolFilterBusinessObject();
			var filter = (ModuleTextFilter)genericConsolFilter["House Bill"];
			filter.IsActive = true;
			filter.Property = "Hello";

			var jobConsolFilter = new JobConsolFilterBusinessObject();
			var mapper = new GenericConsolToForwardingConsolFilterHelper(genericConsolFilter, jobConsolFilter);
			mapper.MapFilters();

			AssertContains("Generic Consol Filter", "JS_HouseBill like 'Hello%'", genericConsolFilter.Filter.LiteralTextADO);
			AssertContains("Job Consol Filter", "JS_HouseBill like 'Hello%'", jobConsolFilter.Filter.LiteralTextADO);
		}

		#endregion

		#region TestMapFilters_ConversionsAndExceptions

		public void TestMapFilters_ConversionsAndExceptions()
		{
			var bookingConsolFilterType = ObjectFactory.GetType<IDtbBookingConsolidationFilterBusinessObject>();
			var bookingConsolFilter = (FilterStripBusinessObject)Activator.CreateInstance(bookingConsolFilterType);
			var genericConsolFilter = new GenericConsolFilterBusinessObject();

			var conversions = new Dictionary<string, string>();
			conversions.Add("Booking Reference #", "Booking Transport Reference");

			var exceptions = new List<string>();
			exceptions.Add("House Bill");

			var genericConsolFilterHelper = new GenericConsolToForwardingConsolFilterHelper(genericConsolFilter, bookingConsolFilter, conversions, exceptions);

			// Test Conversions.
			var bookingReferenceFilter = (ModuleTextFilter)genericConsolFilter["Booking Reference #"];
			bookingReferenceFilter.IsActive = true;
			bookingReferenceFilter.Property = "ABC";
			genericConsolFilterHelper.MapFilters();
			AssertEquals("If active filter in exception list, then filter should return no result query.", false, bookingConsolFilter.ReturnNoResultsQuery);
			AssertContains("Booking Consol Filter", "KM_TransportReference like 'ABC%'", bookingConsolFilter.Filter.LiteralTextADO);

			// Test Exception
			var houseBillFilter = (ModuleTextFilter)genericConsolFilter["House Bill"];
			houseBillFilter.IsActive = true;
			houseBillFilter.Property = "ABC";
			genericConsolFilterHelper.MapFilters();
			AssertEquals("If active filter in exception list, then filter should return no result query.", true, bookingConsolFilter.ReturnNoResultsQuery);
			AssertContains("Booking Consol Filter", "KM_TransportReference like 'ABC%'", bookingConsolFilter.Filter.LiteralTextADO);
		}

		#endregion
	}
}
