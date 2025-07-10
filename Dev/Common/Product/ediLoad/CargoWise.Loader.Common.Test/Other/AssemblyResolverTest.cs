using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing.Other
{
	class AssemblyResolverTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestLoadURLHandlerIntegrationAssembly_EmbeddedResourceAvailable_FileCanBeDeleted()
		{
			var urlHandlerIntegrationFile = AssemblyResolver.URLHandlerIntegrationAssemblyName + ".dll";
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var tempPath = Path.Combine(binPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempPath);

			using (new DisposableAction(() => Directory.Delete(tempPath, true)))
			{
				// Arrange
				File.Copy(Path.Combine(binPath, urlHandlerIntegrationFile), Path.Combine(tempPath, urlHandlerIntegrationFile));
				Assert(File.Exists(Path.Combine(tempPath, urlHandlerIntegrationFile)));

				// Act
				var urlHandlerIntegrationAssembly = AssemblyResolver.LoadURLHandlerIntegrationAssembly();

				// Assert
				AssertNotNull(urlHandlerIntegrationAssembly);
			}
		}

		[ExpectNoExceptions]
		public void TestAssemblyLoadFromResource_NotEmbedded_ReturnsNull()
		{
			// Arrange
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var cargoWiseStartAssembly = Assembly.LoadFile(Path.Combine(binPath, "CargoWise.Start.exe"));
			var assemblyName = Guid.NewGuid().ToString();

			// Act
			// Assert
			AssertNull(AssemblyResolver.LoadFromResource(cargoWiseStartAssembly, assemblyName));
		}

		public void TestAssembliesLoadedFromResources_AreSameAsOriginalEmbeddedAssemblyFiles()
		{
			CombineAssertions("Assemblies are loaded from the embedded resource are exactly the same as loaded from the embedded physical files", () =>
			{
				GetEmbeddedAssemblyNames().ToList().ForEach(
					TestAssembliesAreSame_LoadedFromResource_Or_LoadedFromPhysicalFile);
			});
		}

		void TestAssembliesAreSame_LoadedFromResource_Or_LoadedFromPhysicalFile(string assemblyName)
		{
			// Arrange
			var temp = Temp.TempPath;
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var assemblyLoadedFromFile = Assembly.LoadFile(Path.Combine(binPath, assemblyName + ".dll"));
			var cargoWiseStartAssembly = Assembly.LoadFile(Path.Combine(binPath, "CargoWise.Start.exe"));
			var assemblyLoadedFromResource = default(Assembly);
			var oldVersionAssemblyFile = Path.Combine(temp, assemblyName + ".dll");
			var oldVersionAssemblyFileContent = string.Empty;

			File.WriteAllText(oldVersionAssemblyFile, assemblyName, System.Text.Encoding.UTF8);
			Assert(File.Exists(oldVersionAssemblyFile));

			// Act
			using (new DisposableAction(() => File.Delete(oldVersionAssemblyFile)))
			{
				assemblyLoadedFromResource = AssemblyResolver.LoadFromResource(cargoWiseStartAssembly, assemblyName);
				oldVersionAssemblyFileContent = File.ReadAllText(oldVersionAssemblyFile, System.Text.Encoding.UTF8);
			}

			// Assert
			AssertNotNull(assemblyLoadedFromFile);
			AssertNotNull(assemblyLoadedFromResource);
			Assert(AssembliesAreEqual(assemblyLoadedFromFile, assemblyLoadedFromResource));
			Assert(string.Equals(assemblyName, oldVersionAssemblyFileContent));
		}

		static bool AssembliesAreEqual(Assembly assemblyA, Assembly assemblyB)
		{
			if (assemblyA == null || assemblyB == null)
			{
				return false;
			}

			using (var streamA = new MemoryStream())
			using (var streamB = new MemoryStream())
			{
				var jsonWriterA = new JsonTextWriter(new StreamWriter(streamA));
				var jsonWriterB = new JsonTextWriter(new StreamWriter(streamB));
				var jsonSerializer = new JsonSerializer();
				jsonSerializer.Serialize(jsonWriterA, assemblyA);
				jsonSerializer.Serialize(jsonWriterB, assemblyB);
				jsonWriterA.Flush();
				jsonWriterB.Flush();

				return Enumerable.SequenceEqual(streamA.ToArray(), streamB.ToArray());
			}
		}

		IEnumerable<string> GetEmbeddedAssemblyNames()
		{
			return new[]
			{
				AssemblyResolver.ApplicationManagerCommonAssemblyName,
				AssemblyResolver.URLHandlerIntegrationAssemblyName,
			};
		}
	}
}
