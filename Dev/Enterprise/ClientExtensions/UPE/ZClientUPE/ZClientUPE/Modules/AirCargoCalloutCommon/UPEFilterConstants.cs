using System;
using CargoWise.Types;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Module
{
	public static class UPEFilterConstants
	{
		// #####################            NOTE           ###################
		// THESE CONSTANTS ARE OBSOLETE (TO BE REMOVED WHEN NEW FILTERS ARE IN PLACE)
		// #####################################################

		public const string ExpensiveQueryWarningText = "It is recommended you select more criteria otherwise the search may take a long time to complete.";

		public const string Refund = "Refund";
		public const string ArrivalDate = "Arrival Date";
		public const string Zone = "Zone";

		public abstract class QueueReasonFilters
		{
			public const string Unworked = "Unworked";
			public const string UnworkedToday = "Unworked Today";
		}

		public abstract class AirCargoNumberTypes
		{
			public const string Common = "Common";
			public const string AllTrackingNumbers = "All Tracking Numbers";

			public const string MAWB = "Master Bill";
			public const string HAWB = "Tracking Number";
			public const string RelatedWayBillShortNumber = "Short Tracking No.";
			public const string ChildPackageShortOrLongNumber = "Child Tracking No.";
			public const string QueueRemarks = "Queue Remarks";
			public const string InvoiceNumber = "Invoice Number";
		}

		public abstract class JobDeclarationNumberTypes : DeclarationFilterConstants.NumberFilterTypes
		{
			public const string QueueRemarks = "Queue Remarks";
			public const string RefundAndAudit = "Refund & Audit";
			public const string AccountClass = "Account Group";
			public const string AssignedTo = "Assigned To";
		}

		public abstract class OrgDetailTypes
		{
			public const string Name = "Name";
			public const string AccountID = "Account #";

			public const string ConsigneeName = "Consignee Name";
			public const string ConsigneeAccountID = "Consignee Account #";
			public const string ConsigneeStreet = "Consignee Street";
			public const string ConsigneeCity = "Consignee City";
			public const string ConsigneeState = "Consignee State";
			public const string ConsigneePostcode = "Consignee Postcode";
			public const string ConsigneePhone = "Consignee Phone";

			public const string ConsignorName = "Consignor Name";
			public const string ConsignorAccountID = "Consignor Account #";
			public const string ConsignorStreet = "Consignor Street";
			public const string ConsignorCity = "Consignor City";
			public const string ConsignorState = "Consignor State";
			public const string ConsignorPostcode = "Consignor Postcode";
			public const string ConsignorPhone = "Consignor Phone";

			public const string BillToName = "Bill To Name";
			public const string BillToAccountID = "Bill To Account #";
			public const string BillToStreet = "Bill To Street";
			public const string BillToCity = "Bill To City";
			public const string BillToState = "Bill To State";
			public const string BillToPostcode = "Bill To Postcode";
			public const string BillToPhone = "Bill To Phone";
		}

		public abstract class AirCargoPortTypes
		{
			public const string All = "ALL";
			public const string LoadDischarge = "Load / Discharge";
			public const string OriginDestination = "Origin / Destination";
		}

		public abstract class MetroCountry
		{
			public const string Metro = "Metro";
			public const string Other = "Other";
		}

		public static ZDateTime DefaultFromDate
		{
			get { return ZDateTime.Now.AddMonths(-5); }
		}

		public static FilterCategory UPEFilterCategory
		{
			get { return upeFilterCategory ?? (upeFilterCategory = new FilterCategory((NoResString)"UPS Specific")); }
		}
		[ThreadStatic]
		static FilterCategory upeFilterCategory;
	}
}
