using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobDeclarationApportionTest : TestCaseWithFactory
	{
		public void TestApportionOverseasFreight()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true));
		}

		public void TestApportionOverseasInsurance()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasInsurance, false, true));
		}

		public void TestApportionLandingCharges()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.LandingCharges, false, false));
		}

		public void TestApportionExWorksAmount()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.ExWorks, true, true));
		}

		public void TestApportionForeignInlandFreight()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.ForeignInlandFreight, true, true));
		}

		public void TestApportionPackingCosts()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.PackingCost, true, true));
		}

		public void TestApportionOtherCharges()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OtherCharges, true, true));
		}

		public void TestApportionDiscountCharges()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.Discount, false, false));
		}

		public void TestApportionCommissionCharges()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.Commission, true, true));
		}

		public void TestApportionChargeWhenChargeIsRelevantForIncoTerm()
		{
			testDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDeclaration.AutoCreateChargesBasedOnIncoTerm = false;
			testDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			allInvoicesGroup.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100, aUDCurrency.RX_Code);

			BaseJobComInvoiceHeader header2 = allInvoicesGroup.JobComInvoiceHeaders.AddNew();

			header1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header1.JZ_InvoiceAmount = 1000m;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;

			header2.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			header2.JZ_InvoiceAmount = 2000m;
			header2.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			testDeclaration.ResumeApportionment();
			AssertEquals("Landing Charge is not relavant for FOB invoice", 0, header1.GroupCharges.Count);
			AssertEquals("Landing Charge is apportioned to LIS invoice", 100m, header2.GroupCharges[0].J7_Amount);
		}

		public void TestChangeIncoTermUpdateApportion()
		{
			testDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDeclaration.AutoCreateChargesBasedOnIncoTerm = false;
			testDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			allInvoicesGroup.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100, aUDCurrency.RX_Code);

			BaseJobComInvoiceHeader header2 = allInvoicesGroup.JobComInvoiceHeaders.AddNew();

			header1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header1.JZ_InvoiceAmount = 1000m;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;

			header2.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			header2.JZ_InvoiceAmount = 2000m;
			header2.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			testDeclaration.ResumeApportionment();
			AssertEquals("Landing Charge is not relavant for FOB invoice", 0, header1.GroupCharges.GetCharge(CustomsChargeTypeList.Codes.LandingCharges).Length);
			AssertEquals("Landing Charge is apportioned to LIS invoice", 100m, header2.GroupCharges[0].J7_Amount);
		}

		public void TestFOBWithPreFOBCharge()
		{
			header1.JZ_InvoiceAmount = 1000m;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals("FOB Value for $1000 FOB Invoice", 1000M, header1.JZ_Calc_FOBAmount);

			header1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100);
			testDeclaration.ResumeApportionment();
			AssertEquals("FOB value for $1000 FOB invoice", 1000m, header1.JZ_Calc_FOBAmount);
			AssertEquals("CIF value for $1000 FOB invoice", 1000m, header1.JZ_Calc_CIFAmount);
			AssertEquals("Line Total For this invoice", 900m, header1.InvoiceLineTotal);

			header1.Charges.RemoveAndDeleteAll();

			allInvoicesGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 200, aUDCurrency.RX_Code);
			testDeclaration.ResumeApportionment();
			header1.GroupCharges[0].J7_IsIncludedInITOT = false;
			AssertEquals("FOB value for $1000 FOB invoice", 1200m, header1.JZ_Calc_FOBAmount);
			AssertEquals("CIF value for $1000 FOB invoice", 1200m, header1.JZ_Calc_CIFAmount);
			AssertEquals("Line Total For this invoice", 1000m, header1.InvoiceLineTotal);
		}

		public void TestFOBCIFCurrencySameAsInvoiceCurr()
		{
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			RefCurrency fOBCurrency = Factory.Load<RefCurrency>(header1.JZ_Calc_FOBCurrency);
			RefCurrency cIFCurrency = Factory.Load<RefCurrency>(header1.JZ_Calc_CIFCurrency);
			AssertEquals("FOB Currency set the same as invoice Curr", header1.JZ_RX_NKInvoice_Currency, fOBCurrency.RX_Code);
			AssertEquals("CIF Currency set the same as invoice Curr", header1.JZ_RX_NKInvoice_Currency, cIFCurrency.RX_Code);

			header1.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
			fOBCurrency = Factory.Load<RefCurrency>(header1.JZ_Calc_FOBCurrency);
			cIFCurrency = Factory.Load<RefCurrency>(header1.JZ_Calc_CIFCurrency);
			AssertEquals("FOB Currency set the same as invoice Curr", header1.JZ_RX_NKInvoice_Currency, fOBCurrency.RX_Code);
			AssertEquals("CIF Currency set the same as invoice Curr", header1.JZ_RX_NKInvoice_Currency, cIFCurrency.RX_Code);
		}

		public void TestDefaultWeightForInvoiceHeader()
		{
			AssertEquals("New invoice has a weight UQ defaultly set 'KG'", "KG", header1.JZ_WeightUQ);

			header1.JZ_WeightUQ = "T";
			AssertEquals("Weight UQ overwritable", "T", header1.JZ_WeightUQ);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobComInvoiceHeader loadedHeader = newFactory.Load<BaseJobComInvoiceHeader>(header1.PK);
			AssertEquals("Loaded Invoice should have a weight UQ", "T", loadedHeader.JZ_WeightUQ);
		}

		public void TestInvoiceChargeSetBeforeInvoiceCurrency()
		{
			header1.JZ_RX_NKInvoice_Currency = ZString.Empty;
			header1.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100);
			AssertEquals("Landing Charges Currency Is not set", true, header1.Charges[0].J7_RX_NKCurrency.IsEmpty);

			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			AssertEquals("Landing Charges currency is set now", header1.Invoice_Currency.RX_Code, header1.Charges[0].J7_RX_NKCurrency);
		}

		public void TestBalancePrice()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 10m;
			BaseJobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 20m;
			BaseJobComInvoiceLine line3 = header.JobComInvoiceLines.AddNew();
			line3.JI_LinePrice = 30m;
			header.BalancePrice();
			AssertEquals("JZ_InvoiceAmount", 60m, header.JZ_InvoiceAmount);
		}

		[ExpectNoExceptions]
		public void TestInvoiceLines_CountChangedDoesNotErrorDuringDataRefreshAfterBeingDeleted()
		{
			header1.JobComInvoiceLines.AddNew();
			Factory.Save();
			BusinessObjectFactory factoryNew = new BusinessObjectFactory();
			var decFromNewFactory = Factory.Load<JobDeclaration>(testDeclaration.PK);
			decFromNewFactory.JobComInvoiceGroupHeaders.RemoveAndDeleteAll();
			factoryNew.Save();
		}

		public void TestAQISEUPlaceOfDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			AssertEquals(typeof(AUJobDocAddress), invoice.AQISEUPlaceOfDestination.GetType());
		}

		#region Implementation

		BaseJobDeclaration testDeclaration;
		BaseJobComInvoiceGroupHeader allInvoicesGroup;
		BaseJobComInvoiceHeader header1;
		RefCurrency aUDCurrency;
		RefCurrency uSDCurrency;

		protected override void SetUp()
		{
			base.SetUp();
			testDeclaration = Factory.New<BaseJobDeclaration>();
			allInvoicesGroup = testDeclaration.JobComInvoiceGroupHeaders[0];
			header1 = allInvoicesGroup.JobComInvoiceHeaders.AddNew();
			aUDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			uSDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			distributeByForExport = CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
		}

		protected override void TearDown()
		{
			base.TearDown();
			distributeByForExport?.Dispose();
		}

		IDisposable distributeByForExport;

		void AssertApportion(ChargeCodeChargeKey chargeKey)
		{
			header1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			header1.JZ_InvoiceAmount = 1000m;
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;

			allInvoicesGroup.Charges.AddNew(chargeKey.ChargeCode, 150m, JobDeclaration.LocalCurrencyConstantCode);
			testDeclaration.ResumeApportionment();
			AssertEquals("Calculated " + chargeKey.ChargeCode + " - Apportioned", 150.0m, header1.GroupCharges[0].J7_Amount);
		}

		#endregion

	}
}
