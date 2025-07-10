using CargoWise.EntityFramework;

namespace Enterprise.DocumentVisualizer.Delivery
{
	public sealed class EDocsDeliveryParameters : IEDocsDeliveryParameters
	{
		public BusinessObjectFactory Factory { get; set; }
		public IBusiness BusinessObject { get; set; }
		public string DocumentName { get; set; }
		public string DocumentTitle { get; set; }
		public string FileFormat { get; set; }
		public string DocumentType { get; set; }
		public string AttachedFileName { get; set; }
		public bool ShowDraftWatermark { get; set; }
	}
}
