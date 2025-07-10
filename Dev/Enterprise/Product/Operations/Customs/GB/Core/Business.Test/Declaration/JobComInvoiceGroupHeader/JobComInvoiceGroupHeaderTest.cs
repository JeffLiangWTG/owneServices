using System;
using System.Collections.Generic;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using GroupInvoiceCharge = Enterprise.Customs.GB.Business.Declaration.GroupInvoiceCharge;
using InvoiceHeaderActiveCollection = Enterprise.Customs.GB.Business.Declaration.InvoiceHeaderActiveCollection;
using JobComInvoiceGroupHeader = Enterprise.Customs.GB.Business.Declaration.JobComInvoiceGroupHeader;
using JobDeclaration = Enterprise.Customs.GB.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.GB.Business.Test
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	public class JobComInvoiceGroupHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceGroupHeaderTest
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

		public override void TestChargesToImportForLandedCosting()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var groupHeader = dec.JobComInvoiceGroupHeaders[0];
			var fobInvoice = groupHeader.JobComInvoiceHeaders.AddNew();
			fobInvoice.JZ_IncoTerm = "FOB";
			fobInvoice.JZ_InvoiceAmount = 10000m;
			fobInvoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;

			var groupCharges = groupHeader.Charges;
			groupCharges.AddNew(OverseasFreightCode, 1000m, dec.LocalCurrencyCode).J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			var deductionCharge = groupCharges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 100m, dec.LocalCurrencyCode);
			deductionCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			PerformExtraDeductionChargeInitialisation(deductionCharge);
			var groupCommission = groupCharges.AddNew(CustomsChargeTypeList.Codes.Commission, 100m, dec.LocalCurrencyCode);
			groupCommission.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

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

		protected override CodeDescriptionPairList GetExpectedCustomsChargeTypeList()
		{
			var customsChargeTypeList = new Customs.Business.CustomsChargeTypeList();
			customsChargeTypeList.RemoveCode(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight);
			customsChargeTypeList.RemoveCode(Customs.Business.CustomsChargeTypeList.Codes.Commission);
			customsChargeTypeList.RemoveCode(Customs.Business.CustomsChargeTypeList.Codes.ExWorks);
			customsChargeTypeList.RemoveCode(Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight);
			customsChargeTypeList.RemoveCode(Customs.Business.CustomsChargeTypeList.Codes.LandingCharges);
			customsChargeTypeList.RemoveCode(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges);
			customsChargeTypeList.RemoveCode(Customs.Business.CustomsChargeTypeList.Codes.PackingCost);
			customsChargeTypeList.AddPair(ChargesProvider.OverseasFreightInChiefTerminologyCode_AWB, ChargesProvider.ChargeDescriptionsForGB.OverseasFreight);
			customsChargeTypeList.AddPair(ChargesProvider.AirFreightCode, ChargesProvider.AirFreightDesc);
			customsChargeTypeList.AddPair(ChargesProvider.VATAdjustmentCode, ChargesProvider.VATAdjustmentDesc);
			customsChargeTypeList.Sort();
			return customsChargeTypeList;
		}

		protected override string OverseasFreightCode => ChargesProvider.OverseasFreightInChiefTerminologyCode_AWB;

		protected override string OverseasFreightDescription => ChargesProvider.ChargeDescriptionsForGB.OverseasFreight;

		protected override Type ExpectedTypeOfCharges => typeof(GroupInvoiceChargeCollection<GroupInvoiceCharge>);

		protected override EU.Business.Declaration.JobDeclaration GetNewJobDeclarationForTest()
		{
			var declaration = base.GetNewJobDeclarationForTest();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return declaration;
		}
	}
}
