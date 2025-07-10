using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public partial class BRCargoStatusList
	{
		public static ZString MapToCWCode(ZString code)
		{
			switch (code.ToUpper())
			{
				case "DESVINCULADA":
					return BRCargoStatusList.Codes.Unbound;
				case "ENTREGUE":
					return BRCargoStatusList.Codes.Delivered;
				case "ATRACADA":
					return BRCargoStatusList.Codes.Moored;
				case "VINCULADA":
					return BRCargoStatusList.Codes.Bound;
				default:
					return ZString.Empty;
			}
		}
	}
}
