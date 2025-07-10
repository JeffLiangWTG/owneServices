using NUnit.Framework;
using static Enterprise.DocumentWrappers.DocLandedCostHeader;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(EmptyDocCusEntryHeaderCollection))]
	sealed class EmptyDocCusEntryHeaderCollectionTest : DocBaseWrapperCollectionTest<EmptyDocCusEntryHeaderCollection>
	{
		protected override EmptyDocCusEntryHeaderCollection GetNewDocumentWrapperCollection()
		{
			return new EmptyDocCusEntryHeaderCollection(Factory);
		}

		protected override object GetNewObjectToWrap()
		{
			return Factory.New<Enterprise.Customs.AU.Declaration.Business.CusEntryHeader>();
		}
	}
}
