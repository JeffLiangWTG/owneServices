namespace Enterprise.Accounting.Integration
{
	public interface IAutoRatingDescriptionMacroExpander
	{
		bool CanExpandMacros { get; }
		string ExpandMacro(string macro);
	}
}
