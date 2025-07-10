using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(IM2AdjustmentsDocumentWrapper))]
	sealed class IM2AdjustmentsDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		#region TestSourceIdentifierProvider

		public void TestISourceIdentifierProvider()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_MergeBy = "TRF";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).RX_Code;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301100000";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			var im2 = declaration.GetNewCopyToB2Declaration();
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.ApportionmentDirty = false;
			im2.DoMerge();
			Factory.Save();
			var wrapper = new IM2AdjustmentsDocumentWrapper(im2);

			var supporter = wrapper as ISourceIdentifierProvider;
			AssertNotNull("IM2AdjustmentsDocumentWrapper should implement ISourceIdentifierProvider", supporter);
			AssertEquals("supporter.SourceIdentifier", im2.PK, supporter?.SourceIdentifier);
		}

		#endregion

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		public void TestNoChangedLine()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_MergeBy = "TRF";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).RX_Code;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301100000";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			var im2 = declaration.GetNewCopyToB2Declaration();
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.ApportionmentDirty = false;
			im2.DoMerge();
			var docPages = new IM2AdjustmentsDocumentWrapper(im2).DocumentPages;
			AssertEquals("No DocPage", 0, docPages.Count);
		}

		public void TestDeletedLine()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.ApportionmentDirty = true;
			using (declaration.SuspendMarkApportionmentDirty())
			{
				declaration.CA_MergeBy = "TRF";
			}
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301100000";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0301100001";
			invoiceLine2.JI_LineNo = 1;
			invoiceLine2.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.Canada;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			var im2 = declaration.GetNewCopyToB2Declaration();
			im2.InvoiceLines[0].Delete();
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.DoMerge();
			var docPages = new IM2AdjustmentsDocumentWrapper(im2).DocumentPages;
			AssertEquals("One deleted docPage", 1, docPages.Count);
		}

		public void TestChangedLine()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).RX_Code;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TreatmentCode = "04";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			var im2 = declaration.GetNewCopyToB2Declaration();
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.InvoiceLines[0].CA_TreatmentCode = "05";
			im2.DoMerge();
			var docPages = new IM2AdjustmentsDocumentWrapper(im2).DocumentPages;
			AssertEquals("Change Lines on multiply sub-headers", 1, docPages.Count);
		}

		public void TestThrowExceptionWhenMessageTypeIsInvalid()
		{
			var notIM2 = Factory.New<JobDeclaration>();
			notIM2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var exception = AssertExceptionThrown<InvalidOperationException>(() =>
			{
				var wrapper = new IM2AdjustmentsDocumentWrapper(notIM2);
			});
			AssertEquals("Declaration must be a Copy IM2 Adjustments.", exception.Message);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).RX_Code;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TreatmentCode = "04";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			var im2 = declaration.GetNewCopyToB2Declaration();
			return new IM2AdjustmentsDocumentWrapper(im2);
		}
	}
}
