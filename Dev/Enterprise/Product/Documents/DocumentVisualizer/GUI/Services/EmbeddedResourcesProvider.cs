using System;
using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class EmbeddedResourcesProvider : IResourceProvider
	{
		public EmbeddedResourcesProvider()
		{
			this.lazyResources = new Lazy<IReadOnlyDictionary<string, Func<object>>>(GetResources);
		}

		public IReadOnlyDictionary<string, Func<object>> Resources => lazyResources.Value;
		readonly Lazy<IReadOnlyDictionary<string, Func<object>>> lazyResources;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		IReadOnlyDictionary<string, Func<object>> GetResources()
		{
			const string pathPrefix = "icons/";

			var result = new Dictionary<string, Func<object>>
			{
				[pathPrefix + "cancel"] = () => Properties.Resources.cancel,
				[pathPrefix + "data"] = () => Properties.Resources.data,
				[pathPrefix + "deliver_document"] = () => Properties.Resources.deliver_document,
				[pathPrefix + "macro_evaluator"] = () => Properties.Resources.macro_evaluator,
				[pathPrefix + "messaging"] = () => Properties.Resources.messaging,
				[pathPrefix + "reset"] = () => Properties.Resources.reset,
				[pathPrefix + "save"] = () => Properties.Resources.save,
				[pathPrefix + "send_document"] = () => Properties.Resources.send_document,
				[pathPrefix + "send_message"] = () => Properties.Resources.send_message,
				[pathPrefix + "settings"] = () => Properties.Resources.settings,
				[pathPrefix + "tools"] = () => Properties.Resources.tools,
				[pathPrefix + "withdraw_document"] = () => Properties.Resources.withdraw_document
			};

			return result;
		}
	}
}
