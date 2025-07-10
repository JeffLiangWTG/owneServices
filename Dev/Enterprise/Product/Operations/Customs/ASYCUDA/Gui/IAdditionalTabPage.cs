using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public interface IAdditionalTabPage : IDisposable
	{
		ZUserControl AdditionalTabPageUserControl { get; }
		ResourceStringData AdditionalTabPageCaption { get; }
		AdditionalTabPageVisibility AdditionalControlVisibility { get; }
		int TabPageSequence { get; }
	}
}
