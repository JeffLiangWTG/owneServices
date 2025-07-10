using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureMovementHeaderProcessTask : ProcessTask, Integration.Customs.EU.NCTS.IDepartureMovementHeaderProcessTask
	{
		public NctsDepartureMovementHeaderProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override Type ParentType
		{
			get { return typeof(NctsDepartureMovementHeader); }
		}

		public new NctsDepartureMovementHeader Parent
		{
			get { return (NctsDepartureMovementHeader)base.Parent; }
		}

		public override ControllerID ParentControllerID => ControllerIDs.Customs.EU.NctsMovementController;
	}
}
