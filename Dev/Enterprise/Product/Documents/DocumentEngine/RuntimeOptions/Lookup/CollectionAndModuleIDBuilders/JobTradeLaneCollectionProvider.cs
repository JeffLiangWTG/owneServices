using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class JobTradeLaneCollectionProvider : CollectionProviderWithCodeSupport
	{
		public JobTradeLaneCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IJobTradeLaneCollection>(), new object[] { BusinessObjectFactory, new AdhocCollectionRelationship(ObjectFactory.GetType<IJobTradeLane>()) });
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IJobTradeLaneCollection>(), new object[] { BusinessObjectFactory });
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.TradeLane;

		public override int MaxLength => JobTradeLaneSchema.EJ_Code.MaxLength;
	}
}
