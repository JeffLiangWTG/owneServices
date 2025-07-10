using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class SplitToFsrProvider : CusAwbToFsrProvider
	{
		public SplitToFsrProvider(SplitConsignment split, string fsrRequestType)
			: base(split, fsrRequestType)
		{
			this.split = split;
		}

		protected override ZString HousewaybillNumberCore
		{
			get
			{
				var splitHouse = split as SplitHouse;
				return (splitHouse != null) ? splitHouse.HAWB.CS_HAWB : ZString.Empty;
			}
		}

		protected override ZString SplitReferenceCore
		{
			get { return split.SplitReference; }
		}

		readonly SplitConsignment split;
	}
}
