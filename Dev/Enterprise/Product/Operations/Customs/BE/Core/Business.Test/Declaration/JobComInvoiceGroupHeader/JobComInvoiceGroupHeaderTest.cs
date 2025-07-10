
using System.Collections.Generic;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceGroupHeader))]
sealed class JobComInvoiceGroupHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceGroupHeaderTest
{
	protected override CodeDescriptionPairList GetExpectedCustomsChargeTypeList() => new BECustomsChargeTypeList();

	protected override void SetUp()
	{
		base.SetUp();

		// change DistributeBy to Value based as in the base test the apportion charge is tested presuming OFT is value based
		IncoTermAndCustomsChargeFactory incoTermAndCustomsChargeFactory = new IncoTermAndCustomsChargeFactory();
		(incoTermAndCustomsChargeFactory.GetCharge("OFT") as CustomsChargeCode).DistributeBy = ChargeDistributeByList.Codes.Value;
	}

	protected override string OverseasFreightDescription => BECustomsChargeTypeList.Descriptions.OverseasFreight;

	public override void TestChargesToImportForLandedCosting()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_TransportMode = Core.Constants.TransportModes.Air;
		var groupHeader = dec.JobComInvoiceGroupHeaders[0];
		var fobInvoice = groupHeader.JobComInvoiceHeaders.AddNew();
		fobInvoice.JZ_IncoTerm = "FOB";
		fobInvoice.JZ_InvoiceAmount = 10000m;
		fobInvoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;

		var groupCharges = groupHeader.Charges;
		groupCharges.AddNew(OverseasFreightCode, 1000m, dec.LocalCurrencyCode).J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
		var deductionCharge = groupCharges.AddNew(BECustomsChargeTypeList.Codes.DeductionCharge, 100m, dec.LocalCurrencyCode);
		deductionCharge.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
		PerformExtraDeductionChargeInitialisation(deductionCharge);
		var groupCommission = groupCharges.AddNew(BECustomsChargeTypeList.Codes.Commission, 100m, dec.LocalCurrencyCode);
		groupCommission.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;

		dec.ResumeApportionment();
		AssertEquals("FOB Invoice has three charges apportioned", 3, fobInvoice.GroupCharges.Count);
		AssertEquals("First row is OFT", OverseasFreightCode, fobInvoice.GroupCharges[0].J7_ChargeType);
		AssertEquals("Apportioned OFT not included in lines", false, fobInvoice.GroupCharges[0].J7_IsIncludedInITOT);

		AssertEquals("Second row is DED", BECustomsChargeTypeList.Codes.DeductionCharge, fobInvoice.GroupCharges[1].J7_ChargeType);
		AssertEquals("Apportioned DED not included in lines", false, fobInvoice.GroupCharges[1].J7_IsIncludedInITOT);

		groupCommission.J7_IsIncludedInITOT = true;
		dec.ResumeApportionment();

		var result = new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)groupHeader).ChargesToImportForLandedCosting).ToArray();
		AssertEquals("two charges in Result", 2, result.Length);
		AssertEquals("Overseas freight code", true, result[0].ChargeDescription.Contains(OverseasFreightDescription.ToUpper()));
		AssertEquals("DED should be brought", true, result[1].ChargeDescription.Contains("Deduction (or Discount) from Entry"));
		AssertEquals("DED should be brought as a negative amount", -100m, result[1].AmountToDistribute.Amount);
		AssertEquals("OTH charge should not be brought as this is included in lines", false, result[1].ChargeDescription.Contains(BECustomsChargeTypeList.Descriptions.Commission.ToString().ToUpper()));
	}
}
