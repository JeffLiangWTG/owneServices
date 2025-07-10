using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using CargoWise.Common;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace CargoWise.BuildTools
{
	#region class BaseSourcePathNotFoundException

	[Serializable]
	public sealed class BaseSourcePathNotFoundException : Exception
	{
		public BaseSourcePathNotFoundException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		public BaseSourcePathNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	#endregion

	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer only tool")]
	public static class BuildConstants
	{
		public const string CargoWiseOneExeForVersionInfo = "CargoWiseOne.exe";

		[SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Consumed tests have SOURCE_CODE")]
		public static string GetClientDocumentXmlPath(string clientCode)
		{
			Argument.NotNullOrEmpty(clientCode, nameof(clientCode));
			string result = Path.Combine(LocalEnterprisePath, @"Enterprise\ClientExtensions");
			result = Path.Combine(result, clientCode);
			result = Path.Combine(result, "Documents");
			result = Path.Combine(result, clientCode + "Documents.xml");
			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Consumed tests have SOURCE_CODE")]
		public static string[] GetClientDocumentXmlPaths()
		{
			List<string> result = new List<string>();

			var retryPolicy = new RetryPolicy<ErrorDetectionStrategy>(new FixedInterval(3, TimeSpan.FromSeconds(15)));
			var directories = retryPolicy.ExecuteAction(() =>
			{
				return Directory.GetDirectories(Path.Combine(LocalEnterprisePath, @"Enterprise\ClientExtensions"));
			});

			foreach (string directory in directories)
			{
				string clientCode = Path.GetFileName(directory);
				string xmlFileName = GetClientDocumentXmlPath(clientCode);
				if (File.Exists(xmlFileName))
				{
					result.Add(xmlFileName);
				}
			}
			return result.ToArray();
		}

		class ErrorDetectionStrategy : ITransientErrorDetectionStrategy
		{
			public bool IsTransient(Exception ex)
			{
				return ex is IOException;
			}
		}

		public static string GetLocalEnterprisePath(string fileName)
		{
			Argument.NotNullOrEmpty(fileName, nameof(fileName));
			return GetLocalEnterprisePath(fileName, true);
		}

		public static string GetLocalEnterprisePath(string fileName, bool throwOnError)
		{
			Argument.NotNullOrEmpty(fileName, nameof(fileName));
			string localPath = Path.GetFullPath(Path.GetDirectoryName(fileName));
			bool success = false;

			do
			{
				if (File.Exists(Path.Combine(localPath, FileExpectedAtRootSourceTree)))
				{
					success = true;
					break;
				}

				localPath = Path.GetDirectoryName(localPath);
			}
			while (!string.IsNullOrEmpty(localPath));

			if (success)
			{
				if (!localPath.EndsWith("\\"))
				{
					localPath += "\\";
				}

				return localPath;
			}
			else if (throwOnError)
			{
				throw new BaseSourcePathNotFoundException("Could not determine the local source path using the filename '" + fileName + "'.");
			}
			else
			{
				return null;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Consumed tests have SOURCE_CODE")]
		public static string GetLocalPath(string relativeFilePath)
		{
			Argument.NotNullOrEmpty(relativeFilePath, nameof(relativeFilePath));
			var result = Path.Combine(LocalEnterprisePath, relativeFilePath);
			return result;
		}

		public static string GetServerPath(string relativeFilePath)
		{
			Argument.NotNullOrEmpty(relativeFilePath, nameof(relativeFilePath));
			string formattedRelativeFilePath = relativeFilePath;

			if (!formattedRelativeFilePath.StartsWith("/"))
			{
				formattedRelativeFilePath = '/' + formattedRelativeFilePath;
			}

			var result = ServerEnterprisePath + formattedRelativeFilePath;
			return result;
		}

		// This property is referenced via reflection in NUnit.

		[SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Consumed tests have SOURCE_CODE")]
		public static string LocalEnterprisePath
		{
			get
			{
#if DEBUG
				if (WTG.TestHelpers.TestingState.IsRunningOnDAT)
				{
					return WTG.TestHelpers.TestCase.BaseSourcePath;
				}
#endif
				if (localEnterprisePath == null)
				{
					localEnterprisePath = LocalEnterprisePathHelper();
				}
				return localEnterprisePath;
			}
			set
			{
				Argument.NotNullOrEmpty(value, nameof(value));
				localEnterprisePath = value;
			}
		}

		static string LocalEnterprisePathHelper()
		{
			return GetLocalEnterprisePath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BuildTools.dll"));
		}

		public static bool LocalSourcePathAvailable
		{
			get
			{
				try
				{
					return !string.IsNullOrEmpty(BuildConstants.LocalEnterprisePath);
				}
				catch (BaseSourcePathNotFoundException)
				{
					return false;
				}
			}
		}

		public static string ServerEnterprisePath
		{
			get
			{
#if DEBUG
				if (WTG.TestHelpers.TestingState.IsRunningOnDAT)
				{
					return "$/Dummy";
				}
#endif
				return serverEnterprisePath;
			}
		}

		const string MainDbSchemaFileName = "Schema_Main.sql";
		const string DocManagerDbSchemaFileName = "Schema_DocManager.sql";

		static string DbSchemaFolderWindowsPath
		{
			get
			{
				return GetLocalPath(@"Database\Odyssey\Resource\Resource\AutoGenerated\");
			}
		}

		public static string MainDbSchemaFileWindowsPath
		{
			get
			{
				return DbSchemaFolderWindowsPath + MainDbSchemaFileName;
			}
		}

		public static string DocManagerDbSchemaFileWindowsPath
		{
			get
			{
				return DbSchemaFolderWindowsPath + DocManagerDbSchemaFileName;
			}
		}

		static string DbSchemaFolderSourceControlPath
		{
			get
			{
				return GetServerPath("Database/Odyssey/Resource/Resource/AutoGenerated/");
			}
		}

		public static string MainDbSchemaFileSourceControlPath
		{
			get
			{
				return DbSchemaFolderSourceControlPath + MainDbSchemaFileName;
			}
		}

		public static string SchemaProjectPath
		{
			get
			{
				return GetLocalPath(@"Database\Odyssey\Schema\Schema\Enterprise.ZArchitecture.Schema.csproj");
			}
		}

		public const string BuildXmlFileName = "Build.xml";
		public const string SolutionXmlFileName = "Solutions.xml";
		public const string BusinessObjectsXmlFileName = "BusinessObjects.xml";

		public const string FileExpectedAtRootSourceTree = "CommonAssemblyInfo.cs";

		public const string ReleaseInfoXmlFileName = "ReleaseInfo.xml";
		public const string ReleaseInfoXmlFilePath = @"Enterprise\Product\Main\Enterprise.Main\" + ReleaseInfoXmlFileName;

		public static readonly DateTime BaseReleaseDateCw1Obsolete = new DateTime(2013, 8, 8);

		#region Implementation

		[ThreadStatic]
		static string localEnterprisePath;

		[ThreadStatic]
		static string serverEnterprisePath;

		#endregion
	}
}
