using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ApplicationLogging.Business.Test
{
	[TestedType(typeof(ApplicationLogger))]
	public class ApplicationLoggerTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var logger = factory.NewWithValidTestData<ApplicationLogger>();
			logger.ALG_Product = "CargoWise";
			return logger;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var logger = Factory.New<ApplicationLogger>();
			logger.ALG_Product = "CargoWise";
			return logger;
		}

		[TestedType(typeof(ApplicationLogger))]
		class ApplicationLoggerAuditParentTest : AuditParentTest<ApplicationLogger>
		{
			protected override ApplicationLogger NewTestAuditParent()
			{
				return Factory.New<ApplicationLogger>();
			}
		}
	}
}
