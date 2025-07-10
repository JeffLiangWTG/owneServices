using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	public static class FilterExtensions
	{
		public static IMacroScope CreateFilterEvaluatorScope(this BusinessObject parent)
		{
			var scope = new MacroScope(parent);
			scope.SetVariable(VariableNames.Environment, new Enterprise.MasterFiles.Business.Macros.Environment());

			return scope;
		}

		public static bool IsApplicable(this BusinessObject obj, string filter)
		{
			if (obj == null
				|| !obj.IsSupportedByDocumentVisualizer())
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(filter))
			{
				return true;
			}

			var macroExpression = filter.With(FilterContext).CreateExpression();

			object evaluationResult = null;

			using (var scope = CreateFilterEvaluatorScope(obj))
			{
				evaluationResult = macroExpression.Evaluate(scope);
			}

			if (!macroExpression.Errors.Any())
			{
				if (evaluationResult is bool boolResult)
				{
					return boolResult;
				}

				if (evaluationResult is ZBool zBoolResult)
				{
					return zBoolResult;
				}
			}

			return false;
		}

		public static void ValidateFilterExpression(this BusinessObject validatee, string filter)
		{
			if (validatee == null || string.IsNullOrWhiteSpace(filter))
			{
				return;
			}

			var macroExpression = filter
				.With(FilterContext)
				.CreateExpression();

			using (var scope = CreateFilterEvaluatorScope(null))
			{
				macroExpression.Evaluate(scope);
			}

			var errors = macroExpression.ToFormatString();

			if (!string.IsNullOrWhiteSpace(errors))
			{
				validatee.AddRowError(Res.GetString("80cf58ea-bc46-4a2e-a012-d7ad02029c15", "Macro has the following compilation errors: {0}", errors));
			}
		}

		public static IMacroEvaluationContext FilterContext
		{
			get
			{
				if (filterContext == null)
				{
					filterContext = new IMacroLibrary[] { new StandardLibrary(), new DataLibrary(), new FilterLibrary() }.CreateContext();
				}

				return filterContext;
			}
		}

		[ThreadStatic]
		static IMacroEvaluationContext filterContext;
	}
}
