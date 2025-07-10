using System;
using CargoWise.Definitions;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions
{
	public class CcsukOperationalActionSupporterMawb : CcsukOperationalActionSupporter
	{
		public override SecurityCheckpoint BaseCheckpoint => Env.Security.AirCcsukMaster;

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CusMAWB; }
		}

		public override Type RootType
		{
			get { return typeof(CusMAWB); }
		}
	}
}
