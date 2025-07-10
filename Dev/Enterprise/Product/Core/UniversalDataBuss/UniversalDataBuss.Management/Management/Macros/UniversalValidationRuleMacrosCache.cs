using System;
using System.Collections.Generic;
using CargoWise.Macros;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Management
{
	internal class UniversalValidationRuleMacrosCache
	{
		readonly Dictionary<ZString, IMacroExpression> macros;
		readonly IMacroEvaluationContext macroContext;
		[ThreadStatic]
		static UniversalValidationRuleMacrosCache instance;

		public static UniversalValidationRuleMacrosCache Instance => instance ?? (instance = new UniversalValidationRuleMacrosCache());

		UniversalValidationRuleMacrosCache()
		{
			macros = new Dictionary<ZString, IMacroExpression>();
			var libraries = new IMacroLibrary[]
			{
				new StandardLibrary(),
				new UniversalMacroLibrary(),
			};
			macroContext = libraries.CreateContext();
		}

		public IMacroExpression GetMacro(ZString macro)
		{
			if (macros.TryGetValue(macro, out var macroExpression))
			{
				return macroExpression;
			}
			else
			{
				var expression = macro.ToString().With(macroContext).CreateExpression();
				expression.Compile();
				macros.Add(macro, expression);
				return expression;
			}
		}
	}
}
