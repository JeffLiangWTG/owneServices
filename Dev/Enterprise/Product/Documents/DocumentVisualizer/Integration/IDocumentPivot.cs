using System;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IDocumentPivot
	{
		string DocumentTitle { get; }
		string DataContext { get; }
		string DataStoreName { get; }
		string DocType { get; }
		string Purpose { get; }
		string MenuName { get; }
		string[] DeliveryModes { get; }
		Guid TemplatePK { get; }
		bool SaveCopyToEDocs { get; }
		bool IsSystemDefined { get; }
	}
}
