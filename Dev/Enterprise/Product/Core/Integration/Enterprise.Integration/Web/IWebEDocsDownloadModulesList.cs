using CargoWise.Integration;

namespace Enterprise.Integration.Web
{
	public interface IWebEDocsDownloadModulesList
	{
		ICodeDescription AllModules { get; }
		ICodeDescriptionPairList ModulesCodeDescriptionPairList { get; }
	}
}
