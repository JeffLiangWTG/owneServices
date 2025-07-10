using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBProcessTask : ProcessTask, Integration.Customs.AU.ICusHAWBProcessTask
	{
		public CusHAWBProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(CusHAWB); }
		}

		public new CusHAWB Parent
		{
			get { return (CusHAWB)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Customs.AU.HouseAirCargo; }
		}
	}
}
