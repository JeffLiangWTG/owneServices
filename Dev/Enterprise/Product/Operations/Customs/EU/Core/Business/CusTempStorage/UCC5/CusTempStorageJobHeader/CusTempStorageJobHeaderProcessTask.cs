using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageJobHeaderProcessTask : Enterprise.MasterFiles.Business.ProcessTasks
	{
		public CusTempStorageJobHeaderProcessTask(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row) { }

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Customs.TemporaryStorage; }
		}

		protected override Type ParentType
		{
			get { return typeof(CusTempStorageJobHeader); }
		}

		public new CusTempStorageJobHeader Parent
		{
			get { return (CusTempStorageJobHeader)base.Parent; }
		}

		#endregion
	}
}
