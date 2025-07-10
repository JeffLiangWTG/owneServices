#if DEBUG
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI.JSInterop;
using Microsoft.JSInterop;

namespace CargoWise.Windows.UI;

partial class ControlInformationOverlayComponent
{
	public override bool UseParentDivForLayout => false;

	public KForm HostForm { get; set; }

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			AssertHostFormNotNull();
			await (GetJSInterop<IControlInformationOverlayJSInterop>()?.InitializeAsync(this) ?? Task.CompletedTask);
		}

		await base.OnAfterRenderAsync(firstRender);
	}

	[JSInvokable]
	public async Task OnMouseDownAsync(string targetWinzorControlId)
	{
		AssertHostFormNotNull();

		var highlightedControl = HostForm.FindControlByWinzorControlId(targetWinzorControlId);
		if (highlightedControl != null)
		{
			await InvokeWinzorDispatcherAsync(() => ObjectFactory.Get<IInfoDiggerProvider>().ShowInfoDigger(highlightedControl));
		}
	}

	[MemberNotNull(nameof(HostForm))]
	void AssertHostFormNotNull()
	{
		if (HostForm is null)
		{
			throw new NullReferenceException("HostForm can not be null.");
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			HostForm?.WinzorSpecificControls.Remove(this);
		}
		base.Dispose(disposing);
	}

	public void CenterOnControl(Control target, Form caller)
	{
		InvokeRenderDispatcher(() =>
			GetJSInterop<IControlInformationOverlayJSInterop>()?.HighlightTargetControlAsync(target.WinzorControlId));
	}
}
#endif
