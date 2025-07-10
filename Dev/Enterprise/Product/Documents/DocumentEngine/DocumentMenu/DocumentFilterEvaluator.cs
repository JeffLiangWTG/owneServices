using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngine
{
	sealed class DocumentFilterEvaluator : IDocumentFilterEvaluator
	{
		public bool IsApplicable(BusinessObject bizObj, ZString filter, params BusinessObject[] otherDataSources)
		{
			var boDocDataProvider = BODocDataProvider.Get(bizObj);

			var providers = new List<IBODocDataProvider> { boDocDataProvider };
			if (otherDataSources != null)
			{
				providers.AddRange(otherDataSources.Select(BODocDataProvider.Get));
			}

			return ZExpressionEvaluator.Evaluate(filter, bizObj as IDocumentSupportable, providers.ToArray());
		}
	}
}