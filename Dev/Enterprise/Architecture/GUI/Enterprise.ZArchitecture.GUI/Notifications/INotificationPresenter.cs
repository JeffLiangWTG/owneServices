using System;
using System.Collections.Generic;
using System.Windows.Forms;

using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI
{
	public interface INotificationPresenter : IDisposable
	{
		void Initialize(Control control);
		IEnumerable<INotification> Notifications { get; set; }
	}
}