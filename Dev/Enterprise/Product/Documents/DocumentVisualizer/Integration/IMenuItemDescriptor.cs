using System.Collections.Generic;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IMenuItemDescriptor
	{
		string Caption { get; }

		object Image { get; }

		void Invoke();

		bool IsEnabled();

		bool IsVisible();

		IEnumerable<IMenuItemDescriptor> MenuItems { get; }
	}
}
