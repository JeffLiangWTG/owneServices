using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CFSShipmentWrapperDepotCusOutturnCollection : GenericCusOutturnCollection<DepotCusOutturn, CFSShipmentWrapper>
	{
		public CFSShipmentWrapperDepotCusOutturnCollection(CFSShipmentWrapper wrapper)
			: base(wrapper)
		{
		}
	}
}
