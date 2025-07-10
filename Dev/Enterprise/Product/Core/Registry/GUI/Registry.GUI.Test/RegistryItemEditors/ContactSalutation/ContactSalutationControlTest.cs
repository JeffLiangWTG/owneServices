using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ContactSalutationControl))]
	sealed class ContactSalutationControlTest : RegistryZUserControlTestCase
	{
		public void TestData()
		{
			ContactSalutationCollection collection = new ContactSalutationCollection();
			using (ContactSalutationControl control = new ContactSalutationControl())
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
			return ((ContactSalutationControl)control).contactSalutationsGrid.ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new ContactSalutationControl();
		}
	}
}
