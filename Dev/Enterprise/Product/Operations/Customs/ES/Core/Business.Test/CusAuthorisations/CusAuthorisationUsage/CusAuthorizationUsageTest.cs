using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing;

[TestedType(typeof(CusAuthorizationUsage))]
class CusAuthorizationUsageTest : EnterpriseBusinessObjectTestCase
{
	public void TestValidationType()
	{
		AssertType<CusAuthorizationUsageValidation>(Factory.New<CusAuthorizationUsage>().Validation);
	}

	public void TestAGC_Code() => CombineAssertions(() =>
	{
		AssertResourceStringData(Factory.New<CusAuthorizationUsage>().AGC_CodeInfo, "Type", "Type", "Type", "[12 12 002 000] Type", ["IMPUCC63-26F2-41DA-8AFD-1F49A93B7BDF"]);
	});

	public void TestAGC_Number() => CombineAssertions(() =>
	{
		AssertResourceStringData(Factory.New<CusAuthorizationUsage>().AGC_NumberInfo, "Reference", "Refer.", "Ref.", "[12 12 001 000] Reference", ["IMPUCC63-26F2-41DA-8AFD-1F49A93B7BDF"]);
	});

	void AssertResourceStringData(ZPropertyInfo info, string caption, string mediumCaption, string shortCaption, string fullDescription, string[] multipleResourceKeysInPreferenceOrder = null)
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info.PropertyDescriptor, multipleResourceKeysInPreferenceOrder);
		AssertEquals("Caption", caption, captionResourceString.Caption);
		AssertEquals("MediumCaption", mediumCaption, captionResourceString.MediumCaption);
		AssertEquals("ShortCaption", shortCaption, captionResourceString.ShortCaption);
		AssertEquals("FullDescription", fullDescription, captionResourceString.FullDescription);
	}
}
