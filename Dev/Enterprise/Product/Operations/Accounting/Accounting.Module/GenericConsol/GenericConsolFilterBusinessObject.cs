using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GenericConsol;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Integration.TransportBooking;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class GenericConsolFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		#region Filter constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter name")]
		public static class FilterConstants
		{
			public const string BookingReferenceNumber = "Booking Reference #";
			public const string CoLoadMasterBill = "Co-Load Master Bill #";
			public const string ContainerNumber = "Container #";
			public const string HouseBill = "House Bill";
			public const string MasterBill = "Master Bill";
			public const string ShipmentNumber = "Shipment #";
			public const string FlightVoyageNumber = "Flight/Voyage # and Vessel";
			public const string ETA = "ETA";
			public const string ATA = "ATA";
			public const string ETD = "ETD";
			public const string ATD = "ATD";

			public const string RunSheetNumber = "RunSheetNumber";
		}

		static string[] GetForwardingConsolFilters()
		{
			return new string[] {
				FilterConstants.BookingReferenceNumber,
				FilterConstants.CoLoadMasterBill,
				FilterConstants.ContainerNumber,
				FilterConstants.HouseBill,
				FilterConstants.MasterBill,
				FilterConstants.ShipmentNumber,
				FilterConstants.FlightVoyageNumber,
				FilterConstants.ETA,
				FilterConstants.ATA,
				FilterConstants.ETD,
				FilterConstants.ATD
			};
		}

		static string[] GetRunSheetConsolFilters()
		{
			return new string[] { FilterConstants.RunSheetNumber };
		}

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddNumberFilters(result);
			AddDateFilters(result);
			return result;
		}

		protected override bool ShouldAddUserDefinedFiltersCore => false;

		#region Filter

		public override ZQuery Filter
		{
			get
			{
				var genericConsolQuery = new ZDBOnlyQuery(typeof(GenericConsol));
				genericConsolQuery.AddSubQuery(GetForwardingConsolSubQuery(), JoinCondition.Or);
				genericConsolQuery.AddSubQuery(GetTransportBookingConsolSubQuery(), JoinCondition.Or);
				genericConsolQuery.AddSubQuery(GetConsignmentRunSheetConsolSubQuery(), JoinCondition.Or);

				return genericConsolQuery;
			}
		}

		#region GetForwardingConsolSubQuery

		ZDBOnlySubQuery GetForwardingConsolSubQuery()
		{
			var forwardingConsolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConsolSchema.PK);
			var fwdConsolFilterBizObj = new JobConsolFilterBusinessObject();
			var fwdConsolFilterHelper = new GenericConsolToForwardingConsolFilterHelper(this, fwdConsolFilterBizObj, ForwardingFilterConversions, ForwardingFilterExceptions);
			fwdConsolFilterHelper.MapFilters();
			forwardingConsolSubQuery.AddToFilter(fwdConsolFilterBizObj.Filter);

			return forwardingConsolSubQuery;
		}

		#region ForwardingFilterConversions

		Dictionary<string, string> ForwardingFilterConversions
		{
			get
			{
				if (forwardingFilterConversions == null)
				{
					forwardingFilterConversions = new Dictionary<string, string>();
				}
				return forwardingFilterConversions;
			}
		}

		Dictionary<string, string> forwardingFilterConversions;

		#endregion

		#region ForwardingFilterExceptions

		List<string> ForwardingFilterExceptions
		{
			get
			{
				if (forwardingFilterExceptions == null)
				{
					forwardingFilterExceptions = new List<string>();
					var runSheetConsolFilters = GetRunSheetConsolFilters();

					foreach (var moduleFilter in this)
					{
						if (Array.IndexOf<string>(runSheetConsolFilters, moduleFilter.Description) > -1)
						{
							forwardingFilterExceptions.Add(moduleFilter.Description);   // exclude runsheet consol filter
						}
					}
				}
				return forwardingFilterExceptions;
			}
		}

		List<string> forwardingFilterExceptions;

		#endregion

		#endregion

		#region GetTransportBookingConsolSubQuery

		ZDBOnlySubQuery GetTransportBookingConsolSubQuery()
		{
			var transportBookingConsolType = ObjectFactory.GetType<IDtbBookingConsolidation>();
			var transportBookingConsolSubQuery = new ZDBOnlySubQuery(transportBookingConsolType, DtbBookingConsolidationSchema.PK);

			var transportBookingConsolFilterBizOType = ObjectFactory.GetType<IDtbBookingConsolidationFilterBusinessObject>();
			var transportBookingConsolFilterBizO = (FilterStripBusinessObject)Activator.CreateInstance(transportBookingConsolFilterBizOType);
			var consolFilterHelper = new GenericConsolToForwardingConsolFilterHelper(this, transportBookingConsolFilterBizO, TransportBookingFilterConversions, TransportBookingFilterExceptions);
			consolFilterHelper.MapFilters();
			transportBookingConsolSubQuery.AddToFilter(transportBookingConsolFilterBizO.Filter);

			return transportBookingConsolSubQuery;
		}

		#region TransportBookingFilterConversions

		Dictionary<string, string> TransportBookingFilterConversions
		{
			get
			{
				if (transportBookingFilterConversions == null)
				{
					transportBookingFilterConversions = new Dictionary<string, string>();
				}
				return transportBookingFilterConversions;
			}
		}

		Dictionary<string, string> transportBookingFilterConversions;

		#endregion

		#region TransportBookingFilterExceptions

		List<string> TransportBookingFilterExceptions
		{
			get
			{
				if (transportBookingFilterExceptions == null)
				{
					transportBookingFilterExceptions = new List<string>();
					var forwardingConsolFilters = GetForwardingConsolFilters();
					var runSheetConsolFilters = GetRunSheetConsolFilters();

					foreach (var moduleFilter in this)
					{
						if (Array.IndexOf<string>(forwardingConsolFilters, moduleFilter.Description) > -1 ||
							Array.IndexOf<string>(runSheetConsolFilters, moduleFilter.Description) > -1)
						{
							transportBookingFilterExceptions.Add(moduleFilter.Description); // exclude all filters
						}
					}
				}
				return transportBookingFilterExceptions;
			}
		}

		List<string> transportBookingFilterExceptions;

		#endregion

		#endregion

		#region GetConsignmentRunSheetConsolSubQuery

		ZDBOnlySubQuery GetConsignmentRunSheetConsolSubQuery()
		{
			var runSheetConsolSubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbConsignmentRunSheet>(), DtbConsignmentRunSheetSchema.PK);

			var runSheetConsolFilterBizOType = ObjectFactory.GetType<IDtbConsignmentRunSheetFilterBusinessObject>();
			var runSheetConsolFilterBizO = (FilterStripBusinessObject)Activator.CreateInstance(runSheetConsolFilterBizOType);
			var consolFilterHelper = new GenericConsolToForwardingConsolFilterHelper(this, runSheetConsolFilterBizO, RunSheetFilterConversions, RunSheetFilterExceptions);
			consolFilterHelper.MapFilters();
			runSheetConsolSubQuery.AddToFilter(runSheetConsolFilterBizO.Filter);

			return runSheetConsolSubQuery;
		}

		#region RunSheetFilterConversions

		Dictionary<string, string> RunSheetFilterConversions
		{
			get
			{
				if (runSheetFilterConversions == null)
				{
					runSheetFilterConversions = new Dictionary<string, string>();
				}
				return runSheetFilterConversions;
			}
		}

		Dictionary<string, string> runSheetFilterConversions;

		#endregion

		#region RunSheetFilterExceptions

		List<string> RunSheetFilterExceptions
		{
			get
			{
				if (runSheetFilterExceptions == null)
				{
					runSheetFilterExceptions = new List<string>();
					var forwardingConsolFilters = GetForwardingConsolFilters();

					foreach (var moduleFilter in this)
					{
						if (Array.IndexOf<string>(forwardingConsolFilters, moduleFilter.Description) > -1)
						{
							runSheetFilterExceptions.Add(moduleFilter.Description); // exclude all forwarding consols filters
						}
					}
				}
				return runSheetFilterExceptions;
			}
		}

		List<string> runSheetFilterExceptions;

		#endregion

		#endregion

		#endregion

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddNumberFilter(FilterConstants.BookingReferenceNumber, GetNumberQueryPlaceHolder /*GetBookingReferenceQuery*/);
			filter.MultilingualDescription = ResString.GetMultilingualString("GenericConsol|JobConsolFilter|BookingReference", "Booking Reference #");
			filter.Prefix = "B";
			filter = filters.AddNumberFilter(FilterConstants.CoLoadMasterBill, GetNumberQueryPlaceHolder /*GetCoLoadMasterBillNoQuery*/);
			filter.MultilingualDescription = ResString.GetMultilingualString("GenericConsol|JobConsolFilter|CoLoadMasterBill", "Co-Load Master Bill #");
			filter.Prefix = "L";
			filter = filters.AddNumberFilter(FilterConstants.ContainerNumber, GetNumberQueryPlaceHolder /*GetContainerNoQuery*/);
			filter.MultilingualDescription = ResString.GetMultilingualString("GenericConsol|JobConsolFilter|Container", "Container #");
			filter.Prefix = "T";
			filter = filters.AddNumberFilter(FilterConstants.HouseBill, GetNumberQueryPlaceHolder /*GetHouseBillQuery*/);
			filter.MultilingualDescription = ResString.GetMultilingualString("GenericConsol|JobConsolFilter|HouseBill", "House Bill");
			filter.Prefix = "H";

			var masterBillFilter = filters.AddNumberFilter(FilterConstants.MasterBill, GetNumberQueryPlaceHolder /*GetMasterBillQuery*/);
			masterBillFilter.MultilingualDescription = ResString.GetMultilingualString("GenericConsol|JobConsolFilter|MasterBill", "Master Bill");
			masterBillFilter.IsCommon = true;
			masterBillFilter.Prefix = "M";

			filter = filters.AddFountainFilter(FilterConstants.ShipmentNumber, GetNumberQueryPlaceHolder /*GetShipmentNoQuery*/, "S");
			filter.MultilingualDescription = ResString.GetMultilingualString("GenericConsol|JobConsolFilter|Shipment", "Shipment #");
			filter.Prefix = "S";

			var flightVoyageNoFilter = filters.AddTextAndNkFilter(FilterConstants.FlightVoyageNumber, GetTextAndNKQueryPlaceHolder /*GetFlightVoyageNumberAndVesselQuery*/, ModuleIDs.RefVessel, BindingLists.RefVessel_List);
			flightVoyageNoFilter.MultilingualDescription = ResString.GetMultilingualString("GenericConsol|JobConsolFilter|FlightVoyageAndVessel", "Flight/Voyage # and Vessel");
			flightVoyageNoFilter.Category = FilterCategories.NumbersAndReferences;
			flightVoyageNoFilter.Prefix = "V";

			var runsheetFilter = filters.AddFountainFilter(FilterConstants.RunSheetNumber, GetNumberQueryPlaceHolder, "CR");
			runsheetFilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.RunSheetNumberFilterDescription;
			runsheetFilter.Prefix = "CR";
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(FilterConstants.ETA, GetDateQueryPlaceHolder).MultilingualDescription = ResString.GetMultilingualString("GenericConsol|JobConsolFilter|ETA", "ETA");
			filters.AddDateFilter(FilterConstants.ATA, GetDateQueryPlaceHolder).MultilingualDescription = ResString.GetMultilingualString("GenericConsol|JobConsolFilter|ATA", "ATA");
			filters.AddDateFilter(FilterConstants.ETD, GetDateQueryPlaceHolder).MultilingualDescription = ResString.GetMultilingualString("GenericConsol|JobConsolFilter|ETD", "ETD");
			filters.AddDateFilter(FilterConstants.ATD, GetDateQueryPlaceHolder).MultilingualDescription = ResString.GetMultilingualString("GenericConsol|JobConsolFilter|ATD", "ATD");
		}

		BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		ZQuery GetNumberQueryPlaceHolder(SQLComparisonOperator comparisonOperator, ZString searchValue)
		{
			return new ZQuery();
		}

		ZQuery GetTextAndNKQueryPlaceHolder(SQLComparisonOperator comparisonOperator, ZString text, ZString nk)
		{
			return new ZQuery();
		}

		ZQuery GetDateQueryPlaceHolder(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return new ZQuery();
		}
	}
}
