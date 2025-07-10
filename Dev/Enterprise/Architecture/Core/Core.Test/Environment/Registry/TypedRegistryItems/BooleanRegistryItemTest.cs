using System;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(BooleanRegistryItem))]
	public class BooleanRegistryItemTest : StronglyTypedRegistryItemTestCase<bool>
	{
		protected override StronglyTypedRegistryItem<bool, bool> GetNewRegistryItem()
		{
			return new BooleanRegistryItem("", null, null, null, RegistryStorageFlags.All, false);
		}

		public void TestValueChanged()
		{
			BooleanRegistryItem registryItem = new BooleanRegistryItem("", null, null, null, RegistryStorageFlags.All, false);
			registryItem.ValueChanged += new EventHandler(RegistryItem_ValueChanged);
			AssertEquals("Precondition: ValueChangedFired should be false.", false, ValueChangedFired);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("ValueChangedFired should be true.", true, ValueChangedFired);
		}

		void RegistryItem_ValueChanged(object sender, EventArgs e)
		{
			ValueChangedFired = true;
		}

		bool ValueChangedFired;
	}
}
