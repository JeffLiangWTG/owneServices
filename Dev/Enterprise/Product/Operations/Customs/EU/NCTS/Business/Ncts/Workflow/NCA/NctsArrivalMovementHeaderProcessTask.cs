using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsArrivalMovementHeaderProcessTask : ProcessTask, Integration.Customs.EU.NCTS.IArrivalMovementHeaderProcessTask
	{
		public NctsArrivalMovementHeaderProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override System.Type ParentType
		{
			get { return typeof(NctsArrivalMovementHeader); }
		}

		public new NctsArrivalMovementHeader Parent
		{
			get { return (NctsArrivalMovementHeader)base.Parent; }
		}

		public override ControllerID ParentControllerID => ControllerIDs.Customs.EU.NctsMovementController;
	}
}
