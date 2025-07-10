using CargoWise.EntityFramework;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.IN.Business.Testing;

public sealed class CaptionTestHelper
{
	public static void AssertCaptions(ZPropertyInfo propertyInfo, string expectedCaption, string expectedMediumCaption, string expectedShortCaption)
	{
		var name = propertyInfo.Name;
		var resData = DataBoundResourceStrings.GetDataForProperty(propertyInfo);
		AssertNotNull($"{name}: Res String data", resData);
		AssertEquals($"{name}: Caption", expectedCaption, resData.Caption);
		AssertEquals($"{name}: MediumCaption", expectedMediumCaption, resData.MediumCaption);
		AssertEquals($"{name}: ShortCaption", expectedShortCaption, resData.ShortCaption);
	}
}
