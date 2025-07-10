using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration.TransportBooking;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.TransportBooking.Subscribers
{
	public class DtbBookingConsolidationMasterBookingReplicationSubscriber : BaseDtbMasterBookingUpdateOnlyReplicationSubscriber
	{
		public DtbBookingConsolidationMasterBookingReplicationSubscriber() : base()
		{
		}

		public DtbBookingConsolidationMasterBookingReplicationSubscriber(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override string Code => "MBC";

		public override ITableSchema Table => DtbBookingConsolidationSchema.Instance;

		protected override Type BizOType
		{
			get
			{
				if (bizOType == null)
				{
					bizOType = CargoWise.Application.ObjectFactory.GetType<IDtbBookingConsolidation>();
				}

				return bizOType;
			}
		}
		Type bizOType;

		protected override IEnumerable<SchemaColumn> OtherIncludedColumns => new SchemaColumn[]
		{
			DtbBookingConsolidationSchema.KB_JobID,
		};

		protected override string GetChangeRowEntityDescription(DataRow changeRow, BusinessObject parentEntity)
		{
			return FormattableString.Invariant($"Booking Consolidation Job ID {changeRow[DtbBookingConsolidationSchema.Constants.KB_JobID].GetDataRowValue<string>()}");
		}

		protected override string GetChangeRowEntityMultilingualDescription(DataRow changeRow, BusinessObject parentEntity)
		{
			return Res.GetString("02b4dad1-6e34-43a0-a50f-377c06aa5c53", "Booking Consolidation Job ID {0}", changeRow[DtbBookingConsolidationSchema.Constants.KB_JobID].GetDataRowValue<string>());
		}

		protected override string GetEntityDescription(BusinessObject entity)
		{
			return FormattableString.Invariant($"Booking Consolidation Job ID {entity[DtbBookingConsolidationSchema.Constants.KB_JobID]}");
		}

		protected override string GetEntityMultilingualDescription(BusinessObject entity)
		{
			return Res.GetString("d271f6e5-95d3-4aca-85ef-bdd7da629811", "Booking Consolidation Job ID {0}", entity[DtbBookingConsolidationSchema.Constants.KB_JobID]);
		}

		protected override string GetErrorNoteSubAndMasterMessagePart(RowChangeType rowChangeType, BusinessObject subEntity, BusinessObject subDtbBooking, DataRow masterEntityChangeRow)
		{
			var subEntityJobID = subEntity == null ? ZString.Empty : (ZString)subEntity[DtbBookingConsolidationSchema.KB_JobID];
			return GetErrorNoteSubAndMasterMessagePartCore(rowChangeType, subEntityJobID, masterEntityChangeRow[DtbBookingConsolidationSchema.Constants.KB_JobID].GetDataRowValue<string>());
		}

		string GetErrorNoteSubAndMasterMessagePartCore(RowChangeType rowChangeType, string subJobID, string masterJobID) =>
			Res.GetString("54f1afe1-4e98-4a2c-8d21-f748bc7f209b", "{0} of Booking Consolidation for sub Consolidation Job ID {1} for master Consolidation Job ID {2}", GetRowChangeTypeMultilingualString(rowChangeType), subJobID, masterJobID);

		protected override string GetErrorNoteSubAndMasterLogPart(RowChangeType rowChangeType, BusinessObject subEntity, BusinessObject subDtbBooking, DataRow masterEntityChangeRow)
		{
			var subEntityJobID = subEntity == null ? ZString.Empty : (ZString)subEntity[DtbBookingConsolidationSchema.KB_JobID];
			return GetErrorNoteSubAndMasterLogPartCore(rowChangeType, subEntityJobID, masterEntityChangeRow[DtbBookingConsolidationSchema.Constants.KB_JobID].GetDataRowValue<string>());
		}

		string GetErrorNoteSubAndMasterLogPartCore(RowChangeType rowChangeType, string subJobID, string masterJobID) =>
			FormattableString.Invariant($"{rowChangeType} of Booking Consolidation for sub Consolidation Job ID {subJobID} for master Consolidation Job ID {masterJobID}");

		// will be good enough for cases where we only have one booking on the consolidation
		protected override BusinessObject GetDtbBookingFromEntity(BusinessObject entity) =>
			(BusinessObject)((IActiveBusinessObjectCollection<IDtbBooking>)entity[nameof(IDtbBookingConsolidation.Bookings)]).FirstOrDefault();
	}
}
