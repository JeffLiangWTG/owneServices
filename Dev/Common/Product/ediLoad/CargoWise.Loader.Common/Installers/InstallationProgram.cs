using CargoWise.Common;

namespace CargoWise.Loader.Common
{
	public class InstallationProgram : InstallationProgramBase
	{
		string fullPathOfProgramToRun;

		public InstallationProgram(Installation installation)
			: base(installation)
		{
		}

		public override string FullPathOfProgramToRun
		{
			get { return fullPathOfProgramToRun; }
		}

		public void SetFullPathOfProgramToRun(string value)
		{
			Argument.NotNullOrEmpty(value, nameof(value));
			fullPathOfProgramToRun = value;
		}
	}
}

