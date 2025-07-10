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
	public class ClientDeviceHeaderTemplateController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ClientModuleRegistration.ClientDeviceTemplate; }
		}

		public override ControllerID ID
		{
			get { return ClientControllerRegistration.ClientDeviceTemplate; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ClientDeviceHeader); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ClientDeviceHeaderForm((ClientDeviceHeader)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return EDISecurityCheckpoints.DeviceTemplatesDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return EDISecurityCheckpoints.DeviceTemplatesEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return EDISecurityCheckpoints.DeviceTemplatesNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return EDISecurityCheckpoints.DeviceTemplatesView; }
		}
	}
}
