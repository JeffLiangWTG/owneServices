using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAOceanBillProcessTask : ProcessTask, Integration.Customs.AU.ICusSCAOceanBillProcessTask
	{
		public CusSCAOceanBillProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(CusSCAOceanBill); }
		}

		public new CusSCAOceanBill Parent
		{
			get { return (CusSCAOceanBill)base.Parent; }
		}

		public override ControllerID ParentControllerID => Parent != null && Parent.Consol == null ? ControllerIDs.Customs.AU.SeaCargoStandAloneController : ControllerIDs.Customs.AU.SeaCargo;
	}
}
