using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public interface IRelatedItemsNameProvider
	{
		ZString GetNameOfRelatedItem(IBusiness relatedItem);
	}
}
