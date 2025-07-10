
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.AWB
{
	public interface IJASExportAWBHeader
	{
		JASOrgHeader Shipper { get; }
		JASOrgHeader Consignee { get; }
		ZString ShipperAccountForJXC { get; }
		ZString ConsigneeAccountForJXC { get; }
	}
}
