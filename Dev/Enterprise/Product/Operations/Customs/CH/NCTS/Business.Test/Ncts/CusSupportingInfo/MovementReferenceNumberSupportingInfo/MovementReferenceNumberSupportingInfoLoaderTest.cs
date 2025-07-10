using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(MovementReferenceNumberSupportingInfo.Loader))]
sealed class MovementReferenceNumberSupportingInfoLoaderTest : LoaderTestCase
{
	protected override BusinessObject.Loader GetNewLoaderToTest() => new MovementReferenceNumberSupportingInfo.Loader(Factory);

	public void TestLoadMovementReferenceNumbers() => CombineAssertions(() =>
	{
		SetupMovementReferenceNumbers();

		var mrns = new MovementReferenceNumberSupportingInfo.Loader(Factory).LoadMovementReferenceNumbers("100.2", "100.3", "200.1", "300.1");
		AssertContainsExactElementsInAnyOrder(new string[] { "100.2", "100.3", "200.1", "300.1" }, mrns.Select(x => x.CSI_ReferenceNumber));
	});

	public void TestFindByMovementReferenceNumber() => CombineAssertions(() =>
	{
		var defaultBranch = GlbBranch.CurrentBranch;
		var otherBranch = GlbCompany.GetCurrentCompany(Factory).Branches.AddNew();
		otherBranch.GB_Code = "B2";
		otherBranch.GB_IsActive = ZBool.True;
		var otherCompany = Factory.New<GlbCompany>();
		otherCompany.GC_Code = "C9";
		var otherCompanyBranch = otherCompany.Branches.AddNew();
		otherCompanyBranch.GB_Code = "B9";
		otherCompanyBranch.GB_IsActive = ZBool.True;
		Factory.Save();

		var movementReference1 = CreateMovementReference("100");

		var movementReference2 = CreateMovementReference("200");
		movementReference2.Parent.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		MovementReferenceNumberSupportingInfo movementReference3;
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, otherBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
		{
			movementReference3 = CreateMovementReference("300");
		}

		MovementReferenceNumberSupportingInfo movementReference4;
		using (DisposableEnvironment.ForBranch(otherCompanyBranch.PK.ToGuid()))
		{
			movementReference4 = CreateMovementReference("400");
		}

		Factory.Save();

		var loader = new MovementReferenceNumberSupportingInfo.Loader(Factory);

		AssertEquals("Existing BO", movementReference1.PK, FindMovementReference("100")?.PK);
		AssertNull("Other MRNNumber", FindMovementReference("109"));
		AssertNull("Not NCTS5", FindMovementReference("200"));
		AssertEquals("Other branch", movementReference3.PK, FindMovementReference("300")?.PK);
		AssertNull("Other company", FindMovementReference("400"));

		MovementReferenceNumberSupportingInfo CreateMovementReference(string mrn)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementReference = nctsHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
			movementReference.CSI_ReferenceNumber = mrn;
			return movementReference;
		}

		MovementReferenceNumberSupportingInfo FindMovementReference(string mrnNumber, string movementType = NctsMovementType.Codes.Departure)
		{
			return loader.FindByMovementReferenceNumber(mrnNumber);
		}
	});

	void SetupMovementReferenceNumbers()
	{
		var otherBranch = GlbCompany.GetCurrentCompany(Factory).Branches.AddNew();
		otherBranch.GB_Code = "B2";
		otherBranch.GB_IsActive = ZBool.True;
		var otherCompany = Factory.New<GlbCompany>();
		otherCompany.GC_Code = "C9";
		var otherCompanyBranch = otherCompany.Branches.AddNew();
		otherCompanyBranch.GB_Code = "B9";
		otherCompanyBranch.GB_IsActive = ZBool.True;
		Factory.Save();

		CreateNctsArrivalMovementHeader("100.1", "100.2", "100.3");
		CreateNctsArrivalMovementHeader("200.1", "200.2");
		CreateNctsArrivalMovementHeader("100.2", "100.3").Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, otherBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
		{
			CreateNctsArrivalMovementHeader("300.1");
		}

		using (DisposableEnvironment.ForBranch(otherCompanyBranch.PK.ToGuid()))
		{
			CreateNctsArrivalMovementHeader("100.2", "100.3");
		}

		Factory.Save();

		NctsArrivalMovementHeader CreateNctsArrivalMovementHeader(params string[] movementReferenceNumbers)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			foreach (var mrn in movementReferenceNumbers)
			{
				nctsHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew().CSI_ReferenceNumber = mrn;
			}
			return nctsHeader.ArrivalMovementHeader;
		}
	}
}
