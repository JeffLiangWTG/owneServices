using System.IO;
using CargoWise.Main.Startup.Tools.JetBrains;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Main.Startup.Tools
{
	public interface IDialogService
	{
		bool AcceptLicenseAgreement(string license);
		string Download(string fileUrl);
		string SelectFolder(string defaultFolder, string description);
		Stream SelectSaveAs(string fileName);
		void ProfilePerformance(ProfilePerformanceModel model);
		void ProfileMemory(ProfileMemoryModel model);
		IZFolderBrowserDialog CreateSelectFolderDialog();
		bool CheckFolderAccess(string path);
	}
}
