using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(RecommendationLetterAddInfo))]
	sealed class RecommendationLetterAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new RecommendationLetterAddInfo(Factory.New<RecommendationLetter>().B7_AddInfoDataInfo);
	}
}
