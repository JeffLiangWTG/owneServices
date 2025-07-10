using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Billing.StlCollector.Retriever
{
	class AllStlScriptFactory : IScriptFactory
	{
		readonly ILogger logger;

		public AllStlScriptFactory() : this(null)
		{
		}

		public AllStlScriptFactory(ILogger logger)
		{
			this.logger = logger;
		}

#if DEBUG

		public static IDisposable SetTemporaryFeatureCodeFilterForTest(IEnumerable<string> featureCodes)
		{
			featureCodeTestFilter = featureCodes;
			return new DisposableAction(() => featureCodeTestFilter = null);
		}

		[ThreadSafe]
		static IEnumerable<string> featureCodeTestFilter;

		public IEnumerable<IStlItem> CreateScripts(BusinessObjectFactory factory)
		{
			var allScripts = CreateScriptsInternal(factory);
			return featureCodeTestFilter != null ? allScripts.Where(s => featureCodeTestFilter.Contains(s.Code)) : allScripts;
		}
#else
		public IEnumerable<IStlItem> CreateScripts(BusinessObjectFactory factory) => CreateScriptsInternal(factory);
#endif

		IEnumerable<IStlItem> CreateScriptsInternal(BusinessObjectFactory factory)
		{
			var staticScripts = new CustomStlCollectorFactory().CreateScripts(factory).ToDictionary(s => s.Code);
			foreach (var script in staticScripts.Values)
			{
				yield return script;
			}
			var dynamicScripts = new RefStlScriptFactory().CreateScripts(factory);
			foreach (var script in dynamicScripts)
			{
				if (staticScripts.ContainsKey(script.Code))
				{
					var errorMessage = $"The STL collector with FeatureCode={script.Code} has both custom and dynamic definitions.  The Dynamic collector could not be added to the system.";
					ErrorReporter.ReportOnce(errorMessage);
					logger?.Log(LogType.Error, errorMessage);
				}
				else
				{
					yield return script;
				}
			}
		}
	}
}
