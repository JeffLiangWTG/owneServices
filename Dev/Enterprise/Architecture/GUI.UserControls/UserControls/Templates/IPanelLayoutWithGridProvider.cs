using System;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IPanelLayoutWithGridProvider : IPanelLayoutProvider
	{
		Type GridUserControlType { get; }
	}
}
