using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ArrivalCustomerReferenceFormat))]
class ArrivalCustomerReferenceFormatTest : RegistryBusinessObjectTemplateTestCase<ArrivalCustomerReferenceFormat>
{
	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => true;

	protected override BusinessObject GetNewBusinessObject()
	{
		return new ArrivalCustomerReferenceFormat();
	}

	protected override ArrivalCustomerReferenceFormat GetBusinessObjectToClone()
	{
		return GetBusinessObjectToSerialise();
	}

	protected override ArrivalCustomerReferenceFormat GetBusinessObjectToSerialise()
	{
		var referenceFormat = new ArrivalCustomerReferenceFormat();

		var customFormat1 = referenceFormat.CustomFormats.AddNew();
		var customFormat2 = referenceFormat.CustomFormats.AddNew();
		customFormat1.AuthorizationLocationCode = "A1";
		customFormat2.AuthorizationLocationCode = "A2";
		return referenceFormat;
	}

	public void TestUseSystemDefinedFormat() => CombineAssertions(() =>
	{
		var referenceFormat = new ArrivalCustomerReferenceFormat();
		CaptionTestHelper.AssertCaptions(referenceFormat.UseSystemDefinedFormatInfo, caption: "Use system-defined format");
		AssertEquals("default", ZBool.True, referenceFormat.UseSystemDefinedFormat);
	});

	public void TestNotUseSystemDefinedFormat() => CombineAssertions(() =>
	{
		var referenceFormat = new ArrivalCustomerReferenceFormat();
		referenceFormat.UseSystemDefinedFormat = ZBool.False;
		AssertEquals("UseSystemDefinedFormat = ZBool.False", ZBool.True, referenceFormat.NotUseSystemDefinedFormat);
		referenceFormat.UseSystemDefinedFormat = ZBool.True;
		AssertEquals("UseSystemDefinedFormat = ZBool.True", ZBool.False, referenceFormat.NotUseSystemDefinedFormat);
		referenceFormat.NotUseSystemDefinedFormat = ZBool.True;
		AssertEquals("NotUseSystemDefinedFormat = ZBool.True", ZBool.False, referenceFormat.UseSystemDefinedFormat);
		referenceFormat.NotUseSystemDefinedFormat = ZBool.False;
		AssertEquals("NotUseSystemDefinedFormat = ZBool.False", ZBool.True, referenceFormat.UseSystemDefinedFormat);
	});

	public void TestCustomFormats()
	{
		var referenceFormat = new ArrivalCustomerReferenceFormat();
		var customformat = referenceFormat.CustomFormats.AddNew();
		AssertSame("Collection works", customformat, referenceFormat.CustomFormats.First());
	}

	public void TestGetFormatByAuthorizationLocationCodeEmtpy()
	{
		var referenceFormat = new ArrivalCustomerReferenceFormat();
		AssertEquals("GetFormatByAuthorizationLocationCode returns null when reference format is not present.", null, referenceFormat.GetFormatByAuthorizationLocationCode("001"));
	}

	public void TestGetFormatByAuthorizationLocationCodeValid()
	{
		var destinationTraderOrg = Factory.New<OrgHeader>();
		destinationTraderOrg.OH_Code = "1234";
		destinationTraderOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, "2233445566", "CH");

		var authorisationHeader = Factory.New<CusAuthorisationHeader>();
		authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar;
		authorisationHeader.CPH_OH_PermitHolder = destinationTraderOrg.PK;
		authorisationHeader.CPH_Number = "001";
		authorisationHeader.CPH_StartDate = ZDate.Today.AddDays(-10);
		authorisationHeader.CPH_EndDate = ZDate.Today.AddDays(10);

		var fallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany, null, null);
		var referenceFormat = new ArrivalCustomerReferenceFormat();
		var customFormat = new CustomArrivalCustomerReferenceFormat(fallbackLevel, Factory);
		customFormat.AuthorizationLocationCode = "001";
		referenceFormat.CustomFormats.Add(customFormat);

		AssertEquals("GetFormatByAuthorizationLocationCode returns reference format based on the AuthorizedLocationCode.", customFormat, referenceFormat.GetFormatByAuthorizationLocationCode("001"));
	}
}
