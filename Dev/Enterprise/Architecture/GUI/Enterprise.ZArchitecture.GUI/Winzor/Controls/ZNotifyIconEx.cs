using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinzorFramework;

namespace Enterprise.ZArchitecture.GUI;

public class ZNotifyIconEx : Component
{
	public ZNotifyIconEx()
	{
	}

	public void DisplayToastNotification(string title, string body)
	{
		var form = WinzorDispatcher.Current.CurrentContext?.Form;
		form?.InvokeRenderDispatcher(async () =>
		{
			await (form?.Interop?.CreateNotificationAsync(title, body) ?? Task.CompletedTask);
		});
	}
}
