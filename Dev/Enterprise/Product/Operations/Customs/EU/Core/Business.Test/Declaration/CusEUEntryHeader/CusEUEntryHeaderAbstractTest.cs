using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(CusEUEntryHeader))]
	public abstract class CusEUEntryHeaderAbstractTest<T> : IAddInfoChildUniqueIndexFailureHandlerSupporterTestCase<T> where T : CusEUEntryHeader
	{
		protected override string ExpectedUniqueIndexName => ZArchitecture.Schema.CusEUEntryHeaderSchema.Constants.Indexes.FK_UX__EUH_CH;

		protected override EnterpriseBusinessObject GetParent(T bizObj) => bizObj.EntryHeader;

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return entryHeader.AddInfoChild;
		}
	}
}
