using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
namespace Enterprise.Registry.Business.Testing
{
	public abstract class NonPersistentBusinessObjectRegistryDataTypeWithEnabledItemTest<T> : NonPersistentBusinessObjectRegistryDataTypeTestCase<T> where T : IRegistryDataType
	{
		protected abstract IRegistryItem GetRegistryItem();

		protected abstract RegistryBusinessObjectCollection[] RegistriesWithEnabledItem();

		protected abstract RegistryBusinessObjectCollection[] RegistriesWithoutEnabledItem();

		protected virtual string ExpectedExceptionMessage => "This registry should have an enabled item before it is saved.";

		public void TestRegistriesWithEnabledItem()
		{
			foreach (var registry in RegistriesWithEnabledItem())
			{
				AssertNoExceptionThrown(() => GetNewDataType().ValidateBeforeRegistryFormSave(GetRegistryItem(), registry, Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			}
		}

		public void TestRegistriesWithoutEnabledItem()
		{
			foreach (var registry in RegistriesWithoutEnabledItem())
			{
				AssertExceptionThrown<RegistryValidationException>(
					message: "Exception occurs when there is no enabled item for the registry",
					expectedExceptionMessage: ExpectedExceptionMessage,
					() => GetNewDataType().ValidateBeforeRegistryFormSave(GetRegistryItem(), registry, Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			}
		}
	}
}
