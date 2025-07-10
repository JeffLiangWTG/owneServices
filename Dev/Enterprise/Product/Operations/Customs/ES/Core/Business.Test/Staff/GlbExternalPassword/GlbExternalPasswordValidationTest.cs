using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(GlbExternalPasswordValidation))]
	public class GlbExternalPasswordValidationTest : GlbExternalPasswordWithCertificateValidationTest<GlbExternalPassword, GlbExternalPasswordValidation>
	{
		public void TestCheckGP_UserID()
		{
			var collection = new GlbExternalPasswordCollection(Factory.New<GlbStaff>());
			var sub1 = collection.AddNew();
			var sub2 = collection.AddNew();

			sub1.GP_Name = ZString.Empty;
			sub2.GP_Name = ZString.Empty;

			var errorCertificateNameMustNotBeEmpty = "The certificate name must not be empty.";
			AssertEquals(true, sub1.GP_NameInfo.HasError(errorCertificateNameMustNotBeEmpty));
			AssertEquals(true, sub2.GP_NameInfo.HasError(errorCertificateNameMustNotBeEmpty));

			sub1.GP_Name = "Test1";
			var errorCertificateNameMustBeUnique = "The certificate name has already been registered, it must be unique.";
			AssertEquals(false, sub1.GP_NameInfo.HasError(errorCertificateNameMustBeUnique));

			sub2.GP_Name = "Test1";
			AssertEquals(true, sub2.GP_NameInfo.HasError(errorCertificateNameMustBeUnique));
		}
	}
}
