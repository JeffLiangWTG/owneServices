using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.ApplicationLogging.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ApplicationLogging.GUI.Testing
{
	[TestedType(typeof(ApplicationActiveLoggerController))]
	internal class ApplicationActiveLoggerControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ClientControllerRegistration.ApplicationActiveLogger;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var logger = Factory.NewWithValidTestData<ApplicationLogger>();
			logger.ALG_Product = "CargoWise";

			var activeLogger = Factory.New<ApplicationActiveLogger>();
			activeLogger.AAL_Environment = Guid.NewGuid().ToString();
			activeLogger.AAL_ActiveUntil = ZDateTimeOffset.Now;
			activeLogger.AAL_ALG_ApplicationLogger = logger.PK;
			Factory.Save();
			return activeLogger;
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var logger = Factory.NewWithValidTestData<ApplicationLogger>();
			logger.ALG_Product = "CargoWise";

			var activeLogger = Factory.New<ApplicationActiveLogger>();
			activeLogger.AAL_Environment = Guid.NewGuid().ToString();
			activeLogger.AAL_ActiveUntil = ZDateTimeOffset.Now;
			activeLogger.AAL_ALG_ApplicationLogger = logger.PK;
			return activeLogger;
		}

		public void TestCheckPointForDelete()
		{
			AssertEquals(EDISecurityCheckpoints.ApplicationLoggingApplicationActiveLoggerDelete, Controller.CheckPointForDeleteExposedForTest);
		}

		public void TestCheckPointForEdit()
		{
			AssertEquals(EDISecurityCheckpoints.ApplicationLoggingApplicationActiveLoggerEdit, Controller.CheckPointForEditExposedForTest);
		}

		public void TestCheckPointForNew()
		{
			AssertEquals(EDISecurityCheckpoints.ApplicationLoggingApplicationActiveLoggerNew, Controller.CheckPointForNewExposedForTest);
		}

		public void TestCheckPointForView()
		{
			AssertEquals(EDISecurityCheckpoints.ApplicationLoggingApplicationActiveLoggerView, Controller.CheckPointForViewExposedForTest);
		}
	}
}
