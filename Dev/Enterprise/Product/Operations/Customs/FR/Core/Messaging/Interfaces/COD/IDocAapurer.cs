using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.COD
{
	public interface IDocAapurer
	{
		ZString DocumentCode { get; } //Articles/Article/Documents/DocumentAapurer/doc
		ZString DocumentReference { get; } //Articles/Article/Documents/DocumentAapurer/refdoc
	}
}
