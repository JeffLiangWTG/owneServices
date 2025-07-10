using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.DocumentWrappers.Testing
{
	[TestedType(typeof(ImportAccompanyingDocumentEntryLineWrapperCollection))]
	public class ImportAccompanyingDocumentEntryLineWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImportAccompanyingDocumentEntryLineWrapperCollection>
	{
		protected override System.Type GetExpectedCollectionType()
		{
			return typeof(ImportAccompanyingDocumentEntryLineWrapperCollection);
		}

		protected override ImportAccompanyingDocumentEntryLineWrapperCollection GetCollectionToTest()
		{
			return new ImportAccompanyingDocumentEntryLineWrapperCollection(Header);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Header;
			return ImportAccompanyingDocumentEntryLineWrapper.New(header, header.AllEntryLines.AddNew());
		}

		CusEntryHeader fHeader;
		CusEntryHeader Header
		{
			get { return fHeader ?? (fHeader = Factory.New<CusEntryHeader>()); }
		}
	}
}
