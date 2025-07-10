using CargoWise.Macros;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface ICommand
	{
		string Id { get; }

		string Caption { get; }

		bool IsEnabled { get; }

		bool IsVisible { get; }

		object Image { get; }

		[MacroInvokable]
		bool Invoke();

		[MacroInvokable]
		bool Invoke(MacroMap parameters);
	}
}
