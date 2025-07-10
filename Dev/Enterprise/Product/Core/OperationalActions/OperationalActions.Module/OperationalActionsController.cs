using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.ModulePlugIn;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Services.OperationalActions.Module
{
	public sealed class OperationalActionsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.OperationalActions; }
		}

		protected override ZModulePlugin GetModulePluginCore(ZFilterGridModule module)
		{
			IOperationalActionSupportable supportable = module as IOperationalActionSupportable;
			OperationalActionSupporter supporter;
			ZModulePlugin result;

			if (
				supportable == null ||
				(supporter = supportable.OperationalActionSupporter) == null
				)
			{
				result = null;
			}
			else
			{
				OperationalActionContext context = new OperationalActionContext(supporter, module.ID.Description, module.WorkflowType);
				result = new OperationalActionsModulePlugin(supporter.GetModuleSelection(module), context);
			}

			return result;
		}

		#region Not Supported

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Not Supported"); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Not Supported");
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
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
