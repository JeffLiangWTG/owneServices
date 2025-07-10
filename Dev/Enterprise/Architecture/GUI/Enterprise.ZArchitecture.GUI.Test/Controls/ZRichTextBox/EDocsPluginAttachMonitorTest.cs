using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.RichEdit.Testing
{
	sealed class EDocsPluginAttachMonitorTest : TestCase
	{
		public void TestEDocsPluginAttachMonitor()
		{
			var monitorEDocAttachment = new DummyMonitorEDocAttachment();
			Assert(!monitorEDocAttachment.MonitorEDocAttachmentStared);
			Assert(!monitorEDocAttachment.IsMonitoringDocumentAttachment);

			using (EDocsPluginAttachMonitor.StartMonitorEDocAttachment(monitorEDocAttachment))
			{
				Assert(monitorEDocAttachment.IsMonitoringDocumentAttachment);
				Assert(monitorEDocAttachment.MonitorEDocAttachmentStared);
			}

			Assert(!monitorEDocAttachment.IsMonitoringDocumentAttachment);
			Assert(!monitorEDocAttachment.MonitorEDocAttachmentStared);
		}
	}
}
