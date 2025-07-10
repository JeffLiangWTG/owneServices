using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.Glow.Subscribers
{
	public class GlbDeviceAssignmentDivotSubscriber : ActualDataChangesAuditSubscriber
	{
		public GlbDeviceAssignmentDivotSubscriber()
		{
			businessObjectFactory = new BusinessObjectFactory();
		}

		public override ITableSchema Table => GlbDeviceAssignmentDivotSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => null;

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		public override string Code => "GDA";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "GlbDeviceAssignmentDivot Change Subscriber";

		public override bool IsRequired() => true;

		public override Action<DataRow> CustomFilter => null;

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			var devices = new Dictionary<Guid, DataRow>();
			foreach (DataRow row in changeTable.Rows)
			{
				var v3Device = (Guid)row[GlbDeviceAssignmentDivotSchema.V7_V3_Device.Name];
				if (devices.TryGetValue(v3Device, out var rowCached))
				{
					if (HasEndTimeUtc(rowCached) && !HasEndTimeUtc(row))
					{
						devices[v3Device] = row;
					}
				}
				else
				{
					devices[v3Device] = row;
				}
			}
			var processed = false;
			foreach (var row in devices.Values)
			{
				processed = ProcessChange(logger, row) || processed;
			}
			if (processed)
			{
				businessObjectFactory.Save();
			}
		}

		bool HasEndTimeUtc(DataRow dataRow)
		{
			return dataRow.Table.Columns.Contains(GlbDeviceAssignmentDivotSchema.V7_EndTimeUtc.Name) &&
				!Convert.IsDBNull(dataRow[GlbDeviceAssignmentDivotSchema.V7_EndTimeUtc.Name]);
		}

		bool ProcessChange(ILogger logger, DataRow row)
		{
			var v3Device = (Guid)row[GlbDeviceAssignmentDivotSchema.V7_V3_Device.Name];
			var glbDevice = businessObjectFactory.LoadTop1<GlbDevice>(new ZQuery(GlbDeviceSchema.PK, v3Device));
			if (glbDevice == null)
			{
				return false;
			}

			if (HasEndTimeUtc(row))
			{
				ObjectFactory.Get<ITelematicsNotifier>().NotifyMobileServicesOfNewDeviceParent(
					businessObjectFactory,
					glbDevice.V3_HumanReadableIdentifier,
					string.Empty,
					string.Empty,
					string.Empty);
				return true;
			}

			var parentId = (Guid)row[GlbDeviceAssignmentDivotSchema.V7_ParentID.Name];
			var parentTableCode = (string)row[GlbDeviceAssignmentDivotSchema.V7_ParentTableCode.Name];
			var lookupName = string.Format("{0}_{1}", nameof(ITelematicsBusinessObjectLookupProvider), parentTableCode);
			var provider = ObjectFactory.Get<ITelematicsBusinessObjectLookupProvider>(lookupName);
			var telematicsBusinessObject = provider?.GetBusinessObject(businessObjectFactory, parentId);

			if (telematicsBusinessObject != null)
			{
				ObjectFactory.Get<ITelematicsNotifier>().NotifyMobileServicesOfNewDeviceParent(
					businessObjectFactory,
					glbDevice.V3_HumanReadableIdentifier,
					telematicsBusinessObject.Code,
					telematicsBusinessObject.DescriptionInEnglish,
					parentTableCode);
				return true;
			}
			return false;
		}

		protected readonly BusinessObjectFactory businessObjectFactory;
	}
}
