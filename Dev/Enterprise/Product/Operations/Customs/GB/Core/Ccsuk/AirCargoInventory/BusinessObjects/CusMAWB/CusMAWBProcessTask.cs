using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusMAWBProcessTask : ProcessTask, Integration.Customs.GB.CCSUK.ICusMAWBProcessTask
	{
		public CusMAWBProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(CusMAWB); }
		}

		public new CusMAWB Parent
		{
			get { return (CusMAWB)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Customs.GB.CcsukAirInventory; }
		}
	}
}
