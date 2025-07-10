using System;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IBoundOnPreSaveValidation
	{
		event EventHandler AfterFirstBinding;
	}
}
