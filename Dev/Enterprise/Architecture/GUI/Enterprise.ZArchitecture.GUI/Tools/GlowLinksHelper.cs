using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GlowInterop;

namespace Enterprise.ZArchitecture.GUI
{
	public static class GlowLinksHelper
	{
		const string EntityPkQueryName = "entityPK";

		public static void OpenEnityInGlow(INotifications notify, string formFlowAlias, BusinessObject businessObject, IEnumerable<(string Name, string Value)> additionalQueryStrings = null)
		{
			Argument.NotNull(businessObject, nameof(businessObject));

			var queryStrings = additionalQueryStrings ?? new[] { (EntityPkQueryName, businessObject.PK.ToString()) };
			var url = new GlowUrlProvider(notify).TryGenerateUrl(formFlowAlias, businessObject.HumanReadableName, queryStrings);
			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		}
	}
}
