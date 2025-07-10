using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitHeaderProcessTask : ProcessTask, Integration.Customs.EUExitControl.ICusExitHeaderProcessTask
	{
		public CusExitHeaderProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public override ControllerID ParentControllerID => ControllerIDs.Customs.EU.ExitControl;

		public new CusExitHeader Parent => (CusExitHeader)base.Parent;

		protected override Type ParentType => typeof(CusExitHeader);
	}
}
