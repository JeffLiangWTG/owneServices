using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CustomArrivalCustomerReferenceFormat))]
class CustomArrivalCustomerReferenceFormatTest : RegistryBusinessObjectTemplateTestCase
{
	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => true;

	protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
	{
		return GetBusinessObjectToSerialise();
	}

	protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
	{
		var fallbackLevel = new FallbackLevel(Env.CurrentCompany, null, null);
		return new CustomArrivalCustomerReferenceFormat(fallbackLevel, Factory);
	}

	public void TestGetNewValidation()
	{
		AssertType<CustomArrivalCustomerReferenceFormatValidation>(CustomFormat.Validation);
	}

	public void TestLookups() => CombineAssertions(() =>
	{
		AssertType<CustomArrivalCustomerReferenceFormatLookups>(CustomFormat.Lookups);
		AssertSame("cached", CustomFormat.Lookups, CustomFormat.Lookups);
	});

	public void TestAuthorizationLocationCode() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(CustomFormat.AuthorizationLocationCodeInfo, shortCaption: "Auth. Location Code", caption: "Authorization Location Code");
		AssertEquals("default", ZString.Empty, CustomFormat.AuthorizationLocationCode);
	});

	public void TestPrefix() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(CustomFormat.PrefixInfo, caption: "Prefix");
		AssertEquals("MaxLength", 10, CustomFormat.PrefixInfo.MaxLength);
		AssertEquals("default", ZString.Empty, CustomFormat.Prefix);
	});

	public void TestSuffix() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(CustomFormat.SuffixInfo, caption: "Suffix");
		AssertEquals("MaxLength", 10, CustomFormat.SuffixInfo.MaxLength);
		AssertEquals("default", ZString.Empty, CustomFormat.Suffix);
	});

	public void TestYearOption() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(CustomFormat.YearOptionInfo, caption: "Include Year");
		AssertEquals("default", CustomArrivalCustomerReferenceFormatYearOptionList.Codes.None, CustomFormat.YearOption);
	});

	public void TestSequenceNumberLength() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(CustomFormat.SequenceNumberLengthInfo, caption: "Sequence Number Digits");
		AssertEquals("default", new ZByte(8), CustomFormat.SequenceNumberLength);
	});

	public void TestIsRemoveLeadingZeros() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(CustomFormat.IsRemoveLeadingZerosInfo, caption: "Remove Leading Zeros");
		AssertEquals("default", ZBool.False, CustomFormat.IsRemoveLeadingZeros);
	});

	public void TestIsRestartOnNewYear() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(CustomFormat.IsRestartOnNewYearInfo, caption: "Restart on New Year");
		AssertEquals("default", ZBool.False, CustomFormat.IsRestartOnNewYear);
	});

	CustomArrivalCustomerReferenceFormat CustomFormat => customFormat ?? (customFormat = new CustomArrivalCustomerReferenceFormat());
	CustomArrivalCustomerReferenceFormat customFormat;
}
