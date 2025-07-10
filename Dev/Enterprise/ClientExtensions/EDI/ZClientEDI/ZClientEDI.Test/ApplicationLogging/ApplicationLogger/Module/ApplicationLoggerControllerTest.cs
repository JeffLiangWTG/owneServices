using CargoWise.EntityFramework;
using Enterprise.Client.EDI.ApplicationLogging.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ApplicationLogging.GUI.Testing
{
	[TestedType(typeof(ApplicationLoggerController))]
	internal class ApplicationLoggerControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ClientControllerRegistration.ApplicationLogger;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var logger = (ApplicationLogger)Factory.NewWithValidTestData(GetBusinessObjectType());
			logger.ALG_Product = "CargoWise";
			Factory.Save();
			return logger;
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var logger = (ApplicationLogger)base.GetBusinessObjectWithoutValidationErrors();
			logger.ALG_Product = "CargoWise";
			return logger;
		}

		public void TestCheckPointForDelete()
		{
			AssertEquals(EDISecurityCheckpoints.ApplicationLoggingApplicationLoggerDelete, Controller.CheckPointForDeleteExposedForTest);
		}

		public void TestCheckPointForEdit()
		{
			AssertEquals(EDISecurityCheckpoints.ApplicationLoggingApplicationLoggerEdit, Controller.CheckPointForEditExposedForTest);
		}

		public void TestCheckPointForNew()
		{
			AssertEquals(EDISecurityCheckpoints.ApplicationLoggingApplicationLoggerNew, Controller.CheckPointForNewExposedForTest);
		}

		public void TestCheckPointForView()
		{
			AssertEquals(EDISecurityCheckpoints.ApplicationLoggingApplicationLoggerView, Controller.CheckPointForViewExposedForTest);
		}
	}
}
