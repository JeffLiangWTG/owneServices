using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(RemoveJobChargeAttributesLinkToCancelledIncompleteInvoice))]
	internal class RemoveJobChargeAttributesLinkToCancelledIncompleteInvoiceTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Remove Job Charge Attributes that link to a cancelled transaction header_1] ON [dbo].[JobChargeAttrib] ([EC_Name]) INCLUDE ([EC_SystemCreateTimeUtc], [EC_SystemLastEditTimeUtc], [EC_Value]) WHERE ([EC_Name]='INV') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void AssertTransformationResults()
		{
			var dt = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT EC_PK FROM JobChargeAttrib WHERE EC_Name = 'INV'");
			var attribs = new List<Guid>();
			foreach (DataRow row in dt.Rows)
			{
				attribs.Add(row.Field<Guid>("EC_PK"));
			}

			Assert("Should exist when link to uncancelled transaction header", attribs.Contains(attrib1));
			Assert("Should exist when link to uncancelled transaction header", attribs.Contains(attrib2));
			Assert("Should NOT exist when link to cancelled transaction header", !attribs.Contains(attrib3));
			Assert("Should NOT exist when link to cancelled transaction header", !attribs.Contains(attrib4));
			Assert("With TRY_CONVERT(), when EC_Name is INV and EC_Value is not a Guid, sql execute without error", attribs.Contains(attrib5));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RemoveJobChargeAttributesLinkToCancelledIncompleteInvoice();
		}

		protected override void PrepareTestData()
		{
			var helper = new TestDbHelper(TestConnection);
			var postDate = DateTime.Now;
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = TestDbHelper.BranchBrnPK;
			var departmentPK = TestDbHelper.DepartmentBrnPK;

			var charge1 = helper.InsertChargeCode(companyPK, "CC1");
			var charge2 = helper.InsertChargeCode(companyPK, "CC2");
			var charge3 = helper.InsertChargeCode(companyPK, "CC3");

			var ah1 = helper.InsertTransactionHeader("IN", "INI", "000001", 50m, postDate, branchPK, departmentPK);
			var ah2 = helper.InsertTransactionHeader("IN", "INI", "000002", 150m, postDate, branchPK, departmentPK, category: "REA", isCancelled: true);

			var job1 = helper.InsertJob("JH001", companyPK, branchPK, departmentPK, "JS", null, "WRK", DateTime.UtcNow);

			var jobCharge1 = helper.InsertJobCharge(job1, branchPK, companyPK, departmentPK, charge1, Guid.Empty, Guid.Empty);
			var jobCharge2 = helper.InsertJobCharge(job1, branchPK, companyPK, departmentPK, charge2, Guid.Empty, Guid.Empty);
			var jobCharge3 = helper.InsertJobCharge(job1, branchPK, companyPK, departmentPK, charge3, Guid.Empty, Guid.Empty);
			var jobCharge4 = helper.InsertJobCharge(job1, branchPK, companyPK, departmentPK, charge1, Guid.Empty, Guid.Empty);
			var jobCharge5 = helper.InsertJobCharge(job1, branchPK, companyPK, departmentPK, charge2, Guid.Empty, Guid.Empty);

			attrib1 = helper.InsertJobChargeAttrib(jobCharge1, "INV", ah1.ToString(), 250);
			attrib2 = helper.InsertJobChargeAttrib(jobCharge2, "INV", ah1.ToString(), 250);
			attrib3 = helper.InsertJobChargeAttrib(jobCharge3, "INV", ah2.ToString(), 250);
			attrib4 = helper.InsertJobChargeAttrib(jobCharge4, "INV", ah2.ToString(), 250);
			attrib5 = helper.InsertJobChargeAttrib(jobCharge5, "INV", "Not a Guid", 250);
		}

		Guid attrib1, attrib2, attrib3, attrib4, attrib5;
	}
}
