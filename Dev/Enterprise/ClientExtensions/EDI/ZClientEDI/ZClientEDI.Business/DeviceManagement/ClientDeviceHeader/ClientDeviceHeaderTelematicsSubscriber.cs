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

namespace Enterprise.Client.EDI.DeviceManagement.Business.Subscribers
{
	public class ClientDeviceHeaderTelematicsSubscriber : ActualDataChangesAuditSubscriber
	{
		public override ITableSchema Table => DmgDeviceHeaderSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => null;

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		public override Action<DataRow> CustomFilter => null;

		public override string Code => "TDS";

		public override string Description => "Telematics DmgDeviceHeader Change Subscriber";

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
			var deviceKind = MobileServicesExtensions.GetDeviceKindForMobileServices((string)row[DmgDeviceHeaderSchema.CDH_DeviceKind.Name]);
			if (isTemplate ||
				string.IsNullOrEmpty(deviceIdentifier) ||
				deviceKind != DeviceKind.WiseTechVehicularPlatform ||
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
			var previousServerCode = row.RowState == DataRowState.Modified ? (string)row[DmgDeviceHeaderSchema.CDH_ServerCode.Name, DataRowVersion.Original] : string.Empty;
			var previousEnterpriseCode = row.RowState == DataRowState.Modified ? (string)row[DmgDeviceHeaderSchema.CDH_EnterpriseCode.Name, DataRowVersion.Original] : string.Empty;
			var previousLicense = ClientDeviceHeaderSubscriberHelper.GetLicenseFromCodes(DataFactory, previousServerCode, previousEnterpriseCode);

			var deviceAssignedTo = licence?.LicenceCodeForSystemMessage ?? string.Empty;
			var deviceWasAssignedTo = previousLicense?.LicenceCodeForSystemMessage ?? string.Empty;
			deviceWasAssignedTo = deviceAssignedTo == deviceWasAssignedTo ? string.Empty : deviceWasAssignedTo;

			var identifier = (string)row[DmgDeviceHeaderSchema.CDH_Identifier.Name];
			var description = (string)row[DmgDeviceHeaderSchema.CDH_Description.Name];

			ObjectFactory.Get<ITelematicsNotifier>().NotifyTelematicsServicesOfDeviceDetails(
				DataFactory,
				EDIDataRegistry.Instance.ActiveMiddlewareService.Value.GetActiveCodeDescriptionPairList().GetAllCodes(),
				identifier,
				description,
				deviceIdentifier,
				deviceWasAssignedTo,
				deviceAssignedTo);
			return true;
		}

		BusinessObjectFactory DataFactory => dataFactory ?? (dataFactory = new BusinessObjectFactory { RefreshEnabled = false });
		BusinessObjectFactory dataFactory;
	}
}
