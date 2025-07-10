using System;
using CargoWise.Definitions;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.DeviceManagement.Module
{
	class ClientDeviceHeaderOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType => typeof(ClientDeviceHeader);
		public override BusinessContext BusinessContext => BusinessContext.ClientDeviceMgmt;
		public override SecurityCheckpoint CustomizationSecurityCheckpoint => EDISecurityCheckpoints.DevicesOperationalActionsCustomization;
		public override SecurityCheckpoint RunSecurityCheckpoint => EDISecurityCheckpoints.DevicesOperationalActionsExecution;

		public override string SingularElementNoun => Res.GetString("ClientDeviceHeader|OperationalActions|SingleElement", "device");
		public override string PluralElementNoun => Res.GetString("ClientDeviceHeader|OperationalActions|PluralElement", "devices");
	}
}
