using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	class JobComInvoiceGroupHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceGroupHeaderTest
	{
		protected override CodeDescriptionPairList GetExpectedCustomsChargeTypeList()
		{
			var customsChargeTypeList = new UCCCustomsChargeTypeList();
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.Additions71Charge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.RightToReproduceCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.Deductions71Charge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge);
			if (dec.IsImport)
			{
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.InternationalFreight);
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.TransportCostsAfterEUEntry);
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.UnloadingOfGoods);
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.PortTransitFee);
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.TerminalHandlingCharge);
			}
			else
			{
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.InternationalFreightExp);
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.TransportCostsUntilESBorder);
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.InsuranceUntilESBorder);
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.OtherInternationalPayments);
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.OtherNationalPayments);
			}
			customsChargeTypeList.Sort();
			return customsChargeTypeList;
		}

		public void TestCharges()
		{
			AssertType<GroupInvoiceChargeCollection<GroupInvoiceCharge>>(dec.TopGroupInvoice.Charges);
		}

		public override void TestChargesToImportForLandedCosting()
		{
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var groupHeader = dec.JobComInvoiceGroupHeaders[0];
			var fobInvoice = groupHeader.JobComInvoiceHeaders.AddNew();
			fobInvoice.JZ_IncoTerm = "FOB";
			fobInvoice.JZ_InvoiceAmount = 10000m;
			fobInvoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;

			var groupCharges = groupHeader.Charges;
			groupCharges.AddNew(ESCustomsChargeTypeList.Codes.InternationalFreight, 1000m, dec.LocalCurrencyCode).J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
			var insuranceCharge = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 100m, dec.LocalCurrencyCode);
			insuranceCharge.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
			var groupCommission = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, 100m, dec.LocalCurrencyCode);
			groupCommission.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;

			dec.ResumeApportionment();
			AssertEquals("FOB Invoice has three charges apportioned", 3, fobInvoice.GroupCharges.Count);
			AssertEquals("First row is IFT", ESCustomsChargeTypeList.Codes.InternationalFreight, fobInvoice.GroupCharges[0].J7_ChargeType);
			AssertEquals("Apportioned OFT not included in lines", false, fobInvoice.GroupCharges[0].J7_IsIncludedInITOT);

			AssertEquals("Second row is ONS", UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, fobInvoice.GroupCharges[1].J7_ChargeType);
			AssertEquals("Apportioned ONS included in lines", false, fobInvoice.GroupCharges[1].J7_IsIncludedInITOT);

			AssertEquals("Third row is CBR", UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, fobInvoice.GroupCharges[2].J7_ChargeType);
			AssertEquals("Apportioned CBR included in lines", false, fobInvoice.GroupCharges[2].J7_IsIncludedInITOT);

			groupCommission.J7_IsIncludedInITOT = true;
			dec.ResumeApportionment();

			var result = new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)groupHeader).ChargesToImportForLandedCosting).ToArray();
			AssertEquals("two charges in Result", 2, result.Length);
			AssertEquals("IFT should be brought", true, result[0].ChargeDescription.Contains(ESCustomsChargeTypeList.Descriptions.InternationalFreight.ToString().ToUpper()));
			AssertEquals("ONS should be brought", true, result[1].ChargeDescription.Contains(UCCCustomsChargeTypeList.Descriptions.InsuranceCostsCharge.ToString().ToUpper()));
			AssertEquals("CBR charge should not be brought as this is included in lines", false, result[1].ChargeDescription.Contains(UCCCustomsChargeTypeList.Descriptions.CommissionAndBrokerageCharge.ToString().ToUpper()));
		}

		public void TestChargeTypeListExport()
		{
			dec = (JobDeclaration)GetNewJobDeclarationForTest();
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			Customs.Business.ICommonInvoice commonInvoice = dec.TopGroupInvoice;
			commonInvoice = dec.TopGroupInvoice;
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			var customsChargeTypeList = GetExpectedCustomsChargeTypeList();

			CombineAssertions(() =>
			{
				AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
				AssertEquals("Export Expected Charge Code List", customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
			});
		}

		protected override Type ExpectedTypeOfCharges => typeof(GroupInvoiceChargeCollection<GroupInvoiceCharge>);

		protected override void SetUp()
		{
			base.SetUp();
			dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
		}

		protected override EU.Business.Declaration.JobDeclaration GetNewJobDeclarationForTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			return declaration;
		}

		JobDeclaration dec;
	}
}
