using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Customs
{
	class FreightWrapperCustomsInfoAdapter : ICustomsInfo
	{
		public FreightWrapperCustomsInfoAdapter(FreightWrapper freightWrapper)
		{
			this.freightWrapper = freightWrapper;
		}

		readonly FreightWrapper freightWrapper;

		BaseJobDeclaration ICustomsInfo.Declaration
		{
			get { return freightWrapper.Declaration; }
		}
	}
}
