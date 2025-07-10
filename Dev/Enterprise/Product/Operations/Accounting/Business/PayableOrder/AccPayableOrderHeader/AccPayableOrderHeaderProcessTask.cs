using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.PayableOrder
{
	public class AccPayableOrderHeaderProcessTask : ProcessTasks
	{
		public AccPayableOrderHeaderProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.AccPayableOrder; }
		}

		protected override Type ParentType
		{
			get { return typeof(AccPayableOrderHeader); }
		}

		public new AccPayableOrderHeader Parent
		{
			get { return (AccPayableOrderHeader)base.Parent; }
		}

		public override ZString JobNumber
		{
			get { return Parent.APH_OrderNumber; }
		}

		#endregion
	}
}

