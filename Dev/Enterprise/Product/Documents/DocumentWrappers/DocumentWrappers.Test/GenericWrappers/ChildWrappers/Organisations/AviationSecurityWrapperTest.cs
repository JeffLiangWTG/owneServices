using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Organisations;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(AviationSecurityWrapper))]
	sealed class AviationSecurityWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var wrapper = new AviationSecurityWrapper(Factory.GetNull<OrgCountryData>(), Factory);

			AssertEquals("NO", wrapper.ApprovalType.Code);
			AssertEquals(ZString.Empty, wrapper.ApprovalNumber);
			AssertEquals(ZDate.Empty, wrapper.ExpiryDate);
			AssertEquals(ZDateTime.Empty, wrapper.LastReviewedDate);
			AssertEquals(ZString.Empty, wrapper.Details);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, wrapper.IssuingAuthorityCountry);
		}

		public void TestWrapperMappingFromOrgCountryData()
		{
			var countryData = Factory.New<OrgCountryData>();
			countryData.OV_EXApprovedOrMajorExporter = "AC";
			countryData.OV_EXApprovalNumber = "AC1234";
			countryData.OV_EXApprovalExpiryDate = ZDate.Today;
			countryData.OV_SystemLastEditTimeUtc = ZDateTime.Today;
			countryData.OV_EXExportPermissionDetails = "Some details.";
			countryData.OV_RN_NKIssuingAuthorityCountry = Core.Constants.CountryCodes.Liechtenstein;

			var wrapper = new AviationSecurityWrapper(countryData, Factory);
			AssertEquals("AC", wrapper.ApprovalType.ToString());
			AssertEquals("AC1234", wrapper.ApprovalNumber);
			AssertEquals(ZDate.Today, wrapper.ExpiryDate);
			AssertEquals(ZDateTime.Today, wrapper.LastReviewedDate);
			AssertEquals("Some details.", wrapper.Details);
			AssertEquals("LI", wrapper.IssuingAuthorityCountry);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new AviationSecurityWrapper(Factory.GetNull<OrgCountryData>(), Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var countryData = Factory.New<OrgCountryData>();

			countryData.OV_EXApprovedOrMajorExporter = "NO";

			return new AviationSecurityWrapper(countryData, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
AviationSecurity
======================================================================
Name                                    Type
----------------------------------------------------------------------
ApprovalType                            CodeAndDescription
ApprovalNumber                          String
Details                                 String
ExpiryDate                              Date
IssuingAuthorityCountry                 String
LastReviewedDate                        DateTime
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
ApprovalType : NO - Does not participate in an Aviation Security Scheme
Registry : (No Default Field Value Available on Registry)
";
			}
		}
	}
}
