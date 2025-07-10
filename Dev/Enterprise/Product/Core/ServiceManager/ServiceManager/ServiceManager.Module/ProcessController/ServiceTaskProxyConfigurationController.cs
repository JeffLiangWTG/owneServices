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
	public class ServiceTaskProxyConfigurationController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.ServiceTaskProxyConfiguration; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(StmServiceHost); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var host = (StmServiceHost)businessEntity;
			return new ServiceTaskHostConfigurationForm(host);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ServiceTaskEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ServiceTaskDelete; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ServiceTaskNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ServiceTaskView; }
		}

		#endregion
	}
}
