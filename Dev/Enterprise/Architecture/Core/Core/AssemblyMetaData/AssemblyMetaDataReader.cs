using System.Collections.Generic;
using System.IO;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Xml;

// Assembly level attributes that inherit from AssemblyMetaDataAttribute are detected and saved to a file(s) by AssemblyMetaDataExtractor during the build. AssemblyMetaDataReader reads from the file(s) at runtime

namespace Enterprise.ZArchitecture
{
	public static class AssemblyMetaDataReader
	{
		public static IEnumerable<T> GetAttributes<T>(bool retrieveForAllClients = false) where T : AssemblyMetaDataAttribute
			=> AssemblyMetaDataReaderInstance.GetAttributes<T>(retrieveForAllClients);

		public static string[] AssemblyMetaDataFiles => AssemblyMetaDataReaderInstance.AssemblyMetaDataFiles;

		public static bool FilesExist => AssemblyMetaDataReaderInstance.FilesExist;

		static IAssemblyMetaDataReader AssemblyMetaDataReaderInstance => ObjectFactory.Get<IAssemblyMetaDataReader>();
	}

	class AssemblyMetaDataReaderImpl : IAssemblyMetaDataReader
	{
		IEnumerable<TAssemblyMetaDataAttribute> IAssemblyMetaDataReader.GetAttributes<TAssemblyMetaDataAttribute>(bool retrieveForAllClients)
			=> GetAttributes<TAssemblyMetaDataAttribute>(retrieveForAllClients);

		string[] IAssemblyMetaDataReader.AssemblyMetaDataFiles => AssemblyMetaDataFiles;

		bool IAssemblyMetaDataReader.FilesExist => AssemblyMetaDataFiles.Length > 0;

		IEnumerable<TAssemblyMetaDataAttribute> GetAttributes<TAssemblyMetaDataAttribute>(bool retrieveForAllClients) where TAssemblyMetaDataAttribute : AssemblyMetaDataAttribute
		{
			var serializer = ZXmlSerializer.New(typeof(TAssemblyMetaDataAttribute));

			var returnedAttributes = new HashSet<TAssemblyMetaDataAttribute>();

			foreach (var reader in GetXmlReaders())
			{
				using (reader)
				{
					while (reader.Read())
					{
						if (reader.NodeType == XmlNodeType.Element && reader.Name == typeof(TAssemblyMetaDataAttribute).Name)
						{
							var attribute = (TAssemblyMetaDataAttribute)serializer.Deserialize(reader);

							if (retrieveForAllClients ||
								attribute.ClientSpecificCode == Clients.None ||
								attribute.ClientSpecificCode == ClientHookLoader.Instance.Client)
							{
								if (returnedAttributes.Add(attribute))
								{
									yield return attribute;
								}
							}
						}
					}
				}
			}
		}

		IEnumerable<XmlReader> GetXmlReaders()
		{
			foreach (var assemblyMetaDataFile in AssemblyMetaDataFiles)
			{
				yield return XmlReader.Create(assemblyMetaDataFile);
			}
		}

		protected virtual string RootFolder => 
#if NETFRAMEWORK
			AssemblyLoader.GetBinPath();
#else // NetCore and Winzor assemblies are inside the net8.0/Winzor subfolder so we need parent folder
			AssemblyLoader.GetParentBinPath();
#endif

		string[] GetAssemblyMetaDataFiles()
		{
			return Directory.GetFiles(RootFolder, SubModuleAssemblyMetaDataFileNamePattern, SearchOption.TopDirectoryOnly);
		}

		const string SubModuleAssemblyMetaDataFileNamePattern = "*AssemblyMetaData.xml";

		string[] AssemblyMetaDataFiles => assemblyMetaDataFiles ??= GetAssemblyMetaDataFiles();
		string[] assemblyMetaDataFiles;
	}
}
