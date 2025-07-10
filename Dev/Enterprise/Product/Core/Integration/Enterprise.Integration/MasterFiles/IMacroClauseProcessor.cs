namespace Enterprise.Integration
{
	public interface IMacroClauseProcessor
	{
		public void ProcessPropertyAndValue(string valuePath, out string nextPropertyPath, out string expression);
	}
}
