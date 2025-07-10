using System.ComponentModel;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.ComponentModel.Testing
{
	sealed class ControlEventDescriptorCollectionTest : TestCase
	{
		ControlEventDescriptorCollection collection;

		protected override void SetUp()
		{
			collection = new ControlEventDescriptorCollection(typeof(TextBox));
		}

		public void TestDefaultCollectionCreationWillActuallyHasOneValidatingEventSpecified()
		{
			AssertEquals(1, collection.Count);

			EventDescriptor validatingDescriptor = null;
			foreach (EventDescriptor each in collection)
			{
				validatingDescriptor = each;
				break;
			}

			AssertNotNull(validatingDescriptor);
			AssertEquals("Validating", validatingDescriptor.DisplayName);
			AssertNotEquals("", validatingDescriptor.Category);
		}

		// note: .NET will throw null reference exception if collection has null element
		public void TestInitializedAsZeroLengthEventCollection_IfTypeDoesntHaveValidatingEvent()
		{
			collection = new ControlEventDescriptorCollection(typeof(TestClass));
			Assert(collection.Count == 0);
		}

		public void TestFindCreatesOneDescriptorAtTime()
		{
			var descriptor = collection.Find("TextChanged", false);
			AssertNotNull(descriptor);
			AssertEquals(2, collection.Count);
		}

		public void TestGettingEventsByIndexDoNotTriggersFullInitialization_BreakingContract()
		{
			var descriptor = collection[0];
			AssertNotNull(descriptor);
			Assert(collection.Count == 1);
		}

		class TestClass
		{
		}
	}
}
