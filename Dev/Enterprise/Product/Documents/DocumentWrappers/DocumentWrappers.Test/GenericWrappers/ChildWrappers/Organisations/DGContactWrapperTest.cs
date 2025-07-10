using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(DGContactWrapper))]
	sealed class DGContactWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			DGContactWrapper wrapperEmpty = new DGContactWrapper(null, Factory);

			AssertEquals("wrapperEmpty.ToString()", ZString.Empty, wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Name", ZString.Empty, wrapperEmpty.Name);
			AssertEquals("wrapperEmpty.Phone", ZString.Empty, wrapperEmpty.Phone);
			AssertEquals("wrapperEmpty.PhoneFormatted", ZString.Empty, wrapperEmpty.PhoneFormatted);
			AssertEquals("wrapperEmpty.PhoneType", ZString.Empty, wrapperEmpty.PhoneType);
		}

		public void TestWrapperMappingsOrganisationIsNull()
		{
			OrgMiscServ organisation = null;
			DGContactWrapper wrapperOrg = new DGContactWrapper(organisation, Factory);

			AssertEquals("wrapperOrg.ToString()", ZString.Empty, wrapperOrg.ToString());
			AssertEquals("wrapperOrg.Name", ZString.Empty, wrapperOrg.Name);
			AssertEquals("wrapperOrg.Phone", ZString.Empty, wrapperOrg.Phone);
			AssertEquals("wrapperOrg.PhoneFormatted", ZString.Empty, wrapperOrg.PhoneFormatted);
			AssertEquals("wrapperOrg.PhoneType", ZString.Empty, wrapperOrg.PhoneType);
		}

		public void TestWrapperMappingsFullOrganisationNoContact()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.MiscServ.OM_OC_EXDefaultDGContact = ZGuid.Empty;

			DGContactWrapper wrapperOrg = new DGContactWrapper(organisation.MiscServ, Factory);

			AssertEquals("wrapperOrg.ToString()", ZString.Empty, wrapperOrg.ToString());
			AssertEquals("wrapperOrg.Name", ZString.Empty, wrapperOrg.Name);
			AssertEquals("wrapperOrg.Phone", ZString.Empty, wrapperOrg.Phone);
			AssertEquals("wrapperOrg.PhoneFormatted", ZString.Empty, wrapperOrg.PhoneFormatted);
			AssertEquals("wrapperOrg.PhoneType", ZString.Empty, wrapperOrg.PhoneType);
		}

		public void TestWrapperMappingsFullOrganisationFull()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgContact contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "OSYA BENDER";
			contact.OC_Phone = "1-323-2121";
			contact.OC_HomePhone = "1-323-2122";
			contact.OC_Mobile = "1-323-2123";
			contact.OC_Fax = "1-323-2124";
			organisation.MiscServ.OM_OC_EXDefaultDGContact = contact.PK;
			organisation.MiscServ.OM_EXDefaultDGContactPhoneUsed = PhoneTypeList.Codes.WRK;

			DGContactWrapper wrapperOrg = new DGContactWrapper(organisation.MiscServ, Factory);

			AssertEquals("wrapperOrg.ToString()", "OSYA BENDER", wrapperOrg.ToString());
			AssertEquals("wrapperOrg.Name", "OSYA BENDER", wrapperOrg.Name);
			AssertEquals("wrapperOrg.Phone", "1-323-2121", wrapperOrg.Phone);
			AssertEquals("wrapperOrg.Phone", "1-323-2121", wrapperOrg.PhoneFormatted);
			AssertEquals("wrapperOrg.PhoneType", "WRK", wrapperOrg.PhoneType);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new DGContactWrapper(null, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
DGContact                                        (Default Field: Name)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Name                                    String
Phone                                   String
PhoneFormatted                          String
PhoneType                               String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			return new DGContactWrapper(organisation.MiscServ, Factory);
		}
	}
}
