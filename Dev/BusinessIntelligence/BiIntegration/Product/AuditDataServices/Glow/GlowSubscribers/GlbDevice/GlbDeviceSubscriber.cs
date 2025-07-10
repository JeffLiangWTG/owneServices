using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.MobileServices.Common.Messages;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.Glow.Subscribers
{
	public class GlbDeviceSubscriber : ActualDataChangesAuditSubscriber
	{
		public GlbDeviceSubscriber()
		{
			factory = new BusinessObjectFactory();
		}

		public override ITableSchema Table => GlbDeviceSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => null;

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		public override string Code => "GDC";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "GlbDevice Change Subscriber";

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[GlbDeviceSchema.Constants.V3_IsBYOD] == DBNull.Value
				|| Convert.ToInt32(row[GlbDeviceSchema.Constants.V3_IsBYOD]) != 1)
			{
				row.Delete();
			}
		};

		public override bool IsRequired() => true;

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			foreach (DataRow row in changeTable.Rows)
			{
				if (row.RowState == DataRowState.Added || StatusChangedTo(row, GlbDeviceStatusList.Codes.PendingRegistration))
				{
					ObjectFactory.Get<ITelematicsNotifier>().NotifyMobileServicesOfBYODRegistration(
						factory,
						(string)row[GlbDeviceSchema.V3_HumanReadableIdentifier.Name],
						(string)row[GlbDeviceSchema.V3_Model.Name],
						GetDeviceKindForMobileServices((string)row[GlbDeviceSchema.V3_HardwareKind.Name]),
						(string)row[GlbDeviceSchema.V3_HardwareIdentifier.Name]);
				}
				else if (StatusChangedTo(row, GlbDeviceStatusList.Codes.PendingDeregistration))
				{
					ObjectFactory.Get<ITelematicsNotifier>().NotifyMobileServicesOfBYODDeregistration(
						factory,
						(string)row[GlbDeviceSchema.V3_HumanReadableIdentifier.Name]);
				}
			}

			factory.Save();
		}

		static bool StatusChangedTo(DataRow row, string to)
		{
			var originalStatus = SafeGetColumnValue<string>(row, GlbDeviceSchema.V3_Status, DataRowVersion.Original);
			var currentStatus = (string)row[GlbDeviceSchema.V3_Status.Name, DataRowVersion.Current];
			return originalStatus != currentStatus && currentStatus == to;
		}

		static DeviceKind GetDeviceKindForMobileServices(string deviceKind)
		{
			if (deviceKind == GlbDeviceKindCodes.Android)
			{
				return DeviceKind.Android;
			}
			if (deviceKind == GlbDeviceKindCodes.AppleMobile)
			{
				return DeviceKind.AppleMobile;
			}
			if (deviceKind == GlbDeviceKindCodes.WindowsMobileLegacy)
			{
				return DeviceKind.WindowsMobileLegacy;
			}
			if (deviceKind == GlbDeviceKindCodes.WTGEmbedded)
			{
				return DeviceKind.WiseTechVehicularPlatform;
			}

			return DeviceKind.InvalidKind;
		}

		static T SafeGetColumnValue<T>(DataRow row, SchemaColumn column, DataRowVersion rowVersion = DataRowVersion.Default)
		{
			var rawValue = row[column.Name, rowVersion];
			return rawValue != DBNull.Value ? (T)rawValue : GetDefaultValue();

			T GetDefaultValue() => column.IsNullable ? default : (T)column.SqlDbDefault;
		}

		readonly BusinessObjectFactory factory;
	}
}
