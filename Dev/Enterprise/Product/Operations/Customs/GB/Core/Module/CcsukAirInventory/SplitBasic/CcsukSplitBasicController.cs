using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.GUI.Ccsuk;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class CcsukSplitBasicController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new SplitConsignmentForm((SplitBasic)businessEntity);
		}

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AirCcsukMasterDelete; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AirCcsukMasterEdit; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AirCcsukMasterNew; }
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AirCcsukMasterView; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.GB.CcsukSplitBasicController; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(SplitBasic); }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.Customs.EU.GB.CcsukSplitBasic;
			}
		}
	}
}
