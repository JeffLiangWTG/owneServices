using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ServiceManager.Module
{
	public class ServiceTaskLogViewerController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public ServiceTaskLogViewerController()
		{
		}

		#region Standard Controller Overrides

		public override ControllerID ID
		{
			get { return ControllerIDs.ServiceTaskLogViewer; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ServiceTaskLogViewer); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ServiceTaskLogViewerForm((ServiceTaskLogViewer)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.StmServiceTask; }
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			return sourceEntity;
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ServiceTaskViewLogs; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ServiceTaskViewLogs; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ServiceTaskViewLogs; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ServiceTaskViewLogs; }
		}

		#endregion
	}
}
