using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Integration.DocumentEngine
{
	public interface ITextMacroProcessor
	{
		IReadOnlyCollection<IReportError> ReportErrors { get; }

		string Replace(string text, object[] businessObjects, bool throwError = false, object stmMenuItem = null, bool? useJs = null, bool shouldEscapeAllSpecialCharacters = false);
		IEnumerable<ITextMacroExpression> ParseMacro(string text, IEnumerable<IBusiness> businessObjects);
		IEnumerable<ITextMacroExpression> ParseMacro(string text, IEnumerable<Type> businessObjectTypes);
	}
}
