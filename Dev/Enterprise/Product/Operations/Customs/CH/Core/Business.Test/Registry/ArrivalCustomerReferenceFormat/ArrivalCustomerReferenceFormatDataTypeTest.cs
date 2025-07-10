using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ArrivalCustomerReferenceFormatDataType))]
class ArrivalCustomerReferenceFormatDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ArrivalCustomerReferenceFormatDataType>
{
	protected override ArrivalCustomerReferenceFormatDataType GetNewDataType()
	{
		return new ArrivalCustomerReferenceFormatDataType();
	}

	protected override string ExpectedEditorName => "ArrivalCustomerReferenceFormatConfigRegistryItemEditor";

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var factory = new BusinessObjectFactory();
		var orgHeader = factory.New<OrgHeader>();
		orgHeader.OH_Code = "ORG00001";
		GlbCompany.GetCurrentCompany(factory).GC_OH_OrgProxy = orgHeader.PK;
		var authorisationTestHelper = new CusAuthorisationTestHelper(factory, defaultPermitHolder: orgHeader);
		authorisationTestHelper.CreateCusAuthorisationHeader("A1", CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar);
		authorisationTestHelper.CreateCusAuthorisationHeader("A2", CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar);
		authorisationTestHelper.CreateCusAuthorisationHeader("B1", CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar);
		authorisationTestHelper.CreateCusAuthorisationHeader("B2", CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar);
		factory.Save();

		var fallback = new FallbackLevel(GlbCompany.CurrentCompany, null, null);

		var referenceFormat1 = new ArrivalCustomerReferenceFormat(fallback, null);
		referenceFormat1.UseSystemDefinedFormat = ZBool.True;
		var customFormat11 = referenceFormat1.CustomFormats.AddNew();
		customFormat11.AuthorizationLocationCode = "A1";
		customFormat11.Prefix = "P1";
		customFormat11.Suffix = "S1";
		customFormat11.YearOption = "2";
		customFormat11.SequenceNumberLength = 14;
		customFormat11.IsRemoveLeadingZeros = ZBool.True;
		customFormat11.IsRestartOnNewYear = ZBool.False;
		var customFormat12 = referenceFormat1.CustomFormats.AddNew();
		customFormat12.AuthorizationLocationCode = "A2";
		customFormat12.IsRemoveLeadingZeros = ZBool.False;
		customFormat12.IsRestartOnNewYear = ZBool.True;

		var referenceFormat2 = new ArrivalCustomerReferenceFormat(fallback, null);
		referenceFormat2.UseSystemDefinedFormat = ZBool.False;
		var customFormat21 = referenceFormat2.CustomFormats.AddNew();
		customFormat21.AuthorizationLocationCode = "B1";
		var customFormat22 = referenceFormat2.CustomFormats.AddNew();
		customFormat22.AuthorizationLocationCode = "B2";

		var byteArrayValue1 = Encoding.Unicode.GetBytes(stripSpaces(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrivalCustomerReferenceFormat>
  <UseSystemDefinedFormat>Y</UseSystemDefinedFormat>
  <ArrayOfCustomArrivalCustomerReferenceFormat xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <CustomArrivalCustomerReferenceFormat>
      <AuthorizationLocationCode>A1</AuthorizationLocationCode>
      <Prefix>P1</Prefix>
      <Suffix>S1</Suffix>
      <YearOption>2</YearOption>
      <SequenceNumberLength>14</SequenceNumberLength>
      <IsRemoveLeadingZeros>Y</IsRemoveLeadingZeros>
      <IsRestartOnNewYear>N</IsRestartOnNewYear>
    </CustomArrivalCustomerReferenceFormat>
    <CustomArrivalCustomerReferenceFormat>
      <AuthorizationLocationCode>A2</AuthorizationLocationCode>
      <Prefix />
      <Suffix />
      <YearOption>0</YearOption>
      <SequenceNumberLength>8</SequenceNumberLength>
      <IsRemoveLeadingZeros>N</IsRemoveLeadingZeros>
      <IsRestartOnNewYear>Y</IsRestartOnNewYear>
    </CustomArrivalCustomerReferenceFormat>
  </ArrayOfCustomArrivalCustomerReferenceFormat>
</ArrivalCustomerReferenceFormat>"));

		var byteArrayValue2 = Encoding.Unicode.GetBytes(stripSpaces(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrivalCustomerReferenceFormat>
  <UseSystemDefinedFormat>N</UseSystemDefinedFormat>
  <ArrayOfCustomArrivalCustomerReferenceFormat xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <CustomArrivalCustomerReferenceFormat>
      <AuthorizationLocationCode>B1</AuthorizationLocationCode>
      <Prefix />
      <Suffix />
      <YearOption>0</YearOption>
      <SequenceNumberLength>8</SequenceNumberLength>
      <IsRemoveLeadingZeros>N</IsRemoveLeadingZeros>
      <IsRestartOnNewYear>N</IsRestartOnNewYear>
    </CustomArrivalCustomerReferenceFormat>
    <CustomArrivalCustomerReferenceFormat>
      <AuthorizationLocationCode>B2</AuthorizationLocationCode>
      <Prefix />
      <Suffix />
      <YearOption>0</YearOption>
      <SequenceNumberLength>8</SequenceNumberLength>
      <IsRemoveLeadingZeros>N</IsRemoveLeadingZeros>
      <IsRestartOnNewYear>N</IsRestartOnNewYear>
    </CustomArrivalCustomerReferenceFormat>
  </ArrayOfCustomArrivalCustomerReferenceFormat>
</ArrivalCustomerReferenceFormat>"));

		return new[]
		{
				new ValidSampleAndBinaryValueInDB(referenceFormat1, byteArrayValue1),
				new ValidSampleAndBinaryValueInDB(referenceFormat2, byteArrayValue2),
			};
	}

	static string stripSpaces(string xml) => Regex.Replace(xml, @"\r\s*", "");
}
