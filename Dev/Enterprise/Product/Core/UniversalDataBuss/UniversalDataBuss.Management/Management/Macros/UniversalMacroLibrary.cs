using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Macros;

namespace Enterprise.UniversalDataBuss.Management
{
	public class UniversalMacroLibrary : MacroLibrary
	{
		protected override IEnumerable<IHandler> MacroHandlers
		{
			get
			{
				yield return new Handler<Func<IMacroScope, MacroClosure, object>>(
					"OriginalValue",
					"Returns the original value of a property name when it was loaded or created.",
					(scope, macroClosure) => OriginalValue(scope, macroClosure));
			}
		}

		static object OriginalValue(IMacroScope scope, MacroClosure macroClosure)
		{
			if (scope.Data is BusinessObject bizo)
			{
				const string variableName = "OriginalBizo";
				var variable = scope.FirstOrDefault(x => x.Name == variableName);
				BusinessObject originalBizo;
				if (variable != null)
				{
					originalBizo = variable.Value as BusinessObject;
				}
				else
				{
					originalBizo = new BusinessObjectFactory() { RefreshEnabled = false }
						.Load(bizo.GetType(), bizo.PK);
					scope.SetVariable(variableName, originalBizo);
				}
				if (originalBizo != null)
				{
					using (var childScope = new MacroScope(scope, originalBizo))
					{
						return macroClosure.Invoke(childScope);
					}
				}
			}

			return null;
		}
	}
}
