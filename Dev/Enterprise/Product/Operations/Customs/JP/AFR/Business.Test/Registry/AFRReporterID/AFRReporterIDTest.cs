using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(AFRReporterID))]
	sealed class AFRReporterIDTest : RegistryBusinessObjectTemplateTestCase<AFRReporterID>
	{
		public void TestValidateReporterID()
		{
			var reporter = new AFRReporterID();

			reporter.ReporterID = "";
			AssertNoNotifications(reporter.ReporterIDInfo);

			reporter.ReporterID = "1234";
			reporter.ReporterID = "";
			AssertHasErrorContaining(reporter.ReporterIDInfo, MandatoryValidation.MustBeEntered);

			reporter.ReporterID = "@@@";
			AssertHasError(reporter.ReporterIDInfo, "AFR Reporter ID should only contain uppercase characters and numbers");

			reporter.ReporterID = "345";
			AssertNoNotifications(reporter.ReporterIDInfo);
		}

		public void TestValidatePassword()
		{
			var reporter = new AFRReporterID
			{
				ReporterID = "12345",
				Password = "123"
			};

			reporter.Password = "";
			AssertHasErrorContaining(reporter.PasswordInfo, MandatoryValidation.MustBeEntered);

			reporter.Password = "12343";
			AssertNoErrorContaining(reporter.PasswordInfo, MandatoryValidation.MustBeEntered);
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override AFRReporterID GetBusinessObjectToClone()
		{
			return new AFRReporterID
			{
				ReporterID = "12345",
				Password = "111"
			};
		}

		protected override AFRReporterID GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
