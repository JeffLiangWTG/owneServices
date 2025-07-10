using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	[TestedType(typeof(ManyToManyCreditorExtraInfoCollection))]
	class ManyToManyCreditorExtraInfoCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestElementsAreOrgCreditorGroup()
		{
			JASOrgCreditorGroup creditor = Factory.New<JASOrgCreditorGroup>();
			CognosAccGLAccountDescriptorExtraInfo extraInfo1 = Factory.New<CognosAccGLAccountDescriptorExtraInfo>();
			CognosCreditorMapping mapping1 = Factory.New<CognosCreditorMapping>();
			mapping1.T7_T9 = extraInfo1.PK;
			mapping1.T7_OG = creditor.PK;
			CognosAccGLAccountDescriptorExtraInfo extraInfo2 = Factory.New<CognosAccGLAccountDescriptorExtraInfo>();
			CognosCreditorMapping mapping2 = Factory.New<CognosCreditorMapping>();
			mapping2.T7_T9 = extraInfo2.PK;
			mapping2.T7_OG = creditor.PK;
			ManyToManyCreditorExtraInfoCollection collection = new ManyToManyCreditorExtraInfoCollection(creditor);
			collection.Load();
			AssertEquals(2, collection.Count);
			Assert(collection.Contains(extraInfo1.PK));
			Assert(collection.Contains(extraInfo2.PK));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ManyToManyCreditorExtraInfoCollection(Factory.New<JASOrgCreditorGroup>());
		}
	}
}
