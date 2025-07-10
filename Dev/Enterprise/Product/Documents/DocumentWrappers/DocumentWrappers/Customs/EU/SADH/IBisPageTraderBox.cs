using CargoWise.Types;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public interface IBisPageTraderBox
	{
		ZString Caption { get; }
		ZString ID { get; }
		ZString Content { get; }
	}
}
