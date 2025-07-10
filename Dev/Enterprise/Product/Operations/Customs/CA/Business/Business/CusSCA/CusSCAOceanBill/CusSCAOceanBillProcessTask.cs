using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAOceanBillProcessTask : ProcessTasks, Integration.Customs.CA.ICusSCAOceanBillProcessTask
	{
		public CusSCAOceanBillProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Customs.CA.CusSCAOceanBill; }
		}

		protected override Type ParentType
		{
			get { return typeof(CusSCAOceanBill); }
		}

		public new CusSCAOceanBill Parent
		{
			get { return (CusSCAOceanBill)base.Parent; }
		}

		#endregion
	}
}
