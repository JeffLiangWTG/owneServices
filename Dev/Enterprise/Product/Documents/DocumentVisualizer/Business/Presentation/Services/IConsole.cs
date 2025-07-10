using CargoWise.Macros;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IConsole
	{
		[MacroInvokable]
		void Log(object obj);
	}
}