using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Integration;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.Telematics.Subscribers
{
	public class GlbDeviceLocationSubscriber : ActualDataChangesAuditSubscriber
	{
		public GlbDeviceLocationSubscriber()
		{
			factory = new BusinessObjectFactory();
		}

		public GlbDeviceLocationSubscriber(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public override ITableSchema Table => GlbDeviceLocationSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => null;

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => false;

		public override bool NotifyDelete => false;

		public override string Code => "GDL";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "GlbDeviceLocation Change Subscriber";

		public override bool IsRequired() => LocalCartageDataRegistry.Instance.UseGlbDeviceLocationSubscriber.Value;

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[GlbDeviceLocationSchema.Constants.V2_MeasurementTimeUtc] == DBNull.Value || Convert.ToDateTime(row[GlbDeviceLocationSchema.Constants.V2_MeasurementTimeUtc]) >= ZDateTime.UtcToday.AddDays(1).ToDateTime())
			{
				row.Delete();
			}
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message logging")]
		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			var gpsCartageLegUpdater = ObjectFactory.New<IGPSCartageLegUpdater>(factory);
			var gpsDtbConsignmentRunSheetInstructionUpdater = ObjectFactory.New<IGPSDtbConsignmentRunSheetInstructionUpdater>(factory);
			ZDateTime lastMeasurementTime = ZDateTime.Empty;

			try
			{
				foreach (DataRow row in changeTable.Rows)
				{
					var pk = (Guid)row[AutoGlbDeviceLocation.Schema.PK];
					var query = new ZQuery();
					query.AddToFilter(TelDeviceLocationWithEntitySchema.PK, SQLComparisonOperator.Equal, pk);
					query.AddToFilter(TelDeviceLocationWithEntitySchema.TLL_ParentTableCode, new[] { RefEquipmentSchema.Constants.Prefix, GlbStaffSchema.Constants.Prefix });
					var deviceLocationWithEntity = factory.LoadTop1<IDeviceLocationWithEntity>(query);
					if (deviceLocationWithEntity != null)
					{
						switch(deviceLocationWithEntity.EntityTableCode)
						{
							case RefEquipmentSchema.Constants.Prefix:
								gpsCartageLegUpdater.ProcessLocation(deviceLocationWithEntity);
								gpsDtbConsignmentRunSheetInstructionUpdater.ProcessLocation(deviceLocationWithEntity);
								break;
							case GlbStaffSchema.Constants.Prefix:
								gpsDtbConsignmentRunSheetInstructionUpdater.ProcessLocation(deviceLocationWithEntity);
								break;
						}
					}

					var measurementTime = (DateTime)row[AutoGlbDeviceLocation.Schema.V2_MeasurementTimeUtc];
					if (measurementTime > lastMeasurementTime || !lastMeasurementTime.IsValid)
					{
						lastMeasurementTime = measurementTime;
					}
				}

				if (lastMeasurementTime.IsValid)
				{
					gpsCartageLegUpdater.UpdateLastProcessedEventTime(lastMeasurementTime);
				}

				if (changeTable.Rows.Count > 0)
				{
					factory.Save();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Error(string.Format(CultureInfo.InvariantCulture, "Exception in processing changes in GlbDeviceLocation: {0}", ex));
				throw;
			}
		}

		readonly BusinessObjectFactory factory;
	}
}
