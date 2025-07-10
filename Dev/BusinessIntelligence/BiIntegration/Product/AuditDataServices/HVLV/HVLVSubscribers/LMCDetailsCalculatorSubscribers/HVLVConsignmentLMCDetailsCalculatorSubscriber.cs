using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.eTail.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.HVLV.Subscribers
{
	public class HVLVConsignmentLMCDetailsCalculatorSubscriber : ActualDataChangesAuditSubscriber
	{
		public HVLVConsignmentLMCDetailsCalculatorSubscriber() : this(1000)
		{
		}

		public HVLVConsignmentLMCDetailsCalculatorSubscriber(int batchSize)
		{
			this.batchSize = batchSize;
		}

		readonly int batchSize;

		public override ITableSchema Table => HVLVConsignmentSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[]
		{
			HVLVConsignmentSchema.HVC_RN_NKConsigneeCountryCode,
			HVLVConsignmentSchema.HVC_ConsigneePostcode,
			HVLVConsignmentSchema.HVC_ConsigneeCity,
			HVLVConsignmentSchema.HVC_ConsigneeState,
			HVLVConsignmentSchema.HVC_UndgClass,
		};

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		public override string Code => "HLC";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "HVLV Consignment Last Mile Carrier Details Change Subscriber";

		public override bool IsRequired()
		{
			return HVLVDataRegistry.Instance.HVLVAutomaticallyCalculateLMCDepotDetails.Value;
		}

		public DateTime ProcessAfterDateTimeUtc => ZDateTime.UtcNow.ToDateTime().AddDays(-1);

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[HVLVConsignmentSchema.Constants.HVC_IsActive] == DBNull.Value
				|| Convert.ToInt32(row[HVLVConsignmentSchema.Constants.HVC_IsActive]) != 1)
			{
				row.Delete();
			}
		};

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			var recentlyAddedOrModifiedPKs = changeTable.Select(FormattableString.Invariant($"TranEndTimeUtc > #{ProcessAfterDateTimeUtc}#"), string.Empty, DataViewRowState.ModifiedCurrent | DataViewRowState.Added)  // Data Table filter string
				.Select(row => new ZGuid(row[HVLVConsignmentSchema.Constants.PK]))
				.Distinct()
				.ToList();

			logger.Information($"HLC received dataRows: {changeTable.Rows.Count} total dataRows"); // Log message
			logger.Information($"HLC to process dataRows: {recentlyAddedOrModifiedPKs.Count} dataRows have changed in the past 24 hours"); // Log message

			recentlyAddedOrModifiedPKs.Batch(batchSize).ForEach(LMCDepotDetailsCalculator.UpdateConsignmentsDestinationDetails);
		}

		IHVLVConsignmentLastMileCarrierAndDepotDetailsCalculator LMCDepotDetailsCalculator => lmcDepotDetailsCalculator ?? (lmcDepotDetailsCalculator = ObjectFactory.Get<IHVLVConsignmentLastMileCarrierAndDepotDetailsCalculator>(nameof(IHVLVConsignmentLastMileCarrierAndDepotDetailsCalculator)));
		IHVLVConsignmentLastMileCarrierAndDepotDetailsCalculator lmcDepotDetailsCalculator;
	}
}
