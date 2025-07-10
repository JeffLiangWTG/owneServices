using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.Module
{
	public class RunProgramActionMethodApplicator : OperationalActionMethodApplicator, IObsoleteValidation
	{
		public RunProgramActionMethodApplicator(RunProgramActionMethodSettings settings, BusinessObjectFactory factory)
			: base(Res.GetString("81ebdc73-257b-4610-bd93-ab5da7c207d6", "Run Program"), factory)
		{
			this.settings = settings;
		}

		readonly RunProgramActionMethodSettings settings;

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
					var resolvedArguments = ObjectFactory.Get<ITextMacroProcessor>().Replace(settings.Arguments, new[] { target });

#if DEBUG
					if (Globals.IsTest)
					{
						resolvedArgumentsList.Add(resolvedArguments);
					}
					else
#endif
					{
						ObjectFactory.Get<IProgramLauncher>().Launch(settings.Path, resolvedArguments);
					}
				}
			}
		}

#if DEBUG
		internal List<string> resolvedArgumentsList = new List<string>();
#endif
	}
}
