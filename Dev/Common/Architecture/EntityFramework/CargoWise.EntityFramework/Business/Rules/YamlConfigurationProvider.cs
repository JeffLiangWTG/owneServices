using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.Common;
using WTG.Rules.Configuration;
using WTG.Rules.Engine;
using WTG.Rules.Engine.Impl;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CargoWise.EntityFramework.Business.Rules
{
	public class YamlConfigurationProvider : IRuleConfigurationAdapter, IDomainConfigurationProvider
	{
		public YamlConfigurationProvider(string path)
		{
			Argument.NotNullOrEmpty(path, nameof(path));

			folderPath = path;
		}

		readonly string folderPath;
		IReadOnlyDictionary<string, RuleConfiguration> cachedConfiguration;

		public string GetConfigurationKey(Type t)
		{
			return GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(t, tryToFindOutIfNotSpecified: true)?.Name;
		}

		public Type GetType(string typeName, bool throwOnError)
		{
			return Type.GetType(typeName, throwOnError);
		}

		public bool IsDataTypeObject(Type t)
		{
			return typeof(IBusiness).IsAssignableFrom(t);
		}

		public IRuleImpl GetErrorRuleImpl(Type returnType)
		{
			var methodInfo = typeof(YamlConfigurationProvider).GetMethod(nameof(YamlConfigurationProvider.Throw), BindingFlags.NonPublic | BindingFlags.Static);
			var genericMethodInfo = methodInfo.MakeGenericMethod(returnType);
			return CompiledRuleUtils.GetCompiledRuleImpl(genericMethodInfo);
		}

		static T Throw<T>(string message)
			=> throw new InvalidOperationException(message);

		public Task<IReadOnlyDictionary<string, RuleConfiguration>> GetRuleConfiguration()
		{
			if (cachedConfiguration == null)
			{
				cachedConfiguration = LoadFromFiles();
			}

			return Task.FromResult(cachedConfiguration);
		}

		public IReadOnlyCollection<string> DiscoverAdditionalDependencyPaths(IReadOnlyCollection<ParameterInfo> parameters, IEnumerable<ParameterAndValue> arguments)
		{
			return Array.Empty<string>();
		}

		IReadOnlyDictionary<string, RuleConfiguration> LoadFromFiles()
		{
			var result = new Dictionary<string, RuleConfiguration>();
			var deserializer = GetDeserializer();
			if (Directory.Exists(folderPath))
			{
				foreach (var filePath in Directory.GetFiles(folderPath, "*.yaml"))
				{
					var name = Path.GetFileNameWithoutExtension(filePath);
					using (var reader = new StreamReader(filePath))
					{
						result.Add(name, deserializer.Deserialize<RuleConfiguration>(reader));
					}
				}
			}

			return result;
		}

		IDeserializer GetDeserializer()
		{
			return new DeserializerBuilder()
				.WithNamingConvention(new CamelCaseNamingConvention())
				.Build();
		}

		public bool IsValidDependency(string dependencyPath)
		{
			return true;
		}
	}
}
