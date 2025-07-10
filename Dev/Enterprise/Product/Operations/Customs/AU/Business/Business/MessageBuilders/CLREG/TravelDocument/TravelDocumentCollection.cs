using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class TravelDocumentCollection : DependentCusAddInfoCollection<TravelDocument, CLREGInfoProvider>
	{
		public TravelDocumentCollection(CLREGInfoProvider master)
			: base(master, CusAddInfoTypeAttribute.Codes.AUCLR)
		{
		}
	}
}
