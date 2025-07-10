using System.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace CargoWise.Setup.Services;

public interface IEdiUrlRegistration
{
	void RegisterUrlHandlers();
}

internal class EdiUrlRegistration(IWindowsRegistryProxy registryProxy, ILogger<EdiUrlRegistration> logger) : IEdiUrlRegistration
{
	public void RegisterUrlHandlers()
	{
		var cargowiseStartPath = Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432") ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WiseTech Global", "CargoWise", "CargoWise.Start.exe");

		logger.LogInformation("Registering edient protocol handler");
		SetHKLM(@"SOFTWARE\Classes\edient", "", "URL:edient Protocol");
		SetHKLM(@"SOFTWARE\Classes\edient", "URL Protocol", "");
		SetHKLM(@"SOFTWARE\Classes\edient\DefaultIcon", "", $"\"{cargowiseStartPath}\",0");
		SetHKLM(@"SOFTWARE\Classes\edient\shell\open\command", "", $"\"{cargowiseStartPath}\" \"%1\"");

		logger.LogInformation("Registering EdiEnterprise.edient protocol handler");
		SetHKLM(@"SOFTWARE\Classes\EdiEnterprise.edient", "", "URL:edient Protocol");
		SetHKLM(@"SOFTWARE\Classes\EdiEnterprise.edient", "URL Protocol", "");
		SetHKLM(@"SOFTWARE\Classes\EdiEnterprise.edient\DefaultIcon", "", $"\"{cargowiseStartPath}\",0");
		SetHKLM(@"SOFTWARE\Classes\EdiEnterprise.edient\shell\open\command", "", $"\"{cargowiseStartPath}\" \"%1\"");

		logger.LogInformation("Creating default program registration for edient");
		SetHKLM(@"SOFTWARE\RegisteredApplications", "Enterprise", @"Software\EdiEnterprise\Capabilities");
		SetHKLM(@"SOFTWARE\EdiEnterprise\Capabilities\", "ApplicationName", "EdiEnterprise");
		SetHKLM(@"SOFTWARE\EdiEnterprise\Capabilities\", "ApplicationIcon", $"{cargowiseStartPath},0");
		SetHKLM(@"SOFTWARE\EdiEnterprise\Capabilities\", "ApplicationDescription", "EdiEnterprise");
		SetHKLM(@"SOFTWARE\EdiEnterprise\Capabilities\UrlAssociations", "edi", "EdiEnterprise.edient");
		SetHKLM(@"SOFTWARE\EdiEnterprise\Capabilities\UrlAssociations", "edient", "EdiEnterprise.edient");
		SetHKLM(@"SOFTWARE\EdiEnterprise\Capabilities\FileAssociations", ".edient", "EdiEnterprise.edient");

		void SetHKLM(string path, string key, string value)
		{
			try
			{
				registryProxy.SetLocalMachineValue(RegistryView.Default, path, key, value);
			}
			catch (Exception ex) when(ex is UnauthorizedAccessException or SecurityException)
			{
				logger.LogError(ex, $"Could not set registry '{path}:{key}' due to lack of permission.");
			}
		}
	}
}
