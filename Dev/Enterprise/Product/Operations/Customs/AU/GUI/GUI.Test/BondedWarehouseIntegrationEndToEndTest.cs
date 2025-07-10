using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class BondedWarehouseIntegrationEndToEndTest : Customs.Business.Testing.BondedWarehouseIntegrationEndToEndTest<JobDeclaration, JobComInvoiceLine, AUOrgSupplierPart, Classification, CusClassPartPivot>
	{
		protected override void AssertAdditionalChangeOfOwnershipInwardOnlyFieldsValidation(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2)
			=> throw new NotImplementedException();

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnOriginalError(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage)
			=> throw new NotImplementedException();

		protected override EDIMessage CreateChangeOfOwnershipOriginalResponseMessage(EDIMessage originalMessage, bool isSuccess) => throw new NotImplementedException();

		protected override EDIMessage SendChangeOfOwnershipOriginalMessageViaMenu(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration) => throw new NotImplementedException();

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnOriginalClear(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage)
			=> throw new NotImplementedException();

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnAmendmentError(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage)
			=> throw new NotImplementedException();

		protected override EDIMessage CreateChangeOfOwnershipAmendmentResponseMessage(EDIMessage amendmentMessage, bool isSuccess) => throw new NotImplementedException();

		protected override EDIMessage SendChangeOfOwnershipAmendmentMessageViaMenu(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration) => throw new NotImplementedException();

		protected override void SetupChangeOfOwnershipOriginalClearState(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2) => throw new NotImplementedException();

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnAmendmentClear(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage)
			=> throw new NotImplementedException();

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnWithdrawalError(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage)
			=> throw new NotImplementedException();

		protected override EDIMessage CreateChangeOfOwnershipWithdrawalResponseMessage(EDIMessage withdrawalMessage, bool isSuccess) => throw new NotImplementedException();

		protected override EDIMessage SendChangeOfOwnershipWithdrawalMessageViaMenu(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration) => throw new NotImplementedException();

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnWithdrawalClear(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage)
			=> throw new NotImplementedException();

		protected override IWarehouseIntegrationSupporter GetNewChangeOfOwnershipJob(ZString jobReference, ZString inwardEntryKey, ZDecimal quantity1, ZDecimal quantity2) => throw new NotImplementedException();

		protected override OrgHeader GetNewOwner(IWarehouseIntegrationSupporter changeOfOwnershipJob) => throw new NotImplementedException();

		protected override Customs.Business.SendsMessagesToCustomsShutterUpperer GetNewMessageInitiator() => new SendsMessagesToCustomsShutterUpperer(false);

		protected override ZString GetEntryKey(IWarehouseIntegrationSupporter job)
		{
			var declaration = (JobDeclaration)job;
			return declaration.ActiveEntryHeaders[0].EntryNumber;
		}

		protected override IWarehouseIntegrationSupporter GetNewInwardJob(ZString jobReference, ZDecimal quantity, ZDecimal quantity2)
		{
			var declaration = Helper.GetNewDeclaration(Common.Shared.SharedJobMessageTypeList.Codes.Import, jobReference, ZString.Empty, quantity);
			var invoice = declaration.Invoices[0];
			var invoiceLine2 = Helper.AddInvoiceLine(invoice, Helper.Part2, quantity2, ZString.Empty, 2);
			invoice.JZ_InvoiceAmount += invoiceLine2.JI_LinePrice;
			return declaration;
		}

		protected override void SetupInwardOriginalClearState(IWarehouseIntegrationSupporter inwardjob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, bool createOriginalResponseMessage = true)
		{
			var inwardEntry = inwardDeclaration.ActiveEntryHeaders[0];
			inwardEntry.EntryNumber = InwardEntryNumber;
			inwardEntry.CH_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			inwardEntry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			var iMDRMessage = CreateMessage(inwardDeclaration.JE_DeclarationReference, inwardEntry.EntryNumber, MesaggeNumber1, true);
			iMDRMessage.EM_LinkedObject = inwardEntry;
			iMDRMessage.EM_Status = Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK;
		}

		protected override IWarehouseIntegrationSupporter GetNewOutwardJob(ZString jobReference, ZString inwardEntryKey, ZDecimal quantity, ZDecimal quantity2)
		{
			var declaration = Helper.GetNewDeclaration(Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse, jobReference, inwardEntryKey, quantity);
			var invoice = declaration.Invoices[0];
			var invoiceLine2 = Helper.AddInvoiceLine(invoice, Helper.Part2, quantity2, inwardEntryKey, 2);
			invoice.JZ_InvoiceAmount += invoiceLine2.JI_LinePrice;
			return declaration;
		}

		protected override void SetupOutwardOriginalClearState(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, bool createOriginalResponseMessage = true)
		{
			var outwardEntry = outwardDeclaration.ActiveEntryHeaders[0];
			outwardEntry.EntryNumber = OutwardEntryNumber;
			outwardEntry.CH_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			outwardEntry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			var iMDRMessage = CreateMessage(outwardDeclaration.JE_DeclarationReference, outwardEntry.EntryNumber, MesaggeNumber2, true);
			iMDRMessage.EM_LinkedObject = outwardEntry;
			iMDRMessage.EM_Status = Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK;
		}

		protected override void UpdateQuantity(JobComInvoiceLine invoiceLine, ZDecimal quantity) => invoiceLine.JI_InvoiceQuantity = quantity;

		protected override void GetInvoiceLines(IWarehouseIntegrationSupporter job, out JobComInvoiceLine invoiceLine, out JobComInvoiceLine invoiceLine2)
		{
			var declaration = (JobDeclaration)job;
			var invoiceLines = declaration.InvoiceLines.OfType<JobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ToArray();
			invoiceLine = invoiceLines[0];
			invoiceLine2 = invoiceLines[1];
		}

		protected override Customs.Business.Testing.WhsDataTestHelper<JobDeclaration, AUOrgSupplierPart, Classification, CusClassPartPivot> CreateNewHelper() => new WhsDataTestHelper(Factory);

		protected override EDIMessage SendInwardOriginalMessageViaMenu(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration) => SendMessageViaMenu(inwardDeclaration, "Send Lodgement Message WITH Payment Approved");

		protected override void AssertAdditionalInwardFieldsValidation(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2)
		{
			inwardInvoiceLine.JI_InvoiceQuantity = 0m;
			Factory.Save();
			var messageInitiator = (Customs.Business.SendsMessagesToCustomsShutterUpperer)inwardDeclaration.MessageInitiator;
			SendInwardAmendmentMessageViaMenu(inwardJob, inwardDeclaration);
			AssertContains(Customs.Business.BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), messageInitiator.InvalidOperationText);
		}

		protected override void ProcessResponseAndAssertInwardOnOriginalError(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(responseMessage);
			responseMessage.Factory.Save();
			var subject = "Import Declaration Response(IMDR) Message for Declaration Reference: " + inwardDeclaration.JE_DeclarationReference;
			var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject.StartsWith(subject));
			AssertNotContains("(WHS Receipt:", email.Body);
		}

		protected override EDIMessage CreateInwardOriginalResponseMessage(EDIMessage originalMessage, bool isSuccess)
		{
			var entry = (CusEntryHeader)originalMessage.EM_LinkedObject;
			var declaration = entry.Declaration;
			return CreateMessage(declaration.JE_DeclarationReference, InwardEntryNumber, MesaggeNumber3, isSuccess);
		}

		protected override void ProcessResponseAndAssertInwardOnOriginalClear(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(responseMessage);
			responseMessage.Factory.Save();
			var subject = "Import Declaration Response(IMDR) Message for Declaration Reference: " + inwardDeclaration.JE_DeclarationReference;
			var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject.StartsWith(subject));
			AssertContains("<p>Stock Levels have been updated. (WHS Receipt: <a href=", email.Body);
		}

		protected override void ProcessResponseAndAssertInwardOnAmendmentError(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(responseMessage);
			responseMessage.Factory.Save();
			var subject = "Import Declaration Response(IMDR) Message for Declaration Reference: " + inwardDeclaration.JE_DeclarationReference;
			var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject.StartsWith(subject));
			AssertContains("<p>Previous Stock Levels have been restored. (WHS Receipt: <a href=", email.Body);
		}

		protected override EDIMessage CreateInwardAmendmentResponseMessage(EDIMessage amendmentMessage, bool isSuccess)
		{
			var entry = (CusEntryHeader)amendmentMessage.EM_LinkedObject;
			var declaration = entry.Declaration;
			return CreateMessage(declaration.JE_DeclarationReference, InwardEntryNumber, MesaggeNumber4, isSuccess);
		}

		protected override EDIMessage SendInwardAmendmentMessageViaMenu(IWarehouseIntegrationSupporter job, JobDeclaration inwardDeclaration) => SendMessageViaMenu(inwardDeclaration, "Send Amendment Message");

		protected override void ProcessResponseAndAssertInwardOnAmendmentClear(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(responseMessage);
			responseMessage.Factory.Save();
			var subject = "Import Declaration Response(IMDR) Message for Declaration Reference: " + inwardDeclaration.JE_DeclarationReference;
			var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject.StartsWith(subject));
			AssertContains("<p>Stock Levels have been updated. (WHS Receipt: <a href=", email.Body);
		}

		protected override void ProcessResponseAndAssertInwardOnWithdrawalError(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(responseMessage);
			responseMessage.Factory.Save();
			var subject = "Import Declaration Response(IMDR) Message for Declaration Reference: " + inwardDeclaration.JE_DeclarationReference;
			var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject.StartsWith(subject));
			AssertContains("<p>Previous Stock Levels have been restored. (WHS Receipt: <a href=", email.Body);
		}

		protected override EDIMessage CreateInwardWithdrawalResponseMessage(EDIMessage withdrawalMessage, bool isSuccess)
		{
			var entry = (CusEntryHeader)withdrawalMessage.EM_LinkedObject;
			var declaration = entry.Declaration;
			return CreateMessage(declaration.JE_DeclarationReference, InwardEntryNumber, MesaggeNumber5, isSuccess, ClearWithdrawalMessage);
		}

		protected override EDIMessage SendInwardWithdrawalMessageViaMenu(IWarehouseIntegrationSupporter job, JobDeclaration inwardDeclaration) => SendMessageViaMenu(inwardDeclaration, "Send Withdrawal Message");

		protected override void ProcessResponseAndAssertInwardOnWithdrawalClear(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(responseMessage);
			responseMessage.Factory.Save();
			var subject = "Import Declaration Response(IMDR) Message for Declaration Reference: " + inwardDeclaration.JE_DeclarationReference;
			var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject.StartsWith(subject));
			AssertContains("<p>Stock Levels Update has been canceled.</p>", email.Body);
		}

		protected override EDIMessage SendOutwardOriginalMessageViaMenu(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration) => SendMessageViaMenu(outwardDeclaration, "Send Lodgement Message WITH Payment Approved");

		protected override void AssertAdditionalOutwardFieldsValidation(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2)
		{
			outwardInvoiceLine.JI_InvoiceQuantity = 0m;
			Factory.Save();
			var messageInitiator = (Customs.Business.SendsMessagesToCustomsShutterUpperer)outwardDeclaration.MessageInitiator;
			SendOutwardAmendmentMessageViaMenu(outwardJob, outwardDeclaration);
			AssertContains(Customs.Business.BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), messageInitiator.InvalidOperationText);
		}

		protected override EDIMessage CreateOutwardOriginalResponseMessage(EDIMessage originalMessage, bool isSuccess)
		{
			var entry = (CusEntryHeader)originalMessage.EM_LinkedObject;
			var declaration = entry.Declaration;
			return CreateMessage(declaration.JE_DeclarationReference, OutwardEntryNumber, MesaggeNumber6, isSuccess);
		}

		protected override void ProcessResponseAndAssertOutwardOnOriginalError(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(responseMessage);
			responseMessage.Factory.Save();
			var subject = "Import Declaration Response(IMDR) Message for Declaration Reference: " + outwardDeclaration.JE_DeclarationReference;
			var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject.StartsWith(subject));
			AssertContains("<p>Stock Release has been canceled. (WHS Order: ", email.Body);
		}

		protected override void ProcessResponseAndAssertOutwardOnOriginalClear(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(responseMessage);
			responseMessage.Factory.Save();
			var subject = "Import Declaration Response(IMDR) Message for Declaration Reference: " + outwardDeclaration.JE_DeclarationReference;
			var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject.StartsWith(subject));
			AssertContains("<p>Stock Release can be finalized. (WHS Order: <a href=", email.Body);
		}

		protected override void ProcessResponseAndAssertOutwardOnAmendmentError(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(responseMessage);
			responseMessage.Factory.Save();
			var subject = "Import Declaration Response(IMDR) Message for Declaration Reference: " + outwardDeclaration.JE_DeclarationReference;
			var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject.StartsWith(subject));
			AssertContains("<p>Previous Stock Release has been restored. (WHS Order: <a href=", email.Body);
		}

		protected override EDIMessage CreateOutwardAmendmentResponseMessage(EDIMessage amendmentMessage, bool isSuccess)
		{
			var entry = (CusEntryHeader)amendmentMessage.EM_LinkedObject;
			var declaration = entry.Declaration;
			return CreateMessage(declaration.JE_DeclarationReference, OutwardEntryNumber, MesaggeNumber7, isSuccess);
		}

		protected override EDIMessage SendOutwardAmendmentMessageViaMenu(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration) => SendMessageViaMenu(outwardDeclaration, "Send Amendment Message");

		protected override void ProcessResponseAndAssertOutwardOnAmendmentClear(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(responseMessage);
			responseMessage.Factory.Save();
			var subject = "Import Declaration Response(IMDR) Message for Declaration Reference: " + outwardDeclaration.JE_DeclarationReference;
			var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject.StartsWith(subject));
			AssertContains("<p>Stock Release has been updated. (WHS Order: <a href=", email.Body);
		}

		protected override void ProcessResponseAndAssertOutwardOnWithdrawalError(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(responseMessage);
			responseMessage.Factory.Save();
			var subject = "Import Declaration Response(IMDR) Message for Declaration Reference: " + outwardDeclaration.JE_DeclarationReference;
			var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject.StartsWith(subject));
			AssertContains("<p>Stock Release can be finalized. (WHS Order: <a href=", email.Body);
		}

		protected override EDIMessage CreateOutwardWithdrawalResponseMessage(EDIMessage withdrawalMessage, bool isSuccess)
		{
			var entry = (CusEntryHeader)withdrawalMessage.EM_LinkedObject;
			var declaration = entry.Declaration;
			return CreateMessage(declaration.JE_DeclarationReference, OutwardEntryNumber, MesaggeNumber8, isSuccess, ClearWithdrawalMessage);
		}

		protected override EDIMessage SendOutwardWithdrawalMessageViaMenu(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration) => SendMessageViaMenu(outwardDeclaration, "Send Withdrawal Message");

		protected override void ProcessResponseAndAssertOutwardOnWithdrawalClear(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(responseMessage);
			responseMessage.Factory.Save();
			var subject = "Import Declaration Response(IMDR) Message for Declaration Reference: " + outwardDeclaration.JE_DeclarationReference;
			var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject.StartsWith(subject));
			AssertContains("<p>Stock Release has been canceled. (WHS Order: <a href=", email.Body);
		}

		protected override void ProcessResponse(IWarehouseIntegrationSupporter job, JobDeclaration declaration, JobComInvoiceLine invoiceLine, JobComInvoiceLine invoiceLine2, EDIMessage outgoingMessage, EDIMessage incomingMessage)
		{
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(incomingMessage);
			incomingMessage.Factory.Save();
		}

		ZTestHelper testHelper;
		protected override void SetUp()
		{
			base.SetUp();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			testHelper = new ZTestHelper(Factory);
			testHelper.SetupCertificates();
			var brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "54321";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "AAA";
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
		}

		EDIMessage SendMessageViaMenu(JobDeclaration declaration, string menuItem)
		{
			var sendsMessagesToCustomsGUIMock = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
			sendsMessagesToCustomsGUIMock
				.Protected()
				.Setup<bool>("ShowCPQAForm", ItExpr.IsAny<CusEntryHeaderMessageStatusFilteredCollection>())
				.Returns(true);
			sendsMessagesToCustomsGUIMock
				.Setup(m => m.GenerateWithdrawDecQuestionAndShowCPQAForm(It.IsAny<JobDeclaration>(), It.IsAny<CusEntryHeader[]>()))
				.Returns(ContinueWithSave.Yes);

			var menuMock = new Mock<EDIMenu> { CallBase = true };
			menuMock
				.Protected()
				.Setup<Customs.GUI.SendsMessagesToCustomsGUI>("GetNewMessageInitiator")
				.Returns(sendsMessagesToCustomsGUIMock.Object);
			using (var menu = menuMock.Object)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
				menu.Declaration = declaration;
				menu.MenuItems.FindByText(menuItem).PerformClick();
				if (ErrorReporter.LastMessageReported == "SendMessageWithBondedWarehouseAutomation should not be called with a supporter that has changes.")
				{
					ErrorReporter.Clear();
				}
			}

			return declaration.ActiveEntryHeaders[0].Messages.OfType<CMRIMDMessage>().Where(x => x.IsTransmitMessage).OrderByDescending(x => x.EM_SystemCreateTimeUtc + x.EM_MessageNum).FirstOrDefault();
		}

		CMRIMDRMessage CreateMessage(ZString jobReference, ZString entryNumber, ZString messageNumber, bool isSuccess, string clearMessage = ClearMessage)
		{
			var message = Factory.New<CMRIMDRMessage>();
			message.EM_Status = Messaging.Integration.EDIMessageStatusList.Codes.Queued;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_MessageNum = messageNumber;
			message.EM_MessageText = isSuccess ? string.Format(clearMessage, jobReference, entryNumber) : string.Format(ErrorMessage, jobReference);
			return message;
		}

		WhsDataTestHelper Helper => (WhsDataTestHelper)helper;

		const string InwardEntryNumber = "AAAA7GW6R";
		const string OutwardEntryNumber = "AAAEFKYMA";
		const string ClearMessage = "UNH+000002+CUSRES:D:99B:UN'BGM+961:::IMDR+34B9 8CFF 9D56:1+11'FTX+AHN+++FINALISED:FINALISED'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++BOLEROPLUS PTY LTD'NAD+CB++EAGLE DATAMATION INTERNATIONAL PTY:LIMIT'RFF+ABO:{0}/1/CMT3::3'RFF+ABT:{1}::1'RFF+ABQ:NMNM'RFF+ADU:{0}/1'RFF+AAE:N10/N20'ERP+::0'ERC+ID0547::95'FTX+AAO+++VESSEL VOYAGE NOT FOUND VESSEL ID=9065182,VOYAGE NO=999,LINKING VOYAGE NO=999'TAX+3'MOA+39:0000000002500.00'TAX+3'MOA+40:0000000002500.00'TAX+3'MOA+369:0000000000203.36'TAX+3'MOA+68:0000000000042.00'TAX+3'MOA+128:0000000000263.61'TAX+3'MOA+26:0000000000007.00'TAX+3'MOA+35:0000000000003.75'TAX+3'MOA+23:0000000000049.50'DOC+1+1'CST+1+N20::95'FTX+AAF+++FREE'TAX+1'MOA+40:0000000000500.00'TAX+1'MOA+68:0000000000008.40'TAX+1'MOA+146:000000012.5000'TAX+1'MOA+312:000000000.2100'CST+2+N10::95'FTX+AAF+++FREE'TAX+1'MOA+40:0000000002000.00'TAX+1'MOA+369:0000000000203.36'TAX+1'MOA+56:0000000002033.60'TAX+1'MOA+68:0000000000033.60'CNT+5:2'UNT+58+000002'";
		const string ClearWithdrawalMessage = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+5AFF 24FF EG6:1+11'FTX+AHN+++WITHDRAWN:WITHDRAWN'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++SECURECERTS PTY LTD'NAD+CB++WISETECH GLOBAL LIMITED'RFF+ABO:{0}/1/CMT1::4'RFF+ABT:{1}::4'RFF+ABQ:DONG 1208'RFF+ADU:{0}/1'RFF+AAE:N20'DOC+1+1'CST+1+N20::95'FTX+AAF+++FREE'CST+2+N20::95'FTX+AAF+++FREE'CNT+5:2'UNT+23+000001'";
		const string ErrorMessage = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+16I8 BEHH FIA7:1+11'NAD+MR+AAA374M::95'RFF+ABO:{0}/1/CMT1::2'ERP+::0'ERC+ID0763::95'FTX+AAO+++LODGEMENT DECLARATION QUESTION NUMBER=000000000000010 IS NOT ALLOWED FOR THIS TRANSACTION'CNT+55:1'UNT+9+000001'";

		const string MesaggeNumber8 = "00000000398438000008";
		const string MesaggeNumber7 = "00000000398438000007";
		const string MesaggeNumber6 = "00000000398438000006";
		const string MesaggeNumber5 = "00000000398438000005";
		const string MesaggeNumber4 = "00000000398438000004";
		const string MesaggeNumber3 = "00000000398438000003";
		const string MesaggeNumber2 = "00000000398438000002";
		const string MesaggeNumber1 = "00000000398438000001";
	}
}
