using System;
using System.IO;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Environment
{
	public class InstallationEnvironment : MarshalByRefObject
	{
		InstallationEnvironment() { }
		public static InstallationEnvironment Instance { get { return new InstallationEnvironment(); } }

		public string GetTargetInstallPath(VersionNumber version)
		{
			return Path.Combine(BaseInstallPath, version.ToString());
		}

		public string BaseInstallPath
		{
			get
			{
				var result = OverridableGetBinPath.Value();
				Version unused = null;

				if (Version.TryParse(Path.GetFileName(result), out unused))
				{
					result = Path.GetDirectoryName(result);
				}

				return result;
			}
		}

		internal static readonly Overridable<Func<string>> OverridableGetBinPath = new Overridable<Func<string>>(AssemblyLoader.GetBinPath);
	}
}
