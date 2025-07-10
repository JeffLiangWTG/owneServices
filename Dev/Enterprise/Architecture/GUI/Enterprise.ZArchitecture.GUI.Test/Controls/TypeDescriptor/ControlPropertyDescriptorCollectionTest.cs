using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.ComponentModel.Testing
{
	sealed class ControlPropertyDescriptorCollectionTest : TestCase
	{
		public void TestFind_CreatesOneDescriptorAtTime()
		{
			var descriptor = Collection.Find("Text", false);
			AssertNotNull(descriptor);
			AssertEquals(2, Collection.Count);
		}

		public void TestFind_WhenPropertyDoesntExist()
		{
			var descriptor = Collection.Find("MehMehMeh", false);
			AssertNull(descriptor);
			foreach (PropertyDescriptor property in collection)
			{
				AssertNotNull(property);
			}
		}

		public void TestFind_WhenPropertyDescriptorsContainNullValues()
		{
			ErrorReporter.Clear();
			var dodgyCollection = new ControlPropertyDescriptorCollection(typeof(FakeControl), FakeControl.GetPropertyDescriptorsWithNulls());
			AssertEquals("Error should have been reported.", "ControlPropertyDescriptorCollection should not contain null values.", ErrorReporter.LastMessageReported);
			AssertEquals("Null values should have been filtered.", 2, dodgyCollection.Count);

			PropertyDescriptor returnedPropertyDescriptor = null;

			AssertNoExceptionThrown(() => returnedPropertyDescriptor = dodgyCollection.Find("Text", false));
			AssertNotNull("A PropertyDescriptor should have been returned.", returnedPropertyDescriptor);

			AssertNoExceptionThrown(() => returnedPropertyDescriptor = dodgyCollection.Find("Potato", false));
			AssertNull("No PropertyDescriptor should have been returned.", returnedPropertyDescriptor);

			AssertNoExceptionThrown(() => returnedPropertyDescriptor = dodgyCollection.Find("ReadOnly", false));
			AssertNotNull("A PropertyDescriptor should have been returned.", returnedPropertyDescriptor);
			ErrorReporter.Clear();
		}

		public void TestFilteredPropertyDescriptorsWhenArrayNull()
		{
			AssertNoExceptionThrown(() => new ControlPropertyDescriptorCollection(typeof(FakeControl), null));
		}

		public void TestGettingPropertiesByIndexDoNotTriggersFullInitialization_BreakingContract()
		{
			var descriptor = Collection[1];
			AssertNotNull(descriptor);
			Assert(Collection.Count == 2);
		}

		#region Test Classes

		class FakeControl : TextBox
		{
			public static PropertyDescriptor[] GetPropertyDescriptors()
			{
				return new PropertyDescriptor[]
				{
					new ControlPropertyDescriptor<FakeControl, string>("Text"),
					new ControlPropertyDescriptor<FakeControl, bool>("ReadOnly"),
				};
			}

			public static PropertyDescriptor[] GetPropertyDescriptorsWithNulls()
			{
				return new PropertyDescriptor[]
				{
					null,
					new ControlPropertyDescriptor<FakeControl, string>("Text"),
					null,
					null,
					new ControlPropertyDescriptor<FakeControl, bool>("ReadOnly"),
					null
				};
			}
		}

		#endregion

		#region Implementation

		ControlPropertyDescriptorCollection Collection
		{
			get
			{
				if (collection == null)
				{
					var defaultDescriptors = ZControlTypeDescriptor.GetPropertyDescriptors(typeof(FakeControl));
					collection = new ControlPropertyDescriptorCollection(typeof(FakeControl), defaultDescriptors);
				}
				return collection;
			}
		}
		ControlPropertyDescriptorCollection collection;

		#endregion
	}
}
