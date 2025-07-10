using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration.TransportBooking;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.TransportBooking.Subscribers
{
	public class DtbBookingMasterBookingReplicationSubscriber : BaseDtbMasterBookingUpdateOnlyReplicationSubscriber
	{
		public DtbBookingMasterBookingReplicationSubscriber() : base()
		{
		}

		public DtbBookingMasterBookingReplicationSubscriber(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override string Code => "MBB";

		public override ITableSchema Table => DtbBookingSchema.Instance;

		protected override Type BizOType
		{
			get
			{
				if (bizOType == null)
				{
					bizOType = CargoWise.Application.ObjectFactory.GetType<IDtbBooking>();
				}

				return bizOType;
			}
		}
		Type bizOType;

		protected override IEnumerable<SchemaColumn> OtherIncludedColumns => new SchemaColumn[]
		{
			DtbBookingSchema.KM_KB_Booking,
			DtbBookingSchema.KM_KB_BookingConsolidationMultiJob,
			DtbBookingSchema.KM_JobID,
		};

		protected override string GetChangeRowEntityDescription(DataRow changeRow, BusinessObject parentEntity)
		{
			return FormattableString.Invariant($"Booking Job ID {changeRow[DtbBookingSchema.Constants.KM_JobID].GetDataRowValue<string>()}");
		}

		protected override string GetChangeRowEntityMultilingualDescription(DataRow changeRow, BusinessObject parentEntity)
		{
			return Res.GetString("68423808-75b0-4a22-a5a6-845cdb856b00", "Booking Job ID {0}", changeRow[DtbBookingSchema.Constants.KM_JobID].GetDataRowValue<string>());
		}

		protected override string GetEntityDescription(BusinessObject entity) =>
			FormattableString.Invariant($"Booking Job ID {entity[DtbBookingSchema.KM_JobID]}");

		protected override string GetEntityMultilingualDescription(BusinessObject entity) =>
			Res.GetString("d253600c-2584-4322-a550-4a1c85c3d730", "Booking Job ID {0}", entity[DtbBookingSchema.KM_JobID]);

		protected override string GetErrorNoteSubAndMasterMessagePart(RowChangeType rowChangeType, BusinessObject subEntity, BusinessObject subDtbBooking, DataRow masterEntityChangeRow)
		{
			var subEntityJobID = (subEntity == null) ? ZString.Empty : (ZString)subEntity[DtbBookingSchema.KM_JobID];
			return GetErrorNoteSubAndMasterMessagePartCore(rowChangeType, subEntityJobID, masterEntityChangeRow[DtbBookingSchema.Constants.KM_JobID].GetDataRowValue<string>());
		}

		string GetErrorNoteSubAndMasterMessagePartCore(RowChangeType rowChangeType, string subJobID, string masterJobID) =>
			Res.GetString("a86585e5-ff95-438e-b4ef-8f5eb6216a6a", "{0} of Booking for sub Booking Job ID {1} for master Booking Job ID {2}", GetRowChangeTypeMultilingualString(rowChangeType), subJobID, masterJobID);

		protected override string GetErrorNoteSubAndMasterLogPart(RowChangeType rowChangeType, BusinessObject subEntity, BusinessObject subDtbBooking, DataRow masterEntityChangeRow)
		{
			var subEntityJobID = (subEntity == null) ? ZString.Empty : (ZString)subEntity[DtbBookingSchema.KM_JobID];
			return GetErrorNoteSubAndMasterLogPartCore(rowChangeType, subEntityJobID, masterEntityChangeRow[DtbBookingSchema.Constants.KM_JobID].GetDataRowValue<string>());
		}

		string GetErrorNoteSubAndMasterLogPartCore(RowChangeType rowChangeType, string subJobID, string masterJobID) =>
			FormattableString.Invariant($"{rowChangeType} of Booking for sub Booking Job ID {subJobID} for master Booking Job ID {masterJobID}");

		protected override BusinessObject GetDtbBookingFromEntity(BusinessObject entity) => entity;
	}
}
