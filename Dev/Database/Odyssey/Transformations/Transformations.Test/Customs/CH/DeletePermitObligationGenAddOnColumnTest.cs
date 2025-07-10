using System;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.CH;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CH
{
	[TestedType(typeof(DeletePermitObligationGenAddOnColumn))]
	class DeletePermitObligationGenAddOnColumnTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new DeletePermitObligationGenAddOnColumn();
		}

		protected override void PrepareTestData()
		{
			var dataCreator = new TransformationTestDataCreator();
			var chCompanyPK = dataCreator.CreateGlbCompany("C01", "CH");
			var chBranchPK = dataCreator.CreateBranch("B01", "CHBSL", chCompanyPK);
			var deCompanyPK = dataCreator.CreateGlbCompany("C02", "De");
			var deBranchPK = dataCreator.CreateBranch("B02", "CHBSL", deCompanyPK);
			var cargoDescPK = CreateCargoDesc(chBranchPK);
			pk1 = dataCreator.CreateGenAddOnColumn(PermitObligation, "Y", "BY", cargoDescPK);
			pk2 = dataCreator.CreateGenAddOnColumn(PermitObligation, "Y", "BY", CreateCargoDesc(deBranchPK));
			pk3 = dataCreator.CreateGenAddOnColumn(PermitObligation + "X", "Y", "BY", cargoDescPK);

			Guid CreateCargoDesc(Guid branchPK)
			{
				var headerPK = dataCreator.CreateCusInBondHeader("NC5", branchPK, Guid.Empty, string.Empty, "0");
				var billPK = dataCreator.CreateCusInBondBill(headerPK);
				return dataCreator.CreateCusInBondCargoDesc(billPK);
			}
		}

		protected override void AssertTransformationResults()
		{
			var data = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT [XA_PK] FROM [dbo].[GenAddOnColumn]");

			AssertEquals("PermitObligation deleted", false, data.Select($"XA_PK='{pk1}'").Any());
			AssertEquals("Other company: not deleted", true, data.Select($"XA_PK='{pk2}'").Any());
			AssertEquals("Other name: not deleted", true, data.Select($"XA_PK='{pk3}'").Any());
		}

		Guid pk1, pk2, pk3;

		const string PermitObligation = "PermitObligation";
	}
}
