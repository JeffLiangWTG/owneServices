using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CusHawbToFsrProvider : CusAwbToFsrProvider
	{
		public CusHawbToFsrProvider(CusHAWB hawb, string fsrRequestType)
			: base(hawb, fsrRequestType)
		{
			this.hawb = hawb;
		}

		protected override ZString HousewaybillNumberCore
		{
			get { return hawb.CS_HAWB; }
		}

		readonly CusHAWB hawb;
	}
}
