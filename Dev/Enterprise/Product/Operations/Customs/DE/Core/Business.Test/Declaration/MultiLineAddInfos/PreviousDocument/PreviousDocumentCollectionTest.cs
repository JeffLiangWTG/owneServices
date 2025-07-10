using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using NotificationType = CargoWise.EntityFramework.NotificationType;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(PreviousDocumentCollection))]
	class PreviousDocumentCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.PreviousDocumentCollectionTest
	{
		public void TestAllowNew_CusEntryInstruction()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var previousDocumentMaster = instruction.PreviousDocumentMaster;
			var collection = instruction.PreviousDocuments;

			CombineAssertions(() =>
			{
				previousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				AssertEquals("ATZL, has 1 doc -> AllowNew = True", true, collection.AllowNew);

				previousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATA;
				AssertEquals("ATA, has 1 doc -> AllowNew = False", false, collection.AllowNew);

				collection.RemoveAndDeleteAll();
				AssertEquals("Empty -> AllowNew = True", true, collection.AllowNew);
			});
		}

		public void TestAllowNew_InvoiceLine()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var previousDocumentMaster = invoiceLine.PreviousDocumentMaster;
			var collection = invoiceLine.PreviousDocuments;

			CombineAssertions(() =>
			{
				previousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				AssertEquals("ATZL, has 1 doc -> AllowNew = True", true, collection.AllowNew);

				previousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATA;
				AssertEquals("ATA, has 1 doc -> AllowNew = True", true, collection.AllowNew);
			});
		}

		public void TestCreateAdditionalFilter_Export_PreviousDocument()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var doc1 = invoiceLine.PreviousDocuments.AddNew();
			doc1.CSI_Procedure = "XX";
			var procedure1 = invoiceLine.PreviousProcedures.AddNew();
			procedure1.CSI_Procedure = PreviousProcedureList.Codes._ATZL;

			var collection = new PreviousDocumentCollection(invoiceLine, false);
			var completeFilter = collection.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals("Part of Collection", true, doc1.MatchesFilter(completeFilter));
				AssertEquals("Not Part of Collection", false, procedure1.MatchesFilter(completeFilter));
			});
		}

		public void TestCreateAdditionalFilter_Export_PreviousProcedure()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var doc1 = invoiceLine.PreviousDocuments.AddNew();
			doc1.CSI_Procedure = "XX";
			var procedure1 = invoiceLine.PreviousProcedures.AddNew();
			procedure1.CSI_Procedure = PreviousProcedureList.Codes._ATAV;

			var collection = new PreviousDocumentCollection(invoiceLine, true);
			var completeFilter = collection.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals("Not Part of Collection", false, doc1.MatchesFilter(completeFilter));
				AssertEquals("Part of Collection", true, procedure1.MatchesFilter(completeFilter));
			});
		}

		public void TestSetDefaultsForNewChild_ProcedureATA()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATA;
			var previousDocument = instruction.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Procedure", PreviousProcedureList.Codes._ATA, previousDocument.CSI_Procedure);
				AssertEquals("CSI_Status", YesNoList.Codes.Yes, previousDocument.CSI_Status);
			});
		}

		public void TestSetDefaultsForNewChild_ProcedureATZL()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "DEFAULT2");
			declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var previousDocument = instruction.PreviousDocuments.AddNew();
			AssertEquals("DEFAULT2", previousDocument.AuthorizationNumber);
		}

		public void TestSetDefaultsForNewChild_Export_NewPreviousProcedure()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;

			var newPreviousProcedure = invoiceLine.PreviousProcedures.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Procedure", PreviousProcedureList.Codes._ATZL, newPreviousProcedure.CSI_Procedure);
				AssertEquals("CSI_Status", PreviousStatusList.Codes.Yes, newPreviousProcedure.CSI_Status);
			});
		}

		public void TestSetDefaultsForNewChild_Export_NewPreviousDocument()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Procedure", ZString.Empty, previousDocument.CSI_Procedure);
				AssertEquals("CSI_Status", ZString.Empty, previousDocument.CSI_Status);
			});
		}

		public void TestSetDefaultsForNewChild_Import_ATZL()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			instruction.PreviousDocumentMaster.AuthorizationNumber = "123";
			instruction.PreviousDocumentMaster.CSI_ReferenceNumber2 = "ABC";

			var newPreviousDocument = instruction.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Procedure", PreviousProcedureList.Codes._ATZL, newPreviousDocument.CSI_Procedure);
				AssertEquals("AuthorizationNumber", "123", newPreviousDocument.AuthorizationNumber);
				AssertEquals("CSI_ReferenceNumber2", "ABC", newPreviousDocument.CSI_ReferenceNumber2);
			});
		}

		public void TestSetDefaultsForNewChild_Import_ATAV()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			instruction.PreviousDocumentMaster.SimplifiedGrantAuthorizationFlag = true;
			instruction.PreviousDocumentMaster.AuthorizationNumber = "123";
			instruction.PreviousDocumentMaster.CSI_CustomsOffice = "ABC";

			var newPreviousDocument = instruction.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Procedure", PreviousProcedureList.Codes._ATAV, newPreviousDocument.CSI_Procedure);
				AssertEquals("SimplifiedGrantAuthorizationFlag", true, newPreviousDocument.SimplifiedGrantAuthorizationFlag);
				AssertEquals("AuthorizationNumber", "123", newPreviousDocument.AuthorizationNumber);
				AssertEquals("CSI_CustomsOffice", "ABC", newPreviousDocument.CSI_CustomsOffice);
			});
		}

		public void TestTypedSingleParameterAddNew()
		{
			var bizO = GetCollectionToTest().AddNew(typeof(PreviousDocument));
			AssertNotNull(bizO);
			AssertType<PreviousDocument>(bizO);
		}

		public void TestMaxCount_ExportInvoiceHeader()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			AssertMaxCountValidator(invoice.PreviousDocuments, 99, false, NotificationType.Error, "You may enter a maximum of 99 Previous Documents");
		}

		public void TestMaxCount_ExportInvoiceHeader_InAESTransitionPeriod()
		{
			var transitionPeriodActive = true;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, transitionPeriodActive))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				var invoice = declaration.Invoices.AddNew();
				AssertMaxCountValidator(invoice.PreviousDocuments, 9, false, NotificationType.Error, "You may enter a maximum of 9 Previous Documents");
			}
		}

		public void TestMaxCount_ExportInvoiceLine()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertMaxCountValidator(invoiceLine.PreviousDocuments, 99, false, NotificationType.Error, "You may enter a maximum of 99 Previous Documents");
		}

		public void TestMaxCount_ExportInvoiceLine_InAESTransitionPeriod()
		{
			var transitionPeriodActive = true;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, transitionPeriodActive))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				AssertMaxCountValidator(invoiceLine.PreviousDocuments, 9, false, NotificationType.Error, "You may enter a maximum of 9 Previous Documents");
			}
		}

		public void TestMaxCount_ExportInvoiceLine_PreviousProcedures()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var prevProcedures = invoiceLine.PreviousProcedures as ISupportMaxCountValidation;
			AssertNull(prevProcedures.MaxCountValidator.Notification);
		}

		public void TestMaxCount_ExportCusEntryInstruction()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var prevDocuments = instruction.PreviousDocuments as ISupportMaxCountValidation;
			AssertNull(prevDocuments.MaxCountValidator.Notification);
		}

		public void TestMaxCount_ImportInvoiceHeader()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var prevDocuments = invoice.PreviousDocuments as ISupportMaxCountValidation;
			AssertNull(prevDocuments.MaxCountValidator.Notification);
		}

		public void TestMaxCount_ImportInvoiceLine()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var prevDocuments = invoiceLine.PreviousDocuments as ISupportMaxCountValidation;
			AssertNull(prevDocuments.MaxCountValidator.Notification);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new PreviousDocumentCollection(Factory.New<PreviousDocument>(), false);

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<PreviousDocument>();

		protected override CusSupportingInfoCollection<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument> GetCusSupportingInfoCollection() => new PreviousDocumentCollection(Factory.New<JobDeclaration>(), false);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;

		void AssertMaxCountValidator(ISupportMaxCountValidation collection, int expectedMaxCount, bool expectedWarnAtHalfway, INotificationType expectedNotificationType, string expectedMessage)
		{
			var maxCountValidator = collection.MaxCountValidator;
			var notification = maxCountValidator.Notification;
			CombineAssertions(() =>
			{
				AssertEquals("MaxCount", expectedMaxCount, maxCountValidator.MaxCount);
				AssertEquals("WarnAtHalfway", expectedWarnAtHalfway, maxCountValidator.WarnAtHalfway);
				AssertEquals("Notification Type", expectedNotificationType, notification.Type);
				AssertEquals("Notification Message", expectedMessage, notification.Message);
			});
		}
	}
}
