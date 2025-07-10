using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class ChargesComparerTest : TestCaseWithFactory
	{
		public void TestInsurancePercent()
		{
			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_InvoiceAmount = 1000;
			invoiceHeader.JZ_RX_NKInvoice_Currency = CND.RX_Code;

			InvoiceGroupHeader.Charges.RemoveAndDeleteAll();
			invoiceHeader.Charges.RemoveAndDeleteAll();

			//group charges
			AddNewCharge(InvoiceGroupHeader.Charges, Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND.RX_Code, 200m, 0m);
			AddNewCharge(InvoiceGroupHeader.Charges, Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND.RX_Code, 0m, 1m);

			//invoice header charges
			AddNewCharge(invoiceHeader.Charges, Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, CND.RX_Code, 100m, 0m);

			//invoice line amounts
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 400;

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 500;

			Declaration.ResumeApportionment();

			AssertEquals(2, invoiceHeader.GroupCharges.Count);
			AssertEquals(12m, invoiceHeader.GroupCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND));
			AssertEquals(200m, invoiceHeader.GroupCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND));

			AssertEquals(3, invoiceLine1.ApportionedCharges.Count);
			AssertEquals(44.44m, invoiceLine1.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, CND));
			AssertEquals(5.33m, invoiceLine1.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND));
			AssertEquals(88.89m, invoiceLine1.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND));

			AssertEquals(3, invoiceLine2.ApportionedCharges.Count);
			AssertEquals(55.56m, invoiceLine2.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, CND));
			AssertEquals(6.67m, invoiceLine2.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND));
			AssertEquals(111.11m, invoiceLine2.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND));
		}

		public void TestInsurancePercent1()
		{
			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoiceHeader.JZ_InvoiceAmount = 1000;
			invoiceHeader.JZ_RX_NKInvoice_Currency = CND.RX_Code;

			InvoiceGroupHeader.Charges.RemoveAndDeleteAll();
			invoiceHeader.Charges.RemoveAndDeleteAll();

			//group charges
			AddNewCharge(InvoiceGroupHeader.Charges, Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND.RX_Code, 0m, 1m);

			//invoice header charges
			AddNewCharge(invoiceHeader.Charges, Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, CND.RX_Code, 100m, 0m);
			AddNewCharge(invoiceHeader.Charges, Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND.RX_Code, 200m, 0m);

			//invoice line amounts
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 400;

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 500;

			Declaration.ResumeApportionment();

			AssertEquals(1, invoiceHeader.GroupCharges.Count);
			AssertEquals(12m, invoiceHeader.GroupCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND));

			AssertEquals(3, invoiceLine1.ApportionedCharges.Count);
			AssertEquals(44.44m, invoiceLine1.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, CND));
			AssertEquals(5.33m, invoiceLine1.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND));
			AssertEquals(88.89m, invoiceLine1.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND));

			AssertEquals(3, invoiceLine2.ApportionedCharges.Count);
			AssertEquals(55.56m, invoiceLine2.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, CND));
			AssertEquals(6.67m, invoiceLine2.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND));
			AssertEquals(111.11m, invoiceLine2.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND));
		}

		public void TestInsurancePercent2()
		{
			var invoiceHeader1 = Declaration.Invoices.AddNew();
			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader1.JZ_InvoiceAmount = 1000;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = CND.RX_Code;

			var invoiceHeader2 = Declaration.Invoices.AddNew();
			invoiceHeader2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoiceHeader2.JZ_InvoiceAmount = 2000;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = CND.RX_Code;

			InvoiceGroupHeader.Charges.RemoveAndDeleteAll();
			invoiceHeader1.Charges.RemoveAndDeleteAll();
			invoiceHeader2.Charges.RemoveAndDeleteAll();

			//group charges
			AddNewCharge(InvoiceGroupHeader.Charges, Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND.RX_Code, 0m, 1m);
			AddNewCharge(InvoiceGroupHeader.Charges, Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND.RX_Code, 400m, 0m);

			//invoice header charges
			AddNewCharge(invoiceHeader1.Charges, Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, CND.RX_Code, 100m, 0m);

			//invoice line amounts
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 400;

			var invoiceLine2 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 500;

			var invoiceLine3 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 900;

			var invoiceLine4 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 1100;

			Declaration.ResumeApportionment();

			AssertEquals(2, invoiceHeader1.GroupCharges.Count);
			AssertEquals(11.25m, invoiceHeader1.GroupCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND));
			AssertEquals(124.14m, invoiceHeader1.GroupCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND));

			AssertEquals(2, invoiceHeader2.GroupCharges.Count);
			AssertEquals(19.80m, invoiceHeader2.GroupCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND));
			AssertEquals(275.86m, invoiceHeader2.GroupCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND));

			AssertEquals(3, invoiceLine1.ApportionedCharges.Count);
			AssertEquals(44.44m, invoiceLine1.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, CND));
			AssertEquals(5m, invoiceLine1.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND));
			AssertEquals(55.17m, invoiceLine1.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND));

			AssertEquals(3, invoiceLine2.ApportionedCharges.Count);
			AssertEquals(55.56m, invoiceLine2.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, CND));
			AssertEquals(6.25m, invoiceLine2.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND));
			AssertEquals(68.97m, invoiceLine2.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND));

			AssertEquals(2, invoiceLine3.ApportionedCharges.Count);
			AssertEquals(8.91m, invoiceLine3.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND));
			AssertEquals(124.14m, invoiceLine3.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND));

			AssertEquals(2, invoiceLine4.ApportionedCharges.Count);
			AssertEquals(10.89m, invoiceLine4.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND));
			AssertEquals(151.72m, invoiceLine4.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND));
		}

		public void TestInsurancePercentAndFreightPercent()
		{
			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_InvoiceAmount = 1000;
			invoiceHeader.JZ_RX_NKInvoice_Currency = CND.RX_Code;

			InvoiceGroupHeader.Charges.RemoveAndDeleteAll();
			invoiceHeader.Charges.RemoveAndDeleteAll();

			//group charges
			AddNewCharge(InvoiceGroupHeader.Charges, Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND.RX_Code, 0m, 20m);
			AddNewCharge(InvoiceGroupHeader.Charges, Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND.RX_Code, 0m, 1m);

			//invoice header charges
			AddNewCharge(invoiceHeader.Charges, Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, CND.RX_Code, 100m, 0m);

			//invoice line amounts
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 400;

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 500;

			Declaration.ResumeApportionment();

			AssertEquals(2, invoiceHeader.GroupCharges.Count);
			AssertEquals(11.80m, invoiceHeader.GroupCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND));
			AssertEquals(180m, invoiceHeader.GroupCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND));

			AssertEquals(3, invoiceLine1.ApportionedCharges.Count);
			AssertEquals(44.44m, invoiceLine1.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, CND));
			AssertEquals(5.24m, invoiceLine1.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND));
			AssertEquals(80m, invoiceLine1.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND));

			AssertEquals(3, invoiceLine2.ApportionedCharges.Count);
			AssertEquals(55.56m, invoiceLine2.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, CND));
			AssertEquals(6.56m, invoiceLine2.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CND));
			AssertEquals(100m, invoiceLine2.ApportionedCharges.GetCharge(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CND));
		}

		static void AddNewCharge(IJobComInvChargeCollection<JobComInvCharge> charges, string chargeType, string chargeCurrency, ZDecimal chargeAmount, ZDecimal chargePercent)
		{
			var charge = charges.AddNew();
			charge.J7_ChargeType = chargeType;
			charge.J7_RX_NKCurrency = chargeCurrency;

			if (chargeAmount > 0)
			{
				charge.J7_Amount = chargeAmount;
			}

			if (chargePercent > 0)
			{
				charge.J7_Percentage = chargePercent;
			}
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		JobComInvoiceGroupHeader InvoiceGroupHeader
		{
			get { return Declaration.JobComInvoiceGroupHeaders[0]; }
		}

		RefCurrency CND
		{
			get { return cnd ?? (cnd = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.China)); }
		}
		RefCurrency cnd;

		protected override void SetUp()
		{
			base.SetUp();
			Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
		}
	}
}
