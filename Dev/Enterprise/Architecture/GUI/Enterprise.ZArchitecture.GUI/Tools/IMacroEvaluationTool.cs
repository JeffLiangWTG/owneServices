using System.Windows.Forms;

namespace Enterprise.Integration
{
	public interface IMacroEvaluationTool
	{
		string Name { get; }
		bool ShowAsButton { get; }
		Form Run(IWin32Window parentForm);
		Form Run(IWin32Window parentForm, string defaultMacro, IAntlrMacroContextProvider scope);
	}
}
