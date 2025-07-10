using System;
using CargoWise.Types;

namespace Enterprise.DocumentScanning.Business
{
	public interface IBatchImportManagerInternals
	{
		bool ImportFileContent(DocumentFactory masterFactory, byte[] contents, ZString filenameOnly,
			ZString initialDocType, Guid initialCompanyPK, Guid initialBranchPK, Guid initialDepartmentPK, string fileFullPath = "");
	}
}
