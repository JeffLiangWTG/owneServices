using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Integration.Management
{
	public interface IUniversalTaskSetWriter
	{
		void PopulateTaskSets(IDataObjectWriterStrategy writerStrategy, BusinessObject source, IDataObject destination);
	}
}
