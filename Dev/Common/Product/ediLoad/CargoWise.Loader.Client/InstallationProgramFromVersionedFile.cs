using System.IO;
using CargoWise.Common;

namespace CargoWise.Loader.Client
{
	public class InstallationProgramFromVersionedFile : InstallationProgramFromFile
	{
		public InstallationProgramFromVersionedFile(ClientInstallation installation)
			: base(installation)
		{
			Argument.NotNull(installation, nameof(installation));
		}

		public override string FullPathOfProgramToRun
		{
			get
			{
				return Path.Combine(Installation.Configuration.CurrentPackage, Installation.Configuration.ProgramFileName);
			}
		}
	}
}
