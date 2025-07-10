using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.Shared
{
	[TestedType(typeof(RemoveDuplicateCusClassPivotsV2))]
	[SuppressMessage("Maintainability", "IDE0052: Remove unread private member.", Justification = "Make UT purpose clear")]
	public class RemoveDuplicateCusClassPivotsV2Test : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RemoveDuplicateCusClassPivotsV2();
		}

		protected override void PrepareTestData()
		{
			var helper = new TransformationTestDataCreator();
			op1Pk = helper.CreateOrgSupplierPart("PART1");
			op2Pk = helper.CreateOrgSupplierPart("PART2");

			classification1PK = helper.CreateCusClassification("IMP", "AA", "0000.00.00 00", "FR", "DESC");
			classification2PK = helper.CreateCusClassification("IMP", "BB", "0000.00.00 00", "FR", "DESC");

			var org1Pk = Guid.NewGuid();
			helper.CreateOrgOnly(org1Pk, "ORG1", "Organization1");

			pivot1 = helper.CreateCusClassPartPivot(op1Pk, "FR", org1Pk, "IMP", "0000.00.00 00", new DateTime(2020, 01, 01));//Created before 2024-02-28 will not be removed
			pivot2 = helper.CreateCusClassPartPivot(op1Pk, "FR", null, "IMP", "0000.00.00 00", new DateTime(2020, 01, 01));//Created before 2024-02-28 will not be removed
			pivot3 = helper.CreateCusClassPartPivot(op1Pk, "FR", null, "IMP", "0000.00.00 00", new DateTime(2024, 06, 06));//Duplicated
			pivot4 = helper.CreateCusClassPartPivot(op1Pk, "FR", org1Pk, "EXP", "0000.00.00 00", new DateTime(2024, 06, 06));//Non-empty CI_OH will not be removed
			pivot5 = helper.CreateCusClassPartPivot(op1Pk, "FR", null, "EXP", "0000.00.00 00", new DateTime(2024, 06, 06, 1, 0, 0));//First EXP with empty CI_OH will not be removed
			pivot6 = helper.CreateCusClassPartPivot(op1Pk, "FR", null, "EXP", "0000.00.00 00", new DateTime(2024, 06, 06, 2, 0, 0));//Duplicated
			pivot7 = helper.CreateCusClassPartPivot(op1Pk, "FR", null, "BTH", "0000.00.00 00", new DateTime(2024, 06, 06));//BTH will not be removed
			pivot8 = helper.CreateCusClassPartPivot(op1Pk, "FR", null, "BTH", "0000.00.00 00", new DateTime(2024, 06, 06));//BTH will not be removed
			pivot9 = helper.CreateCusClassPartPivot(op1Pk, "FR", null, "IMP", "0000.00.00 00", new DateTime(2024, 06, 06), null, ["AT1"]);//Any one with attribute will not be removed
			pivot10 = helper.CreateCusClassPartPivot(op1Pk, "FR", null, "IMP", "0000.00.00 00", new DateTime(2024, 06, 06));//Duplicated

			pivot11 = helper.CreateCusClassPartPivot(op1Pk, "FR", null, "IMP", string.Empty, new DateTime(2024, 06, 06, 1, 0, 0), classification1PK);//First of classification1 will not be removed
			pivot12 = helper.CreateCusClassPartPivot(op1Pk, "FR", null, "IMP", string.Empty, new DateTime(2024, 06, 06), classification2PK);//First of classification2 will not be removed
			pivot13 = helper.CreateCusClassPartPivot(op1Pk, "FR", null, "IMP", string.Empty, new DateTime(2024, 06, 06, 2, 0, 0), classification1PK);//Duplicated

			pivot14 = helper.CreateCusClassPartPivot(op1Pk, "AU", null, "HTI", "0000.00.00 00", new DateTime(2020, 01, 01));//Created before 2024-02-28 will not be removed
			pivot15 = helper.CreateCusClassPartPivot(op1Pk, "AU", null, "HTE", "0000.00.00 00", new DateTime(2020, 01, 01));//Created before 2024-02-28 will not be removed
			pivot16 = helper.CreateCusClassPartPivot(op1Pk, "AU", org1Pk, "HTI", "0000.00.00 00", new DateTime(2024, 06, 06));//Non-empty CI_OH will not be removed
			pivot17 = helper.CreateCusClassPartPivot(op1Pk, "AU", null, "HTI", "0000.00.00 00", new DateTime(2024, 06, 06));//Duplicated
			pivot18 = helper.CreateCusClassPartPivot(op1Pk, "AU", null, "HTE", "0000.00.00 00", new DateTime(2020, 01, 01));//Created before 2024-02-28 will not be removed

			pivot19 = helper.CreateCusClassPartPivot(op1Pk, "US", null, "SHB", "0000.00.00 00", new DateTime(2020, 01, 01));//Created before 2024-02-28 will not be removed
			pivot20 = helper.CreateCusClassPartPivot(op1Pk, "US", null, "SHB", "0000.00.00 00", new DateTime(2020, 01, 01));//Created before 2024-02-28 will not be removed
			pivot21 = helper.CreateCusClassPartPivot(op1Pk, "US", org1Pk, "SHB", "0000.00.00 00", new DateTime(2024, 06, 06));//Non-empty CI_OH will not be removed
			pivot22 = helper.CreateCusClassPartPivot(op1Pk, "US", null, "SHB", "0000.00.00 00", new DateTime(2024, 06, 06));//Duplicated

			pivot23 = helper.CreateCusClassPartPivot(op2Pk, "FR", null, "IMP", "0000.00.00 00", new DateTime(2020, 01, 01));//Created before 2024-02-28 will not be removed
			pivot24 = helper.CreateCusClassPartPivot(op2Pk, "FR", null, "IMP", "0000.00.00 00", new DateTime(2024, 06, 06));//Duplicated
		}

		protected override void AssertTransformationResults()
		{
			var results = new List<Guid>();
			TestConnection.ExecuteReader("SELECT CI_PK FROM dbo.CusClassPartPivot", reader => results.Add((Guid)reader["CI_PK"]));
			AssertContainsExactElementsInAnyOrder("Exact and near duplicates should have been deleted. A BTH true or near duplicate should preferably be kept over an IMP or EXP original. Duplicates not BTH or IMP or EXP should be skipped from deletion.",
				new[] {
					pivot1,
					pivot2,
					pivot4,
					pivot5,
					pivot7,
					pivot8,
					pivot9,
					pivot11,
					pivot12,
					pivot14,
					pivot15,
					pivot16,
					pivot18,
					pivot19,
					pivot20,
					pivot21,
					pivot23
					},
				results);
		}

		Guid op1Pk, op2Pk;
		Guid classification1PK, classification2PK;

		Guid pivot1, pivot2, pivot3, pivot4, pivot5, pivot6, pivot7, pivot8, pivot9, pivot10, pivot11, pivot12, pivot13, pivot14, pivot15, pivot16, pivot17, pivot18, pivot19, pivot20, pivot21, pivot22, pivot23, pivot24;
	}
}
