using System;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.CRM.Common.Testing
{
	[TestedType(typeof(CrmOpportunityWorkflowDescriptor))]
	sealed class CrmOpportunityProcessWorkflowDescriptorTest : WorkflowDescriptorTestCase<CrmOpportunityWorkflowDescriptor>
	{
		protected override bool ShouldRunBufferManagementSupportedJobTypeTests => false;

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.CrmOpportunityWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "GLOW Sales Opportunity (DO NOT USE)", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			var collection1 = new CodeDescriptionBoolCollection();
			collection1.Add("AAA", (NoResString)"Sales Type A", true);
			OrganisationsDataRegistry.Instance.GlowOpportunitySalesTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);

			var collection2 = new CodeDescriptionBoolCollection();
			collection2.Add("BBB", (NoResString)"Source Type B");
			OrganisationsDataRegistry.Instance.GlowOpportunityLeadSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection2);

			var collection3 = new CodeDescriptionBoolCollection();
			collection3.Add("CCC", (NoResString)"Product Type C", true);
			OrganisationsDataRegistry.Instance.GlowOpportunityProductType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection3);

			ICodeDescriptionPairList salesTypeList = OrganisationsDataRegistry.Instance.GlowOpportunitySalesTypes.Value.GetCodeDescriptionPairList();
			ICodeDescriptionPairList sourceList = OrganisationsDataRegistry.Instance.GlowOpportunityLeadSource.Value.GetCodeDescriptionPairList();
			ICodeDescriptionPairList productTypeList = OrganisationsDataRegistry.Instance.GlowOpportunityProductType.Value.GetCodeDescriptionPairList();
			AssertEquals("3 sub types", 3, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Sub Type 1 is Sales Type", "Sales Type", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertListEquals("Correct List", salesTypeList, WorkflowDescriptor.SubTypeInformation[0].List);

			AssertEquals("Sub Type 2 is Source Type", "Source Type", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertListEquals("Correct List", sourceList, WorkflowDescriptor.SubTypeInformation[1].List);

			AssertEquals("Sub Type 3 is based on product type label", "Product Type", WorkflowDescriptor.SubTypeInformation[2].Description);
			AssertListEquals("Correct List", productTypeList, WorkflowDescriptor.SubTypeInformation[2].List);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Client | MessageRecipientPartyType.Email; }
		}

		#region Implementation

		#region CrmOpportunityWithConfiguredOrganisationParties

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				CrmOpportunityWithConfiguredOrganisationParties,
			};
		}

		CrmOpportunity CrmOpportunityWithConfiguredOrganisationParties
		{
			get
			{
				if (crmOpportunityWithConfiguredOrganisationParties == null)
				{
					crmOpportunityWithConfiguredOrganisationParties = Factory.NewWithValidTestData<CrmOpportunity>();
					crmOpportunityWithConfiguredOrganisationParties.COP_OH_Organization = ClientOrg.PK;
				}
				return crmOpportunityWithConfiguredOrganisationParties;
			}
		}

		CrmOpportunity crmOpportunityWithConfiguredOrganisationParties;

		#endregion

		void AssertListEquals(string message, ICodeDescriptionPairList expectedList, ICodeDescriptionPairList actualList)
		{
			AssertEquals("Same number of items", expectedList.Count, actualList.Count);
			for (int i = 0; i < expectedList.Count; i++)
			{
				AssertEquals(message + " - Code for item " + i.ToString(), ((ICodeDescription)expectedList[i]).Code, ((ICodeDescription)actualList[i]).Code);
				AssertEquals(message + " - Description for item " + i.ToString(), ((ICodeDescription)expectedList[i]).Description, ((ICodeDescription)actualList[i]).Description);
			}
		}

		#endregion
	}
}
