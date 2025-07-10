using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegHeaderDocumentSupporter : DocumentSupporter
{
	public CusTempStorageRegHeaderDocumentSupporter(BusinessObject parentBusinessObject) : base(parentBusinessObject)
	{
	}

	public override BusinessContext BusinessContext => BusinessContext.TempStorageRegHeader;

	public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.AdvanceFilingRulesCustomiseDocuments;

	protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun) => Array.Empty<DocumentWrapper>();

	protected override DataContext[] GetSupportedDataContexts() => new[] { DataContext.TempStorageRegHeader };
}
