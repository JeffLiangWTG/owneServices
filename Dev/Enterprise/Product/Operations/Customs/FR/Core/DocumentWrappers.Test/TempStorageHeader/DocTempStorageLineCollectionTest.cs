using System.Linq;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.TempStorageHeader.Testing;

[TestedType(typeof(DocTempStorageLineCollection))]
sealed class DocTempStorageLineCollectionTest : DocBaseWrapperCollectionTest<DocTempStorageLineCollection>
{
	protected override object GetNewObjectToWrap()
	{
		return Header.CusTempStorageDec.CusTempStorageLines.First();
	}

	protected override DocTempStorageLineCollection GetNewDocumentWrapperCollection()
	{
		return DocTempStorageHeader.New(Header, Factory).Lines;
	}

	CusTempStorageJobHeader Header
	{
		get
		{
			if (header == null)
			{
				header = Factory.New<CusTempStorageJobHeader>();
				ISTCusTempStorageDec.New(header);
				header.CusTempStorageDec.CusTempStorageLines.AddNew();
			}
			return header;
		}
	}
	CusTempStorageJobHeader header;
}
