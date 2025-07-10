using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class LearningCentreCampaignCollectionProvider : CollectionProvider
	{
		public LearningCentreCampaignCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<ILearningCentreCampaignCollection>(), BusinessObjectFactory, new AdhocCollectionRelationship(ObjectFactory.GetType<ILearningCentreCampaign>()));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<ILearningCentreCampaignCollection>(), BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.LearningCentreCampaign;
	}
}
