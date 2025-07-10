using System;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IHaveSpecialDefaultAction
	{
		IDisposable DefineDefaultActionTriggeredByRecentItems();
	}
}
