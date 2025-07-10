using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Business.Testing;

public class CusClassificationTest : TestCaseWithFactory
{
	public void TestDefaultValues()
	{
		AssertEquals("Country is set", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, testClassification.CC_RN_NKCountryCode);
		AssertEquals("Type is set", CusClassification.ClassificationType.Both, testClassification.CC_ClassificationType);
	}

	#region Implementation
	protected CusClassification testClassification;
	protected override void SetUp()
	{
		base.SetUp();
		testClassification = Factory.New<CusClassification>();
	}
	#endregion
}
