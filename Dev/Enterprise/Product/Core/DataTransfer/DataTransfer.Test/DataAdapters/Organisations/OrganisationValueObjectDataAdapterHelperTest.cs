using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	sealed class OrganisationValueObjectDataAdapterHelperTest : TestCaseWithFactory
	{
		public void TestIsUSDeprecatedSAN()
		{
			var number = new Xsd.RegistrationNumber();
			number.NumberType = Xsd.RegistrationNumberTypes.SAN;
			number.CountryOfRegistration = Core.Constants.CountryCodes.UnitedStates;
			number.Number = "SANTEST";
			var notification = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notification);
			Assert(OrganisationValueObjectDataAdapterHelper.IsUSDeprecatedSAN(number, "XXX", context));
			Assert(notification.HasWarnings);
		}
	}
}
