using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace TestRunner
{
	public static class Runner
	{
		class NotificationHandler : INotificationHandler
		{
			void INotificationHandler.ReportInformation(string message, string caption)
			{
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ShowError(message, caption);
			}

			void INotificationHandler.ReportError(string message, string caption, string errorContext, Exception exception)
			{
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ShowError(message, caption);
			}

			internal static NotificationHandler Instance = new NotificationHandler();
		}

		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Enterprise.Initialisation.Initialiser.InitialiseWinForms();

			CargoWise.EntityFramework.NotificationHandler.Instance = NotificationHandler.Instance;

			string configFile = Path.Combine(TestCase.BaseSourcePath, @"TestRunner.config");
			string assemblyName = null;
			string solutionName = null;
			string serverName = null;
			string databaseName = null;
			bool showTestsForCheckedOutFiles = false;
			if (args.Length > 0)
			{
				List<string> argList = new List<string>();
				argList.AddRange(args);

				int configIndex = argList.IndexOf("-Config");
				if (configIndex >= 0)
				{
					configFile = argList[configIndex + 1];
				}

				int assemblyIndex = argList.IndexOf("-Assembly");
				if (assemblyIndex >= 0)
				{
					assemblyName = argList[assemblyIndex + 1];
				}

				int solutionIndex = argList.IndexOf("-Solution");
				if (solutionIndex >= 0)
				{
					solutionName = argList[solutionIndex + 1];
				}

				int checkedOutIndex = argList.IndexOf("-CheckedOutFiles");
				if (checkedOutIndex >= 0)
				{
					showTestsForCheckedOutFiles = true;
				}

				int serverNameIndex = argList.IndexOf("-Server");
				if (serverNameIndex >= 0)
				{
					serverName = argList[serverNameIndex + 1];
				}

				int databaseNameIndex = argList.IndexOf("-Database");
				if (databaseNameIndex >= 0)
				{
					databaseName = argList[databaseNameIndex + 1];
				}
			}

			Db.InitializeDatabaseDetails(serverName, databaseName);

			Env.LoginController.LoginLocationAutomatically(Env.LoginController.LoginUserDeveloper());
			ErrorDescriptionList errorDescriptions = new ErrorDescriptionList();

			if (showTestsForCheckedOutFiles)
			{
				UnitTestRunner.ShowTestsFromCheckedOutFiles(false);
			}
			else if (assemblyName != null)
			{
				List<string> assemblyNames = new List<string>();
				assemblyNames.AddRange(assemblyName.Split(','));
				UnitTestRunner.ShowUnitTestsFromAssemblies(assemblyNames, assemblyName, errorDescriptions, true);
			}
			else if (solutionName == null && File.Exists(configFile))
			{
				UnitTestRunner.ShowUnitTestsFromAssemblies(AssembliesFromConfigFile(configFile), null, errorDescriptions);
			}
			else
			{
				UnitTestRunner.ShowUnitTestsInSolution(Path.GetFileNameWithoutExtension(solutionName), errorDescriptions);
			}
			Env.LoginController.Logout();
		}

		static List<string> AssembliesFromConfigFile(string configFile)
		{
			List<string> result = new List<string>();
			XmlDocument config = new XmlDocument();
			config.Load(configFile);

			foreach (XmlNode node in config.SelectNodes("descendant::Assembly"))
			{
				result.Add(node.Attributes["Name"].Value);
			}

			return result;
		}
	}
}
