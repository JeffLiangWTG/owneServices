using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using CargoWise.ApplicationManager.Common;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.AppManagerUpgrade
{
	[CodeAlive("Used code in AppManagerInstaller.cs")]
	public class AppManagerUpgradeDetailsProvider : IAppManagerUpgradeDetailsProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Argument string")]
		public AppManagerUpgradeDetails GetDetails(object state)
		{
			string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CargoWiseAppManagerSetup.msi");
			return new AppManagerUpgradeDetails()
			{
				FilePath = file,
				Arguments = " /qn",
				Version = new Version(2, 3, 0),
				Sha1HashInBase64 = GetHash(file),
			};
		}

		string GetHash(string file)
		{
			Argument.NotNullOrEmpty(file, nameof(file));

			using (SHA1 sha1 = SHA1.Create())
			using (FileStream stream = File.OpenRead(file))
			{
				return Convert.ToBase64String(sha1.ComputeHash(stream));
			}
		}
	}
}
