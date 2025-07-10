using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class TariffTestCase : TestCaseWithFactory
	{
		protected AUCSection section;
		protected AUCChapter chapter;

		protected override void SetUp()
		{
			base.SetUp();

			section = Factory.New<AUCSection>();
			section.UG_Section = 1;

			chapter = Factory.New<AUCChapter>();
			chapter.UH_Chapter = "01";
			chapter.UH_UG = section.PK;
		}
	}
}
