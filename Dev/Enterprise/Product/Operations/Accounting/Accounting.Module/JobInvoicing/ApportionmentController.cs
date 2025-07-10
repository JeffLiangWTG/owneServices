using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ApportionmentController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public ApportionmentController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Apportionment; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		#region Implementation

		protected override ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Not supported");
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		#region Security checkpoints

		/// <summary>
		/// Security checkpoints are not required here. They are added on a relevant module at the time of adding the plugin
		/// </summary>
		protected override SecurityCheckpoint CheckPointForView
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

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}
		#endregion

		#endregion

		#region Implementation

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Accounting.Module.Res.GetData("PlugInTabPage|Apportionment", "Consol Costing", "The Consol Costing tab."); } }

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new GUI.JobInvoicing.ConsolCosting.ApportionmentPlugin(businessEntity);
		}

		#endregion
	}
}
