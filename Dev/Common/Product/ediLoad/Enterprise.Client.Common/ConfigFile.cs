using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Common;

namespace Enterprise.Client.Common
{
	public sealed class ConfigFile
	{
		public const string AppManagerDirectoryOverrideIniName = "APPMANAGEROVERRIDE";
		public const string DbInstanceIniName = "INSTANCE";
		public const string DbNameIniName = "DATABASE";
		public const string DbServerIniName = "SERVER";
		public const string FileName = "ediLoad.ini";
		public const string EnterpriseInstanceIniName = "ENTERPRISEINSTANCE";
		public const string LegacyFileName = "ediLoad.exe.config";
		public const string MaintenanceMessageIniName = "MAINTENANCEMESSAGE";
		public const string OtherParametersIniName = "OTHERPARAMETERS";
		public const string GlowServerInitName = "GLOWSERVER";

		public ConfigFile(string directory)
		{
			Argument.NotNull(directory, nameof(directory));

			this.directory = directory;
			Parse();
		}

		public string AppManagerDirectoryOverride { get; private set; }
		public string DbInstance { get; private set; }
		public string DbName { get; private set; }
		public string DbServer { get; private set; }
		public string EnterpriseInstance { get; private set; }
		public string MaintenanceMessage { get; private set; }
		public string OtherParameters { get; private set; }
		public string GlowServer { get; private set; }
		readonly string directory;

		public bool Exists
		{
			get { return File.Exists(FilePath); }
		}

		string FilePath
		{
			get { return Path.Combine(directory, FileName); }
		}

		void Parse()
		{
			if (Exists)
			{
				Regex nameValuePair = new Regex(@"^\s*(\S*?)\s*=\s*(.*)");
				using (StreamReader configReader = File.OpenText(FilePath))
				{
					string line;
					while ((line = configReader.ReadLine()) != null)
					{
						line = line.Trim();
						if (line.StartsWith(";"))
						{
							// Ignore comment lines.
							continue;
						}
						Match match = nameValuePair.Match(line);
						if (match.Success)
						{
							string name = match.Groups[1].Value;
							string value = match.Groups[2].Value;
							switchName(name, value);
						}
					}
				}
			}
		}

		[SuppressMessage("Microsoft.Contracts", "TestAlwaysEvaluatingToAConstant", Justification = "ToUpperInvariant doesn't have any redundant code")]
		void switchName(string name, string value)
		{
			Argument.NotNull(name, nameof(name));

			switch (name.ToUpperInvariant())
			{
				case "SERVER":
					DbServer = value;
					break;
				case "INSTANCE":
					DbInstance = value;
					break;
				case "DATABASE":
					DbName = value;
					break;
				case "OTHERPARAMETERS":
					OtherParameters = value;
					break;
				case "MAINTENANCEMESSAGE":
					MaintenanceMessage = value;
					break;
				case "APPMANAGEROVERRIDE":
					AppManagerDirectoryOverride = value;
					break;
				case "ENTERPRISEINSTANCE":
					EnterpriseInstance = value;
					break;
				case GlowServerInitName:
					GlowServer = value;
					break;
			}
		}
	}
}
