using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.DocumentWrappers.CDS.Testing
{
	[TestedType(typeof(DocCDSEntryLineWrapperCollection))]
	public class DocCDSEntryLineWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocCDSEntryLineWrapperCollection>
	{
		protected override System.Type GetExpectedCollectionType()
		{
			return typeof(DocCDSEntryLineWrapperCollection);
		}

		protected override DocCDSEntryLineWrapperCollection GetCollectionToTest()
		{
			return new DocCDSEntryLineWrapperCollection(Header);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocCDSEntryLineWrapper.New(Header.AllEntryLines.AddNew());
		}

		CusEntryHeader header;
		CusEntryHeader Header
		{
			get { return header ?? (header = Factory.New<CusEntryHeader>()); }
		}
	}
}
