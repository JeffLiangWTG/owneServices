using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CFSTallyContainerOutturnCollection : GenericCusOutturnCollection<TallyOutturn, CFSTallyContainerWrapper>
	{
		public CFSTallyContainerOutturnCollection(CFSTallyContainerWrapper wrapper)
			: base(wrapper)
		{
		}
	}
}
