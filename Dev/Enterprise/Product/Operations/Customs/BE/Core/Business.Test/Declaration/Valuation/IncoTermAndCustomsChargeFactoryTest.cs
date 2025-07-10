using System.Linq;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class IncoTermAndCustomsChargeFactoryTest : EUIncoTermAndCustomsChargeFactoryTest
{
	public override void TestFactoryType()
	{
		AssertEquals(typeof(IncoTermAndCustomsChargeFactory), incoTermAndChargeFactory.GetType());
	}

	public override void TestGetAllIncoTerms()
	{
		AssertEquals("Count", 14, incoTermAndChargeFactory.GetAllIncoTerms().Length);
	}

	public override void TestGetAllCharges()
	{
		var expectedCharges = new string[]
		{
			"MCP",
			"BCM",
			"CBR",
			"CEA",
			"COM",
			"CPA",
			"DED",
			"DIS",
			"ADD",
			"MAC",
			"TRA",
			"OFT",
			"ONS",
			"PSR",
			"RLF",
			"STA",
			"TAX",
			"TDM",
			"EDA",
		};

		AssertContainsExactElementsInAnyOrder(expectedCharges, incoTermAndChargeFactory.GetAllCharges().Select(_ => _.Code));
	}

	public override void TestGetCharge()
	{
		AssertGetCharge(BECustomsChargeTypeList.Codes.AdditionCharge, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.AdditionCharge, BECustomsChargeTypeList.Descriptions.AdditionCharge));
		AssertGetCharge(BECustomsChargeTypeList.Codes.Brokerage, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.Brokerage, BECustomsChargeTypeList.Descriptions.Brokerage));
		AssertGetCharge(BECustomsChargeTypeList.Codes.BuyingCommission, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.BuyingCommission, BECustomsChargeTypeList.Descriptions.BuyingCommission));
		AssertGetCharge(BECustomsChargeTypeList.Codes.Commission, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.Commission, BECustomsChargeTypeList.Descriptions.Commission));
		AssertGetCharge(BECustomsChargeTypeList.Codes.ConstructionErectionAssembly, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.ConstructionErectionAssembly, BECustomsChargeTypeList.Descriptions.ConstructionErectionAssembly));
		AssertGetCharge(BECustomsChargeTypeList.Codes.DeductionCharge, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.DeductionCharge, BECustomsChargeTypeList.Descriptions.DeductionCharge));
		AssertGetCharge(BECustomsChargeTypeList.Codes.Discount, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.Discount, BECustomsChargeTypeList.Descriptions.Discount));
		AssertGetCharge(BECustomsChargeTypeList.Codes.Engineering, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.Engineering, BECustomsChargeTypeList.Descriptions.Engineering));
		AssertGetCharge(BECustomsChargeTypeList.Codes.MaterialsConsumed, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.MaterialsConsumed, BECustomsChargeTypeList.Descriptions.MaterialsConsumed));
		AssertGetCharge(BECustomsChargeTypeList.Codes.MaterialsIncorp, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.MaterialsIncorp, BECustomsChargeTypeList.Descriptions.MaterialsIncorp));
		AssertGetCharge(BECustomsChargeTypeList.Codes.OverseasFreight, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.OverseasFreight, BECustomsChargeTypeList.Descriptions.OverseasFreight));
		AssertGetCharge(BECustomsChargeTypeList.Codes.OverseasInsurance, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.OverseasInsurance, BECustomsChargeTypeList.Descriptions.OverseasInsurance));
		AssertGetCharge(BECustomsChargeTypeList.Codes.Packing, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.Packing, BECustomsChargeTypeList.Descriptions.Packing));
		AssertGetCharge(BECustomsChargeTypeList.Codes.Proceeds, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.Proceeds, BECustomsChargeTypeList.Descriptions.Proceeds));
		AssertGetCharge(BECustomsChargeTypeList.Codes.Royalties, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.Royalties, BECustomsChargeTypeList.Descriptions.Royalties));
		AssertGetCharge(BECustomsChargeTypeList.Codes.StatAjustment, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.StatAjustment, BECustomsChargeTypeList.Descriptions.StatAjustment));
		AssertGetCharge(BECustomsChargeTypeList.Codes.Tax, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.Tax, BECustomsChargeTypeList.Descriptions.Tax));
		AssertGetCharge(BECustomsChargeTypeList.Codes.Tools, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.Tools, BECustomsChargeTypeList.Descriptions.Tools));
		AssertGetCharge(BECustomsChargeTypeList.Codes.Transport, new Common.CustomsChargeCode(BECustomsChargeTypeList.Codes.Transport, BECustomsChargeTypeList.Descriptions.Transport));
	}

	public void TestChargeOverridden()
	{
		AssertEquals(BECustomsChargeTypeList.Descriptions.AdditionCharge, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.AdditionCharge).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.Brokerage, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.Brokerage).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.BuyingCommission, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.BuyingCommission).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.Commission, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.Commission).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.ConstructionErectionAssembly, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.ConstructionErectionAssembly).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.DeductionCharge, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.DeductionCharge).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.Discount, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.Discount).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.Engineering, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.Engineering).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.MaterialsConsumed, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.MaterialsConsumed).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.MaterialsIncorp, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.MaterialsIncorp).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.OverseasFreight, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.OverseasFreight).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.OverseasInsurance, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.OverseasInsurance).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.Packing, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.Packing).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.Proceeds, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.Proceeds).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.Royalties, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.Royalties).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.StatAjustment, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.StatAjustment).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.Tax, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.Tax).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.Tools, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.Tools).Description);
		AssertEquals(BECustomsChargeTypeList.Descriptions.Transport, incoTermAndChargeFactory.GetCharge(BECustomsChargeTypeList.Codes.Transport).Description);
	}

	public void TestFreightToEUBorder()
	{
		var incoTermAndCustomsChargeFactory = new IncoTermAndCustomsChargeFactory();
		AssertEquals(BECustomsChargeTypeList.Codes.OverseasFreight, incoTermAndCustomsChargeFactory.FreightToEUBorderCode);
	}

	public void TestFreightAfterEUBorder()
	{
		var incoTermAndCustomsChargeFactory = new IncoTermAndCustomsChargeFactory();
		AssertEquals(BECustomsChargeTypeList.Codes.Transport, incoTermAndCustomsChargeFactory.FreightAfterEUBorderCode);
	}

	public void TestInlandTransportCodePresent()
	{
		Assert(new IncoTermAndCustomsChargeFactory().GetCharge(BECustomsChargeTypeList.Codes.Transport) != null);
	}

	protected override void SetUp()
	{
		base.SetUp();
		incoTermAndChargeFactory = Common.IncoTermAndCustomsChargeFactory.GetByCountryCode(Enterprise.Core.Constants.CountryCodes.Belgium);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
	protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\BE\Core\Business.Test\Declaration\TestFiles\IncoTermAndCustomsChargeConfiguration.csv";

	protected override string GetCountryContext() => Enterprise.Core.Constants.CountryCodes.Belgium;

	protected override string FreightToEUBorderCode => BECustomsChargeTypeList.Codes.OverseasFreight;

	protected override string FreightAfterEUBorderCode => BECustomsChargeTypeList.Codes.Transport;
}
