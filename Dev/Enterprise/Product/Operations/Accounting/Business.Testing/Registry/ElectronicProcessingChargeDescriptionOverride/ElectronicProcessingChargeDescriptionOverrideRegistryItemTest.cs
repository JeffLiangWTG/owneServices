using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeDescriptionOverrideRegistryItem))]
	public class ElectronicProcessingChargeDescriptionOverrideRegistryItemTest : StronglyTypedRegistryItemTestCase<ElectronicProcessingChargeDescriptionOverrideCollection>
	{
		protected override StronglyTypedRegistryItem<ElectronicProcessingChargeDescriptionOverrideCollection, ElectronicProcessingChargeDescriptionOverrideCollection> GetNewRegistryItem()
		{
			return new ElectronicProcessingChargeDescriptionOverrideRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		public void TestIsVisible()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);

			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(testObjectCreator.NonCurrentNonDemoCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var registryItem = GetNewRegistryItem();
			AssertEquals(true, registryItem.IsVisible(testObjectCreator.NonCurrentNonDemoCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals(false, registryItem.IsVisible(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}
	}
}
