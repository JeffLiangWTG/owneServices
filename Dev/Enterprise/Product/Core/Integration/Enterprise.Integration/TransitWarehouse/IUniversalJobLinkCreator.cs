using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transit.Business.Common
{
	public interface IUniversalJobLinkCreator
	{
		void CreateUniversalJobLink(BusinessObject targetBO);
	}
}
