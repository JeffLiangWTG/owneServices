using CargoWise.EntityFramework;

namespace Enterprise.DocumentVisualizer.Delivery
{
	public interface IEDocsDeliveryParameters
	{
		BusinessObjectFactory Factory { get; }
		IBusiness BusinessObject { get; }
		string DocumentName { get; }
		string DocumentTitle { get; }
		string FileFormat { get; }
		string DocumentType { get; }
		string AttachedFileName { get; }
		bool ShowDraftWatermark { get; }
	}
}
