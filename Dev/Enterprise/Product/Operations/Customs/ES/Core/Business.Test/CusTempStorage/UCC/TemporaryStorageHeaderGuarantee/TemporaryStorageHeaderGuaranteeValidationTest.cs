using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

class TemporaryStorageHeaderGuaranteeValidationTest : BusinessObjectValidationTestCase
{
	public void TestPW_BondNumberAndPW_BondAmountEmpty()
	{
		header.DestinationGoodsLocation.Address.AuthorisationNumber = "TSLoc";
		header.AMA_JobReference = "TS00000001";
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		CombineAssertions(() =>
		{
			guarantee.Validation.ValidatePW_BondNumber();
			guarantee.Validation.ValidatePW_BondAmount();
			AssertNoMessageErrorContaining("When no premises, and PW_BondNumber is empty, no error", guarantee.PW_BondNumberInfo, expectedBondNumberAndBondAmountEmptyError);
			AssertNoMessageErrorContaining("When no premises, and PW_BondAmount is empty, no error", guarantee.PW_BondAmountInfo, expectedBondNumberAndBondAmountEmptyError);

			var premises = Factory.New<CusTempStorageRegPremises>();
			premises.SRP_Type = "ADT";
			premises.SRP_CustomsLocation = "TSLoc";
			premises.SRP_Code = "X";
			premises.SRP_Description = "DESC";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;

			Factory.Save();

			guarantee.Validation.ValidatePW_BondNumber();
			guarantee.Validation.ValidatePW_BondAmount();
			AssertNoMessageErrorContaining("When premises and PW_BondNumber is empty but Declarant and Consignee not the same, no error", guarantee.PW_BondNumberInfo, expectedBondNumberAndBondAmountEmptyError);
			AssertNoMessageErrorContaining("When premises and PW_BondAmount is empty but Declarant and Consignee not the same, no error", guarantee.PW_BondAmountInfo, expectedBondNumberAndBondAmountEmptyError);

			header.AMA_OA_Declarant = orgAddress.PK;
			var bill = header.Bills.FirstOrDefault();
			bill.ABL_OA_Consignee = orgAddress.PK;

			guarantee.Validation.ValidatePW_BondNumber();
			guarantee.Validation.ValidatePW_BondAmount();
			AssertHasMessageErrorContaining("When premises, PW_BondNumber is empty, Declarant and Consignee are the same, error", guarantee.PW_BondNumberInfo, expectedBondNumberAndBondAmountEmptyError);
			AssertHasMessageErrorContaining("When premises, PW_BondAmount is empty, Declarant and Consignee are the same, error", guarantee.PW_BondAmountInfo, expectedBondNumberAndBondAmountEmptyError);

			guarantee.PW_BondNumber = "Number";
			guarantee.PW_BondAmount = 1.0m;
			AssertNoMessageErrorContaining("When premises, Declarant and Consignee are the same but PW_BondNumber is not empty, no error", guarantee.PW_BondNumberInfo, expectedBondNumberAndBondAmountEmptyError);
			AssertNoMessageErrorContaining("When premises, Declarant and Consignee are the same but PW_BondAmount is not empty, no error", guarantee.PW_BondAmountInfo, expectedBondNumberAndBondAmountEmptyError);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			guarantee.PW_BondNumber = ZString.Empty;
			guarantee.PW_BondAmount = ZDecimal.Zero;
			AssertNoMessageErrorContaining("When premises, Declarant and Consignee are the same but PW_BondNumber is empty but is not Expedition, no error", guarantee.PW_BondNumberInfo, expectedBondNumberAndBondAmountEmptyError);
			AssertNoMessageErrorContaining("When premises, Declarant and Consignee are the same but PW_BondAmount is empty but is not Expedition, no error", guarantee.PW_BondAmountInfo, expectedBondNumberAndBondAmountEmptyError);
		});
	}

	const string expectedBondNumberAndBondAmountEmptyError = "For G5 Expedition declarations where the Declarant is the owner of the Destination Goods Location, a Guarantee Reference Number and Liability Amount must be supplied.";

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<TemporaryStorageHeader>();
		guarantee = header.Guarantee;
	}

	TemporaryStorageHeader header;
	TemporaryStorageHeaderGuarantee guarantee;
}
