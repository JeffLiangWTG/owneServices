using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	[TestedType(typeof(ManyToManyDebtorExtraInfoCollection))]
	class ManyToManyDebtorExtraInfoCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestElementsAreOrgDebtorGroup()
		{
			JASOrgDebtorGroup debtor = Factory.New<JASOrgDebtorGroup>();
			CognosAccGLAccountDescriptorExtraInfo extraInfo1 = Factory.New<CognosAccGLAccountDescriptorExtraInfo>();
			CognosDebtorMapping mapping1 = Factory.New<CognosDebtorMapping>();
			mapping1.T8_T9 = extraInfo1.PK;
			mapping1.T8_OJ = debtor.PK;
			CognosAccGLAccountDescriptorExtraInfo extraInfo2 = Factory.New<CognosAccGLAccountDescriptorExtraInfo>();
			CognosDebtorMapping mapping2 = Factory.New<CognosDebtorMapping>();
			mapping2.T8_T9 = extraInfo2.PK;
			mapping2.T8_OJ = debtor.PK;
			ManyToManyDebtorExtraInfoCollection collection = new ManyToManyDebtorExtraInfoCollection(debtor);
			collection.Load();
			AssertEquals(2, collection.Count);
			Assert(collection.Contains(extraInfo1.PK));
			Assert(collection.Contains(extraInfo2.PK));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ManyToManyDebtorExtraInfoCollection(Factory.New<JASOrgDebtorGroup>());
		}
	}
}
