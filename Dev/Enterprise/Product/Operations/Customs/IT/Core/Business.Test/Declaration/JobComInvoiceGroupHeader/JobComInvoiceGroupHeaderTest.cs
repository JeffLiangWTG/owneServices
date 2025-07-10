using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceGroupHeader))]
sealed class JobComInvoiceGroupHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceGroupHeaderTest
{
	public void TestDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		var header = declaration.JobComInvoiceGroupHeaders[0];
		AssertType<JobDeclaration>(header.JobDeclaration);
	}

	public void TestJobComInvoiceHeaders()
	{
		var declaration = Factory.New<JobDeclaration>();
		var header = declaration.JobComInvoiceGroupHeaders[0];
		AssertType<InvoiceHeaderActiveCollection>(header.JobComInvoiceHeaders);
	}

	protected override CodeDescriptionPairList GetExpectedCustomsChargeTypeList()
	{
		var customsChargeTypeList = new UCCCustomsChargeTypeList();
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.Additions71Charge);
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.RightToReproduceCharge);
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.Deductions71Charge);
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge);
		customsChargeTypeList.Sort();
		return customsChargeTypeList;
	}

	public override void TestChargesToImportForLandedCosting()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = MessageTypeList.Codes.Import;
		dec.JE_TransportMode = Core.Constants.TransportModes.Air;
		var groupHeader = dec.JobComInvoiceGroupHeaders[0];
		var fobInvoice = groupHeader.JobComInvoiceHeaders.AddNew();
		fobInvoice.JZ_IncoTerm = "FOB";
		fobInvoice.JZ_InvoiceAmount = 10000m;
		fobInvoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;

		var groupCharges = groupHeader.Charges;
		groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 1000m, dec.LocalCurrencyCode).J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
		var insuranceCharge = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 100m, dec.LocalCurrencyCode);
		insuranceCharge.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
		var groupCommission = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, 100m, dec.LocalCurrencyCode);
		groupCommission.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;

		dec.ResumeApportionment();
		AssertEquals("FOB Invoice has three charges apportioned", 3, fobInvoice.GroupCharges.Count);
		AssertEquals("First row is OFT", UCCCustomsChargeTypeList.Codes.TransportCostsCharge, fobInvoice.GroupCharges[0].J7_ChargeType);
		AssertEquals("Apportioned OFT not included in lines", false, fobInvoice.GroupCharges[0].J7_IsIncludedInITOT);

		AssertEquals("Second row is ONS", UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, fobInvoice.GroupCharges[1].J7_ChargeType);
		AssertEquals("Apportioned ONS included in lines", false, fobInvoice.GroupCharges[1].J7_IsIncludedInITOT);

		AssertEquals("Third row is CBR", UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, fobInvoice.GroupCharges[2].J7_ChargeType);
		AssertEquals("Apportioned CBR included in lines", false, fobInvoice.GroupCharges[2].J7_IsIncludedInITOT);

		groupCommission.J7_IsIncludedInITOT = true;
		dec.ResumeApportionment();

		var result = new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)groupHeader).ChargesToImportForLandedCosting).ToArray();
		AssertEquals("two charges in Result", 2, result.Length);
		AssertEquals("OFT should be brought", true, result[0].ChargeDescription.Contains(UCCCustomsChargeTypeList.Descriptions.TransportCostsCharge.ToString().ToUpper()));
		AssertEquals("ONS should be brought", true, result[1].ChargeDescription.Contains(UCCCustomsChargeTypeList.Descriptions.InsuranceCostsCharge.ToString().ToUpper()));
		AssertEquals("CBR charge should not be brought as this is included in lines", false, result[1].ChargeDescription.Contains(UCCCustomsChargeTypeList.Descriptions.CommissionAndBrokerageCharge.ToString().ToUpper()));
	}
}
