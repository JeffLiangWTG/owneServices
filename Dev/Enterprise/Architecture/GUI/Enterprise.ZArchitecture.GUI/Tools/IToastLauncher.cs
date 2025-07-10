using System;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IToastLauncher
	{
		void Launch(string title, string text);

		void Launch(string title, string text, TimeSpan delay);
	}
}
