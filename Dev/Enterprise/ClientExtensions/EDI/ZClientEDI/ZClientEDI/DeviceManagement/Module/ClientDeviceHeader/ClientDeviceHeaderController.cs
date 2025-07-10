using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.DeviceManagement.GUI;
using Enterprise.Client.EDI.Modules;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.DeviceManagement.Module
{
	public class ClientDeviceHeaderController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID => ClientModuleRegistration.ClientDevice;

		public override ControllerID ID => ClientControllerRegistration.ClientDevice;

		public override Type TypeOfTopLevelBusinessObject => typeof(ClientDeviceHeader);

		protected override IZForm GetForm(IBusiness businessEntity) => new ClientDeviceHeaderForm((ClientDeviceHeader)businessEntity);

		protected override SecurityCheckpoint CheckPointForDelete => EDISecurityCheckpoints.DevicesDelete;

		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.DevicesEdit;

		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.DevicesNew;

		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.DevicesView;
	}
}
