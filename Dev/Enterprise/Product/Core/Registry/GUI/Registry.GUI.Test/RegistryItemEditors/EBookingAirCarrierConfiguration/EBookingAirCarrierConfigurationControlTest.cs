using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(EBookingAirCarrierConfigurationControl))]
	sealed class EBookingAirCarrierConfigurationControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		public void TestMaxLength()
		{
			using (var control = new EBookingAirCarrierConfigurationControl())
			{
				var lastResponseTextBox = (ZTextBox)control.Controls["LastResponseTextBox"];
				AssertEquals("LastResponseTextBox MaxLength should be big enough to accomodate large text", int.MaxValue, lastResponseTextBox.MaxLength);
			}
		}

		protected override IBusiness GetNewBusinessEntity() => new EBookingCarrierConfiguration();
	}
}
