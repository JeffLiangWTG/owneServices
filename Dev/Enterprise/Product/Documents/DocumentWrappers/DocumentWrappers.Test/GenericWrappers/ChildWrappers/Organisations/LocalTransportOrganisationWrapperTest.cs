using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(LocalTransportOrganisationWrapper))]
	sealed class LocalTransportOrganisationWrapperTest : Base.Testing.GenericWrapperTest
	{
		public void TestCompanyCode()
		{
			var localTransportCompanyLabel = "ABC";
			var wrapper = new LocalTransportOrganisationWrapper(OrganisationUsageType.Test, localTransportCompanyLabel, Factory);
			AssertEquals("Wrapper CompanyCode should return LocalTransportCompanyLabel", localTransportCompanyLabel, wrapper.CompanyCode);
		}

		#region Inherited Tests

		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = (LocalTransportOrganisationWrapper)GetNewDocumentWrapper();

			CombineAssertions(() =>
			{
				AssertEquals("TypeDescription", "Test", wrapperEmpty.TypeDescription);
				AssertEquals("CompanyCode", string.Empty, wrapperEmpty.CompanyCode);
				AssertEquals("CompanyName", string.Empty, wrapperEmpty.CompanyName);
				AssertEquals("CompanyNameAndAddress", string.Empty, wrapperEmpty.CompanyNameAndAddress);
				AssertEquals("MainPhone", string.Empty, wrapperEmpty.MainPhone);
				AssertEquals("MainFax", string.Empty, wrapperEmpty.MainFax);
				AssertEquals("MainEmail", string.Empty, wrapperEmpty.MainEmail);
				AssertEquals("ContactName", string.Empty, wrapperEmpty.ContactName);
				AssertEquals("ContactPhone", string.Empty, wrapperEmpty.ContactPhone);
				AssertEquals("ContactFax", string.Empty, wrapperEmpty.ContactFax);
				AssertEquals("ContactEmail", string.Empty, wrapperEmpty.ContactEmail);
				AssertEquals("Contact Count", 0, wrapperEmpty.Contacts.Count);
				AssertEquals("Addresses Count", 0, wrapperEmpty.Addresses.Count);
				AssertEquals("CusCode Count", 0, wrapperEmpty.CustomsCodes.Count);
			});
		}

		#endregion

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
AviationSecurity : (No Default Field Value Available on AviationSecurity)
ClosestPort : 
Country : 
DGContact : 
MainAddress : 
PartAttribute1 : 
PartAttribute2 : 
PartAttribute3 : 
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
LocalTransportOrganisation      (Default Field: CompanyNameAndAddress)
======================================================================
Name                                    Type
----------------------------------------------------------------------
MainAddress                             Address
AviationSecurity                        AviationSecurity
Country                                 Country
DGContact                               DGContact
ClosestPort                             Location
PartAttribute1                          PartAttribute
PartAttribute2                          PartAttribute
PartAttribute3                          PartAttribute
CompanyAddress                          String
CompanyCode                             String
CompanyName                             String
CompanyNameAndAddress                   String
ContactEmail                            String
ContactFax                              String
ContactName                             String
ContactPhone                            String
FirstLineOfCompanyNameAndAddress        String
LocalBusinessRegNo                      String
LocalVATCode                            String
MainEmail                               String
MainFax                                 String
MainPhone                               String
RANumber                                String
RoadCarrierRegistration                 String
RoadCarrierRegistrationHeading          String
SCAC                                    String
TaxCode                                 String
TypeDescription                         String

Addresses                               Address Collection
AssignedStaff                           AssignedStaffMember Collection
Contacts                                Contact Collection
EDocs                                   eDoc Collection
Notes                                   Note Collection
NotesIncludingRelated                   Note Collection
CustomsCodes                            RegistrationNumberCode Collection
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new LocalTransportOrganisationWrapper(OrganisationUsageType.Test, ZString.Empty, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new LocalTransportOrganisationWrapper(OrganisationUsageType.Test, ZString.Empty, Factory);
		}

		#endregion
	}
}
