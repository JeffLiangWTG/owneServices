using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using CargoWise.Common;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class NetworkRibbonResourcesProvider
	{
		public Drawing GetResource(string resourceName)
		{
			if (string.IsNullOrEmpty(resourceName))
			{
				return null;
			}

			var result = resourcesCache.Values.Select(x => x.GetDrawing(resourceName)).WhereNotNull().ToArray();

			if (result.Length > 1)
			{
				throw new ArgumentOutOfRangeException(FormattableString.Invariant($"Resource with name '{resourceName}' appears more than once"));
			}

			if (result.Length < 1)
			{
				throw new ArgumentOutOfRangeException(FormattableString.Invariant($"Resource with name '{resourceName}' was not found"));
			}

			return result[0];
		}

		public NetworkRibbonResources AddRibbonResources(string resourcesAddress)
		{
			if (EnsurePackUriSchemeIsRegistered())
			{
				return resourcesCache.GetOrAdd(resourcesAddress, () => new NetworkRibbonResources(new Uri(resourcesAddress, UriKind.RelativeOrAbsolute)));
			}

			return null;
		}

		public bool HasResources() => resourcesCache.Any();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a name of a uri scheme")]

#if DEBUG
		protected virtual
#endif
		bool EnsurePackUriSchemeIsRegistered()
		{
			// https://stackoverflow.com/questions/6005398/uriformatexception-invalid-uri-invalid-port-specified

			const string packScheme = "pack";

			if (!UriParser.IsKnownScheme(packScheme))
			{
				var scheme = System.IO.Packaging.PackUriHelper.UriSchemePack; // needed to register the pack uri scheme

				if (scheme != packScheme || !UriParser.IsKnownScheme(packScheme))
				{
					ErrorReporter.ReportOnce("Error registering pack uri scheme.");
					return false;
				}
			}

			// http://jake.ginnivan.net/pack-uri-in-unit-tests/

			if (System.Windows.Application.ResourceAssembly == null)
			{
				System.Windows.Application.ResourceAssembly = GetType().Assembly;
			}

			return true;
		}

		readonly Dictionary<string, NetworkRibbonResources> resourcesCache = new Dictionary<string, NetworkRibbonResources>();
	}
}
