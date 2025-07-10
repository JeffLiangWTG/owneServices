using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.EU.NCTS.Module
{
	public class NctsMovementController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.EuNctsMovementDelete; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.EuNctsMovementEdit; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.EuNctsMovementNew; }
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.EuNctsMovement; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var nctsHeader = (NctsHeader)businessEntity;
			if (nctsHeader.IsPhase5)
			{
				if (nctsHeader.IsDepartureMovement)
				{
					return GetPhase5DepartureForm(nctsHeader);
				}
				else
				{
					return GetPhase5ArrivalMovementForm(nctsHeader);
				}
			}
			else
			{
				return new NctsMovementForm(nctsHeader);
			}
		}

		protected virtual IZForm GetPhase5DepartureForm(NctsHeader nctsHeader) => new Phase5DepartureMovementForm(nctsHeader);

		protected virtual IZForm GetPhase5ArrivalMovementForm(NctsHeader nctsHeader) => new Phase5ArrivalMovementForm(nctsHeader);

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var nctsMovement = (NctsHeader)base.GetNewBusinessEntityInLocalFactory();
			//var nctsSettings = ObjectFactory.Get<Integration.Customs.Shared.INctsSettings>();
			//nctsMovement.BH_ApplicationCode = nctsSettings.IsUsingPhase5(nctsMovement.DefaultDataGroupingCode) ? CusInBondApplicationCodeList.Codes.NCTS5 : CusInBondApplicationCodeList.Codes.NCTS4;
			nctsMovement.SetMovementType(HeaderType);
			return nctsMovement;
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.EU.NctsMovementController; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(NctsHeader); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new NctsPlugin((ICusInBondParent)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.EU.NctsMovementModule; }
		}

		internal void ShowNewNctsMovementForm(ZString headerType)
		{
			HeaderType = headerType;
			base.ShowNewForm();
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Res.GetData("PlugInTabPage|NctsMovementController", "NCTS"); }
		}

		protected ZString HeaderType { get; set; }
	}
}

