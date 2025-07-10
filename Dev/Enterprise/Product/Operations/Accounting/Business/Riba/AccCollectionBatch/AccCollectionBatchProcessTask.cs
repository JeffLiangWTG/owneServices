using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.Riba
{
	public class AccCollectionBatchProcessTask : ProcessTasks
	{
		public AccCollectionBatchProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.AccCollectionBatch; }
		}

		protected override Type ParentType
		{
			get { return typeof(AccCollectionBatch); }
		}

		public new AccCollectionBatch Parent
		{
			get { return (AccCollectionBatch)base.Parent; }
		}

		#endregion
	}
}

