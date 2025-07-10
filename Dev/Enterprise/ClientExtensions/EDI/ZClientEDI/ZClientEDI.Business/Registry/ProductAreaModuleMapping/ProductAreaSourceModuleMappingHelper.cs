using Enterprise.Client.EDI.IncidentManager.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.Registry.Business
{
	public static class ProductAreaSourceModuleMappingHelper
	{
		public static SourceModuleCollection SourceModules
		{
			get
			{
				if (sourceModules == null)
				{
					sourceModules = EDIDataRegistry.Instance.SourceModules.Value;
				}
				return sourceModules;
			}
		}

		[ThreadSafe]
		static SourceModuleCollection sourceModules;
	}
}
