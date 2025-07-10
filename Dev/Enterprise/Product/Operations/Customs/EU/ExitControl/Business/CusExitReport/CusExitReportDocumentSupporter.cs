using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitReportDocumentSupporter(CusExitReport exitHeader) : DocumentSupporter(exitHeader)
{
	public override BusinessContext BusinessContext => BusinessContext.CusExitReport;

	public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Environment.Env.Security.None;

	public override bool StorageDocsAreEditableIfInRelated => true;

	protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		=> DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);

	protected override Core.Constants.DataContext[] GetSupportedDataContexts() => [];
}
