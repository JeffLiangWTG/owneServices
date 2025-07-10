using CargoWise.EntityFramework;

namespace Enterprise.Customs.Common
{
	public interface IEDIMessageCollectionOwner
	{
		BusinessObject MessageOwner { get; }
		IBusinessObjectCollection Messages { get; }
	}
}
