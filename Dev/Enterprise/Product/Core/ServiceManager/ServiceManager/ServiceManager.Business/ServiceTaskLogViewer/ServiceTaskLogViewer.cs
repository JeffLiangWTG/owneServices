using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ServiceManager.Business
{
	public class ServiceTaskLogViewer : NonPersistentBusinessObject
	{
		[CodeAlive("Used in NonPersistentBusinessObject construction via Activator.CreateInstance")]
		public ServiceTaskLogViewer()
			: base(new BusinessObjectFactory())
		{
			var loggingSource = SystemDataRegistry.Instance.LoggingMethods.Value.DefaultCode.ToString();
			IsUsingSearchBasedLogViewerControl = loggingSource is LoggingMethods.ELK or LoggingMethods.KAF;
		}

		public bool IsUsingSearchBasedLogViewerControl { get; }

		public FileBasedLogViewer FileBasedLogViewer => fileBasedLogViewer ??= new FileBasedLogViewer();
		FileBasedLogViewer fileBasedLogViewer;

		public SearchBasedLogViewer SearchBasedLogViewer => searchBasedLogViewer ??= new SearchBasedLogViewer();
		SearchBasedLogViewer searchBasedLogViewer;
	}
}

