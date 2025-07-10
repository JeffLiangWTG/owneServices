using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Business
{
	public interface IStmMenuDocumentConfigSource
	{
		MenuEditingMode EditingMode { get; }
		BusinessObjectFactory Factory { get; }
		StmMenuTemplatePivot MenuTemplatePivot { get; }
		StmMenuDocumentConfig GetPersistentDocConfig();
		TemporaryStmMenuDocumentConfig GetTemporaryDocConfig(BusinessObjectFactory factory);
	}
}
