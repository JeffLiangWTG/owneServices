using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RecommendationLetterCollection : DependentCusAddInfoCollection<RecommendationLetter, BusinessObject>
	{
		public RecommendationLetterCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.AURecommendationLetter)
		{
		}
	}
}
