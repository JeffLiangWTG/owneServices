using CargoWise.ComponentModel;

namespace Enterprise.DataTransfer.Integration
{
	public interface IValueObjectExportContext : INotifications
	{
		bool SimplifiedXML { get; }
		string ExportPurpose { get; set; }
	}
}
