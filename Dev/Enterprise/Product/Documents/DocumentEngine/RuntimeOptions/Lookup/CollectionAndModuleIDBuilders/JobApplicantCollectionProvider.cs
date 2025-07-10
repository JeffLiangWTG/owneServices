using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class JobApplicantCollectionProvider : CollectionProvider
	{
		public JobApplicantCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.Recruiter.IHRJobApplicantCollection>(), BusinessObjectFactory, Filter);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.HRJobApplicant;
	}
}
