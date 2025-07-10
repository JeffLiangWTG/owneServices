using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Integration.Billing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Billing.StlCollector.Retriever
{
	interface IScriptLoader
	{
		IEnumerable<IStlScriptWithConfig> Load(BusinessObjectFactory factory);
	}

	class ScriptLoader : IScriptLoader
	{
		public ScriptLoader(ILogger logger = null)
			: this(new CollectionTimeProvider(), logger)
		{
		}

		public ScriptLoader(CollectionTimeProvider timeProvider, ILogger logger)
			: this(new AllStlScriptFactory(logger), timeProvider)
		{
		}

		public ScriptLoader(IScriptFactory scriptFactory)
			: this(scriptFactory, new CollectionTimeProvider())
		{
		}

		public ScriptLoader(IScriptFactory scriptFactory, CollectionTimeProvider timeProvider)
		{
			this.scriptFactory = scriptFactory;
			this.timeProvider = timeProvider;
		}

		readonly IScriptFactory scriptFactory;
		readonly CollectionTimeProvider timeProvider;

		public IEnumerable<IStlScriptWithConfig> Load(BusinessObjectFactory bizOFactory)
		{
			var waterMarkDatas = bizOFactory.Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			var legacyWaterMarkData = waterMarkDatas.FirstOrDefault(sd => sd.SD_Name.ToString().Equals(SystemDataRegistry.StlCollectorHighWaterMarkPrefix, StringComparison.InvariantCultureIgnoreCase));
			var legacyWaterMarkSetting = (legacyWaterMarkData != null) ? new WaterMarkSetting(legacyWaterMarkData) : null;
			return scriptFactory.CreateScripts(bizOFactory).Select(s =>
			{
				var settingName = SystemDataRegistry.StlCollectorHighWaterMarkPrefix + s.Code;
				var waterMarkData = waterMarkDatas.FirstOrDefault(sd => sd.SD_Name.ToString().Equals(settingName, StringComparison.InvariantCultureIgnoreCase));
				if (waterMarkData != null)
				{
					return new ScriptWithConfig(s, new WaterMarkSetting(waterMarkData), legacyWaterMarkSetting, timeProvider);
				}

				return new ScriptWithConfig(s, new WaterMarkSetting(bizOFactory, settingName), legacyWaterMarkSetting, timeProvider);
			}).ToList();
		}
	}
}
