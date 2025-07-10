using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Delivery
{
	public sealed class EmailSubjectMacroValidator : IEmailSubjectMacroValidator
	{
		public string Validate(string emailSubjectMacro)
		{
			if (string.IsNullOrWhiteSpace(emailSubjectMacro))
			{
				return string.Empty;
			}

			var macroExpression = EmailSubjectEvaluator.GetConvertedMacro(emailSubjectMacro)
				.With(EmailSubjectEvaluator.EmailSubjectContext)
				.CreateExpression();

			using (var scope = EmailSubjectEvaluator.CreateEmailSubjectMacroEvaluatorScope(null))
			{
				macroExpression.Evaluate(scope);
			}

			var errors = macroExpression.ToFormatString();

			return !string.IsNullOrEmpty(errors)
				? Res.GetString("0ab5a453-bad8-49ce-aca0-e34fd9f96fa8", "Macro has the following compilation errors: {0}", errors)
				: string.Empty;
		}
	}
}
