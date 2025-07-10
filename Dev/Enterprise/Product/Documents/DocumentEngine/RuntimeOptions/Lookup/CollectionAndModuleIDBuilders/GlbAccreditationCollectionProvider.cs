using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class GlbAccreditationCollectionProvider : CollectionProvider
	{
		public GlbAccreditationCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IGlbAccreditationCollection>(), BusinessObjectFactory, new AdhocCollectionRelationship(ObjectFactory.GetType<IGlbAccreditation>()));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IGlbAccreditationCollection>(), BusinessObjectFactory, Filter);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.GlbAccreditation;
	}
}
