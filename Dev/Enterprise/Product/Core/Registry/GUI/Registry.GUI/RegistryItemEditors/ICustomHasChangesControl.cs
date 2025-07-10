using System;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.GUI
{
	public interface ICustomHasChangesControl
	{
		 event EventHandler<HasChangesChangedEventArgs> HasChangesChanged;
	}
}