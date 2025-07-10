using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAHouseProcessTask : Customs.Business.CusSCAHouseProcessTask, Integration.Customs.AU.ICusSCAHouseProcessTask
	{
		public CusSCAHouseProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(CusSCAHouse); }
		}

		public new CusSCAHouse Parent
		{
			get { return (CusSCAHouse)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Customs.AU.SeaCargoHouseController; }
		}
	}
}
