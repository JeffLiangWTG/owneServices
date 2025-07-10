using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	sealed class NctsDepartureCargoDescPhase4ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBY_HarmonisedTariff()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
			var goodItem = header.MovementHeader.GoodsItems.AddNew();

			var nctsGuarantee = header.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "GUA1";
			nctsGuarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor;

			goodItem.BY_HarmonisedTariff = ZString.Empty;
			AssertHasMessageError("BY_HarmonisedTariff have error if is empty and guarantee pwbondtype = 2", goodItem.BY_HarmonisedTariffInfo, "You have not entered a [33] Commodity Code.");

			goodItem.BY_HarmonisedTariff = "com";
			AssertNoMessageError("BY_HarmonisedTariff have no error if is not empty and guarantee pwbondtype = 2", goodItem.BY_HarmonisedTariffInfo, "You have not entered a [33] Commodity Code.");

			nctsGuarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee;

			goodItem.BY_HarmonisedTariff = ZString.Empty;
			AssertNoMessageError("BY_HarmonisedTariff have no error if guarantee pwbondtype != 2", goodItem.BY_HarmonisedTariffInfo, "You have not entered a [33] Commodity Code.");

			goodItem.BY_HarmonisedTariff = "com";
			AssertNoMessageError("BY_HarmonisedTariff have no error if guarantee pwbondtype != 2", goodItem.BY_HarmonisedTariffInfo, "You have not entered a [33] Commodity Code.");
		}
	}
}
