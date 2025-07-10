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
	[TestedType(typeof(RemoveJobConsolCostAttributesLinkToCancelledIncompleteInvoice))]
	internal class RemoveJobConsolCostAttributesLinkToCancelledIncompleteInvoiceTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Remove Job Consol Cost Attributes that link to a cancelled transaction header_1] ON [dbo].[JobConsolCostAttrib] ([E6A_Name]) INCLUDE ([E6A_Value]) WHERE ([E6A_Name]='INV') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void AssertTransformationResults()
		{
			var dt = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT E6A_PK FROM JobConsolCostAttrib WHERE E6A_Name = 'INV'");
			var attribs = new List<Guid>();
			foreach (DataRow row in dt.Rows)
			{
				attribs.Add(row.Field<Guid>("E6A_PK"));
			}

			Assert("Should exist when link to uncancelled transaction header", attribs.Contains(attrib1));
			Assert("Should exist when link to uncancelled transaction header", attribs.Contains(attrib2));
			Assert("Should NOT exist when link to cancelled transaction header", !attribs.Contains(attrib3));
			Assert("Should NOT exist when link to cancelled transaction header", !attribs.Contains(attrib4));
			Assert("With TRY_CONVERT(), when E6A_Name is INV and E6A_Value is not a Guid, sql execute without error", attribs.Contains(attrib5));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RemoveJobConsolCostAttributesLinkToCancelledIncompleteInvoice();
		}

		protected override void PrepareTestData()
		{
			var helper = new TestDbHelper(TestConnection);
			var postDate = DateTime.Now;
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = TestDbHelper.BranchBrnPK;
			var departmentPK = TestDbHelper.DepartmentBrnPK;
			var consolPK = helper.InsertConsol("C00002288", "ADALV", "CNTAI");

			var charge1 = helper.InsertChargeCode(companyPK, "CC1");
			var charge2 = helper.InsertChargeCode(companyPK, "CC2");
			var charge3 = helper.InsertChargeCode(companyPK, "CC3");

			var ah1 = helper.InsertTransactionHeader("IN", "INI", "000001", 50m, postDate, branchPK, departmentPK);
			var ah2 = helper.InsertTransactionHeader("IN", "INI", "000002", 150m, postDate, branchPK, departmentPK, category: "REA", isCancelled: true);

			var jobConsolCost1 = helper.InsertConsolCost(charge1, "INV001", DateTime.Today, "AUD", 120M, 0M, 1.0M, 120M, "MAN", creditor: null, aPInvoicePK: null, aRInvoicePK: null, consol: consolPK, companyPK);
			var jobConsolCost2 = helper.InsertConsolCost(charge2, "INV002", DateTime.Today, "AUD", 120M, 0M, 1.0M, 120M, "MAN", creditor: null, aPInvoicePK: null, aRInvoicePK: null, consol: consolPK, companyPK);
			var jobConsolCost3 = helper.InsertConsolCost(charge3, "INV003", DateTime.Today, "AUD", 120M, 0M, 1.0M, 120M, "MAN", creditor: null, aPInvoicePK: null, aRInvoicePK: null, consol: consolPK, companyPK);
			var jobConsolCost4 = helper.InsertConsolCost(charge1, "INV004", DateTime.Today, "AUD", 120M, 0M, 1.0M, 120M, "MAN", creditor: null, aPInvoicePK: null, aRInvoicePK: null, consol: consolPK, companyPK);
			var jobConsolCost5 = helper.InsertConsolCost(charge1, "INV004", DateTime.Today, "AUD", 120M, 0M, 1.0M, 120M, "MAN", creditor: null, aPInvoicePK: null, aRInvoicePK: null, consol: consolPK, companyPK);

			attrib1 = helper.InsertJobConsolCostAttrib(jobConsolCost1, "INV", ah1.ToString(), 250);
			attrib2 = helper.InsertJobConsolCostAttrib(jobConsolCost2, "INV", ah1.ToString(), 250);
			attrib3 = helper.InsertJobConsolCostAttrib(jobConsolCost3, "INV", ah2.ToString(), 250);
			attrib4 = helper.InsertJobConsolCostAttrib(jobConsolCost4, "INV", ah2.ToString(), 250);
			attrib5 = helper.InsertJobConsolCostAttrib(jobConsolCost5, "INV", "Not a Guid", 250);
		}

		Guid attrib1, attrib2, attrib3, attrib4, attrib5;
	}
}
