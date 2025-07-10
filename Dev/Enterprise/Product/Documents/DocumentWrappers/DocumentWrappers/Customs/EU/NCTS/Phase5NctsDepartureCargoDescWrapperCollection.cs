using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	public class Phase5NctsDepartureCargoDescWrapperCollection : DocBaseWrapperCollection<Phase5NctsDepartureCargoDescWrapper>
	{
		public Phase5NctsDepartureCargoDescWrapperCollection(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(factory)
		{
			if (nctsHeader.MovementHeader != null)
			{
				foreach (var bill in nctsHeader.Bills)
				{
					foreach (var line in bill.GoodsItems)
					{
						Add(Phase5NctsDepartureCargoDescWrapper.New(line, factory));
					}
				}
			}
		}
	}
}
