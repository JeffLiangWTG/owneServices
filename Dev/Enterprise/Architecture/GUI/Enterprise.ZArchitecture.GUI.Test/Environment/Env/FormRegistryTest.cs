using System.Drawing;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment
{
	class FormRegistryTest : TransactionedTestCase
	{
		public void TestRegistry()
		{
			var registry = new FormRegistry();

			const string testFormName = "TestFormRegistryForm";
			var result = registry.GetFormLocationAndSize(testFormName);
			AssertEquals("Initially", Rectangle.Empty, result);

			registry.SetFormLocationAndSize(testFormName, new Rectangle(10, 20, 30, 40));
			result = registry.GetFormLocationAndSize(testFormName);
			AssertEquals("GetFormLocationAndSize X", 10, result.X);
			AssertEquals("GetFormLocationAndSize Y", 20, result.Y);
			AssertEquals("GetFormLocationAndSize Width", 30, result.Width);
			AssertEquals("GetFormLocationAndSize Height", 40, result.Height);

			registry.ClearFormLocationAndSize(testFormName);
			result = registry.GetFormLocationAndSize(testFormName);
			AssertEquals("After Clearing", Rectangle.Empty, result);
		}

		[ExpectNoExceptions("SetFormLocation and GetFormLocation should not throw an exception if the environment isn't setup")]
		public void TestRegistryWithNoEnvironment()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			{
				var registry = new FormRegistry();

				const string testFormName = "TestFormRegistryForm";
				var result = registry.GetFormLocationAndSize(testFormName);
				AssertEquals("Initially", Rectangle.Empty, result);

				registry.SetFormLocationAndSize(testFormName, new Rectangle(10, 20, 30, 40));
				result = registry.GetFormLocationAndSize(testFormName);
				AssertEquals("GetFormLocationAndSize X - Should be default value, unable to save with no user.", 0, result.X);
				AssertEquals("GetFormLocationAndSize Y - Should be default value, unable to save with no user.", 0, result.Y);
				AssertEquals("GetFormLocationAndSize Width - Should be default value, unable to save with no user.", 0, result.Width);
				AssertEquals("GetFormLocationAndSize Height - Should be default value, unable to save with no user.", 0, result.Height);

				registry.ClearFormLocationAndSize(testFormName);
				result = registry.GetFormLocationAndSize(testFormName);
				AssertEquals("After Clearing", Rectangle.Empty, result);
			}
		}

		public void TestWithFormPositionNull()
		{
			FormRegistry registry = new FormRegistryWithFormPositionNull();

			const string testFormName = "TestFormRegistryForm";
			var result = registry.GetFormLocationAndSize(testFormName);
			AssertEquals("Initially", Rectangle.Empty, result);

			registry.SetFormLocationAndSize(testFormName, new Rectangle(10, 20, 30, 40));
			result = registry.GetFormLocationAndSize(testFormName);
			AssertEquals("After setting location and size", Rectangle.Empty, result);
		}

		protected class FormRegistryWithFormPositionNull : FormRegistry
		{
			protected override IRegistryItem FormPosition
			{
				get { return null; }
			}
		}
	}
}
