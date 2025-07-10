using System;
using CargoWise.Definitions;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions
{
	public class CcsukOperationalActionSupporterHawb : CcsukOperationalActionSupporter
	{
		public override SecurityCheckpoint BaseCheckpoint => Env.Security.AirCcsukHouse;

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CusMAWB; }  // Because CusMAWB and CusHAWB share a document supporter whose context is CusMAWB. 
		}

		public override Type RootType
		{
			get { return typeof(CusHAWB); }
		}

		protected override string ElementNamePrefix
		{
			get { return "House "; }
		}
	}
}
