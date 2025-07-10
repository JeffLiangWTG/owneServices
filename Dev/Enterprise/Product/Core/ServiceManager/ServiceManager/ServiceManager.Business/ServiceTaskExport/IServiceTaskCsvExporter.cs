using System.IO;

namespace Enterprise.ServiceManager.Business
{
	public interface IServiceTaskCsvExporter
	{
		void WriteTo(Stream outputStream);
	}
}
