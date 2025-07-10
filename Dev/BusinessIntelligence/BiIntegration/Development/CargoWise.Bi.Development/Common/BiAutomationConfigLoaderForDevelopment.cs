namespace CargoWise.Bi.Development.Common
{
	using System;
	using System.IO;
	using System.Text;
	using CargoWise.BuildTools;
	using Configuration;
	using Configuration.DataSets;
	using WTG.StaticAnalysis.Annotation;

	public class BiAutomationConfigLoaderForDevelopment
	{
		public static BiAutomationConfigLoaderForDevelopment Instance
		{
			get
			{
				return instance ?? (instance = new BiAutomationConfigLoaderForDevelopment());
			}
		}

		[ThreadSafe]
		static BiAutomationConfigLoaderForDevelopment instance;

		public bool Loaded
		{
			get { return configurationDataSet != null; }
		}

		public BiAutomationConfigDataSet ConfigData
		{
			get
			{
				if (configurationDataSet == null)
				{
					var auxDataSet = new BiAutomationConfigDataSet();
					LoadBiConfiguration(auxDataSet);
					configurationDataSet = auxDataSet;
					configurationDataSet.SortModelDependencyOrder();
				}

				return configurationDataSet;
			}
		}

		static void LoadBiConfiguration(BiAutomationConfigDataSet auxDataSet)
		{
			var biConfigFileTypes = Enum.GetValues(typeof(BiConfigFileType));
			foreach (BiConfigFileType biConfigFileType in biConfigFileTypes)
			{
				using (BiConfigurationFileHandler.CombineConfigFiles(biConfigFileType, out var fileContents))
				{
					byte[] byteArray = Encoding.UTF8.GetBytes(fileContents);
					BiConfiguration.LoadConfigurationXml(auxDataSet, byteArray, clearRows: false);
				}
			}
		}

		BiAutomationConfigDataSet configurationDataSet;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filesystem path segments")]
		public string BiConfigDirectory
		{
			get
			{
				return
					biConfigDirectoryOverride ??
					Path.Combine(BuildConstants.LocalEnterprisePath, "Database", "BusinessIntelligence", "ConfigLoader", "ConfigLoader", "Config");
			}
		}

		public IDisposable ResetConfiguration(string biConfigDirectory = null)
		{
			biConfigDirectoryOverride = biConfigDirectory;
			configurationDataSet = null;

			return new ResetConfigurationOverrideAction(this, (biConfigDirectory != null));
		}

		string biConfigDirectoryOverride;

		sealed class ResetConfigurationOverrideAction : IDisposable
		{
			public ResetConfigurationOverrideAction(BiAutomationConfigLoaderForDevelopment configLoader, bool needReset)
			{
				disposeAction = (needReset)
					? new Action(() => configLoader.ResetConfiguration())
					: new Action(() => { });
			}

			public void Dispose()
			{
				disposeAction();
			}
			readonly Action disposeAction;
		}
	}
}
