using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentVisualizer.DocDataObjects.Testing
{
	sealed class ValidationExtensionsTest : TestCaseWithFactory
	{
		#region TestAddWarning

		public void TestAddWarning()
		{
			var dummy = new DummyDocDataObject();
			dummy.TextInfo.AddWarning(() => dummy.Text == "aaa", "aaa is not a valid value.");

			dummy.Text = "aaa";
			AssertHasWarning(dummy.TextInfo, "aaa is not a valid value.");

			dummy.Text = "bbb";
			AssertNoWarning(dummy.TextInfo, "aaa is not a valid value.");
		}

		#endregion

		#region TestAddWarningIfEmpty

		public void TestAddWarningIfEmpty()
		{
			var dummy = new DummyDocDataObject();
			dummy.TextInfo.AddWarningIfEmpty();

			dummy.Text = "aaa";
			AssertNoWarning(dummy.TextInfo, "Value is required.");

			dummy.Text = "";
			AssertHasWarning(dummy.TextInfo, "Value is required.");
		}

		#endregion

		#region TestAddWarningIfEmpty_MessageOverride

		public void TestAddWarningIfEmpty_MessageOverride()
		{
			var dummy = new DummyDocDataObject();
			dummy.TextInfo.AddWarningIfEmpty("Text should not be empty.");

			dummy.Text = "aaa";
			AssertNoWarning(dummy.TextInfo, "Text should not be empty.");

			dummy.Text = "";
			AssertHasWarning(dummy.TextInfo, "Text should not be empty.");
		}

		#endregion

		#region TestAddMessageError

		public void TestAddMessageError()
		{
			var dummy = new DummyDocDataObject();
			dummy.TextInfo.AddMessageError(() => dummy.Text == "aaa", "aaa is not a valid value.");

			dummy.Text = "aaa";
			AssertHasMessageError(dummy.TextInfo, "aaa is not a valid value.");

			dummy.Text = "bbb";
			AssertNoMessageError(dummy.TextInfo, "aaa is not a valid value.");
		}

		#endregion

		#region TestAddMessageErrroIfEmpty

		public void TestAddMessageErrroIfEmpty()
		{
			var dummy = new DummyDocDataObject();
			dummy.TextInfo.AddMessageErrorIfEmpty();

			dummy.Text = "aaa";
			AssertNoMessageError(dummy.TextInfo, "Value is required.");

			dummy.Text = "";
			AssertHasMessageError(dummy.TextInfo, "Value is required.");
		}

		#endregion

		#region TestAddMessageErrroIfEmpty_MessageOverride

		public void TestAddMessageErrroIfEmpty_MessageOverride()
		{
			var dummy = new DummyDocDataObject();
			dummy.TextInfo.AddMessageErrorIfEmpty("Text should not be empty.");

			dummy.Text = "aaa";
			AssertNoMessageError(dummy.TextInfo, "Text should not be empty.");

			dummy.Text = "";
			AssertHasMessageError(dummy.TextInfo, "Text should not be empty.");
		}

		#endregion

		#region TestAddError

		public void TestAddError()
		{
			var dummy = new DummyDocDataObject();
			dummy.TextInfo.AddError(() => dummy.Text == "aaa", "aaa is not a valid value.");

			dummy.Text = "aaa";
			AssertHasError(dummy.TextInfo, "aaa is not a valid value.");

			dummy.Text = "bbb";
			AssertNoError(dummy.TextInfo, "aaa is not a valid value.");
		}

		#endregion

		#region TestAddErrorIfEmpty

		public void TestAddErrorIfEmpty()
		{
			var dummy = new DummyDocDataObject();
			dummy.TextInfo.AddErrorIfEmpty();

			dummy.Text = "aaa";
			AssertNoError(dummy.TextInfo, "Value is required.");

			dummy.Text = "";
			AssertHasError(dummy.TextInfo, "Value is required.");
		}

		#endregion

		#region TestAddErrorIfEmpty

		public void TestAddErrorIfEmpty_MessageOverride()
		{
			var dummy = new DummyDocDataObject();
			dummy.TextInfo.AddErrorIfEmpty("Text should not be empty.");

			dummy.Text = "aaa";
			AssertNoError(dummy.TextInfo, "Text should not be empty.");

			dummy.Text = "";
			AssertHasError(dummy.TextInfo, "Text should not be empty.");
		}

		#endregion

		#region TestAddDeliveryError

		public void TestAddDeliveryError()
		{
			var dummy = new DummyDocDataObject();
			dummy.TextInfo.AddDeliveryError(() => dummy.Text == "aaa", "aaa is not a valid value.");

			dummy.Text = "aaa";
			Assert("Contains Delivery Error", dummy.TextInfo.Notifications.Any(n => n.Message == "aaa is not a valid value." && n.Type.EnumValueName == nameof(Core.NotificationType.DeliveryError)));

			dummy.Text = "bbb";
			AssertNoNotifications(dummy.TextInfo);
		}

		#endregion
	}
}
