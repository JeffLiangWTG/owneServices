using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.SeaCargo.GUI;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.Module
{
	public class SeaCargoDepotStandAloneController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.SeaCargoDepotStandAloneController; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusOutturnHeader); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new SeaCargoDepotOutturnForm((CusOutturnHeader)businessEntity);
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.AU.Module.Res.GetData("PlugInTabPage|SeaCargoDepotStandAloneController", "Sea Cargo Outturn"); } }

		internal ZPlugIn GetPlugInInternal(IBusiness businessEntity) => GetPlugIn(businessEntity);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			ZPlugIn result = null;
			TallyContainer container = businessEntity as TallyContainer;
			SCDTallyPlugIn oldTallyPlugin = new SCDTallyPlugIn(container);
			if (oldTallyPlugin.IsCMR)
			{
				oldTallyPlugin.Dispose();
				if (container != null)
				{
					result = new SeaCargoDepotOutturnPlugin(container);
				}
			}
			else
			{
				result = oldTallyPlugin;
			}
			return result;
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.AU.SeaCargoOutturnBills; }
		}

		#region CheckPoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AUCustomsSCADepotModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AUCustomsSCADepotModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AUCustomsSCADepotModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AUCustomsSCADepot; }
		}

		#endregion CheckPoints

	}
}
