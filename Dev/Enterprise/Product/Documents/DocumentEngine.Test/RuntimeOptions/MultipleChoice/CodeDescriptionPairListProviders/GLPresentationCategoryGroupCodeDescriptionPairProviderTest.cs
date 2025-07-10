using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class GLPresentationCategoryGroupCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new GLPresentationCategoryGroupCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			GLPresentationCategoryGroupCodeDescriptionPairProvider testPairProvider = new GLPresentationCategoryGroupCodeDescriptionPairProvider();

			Assert(true);
		}

		public void TestGetCodeDescriptionPairList()
		{
			var accountingMock = new Mock<IAccounting>();

			var list = new CodeDescriptionPairList();
			list.AddPair("GP1", "Group 1");
			list.AddPair("IOS", "Category 2");
			list.AddPair("DOC", "Category 2");
			list.AddPair("GP1,IOS,DOC", "Group type");
			accountingMock.Setup(m => m.GLPresentationJournalCategoriesGroupList).Returns(list);
			using (ObjectFactory.Substitute(accountingMock.Object))
			{
				Db.Connection.ExecuteNonQuery($@"
INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 900.0000, 200912, 'AD9AFCC2-52F8-493E-9D5D-3AD6300AE291', '27A55065-AC88-4EC3-8BED-E575E79172CB', '{GlbCompany.CurrentCompany.PK}', '86BB1C22-0865-4685-996E-D56CBD136491', 'LOV')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -300.0000, 201001, '318F6D36-6158-4B3A-8043-B81C996F2F36', '27A55065-AC88-4EC3-8BED-E575E79172CB', '{GlbCompany.CurrentCompany.PK}', '86BB1C22-0865-4685-996E-D56CBD136491', 'VOL')
");

				var provider = new GLPresentationCategoryGroupCodeDescriptionPairProvider();
				var groupCategoriesList = provider.GetCodeDescriptionPairList();
				AssertEquals(6, groupCategoriesList.Count);
				Assert(groupCategoriesList.ContainsCode("GP1"));
				Assert(groupCategoriesList.ContainsCode("IOS"));
				Assert(groupCategoriesList.ContainsCode("DOC"));
				Assert(groupCategoriesList.ContainsCode("GP1,IOS,DOC"));
				Assert(groupCategoriesList.ContainsCode("LOV"));
				Assert(groupCategoriesList.ContainsCode("VOL"));
			}
		}
	}
}
