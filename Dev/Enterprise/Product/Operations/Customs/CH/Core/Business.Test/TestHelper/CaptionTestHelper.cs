using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.CH.Business.Testing;

public sealed class CaptionTestHelper
{
	public static void AssertCaptions(ZPropertyInfo propertyInfo, string multipleResourceKey = null, string caption = null, string shortCaption = null, string mediumCaption = null, string fullDescription = null)
	{
		AssertCaptions(propertyInfo.Name, multipleResourceKey, caption, shortCaption, mediumCaption, fullDescription, DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(propertyInfo, multipleResourceKey, new DataBoundBusinessObject(propertyInfo.BizObj)));
	}

	public static void AssertCaptions<T>(string propertyName, string caption = null, string shortCaption = null, string mediumCaption = null, string fullDescription = null)
	{
		AssertCaptions(propertyName, null, caption, shortCaption, mediumCaption, fullDescription, DataBoundResourceStrings.GetDataForProperty(typeof(T), propertyName));
	}

	static void AssertCaptions(string propertyName, string multipleResourceKey, string caption, string shortCaption, string mediumCaption, string fullDescription, ResourceStringData resourceStringData)
	{
		if (caption != null)
		{
			AssertEquals($"{propertyName} Key={multipleResourceKey} Caption", caption, resourceStringData?.Caption);
		}
		if (shortCaption != null)
		{
			AssertEquals($"{propertyName} Key={multipleResourceKey} ShortCaption", shortCaption, resourceStringData?.ShortCaption);
		}
		if (mediumCaption != null)
		{
			AssertEquals($"{propertyName} Key={multipleResourceKey} MediumCaption", mediumCaption, resourceStringData?.MediumCaption);
		}
		if (fullDescription != null)
		{
			AssertEquals($"{propertyName} Key={multipleResourceKey} FullDescription", fullDescription, resourceStringData?.FullDescription);
		}
	}
}
