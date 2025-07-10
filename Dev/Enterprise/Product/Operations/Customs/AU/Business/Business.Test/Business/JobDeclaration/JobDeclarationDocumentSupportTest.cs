using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(JobDeclarationDocumentSupporter))]
	sealed class JobDeclarationDocumentSupportTest : Customs.Business.Testing.BaseJobDeclarationDocumentSupportTest
	{
		public override void TestGetBODocDataProvidersNotFoundMessage()
		{
			base.TestGetBODocDataProvidersNotFoundMessage();

			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Entry Header cannot be found. Please selecting Brokerage > Answer Declaration Questions to Merge the Declaration.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.EFTPaymentAdvice), null));
			AssertEquals("Entry Header cannot be found. Please selecting Brokerage > Answer Declaration Questions to Merge the Declaration.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.ATD), null));
			AssertEquals("Entry Header cannot be found. Please selecting Brokerage > Answer Declaration Questions to Merge the Declaration.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.CusEntryHeader), null));
			AssertEquals("You must enter Community Protection details before viewing this document.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.DeclarationWithCusEntryHeaders), null));
		}

		public void TestInactiveEntryHeadersAndEntryHeadersWithNoInvoiceLinesDontPrintEFTAdvices()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			var deactivatedAttached = Factory.NewMoq<CusEntryHeader>();
			CusEntryLine entryLine1 = deactivatedAttached.Object.MergedLines.AddNew();
			declaration.CustomsEntryHeaders.Add(deactivatedAttached.Object);
			deactivatedAttached.Setup(m => m.IsActive).Returns(false);

			var deactivatedDetached = Factory.NewMoq<CusEntryHeader>();
			CusEntryLine entryLine2 = deactivatedDetached.Object.MergedLines.AddNew();
			declaration.CustomsEntryHeaders.Add(deactivatedDetached.Object);
			deactivatedDetached.Setup(m => m.IsActive).Returns(false);

			var activeEntry = Factory.NewMoq<CusEntryHeader>();
			CusEntryLine entryLine3 = activeEntry.Object.MergedLines.AddNew();
			declaration.CustomsEntryHeaders.Add(activeEntry.Object);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine3.PK;

			DocumentWrapper[] result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, null);
			AssertEquals("Docwrapper count = customs entry header count", 2, result.Length);
			AssertEquals("should have deactivatedAttached", true, HasWrapperForThis(deactivatedAttached.Object, result));
			AssertEquals("should have activeEntry", true, HasWrapperForThis(activeEntry.Object, result));
		}

		bool HasWrapperForThis(CusEntryHeader entry, DocumentWrapper[] wrappers)
		{
			foreach (DocumentWrapper wrapper in wrappers)
			{
				if (wrapper.WrappedObject == entry)
				{
					return true;
				}
			}
			return false;
		}

		public void TestGetDataStateBeforeRun()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			StmMenuItem menuItem1 = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "EFT Request"));
			DocumentSupporterDataState dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem1);
			Assert("Data state is valid", dataState.IsValid);

			StmMenuItemBase menuItem2 = Factory.LoadTop1<StmMenuItemBase>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Entry Print"));
			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = CusEntryHeaderSchema.Constants.TableName;
			var pivot = menuItem2.Documents.AddNew();
			pivot.SI_SO = template.PK;

			dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem2);
			Assert("Data state is invalid", !dataState.IsValid);
			AssertEquals("Message error", "Entry Print cannot be printed until the Declaration is Merged. Selecting Brokerage > Answer Declaration Questions will Merge the Declaration.", dataState.ErrorMessage);

			declaration.CustomsEntryHeaders.AddNew();
			dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem2);
			Assert("Data state is valid", dataState.IsValid);
		}

		public void TestShowWarningWhenEntryPrintWhenImbalance()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			var header = (CusEntryHeader)declaration.ActiveEntryHeaders[0];
			var line = header.AllEntryLines[0];
			line.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 10m);
			line.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.CountervailingDuty, 20m);
			line.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 30m);
			line.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.GSTAmount, 40m);
			line.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.WetAmount, 50m);
			line.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.LCTAmount, 60m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISContainerCharges, 70m);
			header.CH_TotalPaid = 280m;
			AssertEquals(header.TotalAmountPayable, JobDeclarationDocumentSupporter.TotalPayableAmount(header));
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var declartionForTesting = factory2.Load<JobDeclaration>(declaration.PK);
			var headerForTesting = (CusEntryHeader)declartionForTesting.ActiveEntryHeaders[0];
			header.CH_TotalPaid += 100m;
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 100m);
			Factory.Save();
			AssertEquals(280m, headerForTesting.CH_TotalPaid);
			AssertNotEquals(headerForTesting.TotalAmountPayable, JobDeclarationDocumentSupporter.TotalPayableAmount(headerForTesting));
			var menuItem = factory2.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Entry Print"));
			var result = declartionForTesting.DocumentSupporter.GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem);
			AssertEquals("One question", 1, result.Count);
			var question = result[0];
			AssertMultilineASCIIEquals("Warning message should be displayed", JobDeclarationDocumentSupporter.ImbalanceErrorMessage, question.QuestionText);
			AssertEquals("QuestionType", QuestionType.Warning, question.QuestionType);
			AssertEquals("Title", "Warning: Total payable imbalance", question.Title);
			AssertEquals("DefaultResponse", DocumentEngineCore.DocumentSupport.AnswerType.No, question.DefaultResponse);

			headerForTesting.Reload();
			result = declartionForTesting.DocumentSupporter.GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem);
			AssertEquals("No question", 0, result.Count);
		}

		public void TestDontShowWarningWhenEntryPrintWhenImbalanceWhenHasDeferredDuty()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			var header = (CusEntryHeader)declaration.ActiveEntryHeaders[0];
			var line = header.AllEntryLines[0];
			line.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 10m);
			header.CH_TotalPaid = 20m;
			AssertNotEquals(header.TotalAmountPayable, JobDeclarationDocumentSupporter.TotalPayableAmount(header));
			var menuItem = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Entry Print"));
			var result = declaration.DocumentSupporter.GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem);
			AssertEquals(1, result.Count);
			AssertMultilineASCIIEquals("Warning message should be displayed", JobDeclarationDocumentSupporter.ImbalanceErrorMessage, result[0].QuestionText);

			header.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 3m);
			result = declaration.DocumentSupporter.GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem);
			AssertEquals("WI00315646, we don't show the warning message if there is any duty deferred from Customs.", 0, result.Count);
		}

		public override void TestGetMenuTemplateFilterValue()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals("Menu Filter", "Yes", declaration.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.AUCountry, null));
		}

		public override void TestSupportedDataContexts()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobDeclarationDocumentSupporter documentSupporter = new JobDeclarationDocumentSupporter(declaration);

			AssertEquals("DataContext.CartageAdvice is Supported", true, documentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.CartageAdvice)));
			AssertEquals("DataContext.ATD is Supported", true, documentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.ATD)));
			AssertEquals("DataContext.EFTPaymentAdvice is Supported", true, documentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.EFTPaymentAdvice)));
			// And one from Base
			AssertEquals("DataContext.JobDeclaration is Supported", true, documentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.Declaration)));
		}

		public override void TestGetDocBusinessObjects()
		{
			JobDeclaration declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();

			DocumentWrapper[] result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Declaration, null);
			AssertEquals("Document wrapper for data context of declaration is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Notes, null);
			AssertEquals("Document wrapper for data context of notes is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CommercialInvoice, null);
			AssertEquals("Document wrapper for data context of CommercialInvoice is of type DocJobComInvoiceHeader", mainNameSpace + "DocJobComInvoiceHeader", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ChargeSheet, null);
			AssertEquals("Document wrapper for data context of Charge sheet is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.DeclarationWithCusEntryHeaders, null);
			AssertEquals("Document wrapper for data context of DeclarationWithCusEntryHeaders is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, null);
			AssertEquals("Docwrapper count = customs entry header count", declaration.CustomsEntryHeaders.Count, result.Length);
			AssertEquals("Docwrapper for data context CusEntryHeader is DocCusEntryHeader", mainNameSpace + "DocCusEntryHeader", result[0].GetType().ToString());
		}

		public void TestEFTPaymentAdviceWrapperWithPAYREC()
		{
			JobDeclaration declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			declaration.InvoiceLines[0].JI_CL = entryLine.PK;
			declaration.JE_DeclarationReference = "B00122382";
			entryHeader.CH_BGMReference = "B00122382/1";

			CMRPAYRECMessage message = Factory.New<CMRPAYRECMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.PAYREC;
			entryHeader.Messages.Add(message);

			DocumentWrapper[] result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.EFTPaymentAdvice, null);
			AssertEquals("1 PAYREC on CusEntryHeaders - 1 wrapper should be created", 1, result.Length);
		}

		public void TestEFTPaymentAdviceWrapperWithPAYRECAndREFACC()
		{
			JobDeclaration declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			declaration.InvoiceLines[0].JI_CL = entryLine.PK;
			declaration.JE_DeclarationReference = "B00122382";
			entryHeader.CH_BGMReference = "B00122382/1";

			CMRPAYRECMessage message = Factory.New<CMRPAYRECMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.PAYREC;
			entryHeader.Messages.Add(message);

			CMRREFACCMessage message2 = Factory.New<CMRREFACCMessage>();
			message2.EM_MessageText = CMRImportDeclarationTestData.REFACC;
			entryHeader.Messages.Add(message2);

			DocumentWrapper[] result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.EFTPaymentAdvice, null);
			AssertEquals("PAYREC and REFACC message on CusEntryHeaders - 2 wrapper should be created", 2, result.Length);
		}

		public new void TestTransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var supporter = new JobDeclarationDocumentSupporter(declaration);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Air, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Sea, supporter.TransportMode);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => GetDocumentSupportBusinessObjectWithLandedCosting();

		protected override IDocumentSupportable GetDocumentSupportableBusinessObjectForRunningDocuments()
		{
			var declaration = (JobDeclaration)base.GetDocumentSupportableBusinessObjectForRunningDocuments();

			foreach (CusEntryHeader entryHeader in declaration.CustomsEntryHeaders)
			{
				entryHeader.Questions.AddNew();
				entryHeader.Questions.AddNew();

				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					entryLine.Questions.AddNew();
					entryLine.Questions.AddNew();
				}
			}

			var entryHeader1 = declaration.CustomsEntryHeaders[0];

			var aTDMessage = entryHeader1.Messages.AddNew(typeof(CMRATDMessage));
			aTDMessage.EM_MessageText = CusEntryHeaderTest.ATDMessageText;
			entryHeader1.Messages.Add(CusEntryHeaderDocumentSupportTest.CreateCMRPAYRECMessage(Factory));

			var message = Factory.New<CMRPAYRECMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.PAYREC;
			entryHeader1.Messages.Add(message);

			return declaration;
		}

		protected override string DbHitTestingFailureMessage(IDocumentCommand documentCommand, IDocumentSupportable documentSupportableBO)
		{
			return base.DbHitTestingFailureMessage(documentCommand, documentSupportableBO) + " for " + ((JobDeclaration)documentSupportableBO).JE_MessageType;
		}

		protected override Dictionary<string, int> MaxDBHitCounts
		{
			get
			{
				var maxHits = base.MaxDBHitCounts;

				maxHits["CusEntryNum"] = 4;
				maxHits["CusEntryCPDec"] = 3;
				maxHits["OrgAddressCapability"] = 2;
				maxHits["OrgTimetable"] = 2;
				maxHits["RefDbCmrAU_CMRLodgementQuestion"] = 3;
				maxHits["RefDbCmrAU_CMRTariffRatePeriodSnapshot"] = 7;
				maxHits["QuarantineExDocEstablishmentAndTime"] = 2;
				maxHits["QuarantineExDocLine"] = 2;
				maxHits["RefDbTrfAU_AUCClass"] = 6;

				return maxHits;
			}
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			return JobDeclaration.New(Factory);
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return !documentCommand.SU_MenuName.StartsWith("Landed Costing") && base.ExcludeDocumentCommandTest(documentCommand);
		}

		readonly ZString mainNameSpace = "Enterprise.DocumentWrappers.Customs.AU.";
		#endregion
	}
}
