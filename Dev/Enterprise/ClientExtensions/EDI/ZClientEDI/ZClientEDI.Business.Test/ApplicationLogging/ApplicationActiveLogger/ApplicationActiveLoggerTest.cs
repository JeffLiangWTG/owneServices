using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ApplicationLogging.Business.Test
{
	[TestedType(typeof(ApplicationActiveLogger))]
	public class ApplicationActiveLoggerTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var logger = factory.NewWithValidTestData<ApplicationLogger>();
			logger.ALG_Product = "CargoWise";

			var activeLogger = factory.New<ApplicationActiveLogger>();
			activeLogger.AAL_Environment = Guid.NewGuid().ToString();
			activeLogger.AAL_ActiveUntil = ZDateTimeOffset.Now;
			activeLogger.AAL_ALG_ApplicationLogger = logger.PK;
			return activeLogger;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var logger = Factory.NewWithValidTestData<ApplicationLogger>();
			logger.ALG_Product = "CargoWise";

			var activeLogger = Factory.New<ApplicationActiveLogger>();
			activeLogger.AAL_Environment = Guid.NewGuid().ToString();
			activeLogger.AAL_ActiveUntil = ZDateTimeOffset.Now;
			activeLogger.AAL_ALG_ApplicationLogger = logger.PK;
			return activeLogger;
		}

		public void TestSettingLicenseSetsEnvironment()
		{
			// Arrange
			var logger = Factory.NewWithValidTestData<ApplicationLogger>();
			logger.ALG_Product = "CargoWise";

			var activeLogger = Factory.New<ApplicationActiveLogger>();
			activeLogger.AAL_Environment = Guid.NewGuid().ToString();
			activeLogger.AAL_ActiveUntil = ZDateTimeOffset.Now;
			activeLogger.AAL_ALG_ApplicationLogger = logger.PK;

			var licence = Factory.NewWithValidTestData<LicenceHeader>();
			Factory.Save();

			// Act
			activeLogger.LicenceGuid = licence.PK;

			// Assert
			AssertEquals($"{licence.Database.EnterpriseCode}{licence.Database.LD_ServerCode}", activeLogger.AAL_Environment);
			AssertNoExceptionThrown(() => Factory.Save());
		}
	}
}
