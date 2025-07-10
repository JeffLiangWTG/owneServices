using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class JobVoyageCollectionProvider : CollectionProvider
	{
		public JobVoyageCollectionProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IJobVoyageCollection>(), new object[] { BusinessObjectFactory });
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.JobSeaVoyage;
	}
}
