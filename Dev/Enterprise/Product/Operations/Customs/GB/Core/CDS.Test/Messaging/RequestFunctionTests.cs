using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.CDS.Testing;

namespace Enterprise.Customs.GB.CDS.Messaging.Testing
{
	class RequestFunctionTests : TestCaseWithFactory
	{
		public void TestGetBuilder()
		{
			var testData = new List<(string, Customs.Business.CusdecMessageFunction, Type)>
			{
				(CDSEDIMessageTypeList.Codes.AmendDeclaration, new Customs.Business.CusdecMessageFunction.Amended(), typeof(AmendmentMessageBuilder)),
				(CDSEDIMessageTypeList.Codes.NilAmendment, new Customs.Business.CusdecMessageFunction.Amended(), typeof(NilAmendmentMessageBuilder)),
				(CDSEDIMessageTypeList.Codes.FecChallenge, new Customs.Business.CusdecMessageFunction.Amended(), typeof(FECAmendmentMessageBuilder)),
				(CDSEDIMessageTypeList.Codes.ArrivalNotification, new Customs.Business.CusdecMessageFunction.Amended(), typeof(ArrivalAmendmentMessageBuilder)),
				(CDSEDIMessageTypeList.Codes.CancelDeclaration, new Customs.Business.CusdecMessageFunction.Deleted(), typeof(CancellationRequestMessageBuilder)),
			};

			var objectToSend = MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader);

			CombineAssertions(() =>
			{
				foreach (var (messageType, messageFunction, expectedType) in testData)
				{
					objectToSend.MessageType = messageType;
					var messageBuilder = messageBuilderManager.NewMessageBuilder(objectToSend, new EU.Business.ErrorCollector(), messageFunction);
					AssertType(expectedType, messageBuilder);
				}
			});
		}

		public void TestGetBuilder_New()
		{
			var testData = new List<(string, Type)>
			{
				(ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse, typeof(H1MessageBuilder)),
				(ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing, typeof(H2MessageBuilder)),
				(ImportDeclarationTypeList.Codes.DeclarationForTemporaryAdmission, typeof(H3MessageBuilder)),
				(ImportDeclarationTypeList.Codes.DeclarationForInwardProcessing, typeof(H4MessageBuilder)),
				(ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories, typeof(H5MessageBuilder)),
				(ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration, typeof(I1MessageBuilder)),
				(ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I, typeof(C21IMessageBuilder)),
				(ImportDeclarationTypeList.Codes.ImportClearanceRequestC21N, typeof(C21NMessageBuilder)),
				(ImportDeclarationTypeList.Codes.FinalSupplementaryDeclaration, typeof(FSMessageBuilder)),
				(ImportDeclarationTypeList.Codes.SuperReducedDataSetDeclaration, typeof(H7MessageBuilder)),
				(ImportDeclarationTypeList.Codes.ReducedDataSetDeclaration, typeof(H8MessageBuilder)),
				(ImportDeclarationTypeList.Codes.BulkImportReducedDataSet, typeof(C21BMessageBuilder)),
				(ExportDeclarationTypeList.Codes.DeclarationForExport, typeof(B1MessageBuilder)),
				(ExportDeclarationTypeList.Codes.DeclarationForOutwardProcessing, typeof(B2MessageBuilder)),
				(ExportDeclarationTypeList.Codes.DeclarationForDispatchOfGoods, typeof(B4MessageBuilder)),
				(ExportDeclarationTypeList.Codes.SimplifiedDeclarationForExport, typeof(C1MessageBuilder)),
				(ExportDeclarationTypeList.Codes.ExportClearanceRequestC21E, typeof(C21EMessageBuilder)),
				(ExportDeclarationTypeList.Codes.ExportClearanceRequestC21EEIDRNOP, typeof(CENMessageBuilder)),
			};
			CombineAssertions(() =>
			{
				foreach (var (style, expectedType) in testData)
				{
					instruction.CEI_Style = style;
					var objectToSend = MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader);
					var messageBuilder = messageBuilderManager.NewMessageBuilder(objectToSend, new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
					AssertType(expectedType, messageBuilder);
				}
			});
		}

		public void TestGetBuilder_ControlledGoods()
		{
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "01001CD";
			invoiceLine.JI_CL = entryHeader.MergedLines.AddNew().PK;
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			AssertType<I1MessageBuilderControlledGoods>(messageBuilder);
		}

		public void TestGetBuilder_errorCollector()
		{
			var entryHeader2 = Factory.New<CusEntryHeader>();
			instruction.CEI_Style = "XX";
			declaration.ActiveEntryHeaders.Add(entryHeader2);
			var errorCollector = new EU.Business.ErrorCollector();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), errorCollector, new Customs.Business.CusdecMessageFunction.New());
			AssertNull(messageBuilder);
			AssertContains("CW1 does not support building message type XX", errorCollector.GetErrorsAsString());
			var messageBuilder2 = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), errorCollector, new CusdecMessageFunctionForTest.Other());
			AssertNull(messageBuilder2);
			AssertContains("CW1 cannot make that type of message, please select a valid option from the dropdown list", errorCollector.GetErrorsAsString());
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			entryHeader = Factory.New<CusEntryHeader>();
			instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			declaration.ActiveEntryHeaders.Add(entryHeader);
			messageBuilderManager = new MessageBuilderManager();
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		CusEntryInstruction instruction;
		MessageBuilderManager messageBuilderManager;
		JobDeclarationMessageSendingObjectParent decWrapper;

		class CusdecMessageFunctionForTest : Customs.Business.CusdecMessageFunction
		{
			public class Other : Customs.Business.CusdecMessageFunction
			{
				public Other()
				{ }
			}
		}
	}
}
