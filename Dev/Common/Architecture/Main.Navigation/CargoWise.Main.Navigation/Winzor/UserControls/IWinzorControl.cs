using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components;
using WinzorFramework.JSInterop;

namespace CargoWise.Main.Navigation;

public interface IWinzorControl
{
	Task InvokeAsync(Action action);
	void UpdateMouseData(WebMouseEventArgs args);
	void NotifyStateChanged();
	void PopupMenu(object sender, ToolStripDropDownItem toolStripDropDown, Point point);
	Task<ClientRect?> GetBoundingClientRectAsync(ElementReference element);
}
