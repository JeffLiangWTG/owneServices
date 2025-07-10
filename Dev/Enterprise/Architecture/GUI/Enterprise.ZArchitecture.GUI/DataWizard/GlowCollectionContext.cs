using CargoWise.DataTransfer;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IGlowCollectionContext
	{
		BusinessObject BusinessObject { get; }
		MappingDataModel MappingDataModel { get; }
		string DataDefinitionName { get; }
		GlowLog Log { get; }
	}

	public class GlowCollectionContext : IGlowCollectionContext
	{
		public GlowCollectionContext(BusinessObject businessObject, MappingDataModel mappingDataModel, string dataDefinitionName, GlowLog log)
		{
			BusinessObject = businessObject;
			MappingDataModel = mappingDataModel;
			DataDefinitionName = dataDefinitionName;
			Log = log;
		}

		public BusinessObject BusinessObject { get; }
		public MappingDataModel MappingDataModel { get; }
		public string DataDefinitionName { get; }
		public GlowLog Log { get; }
	}
}
