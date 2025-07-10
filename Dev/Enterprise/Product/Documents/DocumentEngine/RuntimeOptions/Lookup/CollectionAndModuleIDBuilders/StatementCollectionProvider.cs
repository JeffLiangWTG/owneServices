using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.US;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public abstract class StatementCollectionProvider : CollectionProviderWithCodeSupport
	{
		protected StatementCollectionProvider(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return ObjectFactory.Get<ICusStatementHeaderCollection>("ICusStatementHeaderCollection", BusinessObjectFactory, new AdhocCollectionRelationship(CargoWise.Application.ObjectFactory.GetType<Enterprise.Integration.Customs.US.ICusStatementHeader>()));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return ObjectFactory.Get<ICusStatementHeaderCollection>("ICusStatementHeaderCollection", BusinessObjectFactory, GetAdditionalQuery());
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.USCustomsStatement;

		public override int MaxLength => CusStatementHeaderSchema.B2_StatementNumber.MaxLength;

		internal abstract ZQuery GetAdditionalQuery();
	}
}
