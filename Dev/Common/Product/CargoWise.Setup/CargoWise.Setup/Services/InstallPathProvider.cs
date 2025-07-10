using System.Reflection;

namespace CargoWise.Setup.Services;

public interface IInstallPathProvider
{
	string GetInstallPath();
	string GetCurrentVersionFilePath();
	string GetVersionInfoFromDirectory();
}

internal class InstallPathProvider : IInstallPathProvider
{
	public string GetInstallPath() =>
		// Assumes the executing assembly is in the net8/ directory of the installation package, returns the path to the installation package root
		Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "")?.FullName ?? throw new DirectoryNotFoundException();

	// Returns the root of all cargowise version installations
	// e.g. CargoWise is installed to C:/Program Files/WiseTech Global/CargoWise/25.03.26.123/... then this would return C:/Program Files (x86)/WiseTech Global/CargoWise/
	// which is where CargoWise.Start.exe and CurrentVersion are located
	string GetInstallationRoot() => Directory.GetParent(GetInstallPath())?.FullName ?? throw new DirectoryNotFoundException();

	public string GetCurrentVersionFilePath() => Path.Combine(GetInstallationRoot(), "CurrentVersion");

	public string GetVersionInfoFromDirectory() => new DirectoryInfo(GetInstallPath()).Name;
}
