using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	[TestedType(typeof(ManyToManyCognosCreditorCollection))]
	class ManyToManyCognosCreditorCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestElementsAreOrgCreditorGroup()
		{
			CognosAccGLAccountDescriptorExtraInfo extraInfo = Factory.New<CognosAccGLAccountDescriptorExtraInfo>();
			JASOrgCreditorGroup creditor1 = Factory.New<JASOrgCreditorGroup>();
			CognosCreditorMapping mapping1 = Factory.New<CognosCreditorMapping>();
			mapping1.T7_T9 = extraInfo.PK;
			mapping1.T7_OG = creditor1.PK;
			JASOrgCreditorGroup creditor2 = Factory.New<JASOrgCreditorGroup>();
			CognosCreditorMapping mapping2 = Factory.New<CognosCreditorMapping>();
			mapping2.T7_T9 = extraInfo.PK;
			mapping2.T7_OG = creditor2.PK;
			ManyToManyCognosCreditorCollection collection = new ManyToManyCognosCreditorCollection(extraInfo);
			collection.Load();
			AssertEquals(2, collection.Count);
			Assert(collection.Contains(creditor1.PK));
			Assert(collection.Contains(creditor2.PK));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ManyToManyCognosCreditorCollection(Factory.New<CognosAccGLAccountDescriptorExtraInfo>());
		}
	}
}
