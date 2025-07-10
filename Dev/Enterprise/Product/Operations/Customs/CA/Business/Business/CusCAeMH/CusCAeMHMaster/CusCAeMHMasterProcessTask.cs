using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHMasterProcessTask : ProcessTasks
	{
		public CusCAeMHMasterProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Customs.CA.CAHouseBilleManifest; }
		}

		protected override Type ParentType
		{
			get { return typeof(CusCAeMHMaster); }
		}

		public new CusCAeMHMaster Parent
		{
			get { return (CusCAeMHMaster)base.Parent; }
		}

		#endregion
	}
}
