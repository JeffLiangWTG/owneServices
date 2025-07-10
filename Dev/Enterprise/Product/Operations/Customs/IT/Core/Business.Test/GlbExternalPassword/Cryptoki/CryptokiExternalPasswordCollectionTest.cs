using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(CryptokiExternalPasswordCollection))]
sealed class CryptokiExternalPasswordCollectionTest : OneItemPasswordCollectionTest
{
	public void TestPasswordTypeFilter()
	{
		var collection = (CryptokiExternalPasswordCollection)GetCollectionToTest();
		AssertContains("CompleteFilter should contain XADES password type filter", "GP_PasswordType = 'ITX'", collection.CompleteFilter.LiteralTextADOFormatted);
	}

	public void TestGetCryptokiCertificate()
	{
		var collection = (CryptokiExternalPasswordCollection)GetCollectionToTest();
		AssertNull("GetCryptokiCertificate", collection.GetCryptokiCertificate());

		var cryptokiCertificate = collection.AddNew();
		AssertSame("GetCryptokiCertificate", cryptokiCertificate, collection.GetCryptokiCertificate());
	}

	protected override string ExpectedMaxCountValidationMessage => "Only one XADES certificate is allowed.";

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var staff = Factory.New<GlbStaff>();
		return new CryptokiExternalPasswordCollection(staff);
	}
}
