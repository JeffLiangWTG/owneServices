using System.Linq;
using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitHeaderDocumentSupporter(CusExitHeader exitHeader) : DocumentSupporter(exitHeader)
{
	protected CusExitHeader ExitHeader => (CusExitHeader)BusinessObject;

	public override BusinessContext BusinessContext => BusinessContext.CusExitHeader;

	public override BusinessContext[] SupportedChildBusinessContexts => [BusinessContext.CusExitHeader, BusinessContext.CusExitReport];

	public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menu, BusinessContext businessContext, IStmMenuItem childCommandBeingRun)
	{
		IDocumentSupportable[] result = null;

		switch (businessContext)
		{
			case BusinessContext.CusExitHeader:
				result = [ExitHeader];
				break;
			case BusinessContext.CusExitReport:
				var cusExitReports = ExitHeader.CusExitReports;
				result = [.. cusExitReports.Cast<IDocumentSupportable>()];
				break;
		}

		return result;
	}

	public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Environment.Env.Security.None;

	protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		=> DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);

	protected override Core.Constants.DataContext[] GetSupportedDataContexts() => [];
}
