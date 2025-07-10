using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageHeaderProcessTask : Enterprise.MasterFiles.Business.ProcessTasks, Integration.Customs.EU.ITemporaryStorageHeaderProcessTask
	{
		public TemporaryStorageHeaderProcessTask(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row) { }

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Customs.EU.UCC6TemporaryStorage; }
		}

		protected override Type ParentType
		{
			get { return typeof(TemporaryStorageHeader); }
		}

		public new TemporaryStorageHeader Parent
		{
			get { return (TemporaryStorageHeader)base.Parent; }
		}

		#endregion
	}
}
