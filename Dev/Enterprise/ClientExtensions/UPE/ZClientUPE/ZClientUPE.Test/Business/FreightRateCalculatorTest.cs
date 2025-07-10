using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class FreightRateCalculatorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestCalculateFreightRateOnDeclaration()
		{
			UPECusMAWB cusMAWB = Factory.NewWithValidTestData<UPECusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			UPECusHAWB cusHAWB = (UPECusHAWB)cusMAWB.ChildBills.AddNew();
			OrgMatchApproval.Loader loader = new OrgMatchApproval.Loader(Factory);
			loader.LoadOrCreate(cusHAWB.PK, OrgMatchApprovalType.AirCargoConsignee);
			CompanyTariff levelOneTariff = new TestHelper(Factory).NewCompanyTariff();
			RateEntry aIREntry1 = levelOneTariff.AddRateEntry("AIR", "LSE", "US", "AU", "STD", "");
			aIREntry1.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			RateLine aIRRateLine1 = aIREntry1.RateLines[0];
			aIRRateLine1.RateLineItems.RemoveAndDeleteAll();
			aIRRateLine1.Calculator["-10"] = (ZDecimal)100m;
			aIRRateLine1.Calculator["+10"] = (ZDecimal)95m;
			levelOneTariff.Factory.Save();
			cusHAWB.CS_GoodsDescription = "Goods description";
			cusHAWB.CS_RL_NKOrigin = "USLAX";
			cusHAWB.CS_RL_NKDestination = "AUSYD";
			cusHAWB.MAWB.CM_RL_NKDischargePort = "AUSYD";
			cusHAWB.MAWB.CM_RL_NKLoadPort = "USLAX";
			cusHAWB.CS_RS_NK_ServiceLevel = "STD";
			cusHAWB.CS_Weight = 15m;
			cusHAWB.CS_WeightUQ = Core.Constants.Weight.Kilograms;
			cusHAWB.Level1Record = new Level1Record();
			cusHAWB.Level1Record.AddRecordLine("US3295AU9639050704              DAT2773T8Z8W5110001   EA G/RIDGE B/L BANJO BOLT 7/16-24                                                                          1217      USD300                 US8714190060          NLR            AU14138                                                                                                                                            ");
			cusHAWB.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			cusHAWB.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			cusHAWB.UPEConsigneeAddress.P3_OH_MatchOrg = cusHAWB.CS_OH_Consignee;
			cusHAWB.Consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			Factory.Save();
			UPEDeclarationFromAirCargoCreator uPEDeclarationFromAirCargoCreator = new UPEDeclarationFromAirCargoCreator(cusHAWB);
			JobDeclaration declaration = (JobDeclaration)uPEDeclarationFromAirCargoCreator.CreateIgnoreWarnings();
			FreightRateCalculator rateCalculator = new FreightRateCalculator();
			var result = rateCalculator.CalculateFreightRateOnDeclaration(declaration);
			AssertEquals(1425m, result.Key);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, result.Value);
			AssertEquals(2, declaration.JobComInvoiceGroupHeaders[0].Charges.Count);
			BaseJobComInvHeaderCharge baseJobComInvHeaderCharge = declaration.JobComInvoiceGroupHeaders[0].Charges[AUChargeCodeList.Codes.OverseasFreight];
			AssertNotNull("Declaration should have an Overseas Freight Rate", baseJobComInvHeaderCharge);
			AssertEquals("Amount should be 1425.00", 1425.00m, baseJobComInvHeaderCharge.J7_Amount);
			AssertEquals("Currency Should be ", Core.Constants.CurrencyCodes.Australia, baseJobComInvHeaderCharge.J7_RX_NKCurrency);
		}
	}
}
