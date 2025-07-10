using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Billing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.Testing
{
	[TestedType(typeof(AccConsolidationGroup))]
	public class AccConsolidationGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGroupMembersAreDeleted()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery());

			var group1 = Factory.New<AccConsolidationGroup>();
			group1.GroupMembers.AddNew().YM_OH_Organisation = org.PK;
			group1.GroupMembers.AddNew().YM_GC_Company = company.PK;
			group1.Delete();

			var group2 = Factory.New<AccConsolidationGroup>();
			group2.GroupMembers.AddNew().YM_OH_Organisation = org.PK;
			group2.GroupMembers.AddNew().YM_GC_Company = company.PK;

			Factory.Save();

			AssertEquals("group members should be deleted for group 1", 2, Factory.Load<AccConsolidationMember>(new ZQuery()).Length);
		}

		[TestDate(2025, 01, 01, 12, 00, 00)]
		public void TestCollectSaveAccConsolidationGroupUsageData()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery());

			var group = Factory.New<AccConsolidationGroup>();
			group.GroupMembers.AddNew().YM_OH_Organisation = org.PK;
			group.GroupMembers.AddNew().YM_GC_Company = company.PK;

			Factory.Save();

			var ediMessages = Factory.Load<IUsageEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
			AssertEquals("EDI message (messageType = USG) records should be 1", 1, ediMessages.Length);

			var jObject = ediMessages.Select(msg => JObject.Parse(msg.EM_MessageTextDetail)).First();
			AssertEquals(UsageFeatures.Codes.AccConsolidationGroup, jObject.Properties().FirstOrDefault(kp => kp.Name.Equals(UsageProperties.FeatureCode, StringComparison.InvariantCulture))?.Value.ToString());
			AssertEquals(UsageFeatures.Modules.Accounting, jObject.Properties().FirstOrDefault(kp => kp.Name.Equals(UsageProperties.Module, StringComparison.InvariantCulture))?.Value.ToString());
			AssertEquals("Accounting General Ledger Consolidation Group", jObject.Properties().FirstOrDefault(kp => kp.Name.Equals(UsageProperties.FeatureDescription, StringComparison.InvariantCulture))?.Value.ToString());
			AssertEquals("EDIEDIDAT", jObject.Properties().FirstOrDefault(kp => kp.Name.Equals(UsageProperties.ConsolidationGroupClientID, StringComparison.InvariantCulture))?.Value.ToString());
			AssertEquals("2025-01-01 12:00", jObject.Properties().FirstOrDefault(kp => kp.Name.Equals(UsageProperties.ConsolidationGroupLastEditTime, StringComparison.InvariantCulture))?.Value.ToString());
			AssertEquals("2025-01-01 12:00", jObject.Properties().FirstOrDefault(kp => kp.Name.Equals(UsageProperties.ConsolidationGroupCreatedTime, StringComparison.InvariantCulture))?.Value.ToString());
		}
	}
}
