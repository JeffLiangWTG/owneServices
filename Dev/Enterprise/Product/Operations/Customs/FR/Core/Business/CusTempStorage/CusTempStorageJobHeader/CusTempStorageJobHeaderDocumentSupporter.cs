using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.FR.Business.CusTempStorage;

public class CusTempStorageJobHeaderDocumentSupporter : EU.Business.CusTempStorage.CusTempStorageJobHeaderDocumentSupporter
{
	public CusTempStorageJobHeaderDocumentSupporter(CusTempStorageJobHeader parentBusinessObject) : base(parentBusinessObject)
	{
	}

	protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun) => (dataContext is Constants.DataContext.TempStorageHeader) ? [DocumentWrapperHelper.GetFRSpecificDocumentWrapper(dataContext, BusinessObject)] : base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
}
