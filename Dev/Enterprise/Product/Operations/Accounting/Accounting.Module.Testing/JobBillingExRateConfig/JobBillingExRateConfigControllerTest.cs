using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobBillingExRateConfigController))]
	public class JobBillingExRateConfigControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobBillingExRateSysConfig;
		}

		public void TestGetForm_ReturnsForm_WithCorrectBottomToolstrips()
		{
			var configurationCollection = new AccExchangeRateConfigurationCollection(Factory);
			var configuration = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			configurationCollection.Add(configuration);

			using (var form = (ZForm)configController.ShowNewForm())
			{
				AssertNotNull("Form should not be null when open it", form);
				AssertEquals("ODisplayMode of the form should be Browse", ODisplayMode.Browse, form.DisplayMode);
			}
		}

		protected JobBillingExRateConfigController configController => Controller as JobBillingExRateConfigController;
	}
}
