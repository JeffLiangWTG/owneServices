using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.Riba
{
	public class AccCollectionOrderProcessTask : ProcessTasks
	{
		public AccCollectionOrderProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.AccCollectionOrder; }
		}

		protected override Type ParentType
		{
			get { return typeof(AccCollectionOrder); }
		}

		public new AccCollectionOrder Parent
		{
			get { return (AccCollectionOrder)base.Parent; }
		}

		#endregion
	}
}
