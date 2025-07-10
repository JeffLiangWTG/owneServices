using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIOrgOpportunity))]
	public class EDIOrgOpportunityTest : OrgOpportunityTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<EDIOrgOpportunity>();
		}

		protected override OrgOpportunity CreateOpportunity(bool withValidTestData)
		{
			return withValidTestData ? Factory.NewWithValidTestData<EDIOrgOpportunity>() : Factory.New<EDIOrgOpportunity>();
		}

		public void TestLookups()
		{
			var opportunity = CreateOpportunity(false);
			AssertEquals(typeof(EDIOrgOpportunityLookups), opportunity.Lookups.GetType());
		}

		public override void TestNoteTypes()
		{
			var org = Factory.New<EDIOrgHeader>();
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();
			AssertEquals(2, opportunity.NoteTypes.Count);
			AssertCollectionContains(EDIPredefinedNoteTypes.Instance.OpportunityFollowUpNote, opportunity.NoteTypes);
			AssertCollectionContains(EDIPredefinedNoteTypes.Instance.OpportunityStatusSummary, opportunity.NoteTypes);
		}

		public override void TestRentalMultiplierDescription()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();

			AssertEquals("Local Reach", opportunity.RentalMultiplierDescription);
			OrganisationsDataRegistry.Instance.PotentialLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Mary");
			AssertEquals("Mary", opportunity.RentalMultiplierDescription);
		}

		public override void TestTotalDiscountDescription()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();

			AssertEquals("Contracted", opportunity.TotalDiscountDescription);
			OrganisationsDataRegistry.Instance.CurrentLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Sam");
			AssertEquals("Sam", opportunity.TotalDiscountDescription);
		}

		#region Project Opportunity Pivot

		public void TestRelatedProjects()
		{
			var opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			var project1 = Factory.NewWithValidTestData<EDIProject>();
			var project2 = Factory.NewWithValidTestData<EDIProject>();

			project1.WKP_P8_Opportunity = opportunity.PK;
			project2.WKP_P8_Opportunity = opportunity.PK;

			Factory.Save();

			AssertEquals("Should have 2 related projects", 2, opportunity.RelatedProjects.Count);
			Assert("Should have Project 1", opportunity.RelatedProjects.Contains(project1));
			Assert("Should have Project 2", opportunity.RelatedProjects.Contains(project2));
		}

		public void TestRemoveProjectOpportunityPivots()
		{
			var opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			var project1 = Factory.NewWithValidTestData<EDIProject>();
			var project2 = Factory.NewWithValidTestData<EDIProject>();

			project1.WKP_P8_Opportunity = opportunity.PK;
			project2.WKP_P8_Opportunity = opportunity.PK;

			Factory.Save();

			AssertNotNull("Precondition: Should be linked to opportunity", project1.Opportunity);
			AssertNotNull("Precondition: Should be linked to opportunity", project2.Opportunity);

			opportunity.Delete();

			AssertNull("Should not be linked to deleted opportunity", project1.Opportunity);
			AssertNull("Should not be linked to deleted opportunity", project2.Opportunity);
		}

		#endregion

		#region Related Business Objects

		public void TestClient()
		{
			var org = Factory.New<EDIOrgHeader>();
			var opportunity = Factory.New<EDIOrgOpportunity>();
			AssertNull("Pre-condition", opportunity.Client);

			opportunity.P8_OH = org.PK;
			AssertEquals(org, opportunity.Client);
		}

		public void TestBusinessObjectsWithRelatedNotes()
		{
			var org = Factory.New<EDIOrgHeader>();
			var opportunity = Factory.New<EDIOrgOpportunity>();
			AssertNull("Pre-condition", opportunity.BusinessObjectsWithRelatedNotes[0]);

			opportunity.P8_OH = org.PK;
			AssertEquals(org, opportunity.BusinessObjectsWithRelatedNotes[0]);
		}

		#endregion

		public void TestDocDataFieldWrappers()
		{
			var opportunity = Factory.New<EDIOrgOpportunity>();
			Assert(opportunity.IsRegisteredEditableChildObject(opportunity.DocDataFieldWrappers));
			AssertEquals(DocumentNote.LoadNote(opportunity).SystemDefinedFieldWrappers, opportunity.DocDataFieldWrappers);
		}

		public void TestLicenceOrganisation()
		{
			var opportunity = Factory.New<EDIOrgOpportunity>();
			AssertNull(((IClientOrgLicenceProvider)opportunity).LicenceOrganisation);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			opportunity.P8_OH = org.PK;

			AssertEquals(org, ((IClientOrgLicenceProvider)opportunity).LicenceOrganisation);
		}

		public void TestReferenceNumber()
		{
			var opportunity = Factory.New<EDIOrgOpportunity>();
			opportunity.P8_OpportunityID = "OP2349238502";

			AssertEquals("OP2349238502", ((IClientOrgLicenceProvider)opportunity).ReferenceNumber);
		}

		#region PSQ Opportunity Pivot

		public void TestRelatedPSQs()
		{
			var opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			var psq1 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			var psq2 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();

			opportunity.RelatedPSQs.Add(psq1);
			AssertEquals(1, opportunity.RelatedPSQs.Count);
			Factory.Save();

			AssertEquals(false, opportunity.HasChanges);

			opportunity.RelatedPSQs.Add(psq2);
			AssertEquals(2, opportunity.RelatedPSQs.Count);
			AssertEquals(true, opportunity.HasChanges);

			Factory.Save();
			AssertEquals(false, opportunity.RelatedPSQs.HasChanges);
			AssertEquals(false, opportunity.HasChanges);

			opportunity.RelatedPSQs.Remove(psq1);
			AssertEquals(1, opportunity.RelatedPSQs.Count);
			AssertEquals(true, opportunity.HasChanges);
		}

		public void TestCreateNewRelatedPSQs()
		{
			var opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			var psq1 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			var psq2 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();

			opportunity.CreateNewPSQOpportunityPivot(psq1);
			opportunity.CreateNewPSQOpportunityPivot(psq2);

			Factory.Save();

			AssertEquals("Should have 2 related PSQs", 2, opportunity.RelatedPSQs.Count);
			Assert("Should have PSQ 1", opportunity.RelatedPSQs.Contains(psq1));
			Assert("Should have PSQ 2", opportunity.RelatedPSQs.Contains(psq2));
		}

		public void TestLoadPSQOpportunityPivot()
		{
			var opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			var psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();

			GenPivot pivot1 = opportunity.CreateNewPSQOpportunityPivot(psq);

			Factory.Save();

			GenPivot pivot2 = opportunity.LoadPSQOpportunityPivot(psq);

			AssertEquals(pivot1.XX_Relation1ID, pivot2.XX_Relation1ID);
			AssertEquals(pivot1.XX_Relation2ID, pivot2.XX_Relation2ID);
			AssertEquals(pivot1.XX_Relation1TableCode, pivot2.XX_Relation1TableCode);
			AssertEquals(pivot1.XX_Relation2TableCode, pivot2.XX_Relation2TableCode);
		}

		public void TestRemovePSQOpportunityPivot()
		{
			var opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			var psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();

			GenPivot pivot1 = opportunity.CreateNewPSQOpportunityPivot(psq);

			Factory.Save();

			GenPivot pivot2 = opportunity.LoadPSQOpportunityPivot(psq);
			AssertNotNull(pivot2);

			opportunity.RemovePSQOpportunityPivot(psq);
			GenPivot pivot3 = opportunity.LoadPSQOpportunityPivot(psq);
			AssertEquals(null, pivot3);
		}

		public void TestRemoveAllPSQOpportunityPivots()
		{
			var opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			var psq1 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			var psq2 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();

			GenPivot pivot = opportunity.CreateNewPSQOpportunityPivot(psq1);
			GenPivot pivot2 = opportunity.CreateNewPSQOpportunityPivot(psq2);

			Factory.Save();

			opportunity.Delete();

			Assert("Pivot 1 should be deleted", pivot.IsDeleted);
			Assert("Pivot 2 should be deleted", pivot2.IsDeleted);
		}

		public void TestAddandRemoveRelatedPSQs()
		{
			var opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			var psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();

			opportunity.RelatedPSQs.Add(psq);
			Factory.Save();

			GenPivot pivot1 = opportunity.LoadPSQOpportunityPivot(psq);
			AssertNotNull(pivot1);

			AssertEquals("Should have 1 related PSQ", 1, opportunity.RelatedPSQs.Count);
			Assert(opportunity.RelatedPSQs.Contains(psq));

			opportunity.RelatedPSQs.Remove(psq);
			Factory.Save();

			GenPivot pivot2 = opportunity.LoadPSQOpportunityPivot(psq);
			AssertEquals(null, pivot2);

			AssertEquals("Should have no related PSQ", 0, opportunity.RelatedPSQs.Count);
			AssertEquals("Should not have opportunity", false, opportunity.RelatedPSQs.Contains(psq));
		}

		public void TestAddandRemovePSQWithoutSaving()
		{
			var opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			var psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();

			opportunity.RelatedPSQs.Add(psq);
			GenPivot pivot1 = opportunity.LoadPSQOpportunityPivot(psq);
			AssertNotNull(pivot1);

			opportunity.RelatedPSQs.Remove(psq);

			GenPivot pivot2 = opportunity.LoadPSQOpportunityPivot(psq);
			AssertEquals(null, pivot2);
		}

		public void TestHasChangesIsFalseWhenLoaded()
		{
			var opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			var psq1 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();

			opportunity.RelatedPSQs.Add(psq1);
			AssertEquals(1, opportunity.RelatedPSQs.Count);
			Factory.Save();

			AssertEquals(false, opportunity.HasChanges);

			opportunity.RelatedPSQs.Load();
			AssertEquals(false, opportunity.HasChanges);
		}

		public void TestInitializeFromPSQ()
		{
			var opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			var psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ZE1";
			staff1.GS_FullName = "RM One";
			OrgStaffAssignments assignment = client.StaffAssignments.AddNew();
			assignment.O8_GS_NKPersonResponsible = staff1.GS_Code;
			assignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;

			psq.IM_OH_Client = client.PK;
			psq.IM_Description = "description";
			psq.IM_OC_Contact = contact.PK;
			psq.IM_QuoteAmount = 1000;

			opportunity.InitializeFromPSQ(psq);

			AssertEquals(psq.Client, opportunity.Client);
			AssertEquals(psq.IM_Description, opportunity.P8_OpportunityDescription);
			AssertEquals(psq.Contact, opportunity.Contact);
			AssertEquals(psq.IM_QuoteAmount, opportunity.P8_EstimatedValue);
			AssertEquals("CRT", opportunity.P8_Status);
			AssertEquals("OPE", opportunity.P8_Outcome);

			AssertEquals(psq.IM_RX_NKQuoteCurrency, opportunity.P8_RX_NKEstimatedValueCurrency);
			AssertEquals(new ZDecimal(0), opportunity.P8_RentalMultiplier);

			EDIOrgOpportunityValue valueItem = opportunity.ValueItems[0];
			AssertEquals("SER", valueItem.PV_RevenueType);
			AssertEquals(new ZDecimal(1000), valueItem.PV_Value);

			AssertEquals("SER", opportunity.P8_OpportunityType);
			EDIDataRegistry.Instance.DefaultOpportunityObjective.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ENU");
			opportunity.InitializeFromPSQ(psq);
			AssertEquals("ENU", opportunity.P8_OpportunityType);

			AssertEquals(psq.Client.StaffAssignments.OverallSalesRep, opportunity.P8_GS_NKPrimarySalesPerson);
			assignment.O8_GC = ZGuid.NewZGuid();
			opportunity.InitializeFromPSQ(psq);
			AssertEquals(staff1.GS_Code, opportunity.P8_GS_NKPrimarySalesPerson);
		}

		#endregion

		#region File -> Validate All does not load children of related PSQs

		public void TestValidateAllDoesNotLoadRelatedPSQs()
		{
			var opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			var psq1 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			var psq2 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			opportunity.RelatedPSQs.Add(psq1);

			AssertEquals("Precondition: Opportunity has related PSQ ", 1, opportunity.RelatedPSQs.Count);
			Factory.Save();

			bool registeredEditableChild = opportunity.IsRegisteredEditableChildObject(opportunity.RelatedPSQs);
			opportunity.RelatedPSQs.Add(psq2);
			AssertEquals("Precondition: Opportunity has related PSQs", 2, opportunity.RelatedPSQs.Count);
			registeredEditableChild |= opportunity.IsRegisteredEditableChildObject(opportunity.RelatedPSQs);

			string message = "Failure of this test means that a stack overflow exception will happen whenever a user does a File -> Validate All" +
				" on a form of a Opportunity that fits the conditions specified in the test.";

			AssertEquals(message, false, registeredEditableChild);
		}

		#endregion

		public override void TestDefaultEstimatedValueCurrency()
		{
			var opportunity = CreateOpportunity(false);
			AssertEquals("USD", opportunity.P8_RX_NKEstimatedValueCurrency);
		}

		[TestDate(2017, 1, 1)]
		public override void TestDefaultValues()
		{
			var opportunity = CreateOpportunity(false);
			AssertEquals(GlbCompany.CurrentCompany.PK, opportunity.P8_GC);
			AssertEquals(new ZDateTime(2017, 1, 1), opportunity.P8_DateForExchangeRate);
			AssertEquals("USD", opportunity.P8_RX_NKEstimatedValueCurrency);
		}

		public override void TestP8_RX_NKEstimatedValueCurrency_ShouldValidateSalesHeaderCollectionCurrenciesOnChange()
		{
			Assert("This test is not applicable to the subclass", true);
		}

		public override void TestSetMultiplierRecalculatesEstimatedValue()
		{
			Assert("This test is not applicable to the subclass", true);
		}

		public override void TestUpdateEstimatedValue()
		{
			Assert("This test is not applicable to the subclass", true);
		}

		public void TestImportChildInfoOnAttach_CreateIncidentPivot()
		{
			var supportIncident = Factory.NewWithValidTestData<SupportIncident>();
			var orgOpportunity = CreateOpportunity(true);
			Factory.Save();

			var query = new ZQuery(GenPivotSchema.XX_Relation2ID, supportIncident.PK);
			var genPivots = Factory.Load<GenPivot>(query);
			AssertEquals(0, genPivots.Length);

			((IImportChildRelatedActivityInfoOnAttach)orgOpportunity).ImportChildInfo(supportIncident, new ImportRelatedActivityNoDecisionFactory());
			genPivots = Factory.Load<GenPivot>(query);
			AssertEquals(1, genPivots.Length);

			var genPivot = genPivots.First();
			AssertEquals(genPivot.XX_RelationType, Core.Constants.GenPivotTypes.Opportunity);
			AssertEquals(genPivot.Relation1ID, orgOpportunity.PK);
			AssertEquals(genPivot.XX_Relation1TableCode, OrgOpportunitySchema.Constants.Prefix);
			AssertEquals(genPivot.Relation2ID, supportIncident.PK);
			AssertEquals(genPivot.XX_Relation2TableCode, IncidentMainSchema.Constants.Prefix);
		}

		public void TestImportChildInfoOnAttach_DeleteInverseIncidentPivot()
		{
			var supportIncident = Factory.NewWithValidTestData<SupportIncident>();
			var orgOpportunity = CreateOpportunity(true);

			var incidentPivot = Factory.New<GenPivot>();
			incidentPivot.XX_RelationType = Core.Constants.GenPivotTypes.Opportunity;
			incidentPivot.XX_Relation1ID = supportIncident.PK;
			incidentPivot.XX_Relation1TableCode = IncidentMainSchema.Constants.Prefix;
			incidentPivot.XX_Relation2ID = orgOpportunity.PK;
			incidentPivot.XX_Relation2TableCode = OrgOpportunitySchema.Constants.Prefix;

			Factory.Save();

			var pivotQuery = new ZQuery(GenPivotSchema.XX_Relation2ID, supportIncident.PK);
			var inversePivotQuery = new ZQuery(GenPivotSchema.XX_Relation1ID, supportIncident.PK);
			AssertEquals("Precondition: OPP > INC pivot should not have been created yet", 0, Factory.Load<GenPivot>(pivotQuery).Length);
			AssertEquals("Precondition: INC > OPP pivot should not have been deleted yet", 1, Factory.Load<GenPivot>(inversePivotQuery).Length);

			((IImportChildRelatedActivityInfoOnAttach)orgOpportunity).ImportChildInfo(supportIncident, new ImportRelatedActivityNoDecisionFactory());

			AssertEquals("OPP > INC pivot should have been created", 1, Factory.Load<GenPivot>(pivotQuery).Length);
			AssertEquals("INC > OPP pivot should have been deleted", 0, Factory.Load<GenPivot>(inversePivotQuery).Length);
		}

		public void TestImportChildInfoOnDetach_DeleteIncidentPivot()
		{
			var supportIncident = Factory.NewWithValidTestData<SupportIncident>();
			var orgOpportunity = CreateOpportunity(true);

			var newGenPivot = Factory.New<GenPivot>();
			newGenPivot.XX_RelationType = Core.Constants.GenPivotTypes.Opportunity;
			newGenPivot.XX_Relation1ID = orgOpportunity.PK;
			newGenPivot.XX_Relation1TableCode = OrgOpportunitySchema.Constants.Prefix;
			newGenPivot.XX_Relation2ID = supportIncident.PK;
			newGenPivot.XX_Relation2TableCode = IncidentMainSchema.Constants.Prefix;
			Factory.Save();

			var query = new ZQuery(GenPivotSchema.XX_Relation2ID, supportIncident.PK);
			var genPivots = Factory.Load<GenPivot>(query);
			AssertEquals(1, genPivots.Length);

			var genPivot = genPivots.First();
			AssertEquals(genPivot.XX_RelationType, Core.Constants.GenPivotTypes.Opportunity);
			AssertEquals(genPivot.Relation1ID, orgOpportunity.PK);
			AssertEquals(genPivot.XX_Relation1TableCode, OrgOpportunitySchema.Constants.Prefix);
			AssertEquals(genPivot.Relation2ID, supportIncident.PK);
			AssertEquals(genPivot.XX_Relation2TableCode, IncidentMainSchema.Constants.Prefix);

			((IImportChildRelatedActivityInfoOnDetach)orgOpportunity).ImportChildInfo(supportIncident, new ImportRelatedActivityNoDecisionFactory());

			genPivots = Factory.Load<GenPivot>(query);
			AssertEquals(0, genPivots.Length);
		}
	}
}
