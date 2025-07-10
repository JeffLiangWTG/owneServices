using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CustomerService.Module.Testing
{
	[TestedType(typeof(IncidentApprovalFilterBusinessObject))]
	sealed class IncidentApprovalFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new IncidentApprovalFilterBusinessObject();
		}

		#region Module / Requirement / Service

		public void TestMenuSectionFilter()
		{
			CreateTestDataForModuleTesting();

			var filterBizObj = new IncidentApprovalFilterBusinessObject();
			var menuSectionFilter = (ModuleTextFilter)filterBizObj["Menu Section"];
			AssertContainsExactElementsInAnyOrder(new IncidentApprovalLookups(null).MenuSectionList, menuSectionFilter.List);
			menuSectionFilter.IsActive = true;

			menuSectionFilter.Property = "DOC";
			AssertFilteredResult(filterBizObj, "101", "104", "107");

			menuSectionFilter.Property = Cr8ModuleList.Codes.CarbonEnvironmentalCompliance;
			AssertFilteredResult(filterBizObj, "102", "105");

			menuSectionFilter.Property = Cr9ModuleList.Codes.ConsultingAssistanceOnDocbuilderCustomization;
			AssertFilteredResult(filterBizObj, "103", "106");
		}

		public void TestRequirementFilter()
		{
			CreateTestDataForModuleTesting();

			var filterBizObj = new IncidentApprovalFilterBusinessObject();
			var requirementFilter = (ModuleTextFilter)filterBizObj["Requirement"];
			AssertContainsExactElementsInAnyOrder(new IncidentApprovalLookups(null).Cr8ModuleList, requirementFilter.List);
			requirementFilter.IsActive = true;

			requirementFilter.Property = "DOC";
			AssertFilteredResult(filterBizObj, "108");

			requirementFilter.Property = Cr8ModuleList.Codes.CarbonEnvironmentalCompliance;
			AssertFilteredResult(filterBizObj, "109");

			requirementFilter.Property = Cr9ModuleList.Codes.ConsultingAssistanceOnDocbuilderCustomization;
			AssertFilteredResult(filterBizObj, "110");
		}

		public void TestServiceFilter()
		{
			CreateTestDataForModuleTesting();

			var filterBizObj = new IncidentApprovalFilterBusinessObject();
			var serviceFilter = (ModuleTextFilter)filterBizObj["Service"];
			AssertContainsExactElementsInAnyOrder(new IncidentApprovalLookups(null).Cr9ModuleList, serviceFilter.List);
			serviceFilter.IsActive = true;

			serviceFilter.Property = "DOC";
			AssertFilteredResult(filterBizObj, "111");

			serviceFilter.Property = Cr8ModuleList.Codes.CarbonEnvironmentalCompliance;
			AssertFilteredResult(filterBizObj, "112");

			serviceFilter.Property = Cr9ModuleList.Codes.ConsultingAssistanceOnDocbuilderCustomization;
			AssertFilteredResult(filterBizObj, "113");
		}

		void CreateTestDataForModuleTesting()
		{
			#region Menu Section Incidents (101 to 107)

			new IncidentApprovalBuilder(Factory, "101")
				.SetValue(IncidentApprovalSchema.IA_Criticality, Constants.CustomerService.CriticalityCodes.CR1_SystemDown)
				.SetValue(IncidentApprovalSchema.IA_Module, "DOC");

			new IncidentApprovalBuilder(Factory, "102")
				.SetValue(IncidentApprovalSchema.IA_Criticality, Constants.CustomerService.CriticalityCodes.CR2_ModuleDown)
				.SetValue(IncidentApprovalSchema.IA_Module, Cr8ModuleList.Codes.CarbonEnvironmentalCompliance);

			new IncidentApprovalBuilder(Factory, "103")
				.SetValue(IncidentApprovalSchema.IA_Criticality, Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround)
				.SetValue(IncidentApprovalSchema.IA_Module, Cr9ModuleList.Codes.ConsultingAssistanceOnDocbuilderCustomization);

			new IncidentApprovalBuilder(Factory, "104")
				.SetValue(IncidentApprovalSchema.IA_Criticality, Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround)
				.SetValue(IncidentApprovalSchema.IA_Module, "DOC");

			new IncidentApprovalBuilder(Factory, "105")
				.SetValue(IncidentApprovalSchema.IA_Criticality, Constants.CustomerService.CriticalityCodes.CR5_Training)
				.SetValue(IncidentApprovalSchema.IA_Module, Cr8ModuleList.Codes.CarbonEnvironmentalCompliance);

			new IncidentApprovalBuilder(Factory, "106")
				.SetValue(IncidentApprovalSchema.IA_Criticality, Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest)
				.SetValue(IncidentApprovalSchema.IA_Module, Cr9ModuleList.Codes.ConsultingAssistanceOnDocbuilderCustomization);

			new IncidentApprovalBuilder(Factory, "107")
				.SetValue(IncidentApprovalSchema.IA_Criticality, Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest)
				.SetValue(IncidentApprovalSchema.IA_Module, "DOC");

			#endregion

			#region CR8 Incidents (108 to 110)

			new IncidentApprovalBuilder(Factory, "108")
				.SetValue(IncidentApprovalSchema.IA_Criticality, Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement)
				.SetValue(IncidentApprovalSchema.IA_Module, "DOC");

			new IncidentApprovalBuilder(Factory, "109")
				.SetValue(IncidentApprovalSchema.IA_Criticality, Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement)
				.SetValue(IncidentApprovalSchema.IA_Module, Cr8ModuleList.Codes.CarbonEnvironmentalCompliance);

			new IncidentApprovalBuilder(Factory, "110")
				.SetValue(IncidentApprovalSchema.IA_Criticality, Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement)
				.SetValue(IncidentApprovalSchema.IA_Module, Cr9ModuleList.Codes.ConsultingAssistanceOnDocbuilderCustomization);

			#endregion

			#region CR9 Incidents (111 to 113)

			new IncidentApprovalBuilder(Factory, "111")
				.SetValue(IncidentApprovalSchema.IA_Criticality, Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest)
				.SetValue(IncidentApprovalSchema.IA_Module, "DOC");

			new IncidentApprovalBuilder(Factory, "112")
				.SetValue(IncidentApprovalSchema.IA_Criticality, Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest)
				.SetValue(IncidentApprovalSchema.IA_Module, Cr8ModuleList.Codes.CarbonEnvironmentalCompliance);

			new IncidentApprovalBuilder(Factory, "113")
				.SetValue(IncidentApprovalSchema.IA_Criticality, Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest)
				.SetValue(IncidentApprovalSchema.IA_Module, Cr9ModuleList.Codes.ConsultingAssistanceOnDocbuilderCustomization);

			#endregion
		}

		#endregion

		#region TestStatusFilter

		public void TestStatusFilter()
		{
			var inc1 = Factory.New<IncidentApproval>();
			inc1.IA_Status = IncidentApprovalLookups.StatusCodes.New;

			var inc2 = Factory.New<IncidentApproval>();
			inc2.IA_Status = IncidentApprovalLookups.StatusCodes.PendingFeatureResult;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Status"];
			var results = new IncidentApprovalCollection(Factory);

			filter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(inc1.PK));
			Assert("Not filtered", results.Contains(inc2.PK));

			filter.Property = IncidentApprovalLookups.StatusCodes.New;
			results.Load(FilterStripBizO.Filter);
			Assert("This incident has New status", results.Contains(inc1.PK));
			Assert("This incident has Pending Feature Result status", !results.Contains(inc2.PK));

			filter.Property = IncidentApprovalLookups.StatusCodes.PendingFeatureResult;
			results.Load(FilterStripBizO.Filter);
			Assert("This incident has New status", !results.Contains(inc1.PK));
			Assert("This incident has Pending Feature Result status", results.Contains(inc2.PK));

			filter.Property = IncidentApprovalLookups.StatusCodes.FormalQuotationProvided;
			results.Load(FilterStripBizO.Filter);
			Assert("This incident has New status", !results.Contains(inc1.PK));
			Assert("This incident has Pending Feature Result status", !results.Contains(inc2.PK));
		}

		#endregion

		#region TestCustomerStatusFilter

		public void TestCustomerStatusFilter()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("BUG", "It's a bug");
			list.AddPair("FTR", "It's a feature");
			SystemDataRegistry.Instance.CustomerStatuses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var inc1 = Factory.New<IncidentApproval>();
			inc1.IA_ClientSpecifiedStatus = "BUG";

			var inc2 = Factory.New<IncidentApproval>();
			inc2.IA_ClientSpecifiedStatus = "FTR";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Customer Status"];
			var results = new IncidentApprovalCollection(Factory);

			filter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(inc1.PK));
			Assert("Not filtered", results.Contains(inc2.PK));

			filter.Property = "BUG";
			results.Load(FilterStripBizO.Filter);
			Assert("This incident has BUG customer status", results.Contains(inc1.PK));
			Assert("This incident has FTR customer status", !results.Contains(inc2.PK));

			filter.Property = "FTR";
			results.Load(FilterStripBizO.Filter);
			Assert("This incident has BUG customer status", !results.Contains(inc1.PK));
			Assert("This incident has FTR customer status", results.Contains(inc2.PK));

			filter.Property = "FOO";
			results.Load(FilterStripBizO.Filter);
			Assert("This incident has BUG customer status", !results.Contains(inc1.PK));
			Assert("This incident has FTR customer status", !results.Contains(inc2.PK));
		}

		#endregion

		#region TestCompanyFilter

		public void TestCompanyFilter()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "TTA";
			company1.GC_Name = "ZZZ";

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "TTZ";
			company2.GC_Name = "AAA";

			var inc1 = Factory.New<IncidentApproval>();
			inc1.IA_LicenceCode = company1.LicenceKeyIdentifier;

			var inc2 = Factory.New<IncidentApproval>();
			inc2.IA_LicenceCode = company2.LicenceKeyIdentifier;

			var filter = (ModuleTextFilter)FilterStripBizO["Company"];
			var results = new IncidentApprovalCollection(Factory);

			filter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(inc1.PK));
			Assert("Not filtered", results.Contains(inc2.PK));

			filter.Property = company1.LicenceKeyIdentifier;
			results.Load(FilterStripBizO.Filter);
			Assert("This incident is for company1", results.Contains(inc1.PK));
			Assert("This incident is for company2", !results.Contains(inc2.PK));

			filter.Property = company2.LicenceKeyIdentifier;
			results.Load(FilterStripBizO.Filter);
			Assert("This incident is for company1", !results.Contains(inc1.PK));
			Assert("This incident is for company2", results.Contains(inc2.PK));

			filter.Property = "FOO";
			results.Load(FilterStripBizO.Filter);
			Assert("This incident is for company1", !results.Contains(inc1.PK));
			Assert("This incident is for company2", !results.Contains(inc2.PK));
		}

		#endregion

		#region TestReportingUser

		public void TestReportingUser()
		{
			var user = Factory.New<GlbStaff>();
			user.GS_Code = "AA";
			var user2 = Factory.New<GlbStaff>();
			user2.GS_Code = "BB";

			var inc1 = Factory.New<IncidentApproval>();
			inc1.IA_SystemCreateUser = user.GS_Code;

			var inc2 = Factory.New<IncidentApproval>();
			inc2.IA_GS_NKReportingStaff = user.GS_Code;
			inc2.IA_SystemCreateUser = user2.GS_Code;

			var filter = (ModuleNkFilter)FilterStripBizO["Reporting User"];
			var results = new IncidentApprovalCollection(Factory);

			filter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(inc1.PK));
			Assert("Not filtered", results.Contains(inc2.PK));

			filter.Property = user.GS_Code;
			results.Load(FilterStripBizO.Filter);
			Assert("This incident is created by AA and shows up as reported by AA", results.Contains(inc1.PK));
			Assert("This incident is created by BB and shows up as 3rd party AA but reported by BB", !results.Contains(inc2.PK));

			filter.Property = user2.GS_Code;
			results.Load(FilterStripBizO.Filter);
			Assert("This incident is created by AA and shows up as reported by AA", !results.Contains(inc1.PK));
			Assert("This incident is created by BB and shows up as 3rd party AA but reported by BB", results.Contains(inc2.PK));
		}

		#endregion

		#region Implementation

		void AssertFilteredResult(IncidentApprovalFilterBusinessObject filterBizO, params string[] expectedIncidents)
		{
			var actualIncidents = new IncidentApprovalCollection(Factory);
			actualIncidents.Load(filterBizO.Filter);
			var actualIncidentNumbers = actualIncidents.Select(incident => incident.IA_IncidentNumber.ToString());

			AssertContainsExactElementsInAnyOrder(expectedIncidents, actualIncidentNumbers);
		}

		FilterStripBusinessObject FilterStripBizO
		{
			get
			{
				if (fFilterStripBizO == null)
				{
					fFilterStripBizO = GetNewFilterStripBusinessObject();
				}
				return fFilterStripBizO;
			}
		}

		FilterStripBusinessObject fFilterStripBizO;

		#region Helper Classes

		class IncidentApprovalBuilder
		{
			public IncidentApprovalBuilder(BusinessObjectFactory factory, string incidentNumber)
			{
				incident = factory.New<IncidentApproval>();
				incident.IA_IncidentNumber = incidentNumber;
			}

			readonly IncidentApproval incident;

			public IncidentApprovalBuilder SetValue(SchemaColumn column, object value)
			{
				incident[column] = value;
				return this;
			}
		}

		#endregion

		#endregion
	}
}
