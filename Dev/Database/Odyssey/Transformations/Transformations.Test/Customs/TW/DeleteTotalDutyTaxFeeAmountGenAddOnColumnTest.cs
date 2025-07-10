using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.TW;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Customs.TW.Testing
{
	[TestedType(typeof(DeleteTotalDutyTaxFeeAmountGenAddOnColumn))]
	class DeleteTotalDutyTaxFeeAmountGenAddOnColumnTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var data = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT [XA_PK] FROM [dbo].[GenAddOnColumn]");
			CombineAssertions(() =>
			{
				AssertEquals("TW company deleted", false, data.Select($"XA_PK='{pk1}'").Length != 0);
				AssertEquals("Other name: not deleted", true, data.Select($"XA_PK='{pk2}'").Length != 0);
				AssertEquals("Other parent: not deleted", true, data.Select($"XA_PK='{pk3}'").Length != 0);
				AssertEquals("Other company: not deleted", true, data.Select($"XA_PK='{pk4}'").Length != 0);
			});
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new DeleteTotalDutyTaxFeeAmountGenAddOnColumn();

		protected override void PrepareTestData()
		{
			var creator = new TransformationTestDataCreator();
			var twCompanyPK = creator.CreateGlbCompany("~TW", "TW");
			var twBranchPK = creator.CreateGlbBranch("~TW", twCompanyPK);
			var usCompanyPK = creator.CreateGlbCompany("~US", "US");
			var usBranchPK = creator.CreateGlbBranch("~US", usCompanyPK);
			var org = creator.CreateOrg("OH1");
			var declPK1 = creator.CreateJobDeclaration("TW", twBranchPK, twCompanyPK, org, "IMP", DateTime.Now, 1, "");
			var declPK2 = creator.CreateJobDeclaration("US", usBranchPK, usCompanyPK, org, "IMP", DateTime.Now, 2, "");
			var chPK1 = creator.CreateCusEntryHeader("TW", declPK1, 1);
			var chPK2 = creator.CreateCusEntryHeader("US", declPK2, 2);
			var c9PK1 = creator.CreateCusEntryPayInfo(chPK1, 1);
			var c9PK2 = creator.CreateCusEntryPayInfo(chPK2, 2);

			pk1 = creator.CreateGenAddOnColumn(XAName, "1", "C9", c9PK1);
			pk2 = creator.CreateGenAddOnColumn(OtherXAName, "2", "C9", c9PK1);
			pk3 = creator.CreateGenAddOnColumn(XAName, "3", "JE", declPK1);
			pk4 = creator.CreateGenAddOnColumn(XAName, "4", "C9", c9PK2);
		}

		Guid pk1, pk2, pk3, pk4;
		const string XAName = "TotalDutyTaxFeeAmount";
		const string OtherXAName = "OtherChargeDeductionAmount";
	}
}
