using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	sealed class InvoiceChargeTest : Customs.Business.Testing.BaseInvoiceChargeTest
	{
		public void TestSetDefaultValueForIsIncludedInITOT()
		{
			var incotermFactory = invoice.IncoTermAndChargeFactory;
			foreach (var incoterm in incotermFactory.GetAllIncoTerms())
			{
				invoice.JZ_IncoTerm = incoterm;
				var invoiceCharge = invoice.Charges.AddNew() as InvoiceCharge;
				foreach (var chargeType in incotermFactory.GetChargeTypeList(Customs.Common.ChargeParentTypes.Invoice).GetAllCodes())
				{
					var chargeCode = incotermFactory.GetCharge(chargeType);
					var config = incotermFactory.GetConfiguration(incoterm, chargeType);
					if (!chargeCode.IsDutiable && config.IsIncludedInInvoice)
					{
						AssertDefaultIsProperlySet(invoiceCharge, chargeType);
					}
				}
			}
		}

		public void TestJ7_AmountDecimalPlaces()
		{
			var invoiceCharge = invoice.Charges.AddNew() as InvoiceCharge;
			var info = invoiceCharge.J7_AmountInfo;
			invoiceCharge.J7_RX_NKCurrency = "TWD";
			AssertHasDecimalPlacesAttribute(info, 2);

			invoiceCharge.J7_RX_NKCurrency = "JPY";
			AssertHasDecimalPlacesAttribute(info, 0);
		}

		public override void TestJ7_Calc_IsIncludedInInvoice()
		{
			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = GetIncotermToTestIsIncludedInInvoice();

			var aDD = invoice.Charges.AddNew();
			aDD.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			aDD.J7_Amount = 10m;
			aDD.J7_RX_NKCurrency = "AUD";
			AssertEquals("J7_Calc_IsIncludedInInvoice is not readonly", false, aDD.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

			aDD.J7_Calc_IsIncludedInInvoiceAmount = false;
			AssertEquals("Setting this to false should set J7_IsNotIncludedInInvoice as true", true, aDD.J7_IsNotIncludedInInvoice);

			var oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 10m;
			oFT.J7_RX_NKCurrency = "AUD";
			AssertEquals("J7_Calc_IsIncludedInInvoice should be readonly", true, oFT.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
			AssertEquals("OFT's J7_Calc_IsIncludedInInvoice should be false for FOB Invoice", false, oFT.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("OFT's J7_IsNotIncludedInInvoice should be true for FOB Invoice", true, oFT.J7_IsNotIncludedInInvoice);

			invoice.JZ_IncoTerm = GetIncotermTermFreightCanBeIncluded();
			AssertEquals("J7_Calc_IsIncludedInInvoiceAmount for ADD should be changed", false, aDD.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("J7_Calc_IsIncludedInInvoiceAmount for OFT should be changed", true, oFT.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("J7_IsNotIncludedInInvoice for OFT should be changed", false, oFT.J7_IsNotIncludedInInvoice);
		}

		public override void TestSettingIsIncludedInLinesInInvoiceChargeChangesLineApportionedCharge()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
				var oTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000);
				PrepareCharge(oTH);
				invoice.JZ_IncoTerm = invoice.IncotermEquivalentToCFRForTesting;
				invoice.JobDeclaration.ResumeApportionment();

				var line = invoice.JobComInvoiceLines.AddNew();
				line.JI_LinePrice = 10000m;
				testDec.ResumeApportionment();
				AssertEquals("Apportioned charge for line", false, line.ApportionedCharges[0].J7_IsIncludedInITOT);

				oTH.J7_IsIncludedInITOT = true;
				testDec.ResumeApportionment();
				AssertEquals("Apportioned charge for line", true, line.ApportionedCharges[0].J7_IsIncludedInITOT);
			}
		}

		void AssertDefaultIsProperlySet(InvoiceCharge invoiceCharge, ZString chargeType)
		{
			invoiceCharge.J7_ChargeType = chargeType;
			Assert(invoiceCharge.J7_IsIncludedInITOT);
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return Factory.New<InvoiceCharge>();
		}
	}
}
