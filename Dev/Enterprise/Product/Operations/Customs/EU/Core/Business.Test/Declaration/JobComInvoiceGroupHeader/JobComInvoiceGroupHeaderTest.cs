using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	public class JobComInvoiceGroupHeaderTest : Customs.Business.Testing.BaseJobComInvoiceGroupHeaderTest
	{
		public override void TestChargeTypeList()
		{
			var dec = GetNewJobDeclarationForTest();
			ICommonInvoice commonInvoice = dec.TopGroupInvoice;
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));

			var customsChargeTypeList = GetExpectedCustomsChargeTypeList();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		protected virtual CodeDescriptionPairList GetExpectedCustomsChargeTypeList()
		{
			var customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.RemoveCode(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight);
			customsChargeTypeList.AddPair(ChargeTypeList.Codes.InternationalFreight, ChargeTypeList.Descriptions.InternationalFreight);
			customsChargeTypeList.Sort();
			return customsChargeTypeList;
		}

		public override void TestChargesToImportForLandedCosting()
		{
			var dec = GetNewJobDeclarationForTest();
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var groupHeader = dec.JobComInvoiceGroupHeaders[0];
			var fobInvoice = groupHeader.JobComInvoiceHeaders.AddNew();
			fobInvoice.JZ_IncoTerm = "FOB";
			fobInvoice.JZ_InvoiceAmount = 10000m;
			fobInvoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;

			var groupCharges = groupHeader.Charges;
			groupCharges.AddNew(OverseasFreightCode, 1000m, dec.LocalCurrencyCode).J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
			var deductionCharge = groupCharges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 100m, dec.LocalCurrencyCode);
			deductionCharge.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
			PerformExtraDeductionChargeInitialisation(deductionCharge);
			var groupCommission = groupCharges.AddNew(CustomsChargeTypeList.Codes.Commission, 100m, dec.LocalCurrencyCode);
			groupCommission.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;

			dec.ResumeApportionment();
			AssertEquals("FOB Invoice has three charges apportioned", 3, fobInvoice.GroupCharges.Count);
			AssertEquals("First row is OFT", OverseasFreightCode, fobInvoice.GroupCharges[0].J7_ChargeType);
			AssertEquals("Apportioned OFT not included in lines", false, fobInvoice.GroupCharges[0].J7_IsIncludedInITOT);

			AssertEquals("Second row is DED", CustomsChargeTypeList.Codes.DeductionCharge, fobInvoice.GroupCharges[1].J7_ChargeType);
			AssertEquals("Apportioned DED included in lines", false, fobInvoice.GroupCharges[1].J7_IsIncludedInITOT);

			groupCommission.J7_IsIncludedInITOT = true;
			dec.ResumeApportionment();

			var result = new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)groupHeader).ChargesToImportForLandedCosting).ToArray();
			AssertEquals("two charges in Result", 2, result.Length);
			AssertEquals("Overseas freight code", true, result[0].ChargeDescription.Contains(OverseasFreightDescription.ToUpper()));
			AssertEquals("DED should be brought", true, result[1].ChargeDescription.Contains("Deduction (or Discount) from Entry"));
			AssertEquals("DED should be brought as a negative amount", -100m, result[1].AmountToDistribute.Amount);
			AssertEquals("OTH charge should not be brought as this is included in lines", false, result[1].ChargeDescription.Contains(CustomsChargeTypeList.Descriptions.Commission.ToString().ToUpper()));
		}

		public void TestTotalInvoiceAmount()
		{
			var invoiceGroupHeader = Factory.New<JobComInvoiceGroupHeader>();
			invoiceGroupHeader.JZ_InvoiceAmount = 12;
			AssertEquals(12m, invoiceGroupHeader.TotalInvoiceAmount);

			AssertEquals("Caption", "Total Invoice Value", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceGroupHeader), nameof(JobComInvoiceGroupHeader.TotalInvoiceAmount)).Caption);
		}

		public void TestChargesAddDeductTotal() => CombineAssertions(() =>
		{
			Assert("Will be implemented by WI's WI00831078 WI00837660 WI00838181 WI00838254 WI00838273 WI00838919 WI00838938 WI00838971 WI00839025 WI00839039 WI00839057", true);

			var invoiceGroupHeader = Factory.New<JobComInvoiceGroupHeader>();
			AssertEquals("Caption", "Add/Deduct Stat. Value", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceGroupHeader), nameof(JobComInvoiceGroupHeader.ChargesAddDeductTotal)).Caption);
		});

		protected virtual void PerformExtraDeductionChargeInitialisation(BaseGroupInvoiceCharge charge)
		{
		}

		protected virtual string OverseasFreightDescription => ChargeTypeList.Descriptions.InternationalFreight.ToString();

		protected virtual string OverseasFreightCode => CustomsChargeTypeList.Codes.OverseasFreight;

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var declaration = (JobDeclaration)GetNewDeclarationForTest();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			groupHeader.JobComInvoiceHeaders.AddNew();
			groupHeader.Charges.AddNew();

			return groupHeader;
		}

		protected override BaseJobDeclaration GetNewDeclarationForTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			return declaration;
		}

		protected virtual JobDeclaration GetNewJobDeclarationForTest() => Factory.New<JobDeclaration>();

		public void TestTypeDeciderNew()
		{
			Assert("Update BaseJobComInvoiceGroupHeaderTypeDecider to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceGroupHeader)).GetType() == GetExpectedBusinessObjectType());
		}

		protected override Type ExpectedTypeOfCharges => typeof(GroupInvoiceChargeCollection<GroupInvoiceCharge>);
	}
}
