using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.CommissionManagement.GUI
{
	public class ChargeCodeCommissionConfigurationPluginController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region ID

		public override ControllerID ID
		{
			get { return ClientControllerRegistration.ChargeCodeCommissionConfiguration; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		#endregion

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccChargeCode); }
		}

		#region Form

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("");
		}

		#endregion

		#region Plugin

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new ChargeCodeCommissionConfigurationPlugin((AccChargeCode)businessEntity);
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Res.GetData("ca3a6c37-74ba-4e01-b5e2-af3e55197bc4", "Commission Configuration"); }
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
