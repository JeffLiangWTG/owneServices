using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRLodgementQuestionFindBoxListProviderTest : TestCaseWithFactory
	{
		public void TestAddCodeStartsWithFilter()
		{
			AssertEquals("~~blah~~", collection.FindBoxListProvider.NearestMatch("~~blah~~", true, -1).Item1);
			AssertEquals("1-01-JAN-2006", collection.FindBoxListProvider.NearestMatch("1-01-JAN-2006", true, -1).Item1);
			AssertEquals("TEXT1", collection.FindBoxListProvider.NearestMatch("TEXT1", true, -1).Item1);

			AssertEquals("~-", collection.FindBoxListProvider.NearestMatch("~-", true, -1).Item1);
			AssertEquals("~--", collection.FindBoxListProvider.NearestMatch("~--", true, -1).Item1);
			AssertEquals("~~1~~-1-1", collection.FindBoxListProvider.NearestMatch("~~1~~-1-1", true, -1).Item1);
		}

		public void TestAddCodeEqualsFilter()
		{
			AssertEquals(ZGuid.Empty, collection.FindBoxListProvider.PrimaryKeyFromCode(""));
			AssertEquals(ZGuid.Invalid, collection.FindBoxListProvider.PrimaryKeyFromCode("~~blah~~"));
			AssertEquals(ZGuid.Invalid, collection.FindBoxListProvider.PrimaryKeyFromCode("~~1~~"));

			AssertEquals(question1.PK, collection.FindBoxListProvider.PrimaryKeyFromCode("1-01-JAN-2006"));
			AssertEquals(ZGuid.Invalid, collection.FindBoxListProvider.PrimaryKeyFromCode("-"));
			AssertEquals(ZGuid.Invalid, collection.FindBoxListProvider.PrimaryKeyFromCode("~~1~~-1-1"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			question1 = CMRLodgementQuestion.New(Factory);
			question1.CQ_LodgementQuestionIdentifier = 1;
			question1.CQ_LodgementQuestionStartDate = new ZDateTime(2006, 1, 9);
			collection = new CMRLodgementQuestionCollectionTestProxy(Factory);
			Factory.Save();
		}

		CMRLodgementQuestionCollectionTestProxy collection;
		CMRLodgementQuestion question1;

		sealed class CMRLodgementQuestionCollectionTestProxy : CMRLodgementQuestionCollection
		{
			public CMRLodgementQuestionCollectionTestProxy(BusinessObjectFactory factory) : base(factory)
			{
			}

			internal new IFindBoxListProvider FindBoxListProvider => base.FindBoxListProvider;
		}
	}
}
