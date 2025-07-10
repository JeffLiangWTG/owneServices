using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(AutomaticSignatureExternalPasswordCollection))]
sealed class AutomaticSignatureExternalPasswordCollectionTest : OneItemPasswordCollectionTest
{
	public void TestPasswordTypeFilter()
	{
		var collection = (AutomaticSignatureExternalPasswordCollection)GetCollectionToTest();
		AssertContains("CompleteFilter should contain Automatic Signature password type filter", "GP_PasswordType = 'ITA'", collection.CompleteFilter.LiteralTextADOFormatted);
	}

	protected override string ExpectedMaxCountValidationMessage => "Only one Automatic Signature entry is allowed.";

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var staff = Factory.New<GlbStaff>();
		return new AutomaticSignatureExternalPasswordCollection(staff);
	}
}
