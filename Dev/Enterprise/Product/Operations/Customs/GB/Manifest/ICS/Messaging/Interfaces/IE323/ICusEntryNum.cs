using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging.IE323
{
	public interface ICusEntryNum
	{
		ZString DocumentOrReferenceNumber { get; }
		IGoodsItem GoodsItem { get; }
	}
}
