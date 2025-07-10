using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;
using ITNctsDepartureCargoDesc = Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc;

namespace Enterprise.Customs.IT.NCTS.Business;

public class ITNctsDepartureCargoDescWrapperCollection : DocBaseWrapperCollection<NctsDepartureCargoDescWrapper>
{
	public ITNctsDepartureCargoDescWrapperCollection(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(factory)
	{
		Argument.NotNull(nctsHeader, nameof(nctsHeader));
		Argument.NotNull(factory, nameof(factory));
		nctsHeader.DepartureGoodsItems
			.Cast<ITNctsDepartureCargoDesc>()
			.Where(x => !x.IsCustomsStatusDeleted)
			.ForEach(x => Add(ITNctsDepartureCargoDescWrapper.New(x, factory)));
	}
}
