using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WinzorFramework;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test.Winzor;

public class NetworkDiagramFormTest
{
	[Test]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD101:Avoid unsupported async delegates", Justification = "No workaround")]
	public async Task DiagramFormDoesNotCauseMemoryLeakAsync()
	{
		WeakReference form;
		await new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>()).InvokeAsync(async () =>
		{
			form = CreateNetworkDiagramForm();
			GC.Collect();
			GC.WaitForFullGCApproach();
			Assert.That(form.IsAlive, Is.True);

			await DisposeNetworkDiagramFormAsync(form);
			GC.Collect();
			GC.WaitForFullGCApproach();
			Assert.That(form.IsAlive, Is.False);
		});
	}

	WeakReference CreateNetworkDiagramForm()
	{
		return new WeakReference(new NetworkDiagramForm());
	}

	async Task DisposeNetworkDiagramFormAsync(WeakReference reference)
	{
		var form = (NetworkDiagramForm)reference.Target;
		form.EndRenderInvokeForTest();
		await form.Proxy.DisposeAsync();
	}
}

