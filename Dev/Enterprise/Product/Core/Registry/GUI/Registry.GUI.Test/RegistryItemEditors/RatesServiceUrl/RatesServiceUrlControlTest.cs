using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(RatesServiceUrlControl))]
	sealed class RatesServiceUrlControlTest : RegistryZUserControlTestCase
	{
		public void TestButtonTestConnection_InvalidFormat()
		{
			using (var control = new RatesServiceUrlControl())
			{
				control.textBoxUrl.Text = "http:/rates.wisegrid.net";
				control.buttonTestConnection.PerformClick();
				AssertEquals("Invalid URL format", "The URL is of invalid format.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestButtonTestConnection_InvalidURLWithTrailingSpace()
		{
			using (var control = new RatesServiceUrlControl())
			{
				control.textBoxUrl.Text = "http://rates.wisegrid.net ";
				control.buttonTestConnection.PerformClick();
				var invalidUrlMessage = "The Rates Service URL is invalid. Please ensure that it's a valid URL and the service is available.";
				Assert("URL is invalid (Or service is not available)",
					UnitTestUserNotification.Instance.LastMessage.Text.Equals("Test unsuccessful. Please ensure that the Rates Service URL is valid and the service is available.") ||
					UnitTestUserNotification.Instance.LastMessage.Text.Equals(invalidUrlMessage));
			}
		}

		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return null;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((RatesServiceUrlControl)control).ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new RatesServiceUrlControl();
		}

		#endregion
	}
}
