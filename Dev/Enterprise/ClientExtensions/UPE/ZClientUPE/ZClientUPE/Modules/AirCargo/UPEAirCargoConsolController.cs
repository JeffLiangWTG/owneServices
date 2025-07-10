using System;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public class UPEAirCargoConsolController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ClientControllerRegistration.AirCargoConsol; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ClientModuleRegistration.AirCargo; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(UPECusMAWB); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new UPEAirCargoMasterForm((UPECusMAWB)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ACAMasterImportDelete; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ACAMasterImportModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ACAMasterImportModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ACAMasterImportView; }
		}
	}
}