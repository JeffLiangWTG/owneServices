using System;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public sealed class ResourceAccessor : IResourceAccessor
	{
		public ResourceAccessor(params IResourceProvider[] resourceProviders)
		{
			Argument.NotNull(resourceProviders, nameof(resourceProviders));
			this.resourceProviders = resourceProviders;
		}

		readonly IResourceProvider[] resourceProviders;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		public object Get(Uri uri)
		{
			if (uri == null)
			{
				return null;
			}

			const string resourcesScheme = "res";

			if (string.Compare(uri.Scheme, resourcesScheme, StringComparison.Ordinal) != 0)
			{
				return null;
			}

			var key = string.Concat(uri.Host, uri.LocalPath);

			foreach (var resourceProvider in resourceProviders)
			{
				if (resourceProvider.Resources != null
					&& resourceProvider.Resources.TryGetValue(key, out var provider))
				{
					return provider?.Invoke();
				}
			}

			return null;
		}
	}
}
