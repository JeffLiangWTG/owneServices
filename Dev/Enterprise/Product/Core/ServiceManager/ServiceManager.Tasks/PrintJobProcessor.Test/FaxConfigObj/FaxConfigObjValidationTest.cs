using CargoWise.EntityFramework.Testing;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	sealed class FaxConfigObjValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckLocalCountry()
		{
			IServiceTaskSchedule taskSchedule = Factory.New<IServiceTaskSchedule>();
			FaxConfigObj configObj1 = new FaxConfigObj(taskSchedule);

			configObj1.LocalCountry = "AU";
			AssertNoErrors(configObj1.LocalCountryInfo);

			configObj1.LocalCountry = "##";
			AssertHasError(configObj1.LocalCountryInfo, "Enter a valid selection.");

			configObj1.LocalCountry = "";
			AssertHasError(configObj1.LocalCountryInfo, "Please enter a value.");
		}
	}
}
