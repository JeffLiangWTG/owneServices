using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	public class GroupInvoiceChargeTest : EU.Business.Declaration.Testing.GroupInvoiceChargeTest
	{
		public void TestUpdateIsDutiableRelatedFieldsIfNecessary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			declaration.Invoices.AddNew();
			var groupInvoiceHeader = declaration.TopGroupInvoice;
			var transportCharge = groupInvoiceHeader.Charges.AddNew();
			transportCharge.J7_ChargeType = FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge;
			transportCharge.J7_IsDutiable = false;
			transportCharge.J7_IsDutiable = true;
			AssertEquals("Transport charge vatibility at export should remain false whatever its dutiability.", false, transportCharge.J7_IsGSTApplicable);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			transportCharge.J7_IsDutiable = false;
			transportCharge.J7_IsDutiable = true;
			AssertEquals("Transport charge vatibility at import should follow its dutiability.", true, transportCharge.J7_IsGSTApplicable);
		}

		public override void TestChargePrepaidCollectCommittedToApportionedCharge()
		{
			var invoice1 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_InvoiceAmount = 1000;
			invoice1.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			var invoice2 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_IncoTerm = "CIF";
			invoice2.JZ_InvoiceAmount = 1000;
			invoice2.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			var oFT = GroupHeader.Charges.AddNew();
			oFT.J7_ChargeType = GetOverseasFreightChargeCodeForTest();
			oFT.J7_Amount = 100;
			oFT.J7_RX_NKCurrency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			TestDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("PreCondition:OFT is Prepaid", Core.Constants.PaymentType.Prepaid, oFT.J7_PrepaidCollect);
				AssertEquals("1 apportioned Charge", 1, invoice1.GroupCharges.Count);
				AssertEquals("1 apportioned Charge", 1, invoice2.GroupCharges.Count);

				oFT.J7_PrepaidCollect = Core.Constants.PaymentType.Collect;
				TestDec.ResumeApportionment();
				AssertEquals("Now it is collect", Core.Constants.PaymentType.Collect, invoice1.GroupCharges[0].J7_PrepaidCollect);
				AssertEquals("Now it is collect", Core.Constants.PaymentType.Collect, invoice2.GroupCharges[0].J7_PrepaidCollect);
			});
		}

		public override void TestApportionChargeWithSameChargeTypeWithDifferntKeys()
		{
			var invoice = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			//Dutiable FIFT
			GroupCharge.J7_ChargeType = ChargeCodeForTestApportionChargeWithSameChargeTypeWithDifferntKeys;
			GroupCharge.J7_Amount = 100;
			GroupCharge.J7_RX_NKCurrency = GroupHeader.JobDeclaration.LocalCurrencyCode;
			TestDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("PreCondition: Dutiable", true, GroupCharge.J7_IsDutiable);

				var nonDutyFIFT = GroupHeader.Charges.AddNew(ChargeCodeForTestApportionChargeWithSameChargeTypeWithDifferntKeys, 200, GroupHeader.JobDeclaration.LocalCurrencyCode);
				nonDutyFIFT.J7_IsDutiable = false;
				TestDec.ResumeApportionment();
				AssertEquals("Two apportioned Charges", 2, invoice.GroupCharges.Count);
			});
		}

		public void TestTransportChargeVatibilityAtExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			declaration.Invoices.AddNew();
			var groupInvoiceHeader = declaration.TopGroupInvoice;
			var transportCharge = groupInvoiceHeader.Charges.AddNew();
			transportCharge.J7_ChargeType = FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge;
			AssertEquals("Transport charge default vatibility should be false at export.", false, transportCharge.J7_IsGSTApplicable);
		}

		public void TestTransportChargeVatibilityAtImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			var groupInvoiceHeader = declaration.TopGroupInvoice;
			var transportCharge = groupInvoiceHeader.Charges.AddNew();
			transportCharge.J7_ChargeType = FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge;
			AssertEquals("Transport charge default vatibility should be true at import.", true, transportCharge.J7_IsGSTApplicable);
		}

		public void TestFlags()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "DDP";
			var groupInvoiceHeader = declaration.TopGroupInvoice;
			var charge = groupInvoiceHeader.Charges.AddNew();
			charge.J7_IsSystem = true;
			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.AdjustmentCharge;
			AssertChargeFlags(charge, false, false, false, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge;
			AssertChargeFlags(charge, true, false, false, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge;
			AssertChargeFlags(charge, false, true, true, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.InterestCharge;
			AssertChargeFlags(charge, true, false, false, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge;
			AssertChargeFlags(charge, false, true, true, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge;
			AssertChargeFlags(charge, false, true, true, true);
		}

		void AssertChargeFlags(GroupInvoiceCharge charge, bool j7_IsIncludedInITOT, bool j7_IsDutiable, bool j7_IsStatisticalValueApplicable, bool j7_IsGSTApplicable)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Included In Invoice Line", j7_IsIncludedInITOT, charge.J7_IsIncludedInITOT);
				AssertEquals("Dutiable", j7_IsDutiable, charge.J7_IsDutiable);
				AssertEquals("Statable", j7_IsStatisticalValueApplicable, charge.J7_IsStatisticalValueApplicable);
				AssertEquals("VATible", j7_IsGSTApplicable, charge.J7_IsGSTApplicable);
			});
		}

		public void TestReadOnly()
		{
			var groupInvoiceCharge = Factory.New<GroupInvoiceCharge>();
			groupInvoiceCharge.IsSystemCalculated = true;
			Assert(groupInvoiceCharge.J7_ChargeTypeInfo.ReadOnly);
			Assert(groupInvoiceCharge.J7_AmountInfo.ReadOnly);
			Assert(groupInvoiceCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			Assert(groupInvoiceCharge.J7_PercentageInfo.ReadOnly);

			groupInvoiceCharge.IsSystemCalculated = false;
			Assert(!groupInvoiceCharge.J7_ChargeTypeInfo.ReadOnly);
			Assert(!groupInvoiceCharge.J7_AmountInfo.ReadOnly);
			Assert(!groupInvoiceCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			Assert(!groupInvoiceCharge.J7_PercentageInfo.ReadOnly);
		}

		public void TestIsSystemCalculated()
		{
			var groupInvoiceCharge = Factory.New<GroupInvoiceCharge>();
			groupInvoiceCharge.IsSystemCalculated = true;
			Assert(groupInvoiceCharge.J7_IsCalculated);

			groupInvoiceCharge.IsSystemCalculated = false;
			Assert(!groupInvoiceCharge.J7_IsCalculated);
		}
	}
}
