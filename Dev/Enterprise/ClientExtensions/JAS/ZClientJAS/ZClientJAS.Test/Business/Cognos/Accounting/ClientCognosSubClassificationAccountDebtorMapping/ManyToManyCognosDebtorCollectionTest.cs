using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	[TestedType(typeof(ManyToManyCognosDebtorCollection))]
	class ManyToManyCognosDebtorCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestElementsAreOrgDebtorGroup()
		{
			CognosAccGLAccountDescriptorExtraInfo extraInfo = Factory.New<CognosAccGLAccountDescriptorExtraInfo>();
			OrgDebtorGroup debtor1 = Factory.New<OrgDebtorGroup>();
			CognosDebtorMapping mapping1 = Factory.New<CognosDebtorMapping>();
			mapping1.T8_T9 = extraInfo.PK;
			mapping1.T8_OJ = debtor1.PK;
			OrgDebtorGroup debtor2 = Factory.New<OrgDebtorGroup>();
			CognosDebtorMapping mapping2 = Factory.New<CognosDebtorMapping>();
			mapping2.T8_T9 = extraInfo.PK;
			mapping2.T8_OJ = debtor2.PK;
			ManyToManyCognosDebtorCollection collection = new ManyToManyCognosDebtorCollection(extraInfo);
			collection.Load();
			AssertEquals(2, collection.Count);
			Assert(collection.Contains(debtor1.PK));
			Assert(collection.Contains(debtor2.PK));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ManyToManyCognosDebtorCollection(Factory.New<CognosAccGLAccountDescriptorExtraInfo>());
		}
	}
}
