using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.FR.DocumentWrappers.NCTS;

public class NctsDepartureCargoDescWrapperCollection : DocBaseWrapperCollection<Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsDepartureCargoDescWrapper>
{
	public NctsDepartureCargoDescWrapperCollection(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(factory)
	{
		Argument.NotNull(nctsHeader, nameof(nctsHeader));
		Argument.NotNull(factory, nameof(factory));
		var goodsItems = nctsHeader.IsPhase5 ? nctsHeader.Bills.SelectMany(x => x.GoodsItems) : nctsHeader.MovementHeader.GoodsItems;
		goodsItems.ForEach(x => base.Add(NctsDepartureCargoDescWrapper.New(x, factory)));
	}
}
