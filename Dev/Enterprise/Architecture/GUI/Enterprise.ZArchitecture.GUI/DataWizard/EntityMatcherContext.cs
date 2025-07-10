using CargoWise.DataTransfer;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IEntityMatcherContext
	{
		BusinessObjectFactory Factory { get; }
		MappingDataModel MappingDataModel { get; }
		ICompanyFilterProviderContext CompanyFilterProviderContext { get; }
		IBusinessObjectRelationshipFilterProvider RelationshipFilterProvider { get; }
	}

	public class EntityMatcherContext : IEntityMatcherContext
	{
		public EntityMatcherContext(BusinessObjectFactory factory, MappingDataModel mappingDataModel, BusinessObject companyFilterProviderObject = null, IBusinessObjectRelationshipFilterProvider relationshipFilterProvider = null)
		{
			Factory = factory;
			MappingDataModel = mappingDataModel;
			CompanyFilterProviderContext = companyFilterProviderObject as ICompanyFilterProviderContext;
			RelationshipFilterProvider = relationshipFilterProvider;
		}

		public BusinessObjectFactory Factory { get; }
		public MappingDataModel MappingDataModel { get; }
		public ICompanyFilterProviderContext CompanyFilterProviderContext { get; }
		public IBusinessObjectRelationshipFilterProvider RelationshipFilterProvider { get; }
	}
}
