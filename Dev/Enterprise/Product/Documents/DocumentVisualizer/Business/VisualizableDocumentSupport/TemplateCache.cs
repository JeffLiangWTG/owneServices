using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class TemplateCache
	{
		public TemplateCache(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;
		readonly IDictionary<Guid, ITemplate> cache = new Dictionary<Guid, ITemplate>();

		public ITemplate Get(Guid id)
		{
			if (!cache.TryGetValue(id, out var result))
			{
				var templateBizObj = factory.Load<VisualizerTemplate>(id);

				var res = templateBizObj?
					.GetFlexCelWorksheet()
					.CreateTemplate(isSystemDefined: templateBizObj.SO_IsSystemDefined);

				result = res.HasValue && res.Value.IsRight
					? res.Value.Right
					: null;

				cache[id] = result;
			}

			return result;
		}
	}
}
