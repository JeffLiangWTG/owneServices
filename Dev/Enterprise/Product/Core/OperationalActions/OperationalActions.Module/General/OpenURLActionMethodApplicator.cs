using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.Module
{
	public class OpenURLActionMethodApplicator : OperationalActionMethodApplicator, IObsoleteValidation
	{
		public OpenURLActionMethodApplicator(OpenURLActionMethodSettings settings, BusinessObjectFactory factory)
			: base(Res.GetString("10b0eedb-a385-4644-8e21-bce9e55b8e48", "Open URL"), factory)
		{
			this.settings = settings;
		}

		readonly OpenURLActionMethodSettings settings;

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (targets.Length == 0)
			{
				log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("d1d9055d-7743-47e2-9ccb-fd7fd5b10894", "No records selected."));
			}
			else
			{
				foreach (var target in targets)
				{
					var resolvedURL = ObjectFactory.Get<ITextMacroProcessor>().Replace(settings.URL, new[] { target });

#if DEBUG
					if (Globals.IsTest)
					{
						resolvedURLs.Add(resolvedURL);
					}
					else
#endif
					{
						WebUrlLauncher.Launch(resolvedURL);
					}
				}
			}
		}

#if DEBUG
		internal List<string> resolvedURLs = new List<string>();
#endif
	}
}
