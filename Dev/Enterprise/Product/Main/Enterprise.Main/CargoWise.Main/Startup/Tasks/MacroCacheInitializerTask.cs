using CargoWise.Macros;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	sealed class MacroCacheInitializerTask : IPostLoginTask
	{
		public string TaskDescription => Res.GetString("5aa7aba9-6737-4a9b-a538-d3460750be2d", "Macro Cache Initializer");

		public void Execute()
		{
			if (RawDataRegistry.Instance.UseExpressionCachingForFormBuilderMacroEngine.Value)
			{
				MacroEngineSettings.Instance.EnableExpressionCaching();
			}
		}

		public bool ShouldExecute() => true;
	}
}
