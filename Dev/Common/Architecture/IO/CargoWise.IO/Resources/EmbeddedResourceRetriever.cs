using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CargoWise.IO
{
	public sealed class EmbeddedResourceRetriever : IDisposable
	{
		public EmbeddedResourceRetriever()
			: this(Assembly.GetCallingAssembly())
		{
		}

		public EmbeddedResourceRetriever(Assembly assembly)
		{
			this.assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
			DirectoryPath = Path.Combine(Temp.TempPath, Guid.NewGuid().ToString());
		}
		readonly Assembly assembly;

		public string GetString(string resourceName, Encoding encoding)
		{
			resourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
			using (var stream = GetStream(resourceName))
			{
				var result = string.Empty;
				using (var reader = encoding == null ? new StreamReader(stream, true) : new StreamReader(stream, encoding))
				{
					result = reader.ReadToEnd();
				}
				return result;
			}
		}

		public string GetString(string resourceName) => GetString(resourceName, null);

		public Stream GetStream(string resourceName)
		{
			var stream = assembly.GetManifestResourceStream(resourceName);
			if (stream == null)
			{
				var exceptionLines = new List<string>() { $"Missing resource [{resourceName}] in [{assembly.GetName().Name}]" };
#if DEBUG
				var first15ResourceNames = assembly.GetManifestResourceNames().Take(15).OrderBy(t => t).ToList();
				exceptionLines.Add($"First {first15ResourceNames.Count} resource name/s from this assembly:");
				exceptionLines.AddRange(first15ResourceNames);
#endif
				throw new IOException(string.Join(Environment.NewLine, exceptionLines));
			}
			return stream;
		}

		public byte[] GetBytes(string resourceName)
		{
			using (var str = GetStream(resourceName))
			{
				var buffer = new byte[str.Length];
				var result = str.Read(buffer, 0, (int)str.Length);
				if (result < str.Length)
				{
					throw new IOException("Read Failed for resource [" + resourceName + "]");
				}

				return buffer;
			}
		}

		public string DirectoryPath { get; }

		/// <summary>
		/// Saves an embedded resource to a local temp file
		/// </summary>
		/// <param name="resourceName">The name of the embedded resource to save to a local temp file</param>
		/// <param name="fileName">File name to write to</param>
		/// <param name="subDirectory">Optional. If exists, file will be written in this subdirectory. It will be created if not exists.</param>
		/// <returns>The full path of the temp file that has the resource saved</returns>
		public string SaveResourceToFile(string resourceName, string fileName, string subDirectory = default)
		{
			var workingDirectory = subDirectory == default ? DirectoryPath : Path.Combine(DirectoryPath, subDirectory);
			if (!Directory.Exists(workingDirectory))
			{
				Directory.CreateDirectory(workingDirectory);
			}
			var filePath = Path.Combine(workingDirectory, fileName);
			using var fileStream = File.Create(filePath);
			GetStream(resourceName).CopyTo(fileStream);

			return filePath;
		}

		public string SaveResourceToFile(string resourceName) => SaveResourceToFile(resourceName, resourceName);

		/// <summary>
		/// Save all the embedded resources in the assembly associated to this instance
		/// Each embedded resource is saved into a file in the same temp folder
		/// </summary>
		/// <returns>The full path of the temp directory containing the saved embedded resource files</returns>
		public string SaveAllResourcesToFiles()
		{
			var resources = assembly.GetManifestResourceNames();
			foreach (var resourceName in resources)
			{
				SaveResourceToFile(resourceName);
			}
			return DirectoryPath;
		}

		public void Dispose()
		{
			if (Directory.Exists(DirectoryPath))
			{
				Directory.Delete(DirectoryPath, recursive: true);
			}
		}
	}
}
