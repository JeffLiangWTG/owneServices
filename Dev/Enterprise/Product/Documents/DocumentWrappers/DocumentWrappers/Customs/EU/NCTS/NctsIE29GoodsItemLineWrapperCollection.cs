using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.ServiceTask;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	public class NctsIE29GoodsItemLineWrapperCollection : DocBaseWrapperCollection<NctsIE29GoodsItemLineWrapper>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public NctsIE29GoodsItemLineWrapperCollection(NctsIE29CusdecResponseData cusdecResponseData, NctsEdiMessage ediMessage)
			: base(ediMessage.Factory)
		{
			foreach (var line in cusdecResponseData.GoodsItems)
			{
				Add(NctsIE29GoodsItemLineWrapper.New(line, ediMessage));
			}
		}
	}
}
