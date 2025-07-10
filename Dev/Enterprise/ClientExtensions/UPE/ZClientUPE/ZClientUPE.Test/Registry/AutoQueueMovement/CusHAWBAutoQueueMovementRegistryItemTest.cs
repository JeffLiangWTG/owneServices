using System;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(CusHAWBAutoQueueMovementRegistryItem))]
	internal class CusHAWBAutoQueueMovementRegistryItemTest : StronglyTypedRegistryItemTestCase<CusHAWBAutoQueueMovementCollection>
	{
		public void TestConstructor()
		{
			AssertEquals("Test", Item.Name);
			AssertEquals("Category", Item.Category);
			AssertEquals("Caption", Item.Caption);
			AssertEquals("Hint", Item.Hint);
			AssertEquals(typeof(CusHAWBAutoQueueMovementCollection), Item.DataType.DataType);
			AssertEquals(RegistryStorageFlags.System, Item.Storage);
		}

		public void TestValueSetEvent()
		{
			Item.ValueSet += new EventHandler(RegistryItem_ValueSet);
			AssertEquals("Pre-condition. Before the registry item is set", false, RegistryItemValueSetEventCalled);
			CusHAWBAutoQueueMovementCollection collection = new CusHAWBAutoQueueMovementCollection();
			Item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			AssertEquals(true, RegistryItemValueSetEventCalled);
		}

		#region Implementation
		protected override StronglyTypedRegistryItem<CusHAWBAutoQueueMovementCollection, CusHAWBAutoQueueMovementCollection> GetNewRegistryItem()
		{
			return new CusHAWBAutoQueueMovementRegistryItem("Test", "Category", "Caption", "Hint");
		}

		protected new CusHAWBAutoQueueMovementRegistryItem Item
		{
			get
			{
				return (CusHAWBAutoQueueMovementRegistryItem)base.Item;
			}
		}

		void RegistryItem_ValueSet(object sender, EventArgs e)
		{
			RegistryItemValueSetEventCalled = true;
		}

		bool RegistryItemValueSetEventCalled;
		#endregion
	}
}
