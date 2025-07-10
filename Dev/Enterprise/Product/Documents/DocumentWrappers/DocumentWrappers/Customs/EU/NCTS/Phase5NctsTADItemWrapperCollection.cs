using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	public class Phase5NctsTADItemWrapperCollection : DocBaseWrapperCollection<Phase5NctsTADItemWrapper>
	{
		public Phase5NctsTADItemWrapperCollection(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(factory)
		{
			if (nctsHeader.MovementHeader != null)
			{
				AddRange(nctsHeader.Bills.SelectMany(bill => bill.GoodsItems).OrderBy(goodsItem => goodsItem.BY_DeclarationGoodsItemNumber).Select(line => Phase5NctsTADItemWrapper.New(line, factory)));
			}
		}
	}
}
