using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.GUI.Ccsuk;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class CcsukSplitHouseController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new SplitConsignmentForm((SplitHouse)businessEntity);
		}

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AirCcsukHouseDelete; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AirCcsukHouseEdit; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AirCcsukHouseNew; }
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AirCcsukHouseView; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.GB.CcsukSplitHouseController; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(SplitHouse); }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.Customs.EU.GB.CcsukSplitHouse;
			}
		}
	}
}
