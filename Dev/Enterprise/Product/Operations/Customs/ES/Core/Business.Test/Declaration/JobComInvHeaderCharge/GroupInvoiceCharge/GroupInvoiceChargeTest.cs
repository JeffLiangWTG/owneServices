using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	public class GroupInvoiceChargeTest : EU.Business.Declaration.Testing.GroupInvoiceChargeTest
	{
		public void TestValidation()
		{
			var parent = Factory.New<GroupInvoiceCharge>();
			AssertType<GroupInvoiceChargeValidation>(parent.Validation);
		}

		public override void TestApportionChargeWithSameChargeTypeWithDifferntKeys()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var groupInvoiceCharge = declaration.TopGroupInvoice.Charges.AddNew();

			groupInvoiceCharge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge;
			groupInvoiceCharge.J7_Amount = 100;
			groupInvoiceCharge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			groupInvoiceCharge.J7_IsDutiable = true;
			declaration.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("PreCondition: apportioned", 1, invoice.GroupCharges.Count);

				var otherCharge = declaration.TopGroupInvoice.Charges.AddNew(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 200, declaration.LocalCurrencyCode);
				otherCharge.J7_IsDutiable = false;
				declaration.ResumeApportionment();
				AssertEquals("Two apportioned Charges", 2, invoice.GroupCharges.Count);
			});
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

		protected override string GetOverseasFreightChargeCodeForTest() => ESCustomsChargeTypeList.Codes.InternationalFreight;
	}
}
