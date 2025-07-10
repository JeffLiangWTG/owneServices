using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;

namespace Enterprise.Customs.IE.NCTS.DocumentWrappers
{
	public sealed class IENctsDepartureCargoDescWrapperCollection : DocBaseWrapperCollection<NctsDepartureCargoDescWrapper>
	{
		public IENctsDepartureCargoDescWrapperCollection(NctsBill nctsBill, BusinessObjectFactory factory) : base(factory)
		{
			if (nctsBill != null)
			{
				foreach (var line in nctsBill.GoodsItems)
				{
					Add(IENctsDepartureCargoDescWrapper.New(line, factory));
				}
			}
		}
	}
}
