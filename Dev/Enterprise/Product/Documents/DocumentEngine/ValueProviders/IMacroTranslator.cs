namespace Enterprise.DocumentEngine
{
	public interface IMacroTranslator
	{
		object GetValue(string macro, Passes pass, bool shouldSkipEscapingAngleBrackets = false);

		object GetFormulaResult(string expression);
	}
}
