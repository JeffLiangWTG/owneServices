using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderProcessTask : ProcessTask, Integration.Customs.EU.NCTS.INctsHeaderProcessTask
	{
		public NctsHeaderProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Customs.EU.NctsMovementController; }
		}

		public new NctsHeader Parent
		{
			get { return (NctsHeader)base.Parent; }
		}

		protected override Type ParentType
		{
			get { return typeof(NctsHeader); }
		}
	}
}
