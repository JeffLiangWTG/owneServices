using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DeviceManagement.Business.Subscribers
{
	public class ClientDeviceHeaderGlowSubscriber : ActualDataChangesAuditSubscriber
	{
		public override ITableSchema Table => DmgDeviceHeaderSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => null;

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		public override Action<DataRow> CustomFilter => null;

		public override string Code => "GDS";

		public override string Description => "Glow DmgDeviceHeader Change Subscriber";

		public override bool IsRequired() => true;

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			var hasChanged = false;
			foreach (DataRow row in changeTable.Rows)
			{
				hasChanged = ProcessChange(logger, row) || hasChanged;
			}
			if (hasChanged)
			{
				DataFactory.Save();
			}
		}

		bool ProcessChange(ILogger logger, DataRow row)
		{
			var isTemplate = (bool)row[DmgDeviceHeaderSchema.CDH_IsTemplate.Name];
			var deviceIdentifier = (string)row[DmgDeviceHeaderSchema.CDH_DeviceIdentifier.Name];
			if (isTemplate ||
				string.IsNullOrEmpty(deviceIdentifier) ||
				(!ClientDeviceHeaderSubscriberHelper.HasChange(row, DmgDeviceHeaderSchema.CDH_Description) &&
					!ClientDeviceHeaderSubscriberHelper.HasChange(row, DmgDeviceHeaderSchema.CDH_DeviceIdentifier) &&
					!ClientDeviceHeaderSubscriberHelper.HasChange(row, DmgDeviceHeaderSchema.CDH_DeviceKind) &&
					!ClientDeviceHeaderSubscriberHelper.HasChange(row, DmgDeviceHeaderSchema.CDH_EnterpriseCode) &&
					!ClientDeviceHeaderSubscriberHelper.HasChange(row, DmgDeviceHeaderSchema.CDH_Identifier) &&
					!ClientDeviceHeaderSubscriberHelper.HasChange(row, DmgDeviceHeaderSchema.CDH_ServerCode)))
			{
				return false;
			}

			var serverCode = (string)row[DmgDeviceHeaderSchema.CDH_ServerCode.Name];
			var enterpriseCode = (string)row[DmgDeviceHeaderSchema.CDH_EnterpriseCode.Name];
			var licence = ClientDeviceHeaderSubscriberHelper.GetLicenseFromCodes(DataFactory, serverCode, enterpriseCode);
			var deviceAssignedTo = licence?.LicenceCodeForSystemMessage ?? string.Empty;

			var identifier = (string)row[DmgDeviceHeaderSchema.CDH_Identifier.Name];
			var description = (string)row[DmgDeviceHeaderSchema.CDH_Description.Name];
			var deviceKind = (string)row[DmgDeviceHeaderSchema.CDH_DeviceKind.Name];
			var isBYOD = (bool)row[DmgDeviceHeaderSchema.CDH_IsBYOD.Name];

			ObjectFactory.Get<ITelematicsNotifier>().NotifyMobileServicesOfDeviceDetails(
				DataFactory,
				EDIDataRegistry.Instance.MobileServicesEHubClientID.Value,
				identifier,
				isBYOD,
				"WiseTech Global",
				description,
				MobileServicesExtensions.GetDeviceKindForMobileServices(deviceKind),
				deviceIdentifier,
				deviceAssignedTo);
			return true;
		}

		BusinessObjectFactory DataFactory => dataFactory ?? (dataFactory = new BusinessObjectFactory { RefreshEnabled = false });
		BusinessObjectFactory dataFactory;
	}
}
