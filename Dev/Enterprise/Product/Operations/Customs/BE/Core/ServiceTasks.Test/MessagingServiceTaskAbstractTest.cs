using System.Linq;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.BE.ServiceTasks.Testing;

abstract class MessagingServiceTaskAbstractTest<TServiceTask> : ServiceTaskTestCase<TServiceTask> where TServiceTask : MessagingServiceTask, new()
{
	public void TestSingleHostedServiceAttribute()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
		var uniqueAttribute = hostedServiceAttributes.Single();

		CombineAssertions(() =>
		{
			AssertEquals("Category", MessagingServiceTask.MessageServiceTaskCategory, uniqueAttribute.Category);
			AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Belgium, uniqueAttribute.RequiresCompanyInCountry);
			Assert("CanRunInAnyBranch", uniqueAttribute.CanRunInAnyBranch);
			AssertEquals("AllowsMultipleInstances", false, uniqueAttribute.AllowsMultipleInstances);
		});
		AssertSpecificHostedServiceAttributeProperties(uniqueAttribute);
	}

	protected abstract void AssertSpecificHostedServiceAttributeProperties(HostedServiceAttribute attribute);

	public abstract void TestLogging();
}
