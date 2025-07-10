using System;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	public interface ITabPageWithVisibility<T> : ITabPage where T : BusinessObject
	{
		Func<T, bool> IsVisible { get; }
	}

	public class TabPageVisibilityInfo<T> where T : BusinessObject
	{
		public TabPageVisibilityInfo(string tabPageName, Func<T, bool> isVisible)
		{
			TabPageName = tabPageName;
			IsVisible = isVisible;
		}

		public string TabPageName { get; }

		public Func<T, bool> IsVisible { get; }
	}
}
