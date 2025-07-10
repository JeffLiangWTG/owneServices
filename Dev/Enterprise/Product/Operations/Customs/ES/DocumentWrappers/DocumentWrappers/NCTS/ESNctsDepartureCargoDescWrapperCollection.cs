using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;
using ESNctsDepartureCargoDesc = Enterprise.Customs.ES.NCTS.Business.NctsDepartureCargoDesc;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS
{
	public class ESNctsDepartureCargoDescWrapperCollection : DocBaseWrapperCollection<NctsDepartureCargoDescWrapper>
	{
		public ESNctsDepartureCargoDescWrapperCollection(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(factory)
		{
			Argument.NotNull(nctsHeader, nameof(nctsHeader));
			Argument.NotNull(factory, nameof(factory));

			var goodsItems = nctsHeader.IsPhase5 ? nctsHeader.Bills.SelectMany(x => x.GoodsItems) : nctsHeader.MovementHeader.GoodsItems;
			goodsItems.Cast<ESNctsDepartureCargoDesc>().ForEach(x => Add(ESNctsDepartureCargoDescWrapper.New(x, factory)));
		}
	}
}
