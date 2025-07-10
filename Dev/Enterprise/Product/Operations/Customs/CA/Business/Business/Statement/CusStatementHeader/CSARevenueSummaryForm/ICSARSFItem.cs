using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public interface ICSARSFItem
	{
		ZString LineItemNumber { get; }
		ZDecimal MonetaryAmount { get; }
		ZString Type { get; }
		ZString PortCode { get; }
		ZString CodeID { get; }
	}
}
