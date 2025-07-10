using System;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public interface IListEditableControl : IEditableControl
	{
		CurrencyManager ListManager { get; }
		IDisposable PreserveCurrentSelection();
	}
}
