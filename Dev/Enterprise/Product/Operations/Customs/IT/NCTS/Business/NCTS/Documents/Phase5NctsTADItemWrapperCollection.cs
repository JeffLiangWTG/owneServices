using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;

namespace Enterprise.Customs.IT.NCTS.Business;

public class Phase5NctsTADItemWrapperCollection : DocBaseWrapperCollection<Phase5NctsTADItemWrapper>
{
	public Phase5NctsTADItemWrapperCollection(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(factory)
	{
		if (nctsHeader.MovementHeader != null)
		{
			AddRange(nctsHeader.Bills
				.Where(bill => !bill.IsCustomsStatusDeleted && !bill.IsCustomsStatusDeletionRequested)
				.SelectMany(bill => bill.GoodsItems
				.Where(goodsItem => !goodsItem.IsCustomsStatusDeleted && !goodsItem.IsCustomsStatusDeletionRequested)
				.Select(goodsItem => Phase5NctsTADItemWrapper.New(goodsItem, factory)))
			);
		}
	}
}
