using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CustomsReferenceNumberTypesRegistryControl))]
	sealed class CustomsReferenceNumberTypesRegistryControlTest : RegistryZUserControlTestCase
	{
		public void TestData()
		{
			var collection = new CustomsReferenceNumberTypeCollection();

			using (var control = new CustomsReferenceNumberTypesRegistryControl())
			{
				control.Data = collection;
				AssertEquals(collection, control.Data);
			}
		}

		protected override CargoWise.EntityFramework.IBusiness GetNewBusinessEntity()
		{
			return null;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((CustomsReferenceNumberTypesRegistryControl)control).ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new CustomsReferenceNumberTypesRegistryControl();
		}
	}
}
