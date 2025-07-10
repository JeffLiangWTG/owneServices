using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(UPEOrganisationFilterBusinessObject))]
	public class UPEOrganisationFilterBusinessObjectTest : OrganisationFilterBusinessObjectTest
	{
		public void TestClassifierStaffAssignment()
		{
			GlbStaff classifier = Factory.NewWithValidTestData<GlbStaff>();
			OrgHeader orgWithClassifierAssigned = NewOrgWithStaffAssignment(classifier, UPEStaffRoles.Codes.Classifier);
			GlbStaff regVolStaff = Factory.NewWithValidTestData<GlbStaff>();
			OrgHeader orgWithRegVolAssigned = NewOrgWithStaffAssignment(regVolStaff, UPEStaffRoles.Codes.RV);
			GlbStaff decoyStaff = Factory.NewWithValidTestData<GlbStaff>();
			OrgHeader decoyOrg = NewOrgWithStaffAssignment(decoyStaff, UPEStaffRoles.Codes.Classifier);
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[UPEOrganisationFilterBusinessObject.ClassifierOrRVDescription];
			filter.Property = classifier.PK;
			filter.IsActive = true;
			OrgHeader[] matchedOrganisations = (OrgHeader[])Factory.Load(typeof(OrgHeader), FilterStripBizO.Filter /*FilterObject.Filter*/);
			AssertEquals("Only 1 organisation should match", 1, matchedOrganisations.Length);
			AssertEquals("OrgWithClassifierAssigned should be matched", orgWithClassifierAssigned.PK, matchedOrganisations[0].PK);
			filter.Property = regVolStaff.PK;
			matchedOrganisations = (OrgHeader[])Factory.Load(typeof(OrgHeader), FilterStripBizO.Filter);
			AssertEquals("Only 1 organisation should match", 1, matchedOrganisations.Length);
			AssertEquals("RegVolStaff should be matched", orgWithRegVolAssigned.PK, matchedOrganisations[0].PK);
		}

		public void TestUPSCustomerAccountNumberFilter()
		{
			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.CustomsCodeType];
			AssertNotNull(UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumberDescription + " should exist", filter);
			filter.Property = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			filter.IsActive = true;
			OrgHeader[] matchedOrganisations = Factory.Load<OrgHeader>(FilterStripBizO.Filter);
			AssertEquals("No organisations should match", 0, matchedOrganisations.Length);
			var org = Factory.NewWithValidTestData<UPEOrgHeader>();
			OrgCusCode cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			Assert("cusCode.Lookups.OK_CodeType_List > 0", cusCode.Lookups.OK_CodeType_List.Count > 0);
			cusCode.OK_CustomsRegNo = cusCode.Lookups.OK_CodeType_List[0].Code;
			Factory.Save();
			matchedOrganisations = Factory.Load<OrgHeader>(FilterStripBizO.Filter);
			AssertEquals("Only 1 organisation should match", 1, matchedOrganisations.Length);
			AssertEquals("OrgWithClassifierAssigned should be matched", org.PK, matchedOrganisations[0].PK);
		}

		OrgHeader NewOrgWithStaffAssignment(GlbStaff staff, ZString role)
		{
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			OrgStaffAssignments classifierAssignment = consignee.StaffAssignments.AddNew();
			classifierAssignment.O8_GS_NKPersonResponsible = staff.GS_Code;
			classifierAssignment.O8_Role = role;
			return consignee;
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new UPEOrganisationFilterBusinessObject();
		}

		protected override FilterStripBusinessObject FilterStripBizO
		{
			get
			{
				return fFilterStripBizO ?? (fFilterStripBizO = GetNewFilterStripBusinessObject());
			}
		}

		FilterStripBusinessObject fFilterStripBizO;
	}
}
