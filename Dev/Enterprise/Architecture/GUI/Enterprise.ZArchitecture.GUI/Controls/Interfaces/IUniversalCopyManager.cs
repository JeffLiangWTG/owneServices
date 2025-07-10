using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IUniversalCopyManager : IDisposable
	{
		bool AllowsUniversalCopy { get; }
		void AddMenuItems(Menu menu, bool includeEditMenuItems, bool includeCopySchedulesItem, bool lazyPopulate = true);

		event EventHandler<FormShowingForElementArgs> FormShowingForNewElement;
	}

	public class FormShowingForElementArgs : EventArgs
	{
		public FormShowingForElementArgs(IBusiness element)
		{
			Element = element;
			Cancelled = false;
		}

		public readonly IBusiness Element;
		public bool Cancelled;
	}
}