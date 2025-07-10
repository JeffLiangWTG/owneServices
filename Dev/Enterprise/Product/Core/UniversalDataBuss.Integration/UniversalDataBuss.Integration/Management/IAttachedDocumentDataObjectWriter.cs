using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IAttachedDocumentDataObjectWriter
	{
		IAttachedDocument[] GenerateAttachedDocuments(bool metaDataOnly, params IeDocBase[] eDocs);
		IAttachedDocument GenerateAttachedDocument(bool metaDataOnly, IeDoc eDoc, IEnumerable<KeyValuePair<TypeWithDescription, IZType>> contextValues);
	}
}
