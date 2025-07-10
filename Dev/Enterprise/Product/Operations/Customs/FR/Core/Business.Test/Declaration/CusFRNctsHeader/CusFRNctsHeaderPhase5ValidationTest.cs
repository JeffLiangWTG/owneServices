using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class CusFRNctsHeaderPhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCFN_NatureOfSeals()
		{
			nctsHeader.MovementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsDeclarationTypeList.Codes.TIR;
			AssertNoMessageErrors(nctsHeader.FRNctsHeader.CFN_NatureOfSealsInfo);

			nctsHeader.FRNctsHeader.CFN_NatureOfSeals = NatureOfSealsList.Codes.NS3;
			AssertNoMessageErrors(nctsHeader.FRNctsHeader.CFN_NatureOfSealsInfo);

			nctsHeader.MovementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsDeclarationTypeList.Codes.T2;
			nctsHeader.FRNctsHeader.Validation.ValidateCFN_NatureOfSeals();
			AssertNoMessageErrors(nctsHeader.FRNctsHeader.CFN_NatureOfSealsInfo);

			nctsHeader.MovementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsDeclarationTypeList.Codes.TIR;
			nctsHeader.FRNctsHeader.CFN_NatureOfSeals = "2";
			nctsHeader.FRNctsHeader.Validation.ValidateCFN_NatureOfSeals();
			AssertNoMessageErrors(nctsHeader.FRNctsHeader.CFN_NatureOfSealsInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		}
		NctsHeader nctsHeader;
	}
}
