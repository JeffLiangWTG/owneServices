namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IEmailSubjectMacroValidator
	{
		string Validate(string emailSubjectMacro);
	}
}
