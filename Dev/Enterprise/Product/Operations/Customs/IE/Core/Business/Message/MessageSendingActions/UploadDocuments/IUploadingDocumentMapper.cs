using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.Business
{
	public interface IUploadingDocumentMapper
	{
		void AddData(AdditionalInfoSendingObject additionalInfoSendingObject, IEnumerable<IeDoc> documents);
	}
}
