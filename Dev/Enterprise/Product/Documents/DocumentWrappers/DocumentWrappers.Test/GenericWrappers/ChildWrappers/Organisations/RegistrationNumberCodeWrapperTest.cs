using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RegistrationNumberCodeWrapper))]
	sealed class RegistrationNumberCodeWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			OrgCusCode orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_RN_NKCodeCountry = ZString.Empty;
			RegistrationNumberCodeWrapper emptyWrapper = new RegistrationNumberCodeWrapper(orgCusCode, Factory);
			AssertEquals("emptyWrapper.Type.Code", ZString.Empty, emptyWrapper.Type.Code);
			AssertEquals("emptyWrapper.CountryOfIssue.Code", ZString.Empty, emptyWrapper.CountryOfIssue.Code);
			AssertEquals("emptyWrapper.PremisesAddress.Address", ZString.Empty, emptyWrapper.PremisesAddress.Address);
			AssertEquals("emptyWrapper.RegistrationNumberOrCode", ZString.Empty, emptyWrapper.RegistrationNumberOrCode);
		}

		public void TestWrapperMappingsFull()
		{
			OrgAddress premisesAddress = Factory.New<OrgAddress>();
			premisesAddress.OA_Address1 = "68 I OWE U 1 PREMISES ADDRESS";
			premisesAddress.OA_Address2 = "ADDITIONAL LINE";
			premisesAddress.OA_RN_NKCountryCode = "AU";

			OrgCusCode orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CustomsRegNo = "27010649377";
			orgCusCode.OK_CodeType = "GST";
			orgCusCode.OK_RN_NKCodeCountry = "AU";
			orgCusCode.OK_OA_PremisesAddress = premisesAddress.PK;

			RegistrationNumberCodeWrapper fullWrapper = new RegistrationNumberCodeWrapper(orgCusCode, Factory);
			AssertEquals("emptyWrapper.Type.Code", "GST", fullWrapper.Type.Code);
			AssertEquals("emptyWrapper.CountryOfIssue.Code", "AU", fullWrapper.CountryOfIssue.Code);
			AssertEquals("emptyWrapper.PremisesAddress.Address", "68 I OWE U 1 PREMISES ADDRESS\nADDITIONAL LINE\nAUSTRALIA", fullWrapper.PremisesAddress.Address);
			AssertEquals("emptyWrapper.RegistrationNumberOrCode", "27010649377", fullWrapper.RegistrationNumberOrCode);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
RegistrationNumberCode       (Default Field: RegistrationNumberOrCode)
======================================================================
Name                                    Type
----------------------------------------------------------------------
PremisesAddress                         Address
Type                                    CodeAndDescription
CountryOfIssue                          Country
RegistrationNumberOrCode                String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CountryOfIssue : AU - Australia
PremisesAddress : 68 I OWE U 1 PREMISES ADDRESS\nADDITIONAL LINE\nAUSTRALIA
Registry : (No Default Field Value Available on Registry)
Type : ABN - Australian Business Number (GST Registration Code)

";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgAddress premisesAddress = Factory.New<OrgAddress>();
			premisesAddress.OA_Address1 = "68 I OWE U 1 PREMISES ADDRESS";
			premisesAddress.OA_Address2 = "ADDITIONAL LINE";
			premisesAddress.OA_RN_NKCountryCode = "AU";

			OrgCusCode orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CodeType = "ABN";
			orgCusCode.OK_RN_NKCodeCountry = "AU";
			orgCusCode.OK_OA_PremisesAddress = premisesAddress.PK;

			return new RegistrationNumberCodeWrapper(orgCusCode, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new RegistrationNumberCodeWrapper(null, Factory);
		}
	}
}
