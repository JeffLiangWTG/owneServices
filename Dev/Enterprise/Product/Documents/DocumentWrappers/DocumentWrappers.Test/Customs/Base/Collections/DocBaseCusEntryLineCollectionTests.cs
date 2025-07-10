using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestedType(typeof(DocBaseCusEntryLineCollection))]
	class DocBaseCusEntryLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocBaseCusEntryLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CusEntryLine cusEntryLine = EntryHeader.MergedLines.AddNew();
			return DocBaseCusEntryLineTestClass.New(cusEntryLine, Factory);
		}

		protected override DocBaseCusEntryLineCollection GetCollectionToTest()
		{
			return new DocBaseCusEntryLineCollection(Factory);
		}

		protected BaseJobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = Factory.New<BaseJobDeclaration>();
				}
				return fTestDec;
			}
		}
		BaseJobDeclaration fTestDec;

		protected CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = TestDec.CustomsEntryHeaders.AddNew();
				}
				return fEntryHeader;
			}
		}
		CusEntryHeader fEntryHeader;
	}
}
