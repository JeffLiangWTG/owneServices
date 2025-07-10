using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace WTG.TestHelpers.IISExpress
{
	public sealed class IISExpressHostedApplication : SimpleIISExpressHostedApplication
	{
		public static IISExpressHostedApplication CreateFromTestAssembly(Type httpApplicationType, int port, string[] extraAssembliesToCopy = null)
		{
			return CreateFromTestAssembly(httpApplicationType, port, null, extraAssembliesToCopy);
		}
		public static IISExpressHostedApplication CreateFromTestAssembly(Type httpApplicationType, int port, Stream webConfigContent = null, string[] extraAssembliesToCopy = null)
		{
			var tempDir = CreateTemporaryDirectory();
			WriteGlobalAsax(tempDir, httpApplicationType);
			WriteWebConfig(tempDir, webConfigContent, httpApplicationType);
			CopyApplicationAssemblyAndDependencies(tempDir, httpApplicationType);
			CopyExtraAssemblies(tempDir, httpApplicationType, extraAssembliesToCopy);

			return new IISExpressHostedApplication(port, tempDir);
		}

		IISExpressHostedApplication(int port, string path)
			: base(port, path)
		{
		}

		public static HttpClient CreateClient(HttpMessageHandler handler)
		{
			var client = new HttpClient(handler);
			try
			{
				ConfigureHttpClient(client);
			}
			catch
			{
				client.Dispose();
				throw;
			}

			return client;
		}

		public static HttpClient CreateClient()
		{
			var client = new HttpClient();
			try
			{
				ConfigureHttpClient(client);
			}
			catch
			{
				client.Dispose();
				throw;
			}

			return client;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && Directory.Exists(LocationOnDisk))
			{
				var retriesRemaining = 5;

				while (true)
				{
					try
					{
						// Delete the Bin folder junction first, if it is a junction.
						// Otherwise this can cause an UnauthorizedAccessException on the
						// recursive delete, below.
						var binDirectory = Path.Combine(LocationOnDisk, "Bin");

						if (Directory.Exists(binDirectory))
						{
							var binDirAttributes = File.GetAttributes(binDirectory);
							if ((binDirAttributes & FileAttributes.ReparsePoint) != 0)
							{
								Directory.Delete(binDirectory);
							}
						}

						Directory.Delete(LocationOnDisk, recursive: true);
						break;
					}
					catch (UnauthorizedAccessException) when (retriesRemaining > 0)
					{
						Thread.Sleep(TimeSpan.FromMilliseconds(100));
						retriesRemaining--;
					}
				}
			}
		}

		static void ConfigureHttpClient(HttpClient client)
		{
			// Give tested applications some time to spin up.
			client.Timeout = TimeSpan.FromMinutes(4);
		}

		static void CopyFileToDir(string file, string dir, string sourceDir = null)
		{
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			var fileName = Path.GetFileName(file);
			var destination = Path.Combine(dir, fileName);
			if (fileName == file && sourceDir != null)
			{
				file = Path.Combine(sourceDir, fileName);
			}
			File.Copy(file, destination);
		}

		static void RecursiveCopyAssemblyReferencesInDirectory(Assembly assembly, string sourceDirectory, string destinationDirectory)
		{
			foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
			{
				var assemblyName = referencedAssemblyName.Name + ".dll";
				var assemblyFilePath = Path.Combine(sourceDirectory, assemblyName);
				if (File.Exists(assemblyFilePath) && !File.Exists(Path.Combine(destinationDirectory, assemblyName)))
				{
					CopyFileToDir(assemblyFilePath, destinationDirectory);

					Assembly referencedAssembly;
#if NET
					var resolver = new PathAssemblyResolver([sourceDirectory]);
					using (var mlc = new MetadataLoadContext(resolver))
					{
						referencedAssembly = mlc.LoadFromAssemblyPath(assemblyFilePath);
					}
#else
					referencedAssembly = Assembly.ReflectionOnlyLoadFrom(assemblyFilePath);
#endif
					RecursiveCopyAssemblyReferencesInDirectory(referencedAssembly, sourceDirectory, destinationDirectory);
				}
			}
		}

		static string CreateTemporaryDirectory()
		{
			var tempDir = Path.Combine(Path.GetTempPath(), "WTGIntegrationTesting", "IIS", Path.GetRandomFileName());
			if (!Directory.Exists(tempDir))
			{
				Directory.CreateDirectory(tempDir);
			}
			return tempDir;
		}

		static void WriteGlobalAsax(string path, Type httpApplicationType)
		{
			using var fs = File.OpenWrite(Path.Combine(path, "Global.asax"));
			using var writer = new StreamWriter(fs);
			writer.WriteLine("<%@ Application Inherits=\"{0}\" %>", httpApplicationType.FullName);
			writer.Flush();
			fs.SetLength(fs.Position);
		}

		static void WriteWebConfig(string destinationDirectory, Stream webConfigContent, Type httpApplicationType = null)
		{
			if (webConfigContent == null && httpApplicationType == null)
			{
				throw new ArgumentNullException(nameof(webConfigContent), "One of 'webConfigContent' or 'httpApplicationType' must have a value.");
			}
			var destinationPath = Path.Combine(destinationDirectory, "web.config");
			using var resource = webConfigContent == null ? httpApplicationType.Assembly.GetManifestResourceStream("web.config") : null;
			var xml = XDocument.Load(resource ?? webConfigContent);
			xml = PrepareWebConfig(xml);
			var compilation = xml.Element("configuration").Element("system.web").Element("compilation");
			compilation.Add(new XAttribute("tempDirectory", Path.Combine(destinationDirectory, "temp")));
			using var writer = XmlWriter.Create(destinationPath);
			xml.WriteTo(writer);
		}

		static XDocument PrepareWebConfig(XDocument doc)
		{
			XElement customErrorsElement = doc.Element("configuration").Element("system.web").Element("customErrors");
			var errorMode = customErrorsElement?.Attribute("mode");
			if (errorMode != null)
			{
				errorMode.Value = "Off";
			}

			ReplaceServerName(doc);
			return doc;
		}
		static void ReplaceServerName(XDocument doc)
		{
			var serverNameElement = doc.Element("configuration").Element("appSettings").Elements("add").FirstOrDefault(e => e.Attribute("key").Value == "ServerName");
			if (serverNameElement != null && serverNameElement.Attribute("value").Value == ".")
			{
				serverNameElement.Attribute("value").Value = Environment.MachineName;
			}
		}

		static void CopyApplicationAssemblyAndDependencies(string path, Type applicationType)
		{
			var binDir = Path.Combine(path, "Bin");
			var assemblyPath = GetLocationOfAssembly(applicationType.Assembly);
			CopyFileToDir(assemblyPath, binDir);
			RecursiveCopyAssemblyReferencesInDirectory(applicationType.Assembly, Path.GetDirectoryName(assemblyPath), binDir);
		}

		static void CopyExtraAssemblies(string path, Type httpApplicationType, string[] extraAssembliesToCopy)
		{
			if (extraAssembliesToCopy == null)
			{
				return;
			}
			var sourceDir = Path.GetDirectoryName(GetLocationOfAssembly(httpApplicationType.Assembly));
			var binDir = Path.Combine(path, "Bin");
			foreach (var assembly in extraAssembliesToCopy)
			{
				CopyFileToDir(assembly, binDir, sourceDir);
			}
		}

		static string GetLocationOfAssembly(Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			var location = assembly.Location;
			var fileUri = new Uri(location, UriKind.Absolute);
			return fileUri.LocalPath;
		}
	}
}
