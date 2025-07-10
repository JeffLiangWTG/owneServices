using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	sealed class DummyAmountBasedAuthorisationRequirementCollection : AmountBasedAuthorisationRequirementCollection
	{
		public DummyAmountBasedAuthorisationRequirementCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			throw new NotImplementedException();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
	}
}
