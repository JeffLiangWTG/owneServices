using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.eTail.Integration;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.HVLV.Subscribers
{
	public class HVLVBookingHeaderLMCDetailsCalculatorSubscriber : ActualDataChangesAuditSubscriber
	{
		public override ITableSchema Table => HVLVBookingHeaderSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[]
		{
			HVLVBookingHeaderSchema.HVH_OA_OriginDepot,
			HVLVBookingHeaderSchema.HVH_RS_NKBookingServiceLevel,
		};

		public override bool NotifyInsert => false;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		public override string Code => "HLB";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "HVLV BookingHeader Last Mile Carrier Details Change Subscriber";

		public override bool IsRequired()
		{
			return HVLVDataRegistry.Instance.HVLVAutomaticallyCalculateLMCDepotDetails.Value;
		}

		public DateTime ProcessAfterDateTimeUtc => ZDateTime.UtcNow.ToDateTime().AddDays(-1);

		public override Action<DataRow> CustomFilter => null;

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			var dataRows = changeTable.Select(FormattableString.Invariant($"TranEndTimeUtc > #{ProcessAfterDateTimeUtc}#"), string.Empty, DataViewRowState.ModifiedCurrent);// Data Table filter string
			var modifiedUnprocessedBookingHeaderPKs = dataRows.
								Where(row => row[HVLVBookingHeaderSchema.Constants.HVH_IsProcessedAtOriginDepot].Equals(false)).
								Select(row => new Guid(row[HVLVBookingHeaderSchema.Constants.PK].ToString())).ToList();

			logger.Information($"HLB received dataRows: {changeTable.Rows.Count} total dataRows"); // Log message
			logger.Information($"HLB to process dataRows: {modifiedUnprocessedBookingHeaderPKs.Count} dataRows have changed in the past 24 hours"); // Log message

			var query = new ZDBOnlyQuery(typeof(IHVLVBookingHeader));
			query.AddToFilter(HVLVBookingHeaderSchema.PK, modifiedUnprocessedBookingHeaderPKs);

			var bookingHeaders = DataFactory.Load<IHVLVBookingHeader>(query).Cast<BusinessObject>().ToList();
			var bookingHeaderClusterKeys = bookingHeaders.Select(b => Convert.ToInt32((ZInt)b[HVLVBookingHeaderSchema.Constants.HVH_ClusterKey]));

			LMCDepotDetailsCalculator.UpdateConsignmentsDestinationDetailsByClusterKeys(bookingHeaderClusterKeys);
		}

		IHVLVConsignmentLastMileCarrierAndDepotDetailsCalculator LMCDepotDetailsCalculator => lmcDepotDetailsCalculator ?? (lmcDepotDetailsCalculator = ObjectFactory.Get<IHVLVConsignmentLastMileCarrierAndDepotDetailsCalculator>(nameof(IHVLVConsignmentLastMileCarrierAndDepotDetailsCalculator)));
		IHVLVConsignmentLastMileCarrierAndDepotDetailsCalculator lmcDepotDetailsCalculator;

		protected BusinessObjectFactory DataFactory => dataFactory ?? (dataFactory = new BusinessObjectFactory { RefreshEnabled = false });
		BusinessObjectFactory dataFactory;
	}
}
