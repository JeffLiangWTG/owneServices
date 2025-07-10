using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICusEntryHeaderStatusProvider
	{
		ZString MessageStatusDescription { get; }
		ZString CargoStatusDescription { get; }
	}
}
