using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.Registry.Business.Internal.Indexing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Environment.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	class RegistryProvider : IRegistryProvider
	{
		public RegistryProvider()
			: this(ObjectFactory.Get<IRegistryItemSetLocator>(), new DefaultRegistryRelatedFileLocator())
		{
		}

		internal RegistryProvider(IRegistryItemSetLocator setLocator, IRegistryRelatedFilesLocator registryRelatedFileLocator)
		{
			this.registryRelatedFileLocator = registryRelatedFileLocator;
			sets = new Lazy<IReadOnlyDictionary<Type, RegistryItemSet>>(() =>
			{
				return setLocator.GetRegistryItemSets().ToDictionary(s => s.GetType());
			}, LazyThreadSafetyMode.ExecutionAndPublication);

			staticIndex = new Lazy<RegistryStaticIndex>(() =>
			{
				var version = CalculateRegistryVersion();
				if (TryLoadIndex(version, out var index))
				{
					return index;
				}

				index = RegistryDiscovery.DiscoverStaticItems(sets.Value);
				StoreIndex(index, version);

				return index;
			}, LazyThreadSafetyMode.ExecutionAndPublication);

			staticCategoriesCache = new Dictionary<RegistryCategory, bool>();
		}

		static void StoreIndex(RegistryStaticIndex index, string version)
		{
			using (var writer = new StringWriter())
			{
				var xmlWriter = new XmlTextWriter(writer);
				xmlWriter.WriteStartElement("Registry");
				xmlWriter.WriteStartAttribute((NoResString)"Version");
				xmlWriter.WriteValue(version);
				xmlWriter.WriteEndAttribute();

				index.Serialize(xmlWriter);

				xmlWriter.WriteEndElement();

				SystemDataRegistry.Instance.RegistryIndex.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, writer.ToString());
			}
		}

		static bool TryLoadIndex(string version, out RegistryStaticIndex index)
		{
			index = default;

			var content = SystemDataRegistry.Instance.RegistryIndex.Value;
			if (string.IsNullOrEmpty(content))
			{
				return false;
			}

			using (var reader = new StringReader(content))
			{
				try
				{
					var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { IgnoreWhitespace = true });
					if (!xmlReader.IsStartElement("Registry") || xmlReader.GetAttribute("Version") != version || !xmlReader.Read())
					{
						return false;
					}

					index = RegistryStaticIndex.Deserialize(xmlReader);
					return true;
				}
				catch (XmlException)
				{
					throw new FormatException("Bad Index-File Format");
				}
			}
		}

		public IRegistry CreateRegistry(IRegistryItemVisibility visibility)
		{
			return new Registry(staticIndex, sets, staticCategoriesCache, visibility);
		}

		string CalculateRegistryVersion()
		{
			return $"{GetCargoWiseAppMode()}-{GetClientRegistryCode()}-{GetBinariesVersion()}";
		}

		string GetBinariesVersion()
		{
			var exeTime = GetCurrentUpgradeExeTime();

			return exeTime != null
				? $"EXE{FormatDate(exeTime.Value)}"
				: $"DEV{FormatDate(GetRelatedFilesMaxWriteTime())}";

			string FormatDate(DateTime dt)
			{
				return dt.ToString("yyyyMMddHHmmss");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CW1161:Do not use constant string literals - use resource strings instead.", Justification = "Database query")]
		DateTime? GetCurrentUpgradeExeTime()
		{
			var dateTime = Db.Connection.ExecuteScalar($@"
SELECT TOP 1
	{StmUpgradeSchema.Constants.SZ_ExeVersionDate}
FROM
	{StmUpgradeSchema.Constants.SqlSchemaName}.{StmUpgradeSchema.Constants.TableName}
WHERE
	{StmUpgradeSchema.Constants.SZ_Status} = @status
", cmd => cmd.AddParameterBasedOnDbColumn("@status", "CUR", StmUpgradeSchema.SZ_Status));

			return dateTime != null && dateTime != DBNull.Value ? (DateTime?)dateTime : null;
		}

		public DateTime GetRelatedFilesMaxWriteTime()
		{
			var max = registryRelatedFileLocator
				.GetRelatedFilePaths(sets.Value.Values)
				.Max(filePath => (DateTime?)new FileInfo(filePath).LastWriteTime);

			return max ?? DateTime.MinValue;
		}

		static string GetCargoWiseAppMode()
		{
			return DataRegistry.Instance.ProductivityWiseModeEnabled ? "ProdWise" : "CargoWise";
		}

		static string GetClientRegistryCode()
		{
			ClientHook clientHook = ClientHookLoader.Instance.ClientHook;
			if (clientHook != null)
			{
				if (clientHook.AdditionalRegistryItemSet as RegistryItemSet != null)
				{
					return clientHook.Client.ToString();
				}
			}
			return "N/A";
		}

		readonly Lazy<IReadOnlyDictionary<Type, RegistryItemSet>> sets;
		readonly Lazy<RegistryStaticIndex> staticIndex;
		readonly IRegistryRelatedFilesLocator registryRelatedFileLocator;
		readonly Dictionary<RegistryCategory, bool> staticCategoriesCache;

		public class DefaultRegistryRelatedFileLocator : IRegistryRelatedFilesLocator
		{
			public IEnumerable<string> GetRelatedFilePaths(IEnumerable<RegistryItemSet> sets)
			{
				return sets
					.Select(s => s.GetType().Assembly)
					.Distinct()
					.Select(a => a.Location)
					.Where(path => !string.IsNullOrEmpty(path));
			}
		}
	}
}
