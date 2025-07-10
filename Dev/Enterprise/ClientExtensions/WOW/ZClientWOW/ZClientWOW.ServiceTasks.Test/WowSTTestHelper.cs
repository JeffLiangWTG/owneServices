using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.Wow.Testing
{
	public class WowSTTestHelper : SharedTestHelper
	{
		public static JobDeclaration GetPopulatedDeclaration(BusinessObjectFactory factory)
		{
			var result = factory.New<JobDeclaration>();
			result.JE_OH_Importer = factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			result.JE_RL_NKOrigin = "AUSYD";
			result.JE_RL_NKFinalDestination = "MYPKG";
			result.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			result.JE_EntrySubmittedDate = new ZDateTime(2005, 09, 01);
			var item = result.DocsAndCartage.OrderItems.AddNew();
			item.JT_OrderReference = "1111";
			var foreignCurrency = RefCurrency.LoadFromCurrencyCode(factory, Core.Constants.CurrencyCodes.UnitedStates);
			var invHead = result.Invoices.AddNew();
			invHead.JZ_InvoiceNumber = "99999";
			invHead.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			var invLine = invHead.JobComInvoiceLines.AddNew();
			invLine.JI_CustomAttrib1 = "A1";
			invLine.JI_CustomAttrib2 = "A2";
			invLine.JI_PartNo = "76543";
			invLine.JI_LinePrice = 800M;
			invLine.JI_OrderNumber = "2222";
			var appCharge1 = invLine.ApportionedCharges.AddNew();
			appCharge1.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			appCharge1.J7_Amount = 175.4M;
			appCharge1.J7_RX_NKCurrency = foreignCurrency.RX_Code;
			var appCharge2 = invLine.ApportionedCharges.AddNew();
			appCharge2.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance;
			appCharge2.J7_Amount = 250.25M;
			appCharge2.J7_RX_NKCurrency = foreignCurrency.RX_Code;
			var appCharge3 = invLine.ApportionedCharges.AddNew();
			appCharge3.J7_ChargeType = "XXX";
			appCharge3.J7_Amount = 500M;
			appCharge3.J7_RX_NKCurrency = foreignCurrency.RX_Code;
			var cusHead = result.CustomsEntryHeaders.AddNew();
			cusHead.EntryNumber = "B00001111";
			var charge1 = cusHead.Charges.AddNew();
			charge1.C1_ChargeType = CusEntryChargeTypeList.Codes.EntryFee;
			charge1.C1_ChargeAmount = 90M;
			var charge2 = cusHead.Charges.AddNew();
			charge2.C1_ChargeType = CusEntryChargeTypeList.Codes.DeclarationProcessingCharge;
			charge2.C1_ChargeAmount = 100M;
			var charge3 = cusHead.Charges.AddNew();
			charge3.C1_ChargeType = CusEntryChargeTypeList.Codes.AQISContainerCharges;
			charge3.C1_ChargeAmount = 150M;
			var charge4 = cusHead.Charges.AddNew();
			charge4.C1_ChargeType = CusEntryChargeTypeList.Codes.AQISProcessingCharge;
			charge4.C1_ChargeAmount = 200M;
			var cusNum = factory.New<CusEntryNumber>();
			cusNum.CE_ParentID = cusHead.PK;
			cusNum.CE_ParentTable = cusHead.TableName;
			var cusLine = cusHead.MergedLines.AddNew();
			cusLine.CL_CustomsValue = 400M;
			cusLine.CL_DutyPercent = 12M;
			var fee1 = cusLine.Fees.AddNew();
			fee1.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;
			fee1.CF_ChargeAmount = 260M;
			var fee2 = cusLine.Fees.AddNew();
			fee2.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount;
			fee2.CF_ChargeAmount = 50M;
			var fee3 = cusLine.Fees.AddNew();
			fee3.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.GSTVATDeferred;
			fee3.CF_ChargeAmount = 330M;
			var @class = factory.New<Classification>();
			@class.CC_LookupCode = "TestLookup";
			@class.CC_ClassificationType = Classification.ClassificationType.IMP;
			@class.CC_TariffNum = "9999.99.99";
			@class.InstrumentType = "AAA";
			@class.InstrumentCode = "BBB";
			@class.TreatmentCode = "CCC";
			invLine.JI_CL = cusLine.PK;
			invLine.JI_CC = @class.PK;
			invLine.AddInfo.ZA_TILV = "30AUD";
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(result.MergeManager);
			return result;
		}
	}
}
