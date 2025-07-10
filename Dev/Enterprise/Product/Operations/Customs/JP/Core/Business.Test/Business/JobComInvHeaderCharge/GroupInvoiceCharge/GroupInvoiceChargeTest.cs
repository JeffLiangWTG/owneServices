using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	class GroupInvoiceChargeTest : Customs.Business.Testing.BaseGroupInvoiceChargeTest
	{
		public override void TestChargePrepaidCollectCommittedToApportionedCharge()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				var invoice1 = GroupHeader.JobComInvoiceHeaders.AddNew();
				invoice1.JZ_IncoTerm = GetOverseasFreightIncoTermForTest();
				invoice1.JZ_InvoiceAmount = 1000;
				invoice1.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

				var invoice2 = GroupHeader.JobComInvoiceHeaders.AddNew();
				invoice2.JZ_IncoTerm = "CIF";
				invoice2.JZ_InvoiceAmount = 1000;
				invoice2.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

				var oNS = GroupHeader.Charges.AddNew();
				oNS.J7_ChargeType = "ONS";
				oNS.J7_Amount = 100;
				oNS.J7_RX_NKCurrency = GroupHeader.JobDeclaration.LocalCurrencyCode;

				TestDec.ResumeApportionment();
				CombineAssertions(() =>
				{
					AssertEquals("PreCondition:ONS is Prepaid", Core.Constants.PaymentType.Prepaid, oNS.J7_PrepaidCollect);
					AssertEquals("1 apportioned Charge", 1, invoice1.GroupCharges.Count);
					AssertEquals("1 apportioned Charge", 1, invoice2.GroupCharges.Count);

					oNS.J7_PrepaidCollect = Core.Constants.PaymentType.Collect;
					TestDec.ResumeApportionment();
					AssertEquals("Now it is collect", Core.Constants.PaymentType.Collect, invoice1.GroupCharges[0].J7_PrepaidCollect);
					AssertEquals("Now it is collect", Core.Constants.PaymentType.Collect, invoice2.GroupCharges[0].J7_PrepaidCollect);
				});
			}
		}

		public void TestJ7_AmountDecimalPlaces()
		{
			var groupInvoiceCharge = GroupHeader.Charges.AddNew();
			var info = groupInvoiceCharge.J7_AmountInfo;
			groupInvoiceCharge.J7_RX_NKCurrency = "TWD";
			AssertHasDecimalPlacesAttribute(info, 2);

			groupInvoiceCharge.J7_RX_NKCurrency = "JPY";
			AssertHasDecimalPlacesAttribute(info, 0);
		}
	}
}
