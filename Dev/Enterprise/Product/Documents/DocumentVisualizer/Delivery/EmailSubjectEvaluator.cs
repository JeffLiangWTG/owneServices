using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Delivery
{
	sealed class EmailSubjectEvaluator
	{
		public bool TryCreateEmailSubject(BusinessObject obj, string emailSubjectMacro, out string emailSubject)
		{
			emailSubject = string.Empty;

			if (obj == null
				|| string.IsNullOrWhiteSpace(emailSubjectMacro))
			{
				return false;
			}

			var macroExpression = GetConvertedMacro(emailSubjectMacro)
				.With(EmailSubjectContext)
				.CreateExpression();

			object evaluationResult = null;

			using (var scope = CreateEmailSubjectMacroEvaluatorScope(obj))
			{
				evaluationResult = macroExpression.Evaluate(scope);
			}

			if (!macroExpression.Errors.Any())
			{
				emailSubject = Convert.ToString(evaluationResult);
				return true;
			}

			return false;
		}

		#region Implementation

		public static string GetConvertedMacro(string subjectMacro) => string.Concat("\"", subjectMacro, "\"");

		public static IMacroScope CreateEmailSubjectMacroEvaluatorScope(BusinessObject parent)
		{
			var scope = new MacroScope(parent);
			scope.SetVariable(VariableNames.Environment, new Enterprise.MasterFiles.Business.Macros.Environment());

			return scope;
		}

		public static IMacroEvaluationContext EmailSubjectContext
		{
			get
			{
				if (emailSubjectContext == null)
				{
					emailSubjectContext = new[]
					{
						new StandardLibrary()
					}
					.CreateContext();
				}

				return emailSubjectContext;
			}
		}

		[ThreadStatic]
		static IMacroEvaluationContext emailSubjectContext;

		#endregion
	}
}
