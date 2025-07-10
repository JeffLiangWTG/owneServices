using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(RecommendationLetter))]
	sealed class RecommendationLetterTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<RecommendationLetter>
	{
		public void TestProperties()
		{
			var letter = quarantineHeader.RecommendationLetters.AddNew();
			letter.ZA_LetterNumber = "1234444444";
			letter.ZA_LetterDate = new ZDateTime(2017, 03, 15);
			Factory.Save();

			var businessObjFactory = new BusinessObjectFactory();
			var letterLoadObj = businessObjFactory.Load<RecommendationLetter>(letter.PK);
			AssertEquals(letter.ZA_LetterNumber, letterLoadObj.ZA_LetterNumber);
			AssertEquals(letter.ZA_LetterDate, letterLoadObj.ZA_LetterDate);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var quarantineHeader = declaration.Invoices.AddNew().QuarantineExDocHeader;
			var letter = quarantineHeader.RecommendationLetters.AddNew();
			letter.ZA_LetterNumber = "123";
			letter.ZA_LetterDate = ZDate.Today;
			return letter;
		}

		protected override BusinessObject GetNewBusinessObject() => quarantineHeader.RecommendationLetters.AddNew();

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			quarantineHeader = helper.Header1.QuarantineExDocHeader;
		}
		QuarantineExDocHeader quarantineHeader;
	}
}
