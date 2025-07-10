using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public class TransactionHeaderHelperTest : TestCaseWithFactory
	{
		public void Test_GetCreditTerms_WhenCreditNoteUsed_ShouldReturnEmpty()
		{
			var result = TransactionHeaderHelper.GetCreditTerms(TransactionTypes.CreditNote, Core.Constants.CountryCodes.Australia, Constants.InvoiceTerms.FromInvoiceDate, 14.ToString(), new ZDateTime(2022, 07, 14), new ZDateTime(2022, 07, 28));

			AssertEquals("", result.ToString());
		}

		public void Test_GetCreditTerms_WhenInvoiceTermIsEmpty_ShouldReturnCashOnDelivery()
		{
			var result = TransactionHeaderHelper.GetCreditTerms(TransactionTypes.Invoice, Core.Constants.CountryCodes.Australia, string.Empty, 14.ToString(), new ZDateTime(2022, 07, 14), new ZDateTime(2022, 07, 28));

			AssertEquals("Cash on Delivery", result.ToString());
		}

		public void Test_GetCreditTerms_WhenInvoiceTermIsLSIAndDueDateIsCalculatedFromInvoiceDate_ShouldUseInvoiceDate()
		{
			var result = TransactionHeaderHelper.GetCreditTerms(TransactionTypes.Invoice, Core.Constants.CountryCodes.Australia, Constants.InvoiceTerms.LaterOfShipmentOrInvoiceDate, 14.ToString(), new ZDateTime(2022, 07, 14), new ZDateTime(2022, 07, 28));

			AssertEquals("14 days from Inv. Date", result.ToString());
		}

		public void Test_GetCreditTerms_WhenInvoiceTermIsLSIAndDueDateIsNotCalculatedFromInvoiceDate_ShouldUseShipmentDate()
		{
			var result = TransactionHeaderHelper.GetCreditTerms(TransactionTypes.Invoice, Core.Constants.CountryCodes.Australia, Constants.InvoiceTerms.LaterOfShipmentOrInvoiceDate, 15.ToString(), new ZDateTime(2022, 07, 14), new ZDateTime(2022, 07, 28));

			AssertEquals("15 days from shipment", result.ToString());
		}

		public void Test_GetCreditTerms_WhenCountryIsPortugalAndLSIUsed_ShouldUseUnresolvedString()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.Portuguese))
			{
				var result = TransactionHeaderHelper.GetCreditTerms(TransactionTypes.Invoice, Core.Constants.CountryCodes.Portugal, Constants.InvoiceTerms.LaterOfShipmentOrInvoiceDate, 15.ToString(), new ZDateTime(2022, 07, 14), new ZDateTime(2022, 07, 28));

				AssertEquals("15 days from later shipment/invoice date", result.ToString());
			}
		}

		public void Test_GetCreditTerms_WhenCountryIsPortugal_ShouldUseUnresolvedString()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.Portuguese))
			{
				var result = TransactionHeaderHelper.GetCreditTerms(TransactionTypes.Invoice, Core.Constants.CountryCodes.Portugal, Constants.InvoiceTerms.FromShipmentDate, 15.ToString(), new ZDateTime(2022, 07, 14), new ZDateTime(2022, 07, 28));

				AssertEquals("15 days from shipment", result.ToString());
			}
		}

		public void Test_GetCreditTerms_WhenInvalidInvoiceTerm_ShouldUseTermDaysOnlyAndErrorReported()
		{
			var result = TransactionHeaderHelper.GetCreditTerms(TransactionTypes.Invoice, Core.Constants.CountryCodes.Australia, "Invalid Invoice Term", 15.ToString(), new ZDateTime(2022, 07, 14), new ZDateTime(2022, 07, 28));

			AssertEquals("15 days ", result.ToString());
			AssertEquals("Can not determine description from invoice term code 'Invalid Invoice Term'.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void Test_GetCreditTerms_WhenInvoiceTermIsDLPAndShipmentJobIsExport()
		{
			var result = TransactionHeaderHelper.GetCreditTerms(TransactionTypes.Invoice, Core.Constants.CountryCodes.Australia, Constants.InvoiceTerms.FromDeliveryOrPickupDate, 14.ToString(), ZDateTime.Now, ZDateTime.Now, isExport: true);

			AssertEquals("14 days from pickup date", result.ToString());
		}

		public void Test_GetCreditTerms_WhenInvoiceTermIsDLPAndShipmentJobIsImport()
		{
			var result = TransactionHeaderHelper.GetCreditTerms(TransactionTypes.Invoice, Core.Constants.CountryCodes.Australia, Constants.InvoiceTerms.FromDeliveryOrPickupDate, 14.ToString(), ZDateTime.Now, ZDateTime.Now, isExport: false);

			AssertEquals("14 days from delivery date", result.ToString());
		}

		public void Test_GetCreditTerms_WhenInvoiceTermIsMLI()
		{
			var result = TransactionHeaderHelper.GetCreditTerms(TransactionTypes.Invoice, Core.Constants.CountryCodes.Italy, Constants.InvoiceTerms.MultipleInstallments, string.Empty, ZDateTime.Today, ZDateTime.Today);
			AssertEquals("Multiple Installments", result.ToString());
		}

		public void TestGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP_ShipmentTypeIsExport()
		{
			AssertGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP(JobMessageTypeList.Codes.Export);
		}

		public void TestGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP_ShipmentTypeIsImport()
		{
			AssertGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP(JobMessageTypeList.Codes.Import);
		}

		public void TestGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP_ShipmentTypeIsDrawback()
		{
			AssertGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP(JobMessageTypeList.Codes.Drawback);
		}

		public void TestGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP_ShipmentTypeIsMiscellaneousCustoms()
		{
			AssertGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP(JobMessageTypeList.Codes.MiscellaneousCustoms);
		}

		public void TestGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP_ShipmentTypeIsExportDeclarationByExternalBroker()
		{
			AssertGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP(JobMessageTypeList.Codes.ExportDeclarationByExternalBroker);
		}

		public void TestGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP_ShipmentTypeIsExWarehouse()
		{
			AssertGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP(JobMessageTypeList.Codes.ExWarehouse);
		}

		public void TestGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP_ShipmentTypeIsImportDeclarationByExternalBroker()
		{
			AssertGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP(JobMessageTypeList.Codes.ImportDeclarationByExternalBroker);
		}

		public void TestGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP_ShipmentTypeIsRefund()
		{
			AssertGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP(JobMessageTypeList.Codes.Refund);
		}
		public void TestGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP_ShipmentTypeIsWarehousedByExternalAgent()
		{
			AssertGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP(JobMessageTypeList.Codes.WarehousedByExternalAgent);
		}

		public void AssertGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP(string messageType)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "AUCHI";
			declaration.JE_RL_NKFinalDestination = "ITALL";
			declaration.JE_MessageType = messageType;

			var result = TransactionHeaderHelper.GetCreditTerms(TransactionTypes.Invoice, Constants.CountryCodes.Australia, Constants.InvoiceTerms.FromDeliveryOrPickupDate, 14.ToString(), ZDateTime.Now, ZDateTime.Now, isExport: true, declaration);

			if (declaration.IsExportOrNonTransport)
			{
				AssertEquals("14 days from pickup date", result.ToString());
			}
			else
			{
				AssertEquals("14 days from delivery date", result.ToString());
			}
		}

		public void TestGetCreditTerms_DeclarationJobType_WhenInvoiceTermIsDLP_ShipmentTypeIsImportLicense_CountryCodeBrazil()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_RL_NKOrigin = "AUCHI";
				declaration.JE_RL_NKFinalDestination = "ITALL";
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

				var result = TransactionHeaderHelper.GetCreditTerms(TransactionTypes.Invoice, Constants.CountryCodes.Brazil, Constants.InvoiceTerms.FromDeliveryOrPickupDate, 14.ToString(), ZDateTime.Now, ZDateTime.Now, isExport: true, declaration);

				AssertEquals("With Brazil company and Import License shipment type delivery date should be use","14 days from delivery date", result.ToString());
			}
		}

		public void Test_GetCreditTerms_ForPortTransport_WhenInvoiceTermIsDLP()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var cartage = testObjectCreator.CreateCartage();
			cartage.JJ_EstimatedDelivery = new DateTime(2023, 12, 28);
			var cartageJob = testObjectCreator.CreateJob(cartage);
			cartageJob.JH_ParentID = cartage.PK;
			Factory.Save();

			var resultWithDeliveryDate = TransactionHeaderHelper.GetCreditTerms(TransactionTypes.Invoice, Core.Constants.CountryCodes.Australia, Constants.InvoiceTerms.FromDeliveryOrPickupDate, 14.ToString(), ZDateTime.Now, ZDateTime.Now, isExport: false, cartage: cartage);
			AssertEquals("14 days from delivery date", resultWithDeliveryDate.ToString());

			cartage.JJ_A_JCL = new DateTime(2023, 12, 25);
			Factory.Save();
			var resultWithCompletionDate = TransactionHeaderHelper.GetCreditTerms(TransactionTypes.Invoice, Core.Constants.CountryCodes.Australia, Constants.InvoiceTerms.FromDeliveryOrPickupDate, 14.ToString(), ZDateTime.Now, ZDateTime.Now, isExport: false, cartage: cartage);
			AssertEquals("14 days from completion date", resultWithCompletionDate.ToString());
		}

		public void TestCheckHasGeneratedComplianceDocumentBeforeReverse()
		{
			var invoicingBase = SetupInvoiceForAlreadyGeneratedComplianceDocument(false, true);
			AssertEquals(true, TransactionHeaderHelper.CheckHasGeneratedComplianceDocumentBeforeReverse(invoicingBase));
		}

		public void TestCheckHasGeneratedComplianceDocumentBeforeReverse_IsWritingOff()
		{
			var invoicingBase = SetupInvoiceForAlreadyGeneratedComplianceDocument(true, true);
			AssertEquals(false, TransactionHeaderHelper.CheckHasGeneratedComplianceDocumentBeforeReverse(invoicingBase));
		}

		public void TestCheckHasGeneratedComplianceDocumentBeforeReverse_IsNotBadDebtWritingOff()
		{
			var invoicingBase = SetupInvoiceForAlreadyGeneratedComplianceDocument(false, false);
			AssertEquals(true, TransactionHeaderHelper.CheckHasGeneratedComplianceDocumentBeforeReverse(invoicingBase));
		}

		InvoicingBase SetupInvoiceForAlreadyGeneratedComplianceDocument(bool isWritingOff, bool isBadDebtWritingOff)
		{
			var testObjectCreatot = new TestObjectCreator(Factory);

			var vat3 = testObjectCreatot.CreateTaxRate("VAT3", "VAT3", 3);
			vat3.AT_PostingGroupId = 0;

			var ac1 = testObjectCreatot.CreateChargeCode("AC1");
			ac1.AC_AT_GSTRate = vat3.PK;

			InvoicingBase invoicingBase;
			if (isBadDebtWritingOff)
			{
				invoicingBase = Factory.NewWithValidTestData<ARInvoice>();
			}
			else
			{
				invoicingBase = Factory.NewWithValidTestData<APInvoice>();
				AssertEquals(false, invoicingBase is IBadDebtWritingOff);
			}
			var invoiceLine = invoicingBase.Lines.AddNew() as InvoicingLineBase;
			invoiceLine.AL_JH = testObjectCreatot.Job1.PK;
			invoiceLine.AL_AC = ac1.PK;
			invoiceLine.AL_AT = vat3.PK;

			testObjectCreatot.CreateJobCharge(invoiceLine, testObjectCreatot.Job1, ac1);
			Factory.Save();

			if (isWritingOff && isBadDebtWritingOff)
			{
				(invoicingBase as IBadDebtWritingOff).IsWritingOff = true;
			}

			AssertEquals("CheckHasGeneratedComplianceDocumentBeforeReverse is false when transaction doesn't have compliance document.", false, TransactionHeaderHelper.CheckHasGeneratedComplianceDocumentBeforeReverse(invoicingBase));

			testObjectCreatot.CreateComplianceDocumentHeaderWithLine(invoicingBase.AH_Ledger, "desc", "0001", "NTC", "lineDesc", invoicingBase.Lines[0]);
			Factory.Save();

			return invoicingBase;
		}

		public void TestHasGeneratedComplianceDocumentErrorMessage()
		{
			AssertEquals("You cannot reverse an INV or CRD that is linked to a compliance document record. You need to void all compliance document records related to the transaction before proceeding to reverse.", TransactionHeaderHelper.HasGeneratedComplianceDocumentErrorMessage);
		}
	}
}
