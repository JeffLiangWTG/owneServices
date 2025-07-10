using CargoWise.Definitions;
using Enterprise.DocumentEngine.GUI;
using Enterprise.Rating.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class StartupInitEnterpriseUrlHandlerService : BackgroundApplicationStartupTask
	{
		public override int FailureExitCode => ExitCodes.StartupInitEnterpriseUrlHandlerServiceError;

		public override void DoExecute()
		{
			EnterpriseUrlHandlerService.RegisterUrlHandler(ReportUrlHandler.Instance);
			EnterpriseUrlHandlerService.RegisterUrlHandler(ShowReportUrlHandler.Instance);
			EnterpriseUrlHandlerService.RegisterUrlHandler(CustomizeDocumentsUrlHandler.Instance);
			EnterpriseUrlHandlerService.RegisterUrlHandler(CreateAutoratedJobUrlHandler.Instance);
			EnterpriseUrlHandlerService.RegisterUrlHandler(DataMappingUrlHandler.Instance);
			EnterpriseUrlHandlerService.Instance.RegisterRemotingServer();
		}
	}

	class PostLoginInitEnterpriseUrlHandlerService : IPostLoginTask
	{
		public string TaskDescription
		{
			get { return Res.GetString("3012c48c-bd78-4f8d-b4d9-3fc0e5474f46", "URL Handler Service"); }
		}

		public bool ShouldExecute()
		{
			return true;
		}

		public void Execute()
		{
			EnterpriseUrlHandlerService.Instance.RegisterInstance();
		}
	}
}
