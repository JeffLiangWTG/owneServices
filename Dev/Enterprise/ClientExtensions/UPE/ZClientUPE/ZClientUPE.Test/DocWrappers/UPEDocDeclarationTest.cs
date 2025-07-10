using System;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Business.CommercialInvoice;
using Enterprise.Client.UPE.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentWrappers.Customs.AU;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEDocDeclaration))]
	sealed class UPEDocDeclarationTest : DocBaseJobDeclarationAbstractTest<UPEJobDeclaration, UPEDocDeclaration>
	{
		#region Factory Method
		public void TestSuperTypeOverridden()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "DecReference";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			DocDeclaration newDoc = (DocDeclaration)declaration.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.DeclarationWithCusEntryHeaders, null)[0];
			Assert("Constructed doc wrapper of correct overridden type", newDoc is UPEDocDeclaration);
		}

		#endregion
		#region New Properties
		public void TestMasterBillNoList()
		{
			Declaration.Bills.RemoveAndDeleteAll();
			AssertEquals("With no master bills", "", DeclarationWrapper.MasterBillNoList);
			Bill bill1 = Declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill1.CU_MasterBill = "M1";
			AssertEquals("With 1 master bill", "M1", DeclarationWrapper.MasterBillNoList);
			Bill bill2 = Declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.CU_MasterBill = "M2";
			AssertEquals("With 2 master bills", "M1,M2", DeclarationWrapper.MasterBillNoList);
		}

		public void TestCommercialInvoiceImage()
		{
			DocumentFactory factory = new DocumentFactoryProvider().GetFactory(this.Factory);
			StorageMain parent = factory.New<StorageMain>();
			parent.SM_ParentFK = Declaration.PK;
			StorageDocs document = parent.Documents.AddNew();
			document.SC_Date = ZDateTime.Now;
			document.SC_ImageData = UPETestHelper.TestFiles.CommercialInvoiceBmpBytes;
			document.SC_DocType = CommercialInvoiceDocManager.commercialInvoiceDocType;
			Declaration.DocManagerInfo.Documents.Add(document);
			Declaration.DocManagerInfo.Save();
			AssertNotNull("Should find the Commercial Invoice document from eDocs", DeclarationWrapper.CommercialInvoiceImage);
		}

		#endregion
		#region Alternate Broker
		public void TestHasAlternateBroker()
		{
			AssertEquals(false, DeclarationWrapper.HasAlternateBroker);
			Declaration.JE_OH_Importer = Factory.New<UPEOrgHeader>().PK;
			AssertEquals(false, DeclarationWrapper.HasAlternateBroker);
			Declaration.Importer.SetRelatedParty(Factory.New<UPEOrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			AssertEquals(true, DeclarationWrapper.HasAlternateBroker);
		}

		public void TestAlternateBrokerStorageFeeStartDate()
		{
			Declaration.JE_DateOfArrival = new ZDateTime(2006, 1, 5); // Thursday
			GlbHoliday holiday = GlbBranch.CurrentBranch.GlbHolidays.AddNew();
			holiday.GH_Date = new ZDateTime(2006, 1, 9); // Monday
			Declaration.JE_RS_NKServiceLevel = "1";
			AssertEquals("Express shipments date 2 after arrival, excluding weekends and public holidays (10th is Tuesday)", new ZDateTime(2006, 1, 10), DeclarationWrapper.AlternateBrokerStorageFeeStartDate);
			Declaration.JE_RS_NKServiceLevel = "5";
			AssertEquals("Expedited shipments date 3 after arrival, excluding weekends and public holidays (11th is Wednesday)", new ZDateTime(2006, 1, 11), DeclarationWrapper.AlternateBrokerStorageFeeStartDate);
		}

		[TestDate(2006, 2, 10)]
		public void TestAlternateBrokerStorageFeeIncludingGST()
		{
			SetupAlternateBrokerStorageFee();
			AssertEquals("AlternateBrokerStorageFeeIncludingGST for 2 days of storage", 77m, DeclarationWrapper.AlternateBrokerStorageFeeIncludingGST);
		}

		[TestDate(2006, 2, 10)]
		public void TestAlternateBrokerAmountPayable()
		{
			var callout = Factory.NewWithValidTestData<Callout>();
			SetupFinanceFreightChargeIncludingGST(callout);
			SetupFinanceSecurityFeeIncludingGST(callout);
			SetupFinanceTerminalFeeAmountIncludingGST(callout);
			SetupAlternateBrokerStorageFee();
			Factory.Save();
			UPEDataRegistry.Instance.TerminalFeeAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 30m);
			AssertEquals(77m, DeclarationWrapper.AlternateBrokerStorageFeeIncludingGST);
			AssertEquals(11m, DeclarationWrapper.FinanceFreightChargeIncludingGST);
			AssertEquals(8.8m, DeclarationWrapper.FinanceSecurityFeeIncludingGST);
			AssertEquals(15.4m, DeclarationWrapper.FinanceTerminalFeeAmountIncludingGST);
			AssertEquals("AmountPayable should be ITF+Storage+Freight+Security", 77m + 11m + 8.8m + 15.4m, DeclarationWrapper.AlternateBrokerAmountPayable);
		}

		public void TestFinanceFreightChargeIncludingGST()
		{
			var callout = Factory.NewWithValidTestData<Callout>();
			SetupFinanceFreightChargeIncludingGST(callout);
			Factory.Save();
			AssertEquals("When there is a freight charge", 11m, DeclarationWrapper.FinanceFreightChargeIncludingGST);
		}

		public void TestSecurityFeeAmountIncludingGST()
		{
			var callout = Factory.NewWithValidTestData<Callout>();
			SetupFinanceSecurityFeeIncludingGST(callout);
			Factory.Save();
			AssertEquals("When there is a freight charge", 8.8m, DeclarationWrapper.FinanceSecurityFeeIncludingGST);
		}

		public void TestFinanceTerminalFeeAmountIncludingGST()
		{
			var callout = Factory.NewWithValidTestData<Callout>();
			SetupFinanceTerminalFeeAmountIncludingGST(callout);
			Factory.Save();
			AssertEquals("When there is a freight charge", 15.4m, DeclarationWrapper.FinanceTerminalFeeAmountIncludingGST);
		}

		public void TestEstimatedFreight()
		{
			GroupInvoiceCharge charge = Declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			charge.J7_ChargeType = AUChargeCodeList.Codes.OverseasFreight;
			charge.J7_Amount = 10m;
			charge.J7_RX_NKCurrency = "AUD";
			AssertEquals(10m, DeclarationWrapper.EstimatedFreight);
		}

		void SetupFinanceFreightChargeIncludingGST(Callout callout)
		{
			callout.CS_JE_CustomsFormalEntry = Declaration.PK;
			callout.EnsureJobHeaderExists();
			CalloutCharge freightCharge = callout.JobHeader.Charges.AddNew();
			freightCharge.JR_Desc = ShipmentChargeDescription.Freight;
			freightCharge.TaxableAmount = 10m;
			AssertEquals("Freight charge + GST", 11m, callout.FinanceFreightChargeIncludingGST);
		}

		void SetupFinanceSecurityFeeIncludingGST(Callout callout)
		{
			callout.CS_JE_CustomsFormalEntry = Declaration.PK;
			callout.EnsureJobHeaderExists();
			CalloutCharge securityFeeCharge = callout.JobHeader.Charges.AddNew();
			securityFeeCharge.JR_Desc = ShipmentChargeDescription.SecurityFee;
			securityFeeCharge.TaxableAmount = 8m;
			AssertEquals("Freight charge + GST", 8.8m, callout.FinanceSecurityFeeIncludingGST);
		}

		void SetupFinanceTerminalFeeAmountIncludingGST(Callout callout)
		{
			callout.CS_JE_CustomsFormalEntry = Declaration.PK;
			callout.EnsureJobHeaderExists();
			CalloutCharge securityFeeCharge = callout.JobHeader.Charges.AddNew();
			securityFeeCharge.JR_Desc = ShipmentChargeDescription.ITFCharges;
			securityFeeCharge.TaxableAmount = 14m;
			AssertEquals("ITF Charges + GST", 15.4m, callout.FinanceTerminalFeeAmountIncludingGST);
		}

		void SetupAlternateBrokerStorageFee()
		{
			AssertEquals("TestDateAttribute must be applied for storage fee to be setup", new ZDateTime(2006, 2, 10), ZDateTime.Now);
			Declaration.JE_DateOfArrival = new ZDateTime(2006, 2, 6);
			Declaration.JE_RS_NKServiceLevel = "1";
			AssertEquals(new ZDateTime(2006, 2, 8), DeclarationWrapper.AlternateBrokerStorageFeeStartDate);
		}

		#endregion
		#region Implementation
		protected override string TestingCountry => Core.Constants.CountryCodes.Australia;

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", "RAT", 10);
			Factory.Save();
		}

		#endregion
	}
}
