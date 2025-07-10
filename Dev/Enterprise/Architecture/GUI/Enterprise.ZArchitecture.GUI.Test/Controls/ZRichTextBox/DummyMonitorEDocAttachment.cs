using Enterprise.DocumentScanning.Integration;

namespace Enterprise.ZArchitecture.GUI.RichEdit.Testing
{
	sealed class DummyMonitorEDocAttachment : IMonitorEDocAttachment
	{
		internal bool MonitorEDocAttachmentStared;
		public bool IsMonitoringDocumentAttachment { get; set; }

		public void StartMonitorEDocAttachment()
		{
			MonitorEDocAttachmentStared = true;
		}

		public void EndMonitorEDocAttachment()
		{
			MonitorEDocAttachmentStared = false;
		}
	}
}
