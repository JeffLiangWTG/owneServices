using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(B3ImportMessageManager))]
	sealed class B3ImportMessageManagerTest : CAMessageManagerTestCase
	{
		public void TestMessageGetActionCodeDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00001111";
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			Factory.Save();

			var manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
			Factory.Save();

			AssertEquals("Original B3 Message for Declaration B00001111 has been generated.", manager.LastNotification(MessageSubTypes.Create));
		}

		IB3Header GetB3Header()
		{
			var messages = new EDIMessageCollection(Factory.New<OrgHeader>());
			var mock = new Mock<IB3Header>();
			mock.Setup(m => m.BatchNumber).Returns(ZString.Empty);
			mock.Setup(m => m.B3TypeCode).Returns(ZString.Empty);
			mock.Setup(m => m.PaymentCode).Returns(ZString.Empty);
			mock.Setup(m => m.CBSAOffice).Returns(ZString.Empty);
			mock.Setup(m => m.PortOfUnlading).Returns(ZString.Empty);
			mock.Setup(m => m.WarehouseNumber).Returns(ZString.Empty);
			mock.Setup(m => m.TransactionNumber).Returns(ZString.Empty);
			mock.Setup(m => m.BusinessNumber).Returns(ZString.Empty);
			mock.Setup(m => m.GSTNumber).Returns(ZString.Empty);
			mock.Setup(m => m.TransportMode).Returns(ZString.Empty);
			mock.Setup(m => m.CarrierCodeAtImportation).Returns(ZString.Empty);
			mock.Setup(m => m.Messages).Returns(messages);
			mock.Setup(m => m.SumPosAndNeg).Returns(false);

			var mockRelease1 = new Mock<IB3BRelease>();
			mockRelease1.Setup(m => m.CargoControlNumber).Returns(ZString.Empty);
			mockRelease1.Setup(m => m.DateOfRelease).Returns(ZDateTime.Empty);
			mock.Setup(m => m.B3BInputReleases).Returns(new[] { mockRelease1.Object });
			mock.Setup(m => m.TotalValueForDuty).Returns(ZDecimal.Zero);

			var mockSubHeader = new Mock<IB3SubHeader>();
			mockSubHeader.Setup(m => m.B3SubHeaderNumber).Returns(ZInt.Zero);
			mockSubHeader.Setup(m => m.FreightCharges).Returns(ZDecimal.Zero);
			mockSubHeader.Setup(m => m.Vendor).Returns(Factory.New<JobDocAddress>());
			mockSubHeader.Setup(m => m.DateOfDirectShipment).Returns(ZDateTime.Empty);
			mockSubHeader.Setup(m => m.CountryOfOrigin).Returns(ZString.Empty);
			mockSubHeader.Setup(m => m.PlaceOfExport).Returns(ZString.Empty);
			mockSubHeader.Setup(m => m.USPortOfExit).Returns(ZString.Empty);
			mockSubHeader.Setup(m => m.TariffTreatmentCode).Returns(ZString.Empty);
			mockSubHeader.Setup(m => m.TimeLimitUnit).Returns(ZString.Empty);
			mockSubHeader.Setup(m => m.B3TimeLimits).Returns(ZInt.Zero);
			mockSubHeader.Setup(m => m.CurrencyCode).Returns(ZString.Empty);
			mockSubHeader.Setup(m => m.VendorStateAndZip).Returns(new VendorStateAndZipStruct());
			mock.Setup(m => m.PositiveB3SubHeaders).Returns(new[] { mockSubHeader.Object });
			mock.Setup(m => m.NegativeB3SubHeaders).Returns(Array.Empty<IB3SubHeader>());

			var mockLine = new Mock<IClassificationLine1>();
			mockLine.Setup(m => m.B3LineNumber).Returns(ZShort.Zero);
			mockLine.Setup(m => m.RecordIdentifier).Returns(ZString.Empty);
			mockLine.Setup(m => m.B3SubHeaderNumber).Returns(ZInt.Zero);
			mockLine.Setup(m => m.ClassificationNumber).Returns(ZString.Empty);
			mockLine.Setup(m => m.ValueForDutyCode).Returns(ZString.Empty);
			mockLine.Setup(m => m.TariffCode).Returns(ZString.Empty);
			mockLine.Setup(m => m.ValueForCurrency).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.ValueForDuty).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.ValueForTax).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.AuthorityNumber).Returns(ZString.Empty);
			mockLine.Setup(m => m.TRSNumber).Returns(ZString.Empty);
			mockLine.Setup(m => m.PartNumberDescriptions).Returns(Array.Empty<ZString>());

			var mockInvoice1 = new Mock<IInvoiceCrossReference>();
			mockInvoice1.Setup(m => m.InvoiceLineNumber).Returns(ZInt.Zero);
			mockInvoice1.Setup(m => m.InvoicePageNumber).Returns(ZInt.Zero);
			mockInvoice1.Setup(m => m.InvoiceValue).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.InvoiceCrossReferences).Returns(new[] { mockInvoice1.Object });
			mockLine.Setup(m => m.SIMACode).Returns(ZString.Empty);
			mockLine.Setup(m => m.SIMAAssessment).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.ExciseTaxRate).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.ExciseTaxRateType).Returns(RateTypes.Codes.Specific);
			mockLine.Setup(m => m.ExciseExemptionCode).Returns(ZString.Empty);
			mockLine.Setup(m => m.ExciseTaxAmount).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.IsDummyExciseTaxRate).Returns(false);
			mockLine.Setup(m => m.RateOfGST).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.GSTRateType).Returns(RateTypes.Codes.AdValorem);
			mockLine.Setup(m => m.GSTAmount).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.GSTExemptionCode).Returns(ZString.Empty);

			var mockAmounts = new Mock<ITotalAmounts>();
			mockAmounts.Setup(m => m.TotalCustomsDuty).Returns(ZDecimal.Zero);
			mockAmounts.Setup(m => m.TotalSIMAAssessment).Returns(ZDecimal.Zero);
			mockAmounts.Setup(m => m.TotalExciseTax).Returns(ZDecimal.Zero);
			mockAmounts.Setup(m => m.TotalGST).Returns(ZDecimal.Zero);
			mockAmounts.Setup(m => m.TotalAllDutyAndTaxes).Returns(78m);
			mock.Setup(m => m.PositiveTotalAmounts).Returns(mockAmounts.Object);
			mock.Setup(m => m.NegativeTotalAmounts).Returns(mockAmounts.Object);
			return mock.Object;
		}

		public void TestIsCreditCheckRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00001111";
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 1m);
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 2m);
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTDirectAmount, 3m);
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 4m);
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount, 5m);
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount, 6m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 7m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 8m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTDirectAmount, 9m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 10m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount, 11m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount, 12m);

			var messageBuilder = new B3CusdecMessageBuilder<EDIMessage>(GetB3Header(), MessageSubTypes.Create);
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message;

			var sentMessage = Factory.New<B3Message>();
			sentMessage.EM_MessageText = message.EM_MessageText;
			sentMessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			sentMessage.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			sentMessage.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Sent;
			sentMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-1);

			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			interchange.EI_Status = EDIInterchange.Status.Sent;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_From = GlbCompany.CurrentCompany.GC_Code;
			interchange.EI_To = "CAC";
			interchange.EI_BodyText = sentMessage.EM_MessageText;
			interchange.ContainedMessages.Add(sentMessage);

			entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			var b3ImportWrapper = new B3ImportMessageWrapper(entryHeader);
			entryHeader.Messages.Add(sentMessage);
			var manager = new B3ImportMessageManagerForTesting(b3ImportWrapper);
			AssertEquals("78m, entry.TotalDutyAndTax", false, manager.PreCheck4CreditOKToSendChecking_Exposed(declaration));

			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 5m);
			entryHeader.ResetCachedValuesForTest();
			AssertEquals("78m, entry.TotalDutyAndTax", true, manager.PreCheck4CreditOKToSendChecking_Exposed(declaration));
		}

		[TestDate(2012, 7, 30)]
		public void TestExchangeRateReMergeOption()
		{
			var exportDate = new ZDateTime(2012, 7, 29);
			var releasedDate = new ZDateTime(2012, 7, 28);
			var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
			var helper = new DeclarationTestHelper(factory, true);
			var canada = factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada);

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = helper.CreateOrganisation("SUP", "SUPPLIER NAME", "AUMEL", "SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567").PK;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var importer = helper.CreateOrganisation("IMP", "IMPORTER NAME", "CATOR", "IMPORTER ADDRESS", "IMPORTER CITY", "123 4567");
			declaration.JE_OH_Importer = importer.PK;
			declaration.Importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "3021", canada);
			declaration.Importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, "1234", canada);
			declaration.Importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax, "0987654321", canada);
			declaration.JE_CustomsOffice = "351";
			declaration.CA_UnladingOffice = "423";
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";
			declaration.JE_EntryAuthorisationDate = releasedDate;
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "212112345678987654321";
			var supplier1 = helper.CreateOrganisation("SUP", "SUPPLIER NAME", "AUMEL", "SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567").PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			invoice.JZ_OH_Supplier = supplier1;
			invoice.ExporterDocumentaryAddress.OrganisationPK = helper.CreateOrganisation("SUP", "EXPORTER NAME", "USCHI", "EXPORTER ADDRESS", "CHICARGO", "IL", "12321", "123 4567").PK;
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			invoice.CA_USStateOfExport = USStatesList.Codes.NewYork;
			invoice.CA_TradeZone = "168B";
			invoice.CA_USPortOfExit = "2813";
			invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			invoice.JZ_ValuationDateOverride = exportDate;
			helper.USD.SetCustomsRate(releasedDate, releasedDate, 1.111111);
			invoice.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
			invoice.JZ_Weight = 250;
			invoice.JZ_WeightUQ = "LB";
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 1, 1, 1000, 1000, 666, 333, Core.Constants.Weight.Kilograms, 222);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			factory.Save();
			var ediReleaseEntryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.EDIRelease);
			ediReleaseEntryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.AuthorisedToDeliver;
			declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Broker;
			factory.Save();

			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			var entryLine = entryHeader.MergedLines[0];
			AssertEquals("VFD", 1111.11m, entryLine.CL_CustomsValue);

			var manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
			ZString messageText;
			Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			AssertNull("Last Message", manager.LastMessage);

			helper.USD.SetCustomsRate(releasedDate, releasedDate, 1.25);
			factory.Save();

			manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
			manager.SetNextAnswer(false);
			Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			AssertEquals("Last Message", "The exchange rate last used with this job is not current and a new rate now exists. Would you like the system to recalculate now using the most recent data?", manager.LastMessage);
			entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			entryLine = entryHeader.MergedLines[0];
			AssertEquals("VFD", 1111.11m, entryLine.CL_CustomsValue);

			manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
			manager.SetNextAnswer(true);
			Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			AssertEquals("Last Message", "Duty and Tax has been recalculated. These new values will be sent in the B3 message.", manager.LastMessage);
			entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			entryLine = entryHeader.MergedLines[0];
			AssertEquals("VFD", 1250m, entryLine.CL_CustomsValue);

			factory.Save();
			manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
			Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			AssertNull("Last Message", manager.LastMessage);
			entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			entryLine = entryHeader.MergedLines[0];
			AssertEquals("VFD", 1250m, entryLine.CL_CustomsValue);

			using (declaration.SuspendMarkApportionmentDirty())
			{
				((JobComInvoiceLine)invoice.InvoiceLines[0]).DutiesAndTaxes.DeleteAll();
			}
			factory.Save();

			manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
			manager.SetNextAnswer(false);
			Assert("CanSendThisMessage_1", manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			var extraLineForTesting = entryHeader.AllEntryLines.AddNew();
			AssertEquals("Last Message_1", "This job needs Duty and Tax recalculated. Would you like the system to recalculate now using the most recent data?", manager.LastMessage);
			AssertEquals("PositiveClassificationLines re-calculated", 1, manager.DataWrapperForTesting.PositiveClassificationLines.Count());

			manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
			manager.SetNextAnswer(true);
			Assert("CanSendThisMessage_2", manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			AssertEquals("Last Message_2", "Duty and Tax has been recalculated. These new values will be sent in the B3 message.", manager.LastMessage);

			AssertEquals("PositiveClassificationLines re-calculated", 2, manager.DataWrapperForTesting.PositiveClassificationLines.Count());
		}

		public void TestLogCustomsCommenced()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			Factory.Save();

			var manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
			manager.OverrideCanSendThisMessage = true;
			manager.SendMessage(MessageSubTypes.Create, false);

			var commencedEvent = declaration.Logs.MostRecentLogByEventTime(Events.CustomsCommenced, "CA Import");
			AssertNotNull("Customs Commenced event should exist on declaration", commencedEvent);

			manager.ResetDeclaration();
			commencedEvent = declaration.Logs.MostRecentLogByEventTime(Events.CustomsCommenced, "CA Import");
			AssertNull("Customs Commenced event should be cancelled", commencedEvent);
			var cancelledEvent = declaration.Logs.MostRecentLogByEventTime(Events.Cancelled);
			AssertNotNull("Customs Commenced event cancelled should exist on declaration", cancelledEvent);
		}

		public void TestOnMessageQueuedForSending()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			Factory.Save();

			var wrapper = new B3ImportMessageWrapper(entryHeader);
			var testManager = new B3ImportMessageManagerForTesting(wrapper);
			var scheduledTime = ZDateTime.Now.AddMinutes(-10);

			Assert("scheduledTime is not set", testManager.ScheduledTime.IsEmpty);
			Assert("CH_EntrySubmittedDate is not set", entryHeader.CH_EntrySubmittedDate.IsEmpty);
			Assert("JE_EntrySubmittedDate is not set", declaration.JE_EntrySubmittedDate.IsEmpty);

			testManager.ScheduledTime = scheduledTime;
			Assert("scheduledTime is set to scheduledTime", testManager.ScheduledTime == scheduledTime);

			testManager.Call_OnMessageQueuedForSending(MessageSubTypes.Create);

			Assert("CH_EntrySubmittedDate is set to scheduledTime", entryHeader.CH_EntrySubmittedDate == scheduledTime);
			Assert("JE_EntrySubmittedDate is set to scheduledTime", declaration.JE_EntrySubmittedDate == scheduledTime);
			Assert("scheduledTime is reset", testManager.ScheduledTime.IsEmpty);
		}

		public void TestSubmissionDateOnCustomsCommenced()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			Factory.Save();

			Assert(declaration.JE_EntrySubmittedDate.IsEmpty);
			Assert(entryHeader.CH_EntrySubmittedDate.IsEmpty);

			var manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
			manager.OverrideCanSendThisMessage = true;
			manager.SendMessage(MessageSubTypes.Create, false);

			Assert(!declaration.JE_EntrySubmittedDate.IsEmpty);
			Assert(!entryHeader.CH_EntrySubmittedDate.IsEmpty);
		}

		[TestDate(2012, 7, 22)]
		public override void TestCanSendThisMessage()
		{
			using (Globals.SetIsWinzorForTest(true))
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				Env.Security.CAB3MsgSend.IsAllowed = false;
				ZString messageText;
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2012, 7, 21);
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, string.Empty);
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.CL_CustomsValue = 1m;

				var manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertEquals("MessageText", "Job not yet saved, Please save before sending.", messageText);

				Factory.Save();
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				Assert(messageText, messageText.Contains("You do not have the appropriate security rights to run this function."));
				Assert(messageText, messageText.Contains("B3 CUSDEC Message Send"));

				Env.Security.CAB3MsgSend.IsAllowed = true;
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertEquals("MessageText", @"The Network Client ID is not configured,
in the registry for Company - EDI, Branch - BNE. Please contact your System Administrator.", messageText);

				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
				declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
				Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CBSABO, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
				{
					Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CBSABO, Core.Constants.CountryCodes.Canada, new ZDateTime(2012, 7, 25), true))
				{
					Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
					AssertEquals("MessageText", "CBSA is currently in the CARM Blackout period where no CAD entries may be sent. Entry cannot be sent at this time.", manager.LastMessage);
				}

				declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Default;
				var importer = Factory.New<OrgHeader>();
				importer.FillWithValidTestData();
				declaration.JE_OH_Importer = importer.PK;
				declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
				declaration.ImporterAddInfo.ZO_IsGSTDirectAutoRated = true;
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2012, 7, 15);
				declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
				Factory.Save();
				Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CBSABO, Core.Constants.CountryCodes.Canada, new ZDateTime(2012, 7, 25), true))
				{
					Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
					AssertEquals("MessageText", "CBSA is currently in the CARM Blackout period where no CAD entries may be sent. Entry cannot be sent at this time.", manager.LastMessage);
				}

				manager.SetNextAnswer(true);
				declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
				Factory.Save();
				Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CBSABO, Core.Constants.CountryCodes.Canada, new ZDateTime(2012, 7, 25), true))
				{
					Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
					AssertEquals("MessageText", "CBSA is currently in the CARM Blackout period where no CAD entries may be sent. Entry cannot be sent at this time.", manager.LastMessage);
				}

				declaration.JE_MessageSubType = B3EntryTypeList.Codes.NoB3;
				Factory.Save();
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertEquals("MessageText", "B3 messages may not be sent when B3 Entry Type is 'No B3 required'", manager.LastMessage);

				declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
				declaration.JE_OH_Importer = importer.PK;
				declaration.Importer.CompanyData.OB_AROnCreditHold = true;
				declaration.Importer.CompanyData.OB_IsDebtor = true;
				declaration.HasChanges = false;
				declaration.Importer.HasChanges = false;
				Factory.Save();

				Assert("Not OK to send message", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertEquals("Submit message with credit restriction canceled.", messageText);

				declaration.JE_EntryStatus = "MAN";
				Factory.Save();
				Assert("Not OK to send message", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertEquals("Submit message with credit restriction canceled.", messageText);

				AssertCanSendThisMessage(entryHeader, "Submit message with credit restriction canceled.");
				using (CACustomsDataRegistry.Instance.EnableCreditCheckForCLVS.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
					Factory.Save();
					AssertCanSendThisMessage(entryHeader, "");

					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					Factory.Save();
					Assert("Not OK to send message", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
					AssertEquals("Submit message with credit restriction canceled.", messageText);
				}

				declaration.B3EntryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
				declaration.Importer.CompanyData.OB_IsDebtor = false;
				declaration.Importer.CompanyData.OB_AROnCreditHold = false;
				declaration.HasChanges = false;
				declaration.Importer.HasChanges = false;
				Factory.Save();
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertEquals("MessageText", "B3 messages may not be sent when B3 Entry Status is Clear", manager.LastMessage); // TODO

				declaration.B3EntryHeader.CH_EntryStatus = ZString.Empty;
				declaration.Importer.CompanyData.OB_IsDebtor = false;
				declaration.Importer.CompanyData.OB_AROnCreditHold = false;
				declaration.HasChanges = false;
				declaration.Importer.HasChanges = false;
				Factory.Save();

				declaration.TransactionNumber.AccountSecurityCode = "12345";
				declaration.TransactionNumber.SequentialNumber = "00006789";
				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration2.TransactionNumber.AccountSecurityCode = "54321";
				declaration2.TransactionNumber.SequentialNumber = "00006789";
				var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
				entryHeader2.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				entryHeader2.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
				var declaration3 = Factory.New<JobDeclaration>();
				declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration3.TransactionNumber.AccountSecurityCode = "54339";
				declaration3.TransactionNumber.SequentialNumber = "00006789";
				var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
				entryHeader3.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				Factory.Save();
				manager.SetNextAnswer(false);
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertEquals("MessageText", "There is another job B00001002 having the same transaction sequence number waiting for response. A B3 response from Customs does not contain an ASEC number and system might not be able to identify a correct originating job. It is recommended that you wait until other jobs are responded. Are you sure you wish to continue?", manager.LastMessage);

				entryHeader3.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
				Factory.Save();
				manager.SetNextAnswer(false);
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertEquals("MessageText", "There are other jobs B00001002,B00001003 having the same transaction sequence number waiting for response. A B3 response from Customs does not contain an ASEC number and system might not be able to identify a correct originating job. It is recommended that you wait until other jobs are responded. Are you sure you wish to continue?", manager.LastMessage);

				entryHeader2.CH_Status = ZString.Empty;
				entryHeader3.CH_Status = ZString.Empty;
				Factory.Save();

				Assert("OK to send message", manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CBSABO, Core.Constants.CountryCodes.Canada, new ZDateTime(2012, 7, 25), true))
				{
					Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
					AssertEquals("MessageText", "CBSA is currently in the CARM Blackout period where no CAD entries may be sent. Entry cannot be sent at this time.", manager.LastMessage);
				}

				entryHeader2 = null;
				entryHeader3 = null;
				Factory.Save();
				declaration2.B3EntryHeader.Delete();
				declaration3.B3EntryHeader.Delete();
				Assert("OK to send message", manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CBSABO, Core.Constants.CountryCodes.Canada, new ZDateTime(2012, 7, 25), true))
				{
					Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
					AssertEquals("MessageText", "CBSA is currently in the CARM Blackout period where no CAD entries may be sent. Entry cannot be sent at this time.", manager.LastMessage);
				}
			}
		}

		[TestDate(2012, 7, 4)]
		public void TestCanSendB3Message_ScheduleCheck()
		{
			using (CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX"))
			{
				var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
				ZString messageText;
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 1000;
				var line = invoice.JobComInvoiceLines.AddNew();
				JobComInvoiceLineTestHelper.FillInvoiceLine(line, 3, 1, 1000, 1000, 666, 333, Core.Constants.Weight.Kilograms);
				line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Mexico;
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				using (declaration.SuspendMarkApportionmentDirty())
				{
					line.DutiesAndTaxes.DeleteAll();
				}
				Factory.Save();
				var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
				B3ImportMessageManagerForTesting manager;
				CombineAssertions("Case 1: In case B3Defer notification not right, show the defer suggestion as part of Additional Warning", () =>
				{
					Factory.Save();
					manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
					manager.SetNextAnswer(false);
					Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
					AssertEquals("LastMessage.Text", @"This job needs Duty and Tax recalculated. Would you like the system to recalculate now using the most recent data?", manager.LastMessage);
					Assert("MessageInstructionForm should NOT get a chance Show", !manager.Notification.HasShowMessageInstructionFormBeenCalled);
					manager.SetNextAnswer(false);
					Factory.Save();
					manager.SendMessage(MessageSubTypes.Create);
					Assert("MessageInstructionForm should Show", manager.Notification.HasShowMessageInstructionFormBeenCalled);
					AssertEquals("MessageInstructionForm.AdditionalWarningsMessage", @"The following unusual situations have been detected. Please read the following carefully.
One, or more, invoice lines do not have any GST details. It is recommended that you do NOT continue. You should run 'Generate Entries (Merge)' from the Brokerage menu and then check the GST details on lines.
The Release Status indicates that this shipment has not yet been cleared.
The due date for accounting of this Low Value Shipment (released on 04-Jul-12) appears to fall after the Monthly Statement cut-off date of 24-Jul-12, based on the system settings the Entry message will be deferred until the next pay period.
Are you sure you wish to continue and schedule the B3 message at this time?", manager.Notification.AdditionalWarningsMessage);
				});
				CombineAssertions("Case 2: B3Defer notification correct, but said no to Defer suggestion", () =>
				{
					Factory.Save();
					manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader), true, false);
					manager.SetNextAnswer(false);
					Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
					AssertEquals("LastMessage.Text", @"This job needs Duty and Tax recalculated. Would you like the system to recalculate now using the most recent data?", manager.LastMessage);
					Assert("MessageInstructionForm should NOT get the chance to show", !manager.Notification.HasShowMessageInstructionFormBeenCalled);
					Factory.Save();
				});
				CombineAssertions("Case 3: B3Defer notification correct, and said yes to Defer suggestion", () =>
				{
					Factory.Save();
					manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader), true, true);
					manager.SetNextAnswer(true);
					Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
					AssertEquals("LastMessage.Text", @"Duty and Tax has been recalculated. These new values will be sent in the B3 message.", manager.LastMessage);
					Assert("MessageInstructionForm should NOT get a chance Show", !manager.Notification.HasShowMessageInstructionFormBeenCalled);
					Factory.Save();
					manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader), true, true);
					manager.SendMessage(MessageSubTypes.Create);
					Assert("MessageInstructionForm should Show", manager.Notification.HasShowMessageInstructionFormBeenCalled);
					AssertEquals("MessageInstructionForm.AdditionalWarningsMessage", @"The following unusual situations have been detected. Please read the following carefully.
The Release Status indicates that this shipment has not yet been cleared.
The due date for accounting of this Low Value Shipment (released on 04-Jul-12) appears to fall after the Monthly Statement cut-off date of 24-Jul-12, based on the system settings the Entry message will be deferred until the next pay period.
Are you sure you wish to continue and schedule the B3 message at this time?", manager.Notification.AdditionalWarningsMessage);
				});
			}
		}

		[TestDate(2012, 7, 4)]
		public void TestCanSendB3Message_CancelDeferredMessage()
		{
			var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
			ZString messageText;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000;
			var line = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 3, 1, 1000, 1000, 666, 333, Core.Constants.Weight.Kilograms);
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Mexico;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			line.DutiesAndTaxes.DeleteAll();
			Factory.Save();
			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			var testMessage1 = entryHeader.Messages.AddNew();
			testMessage1.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage1.EM_Status = EDIMessage.Status.Queued;
			testMessage1.EM_HeldUntilDate = ZDateTime.UtcNow.AddDays(3);
			Factory.Save();

			B3ImportMessageManagerForTesting manager;
			CombineAssertions("Defer Message", () =>
			{
				Assert(declaration.HasScheduledB3Message);
				manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader), new TestUserNotificationWithB3ScheduleSupport());
				AssertNull("defer instruction not exists before CanSendThisMessage", manager.DeferInstruction_Exposed);
				Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
				AssertNotNull("defer instruction exists", manager.DeferInstruction_Exposed);
				AssertEquals("ActionCode as Defer", "DFR", manager.DeferInstruction_Exposed.DeferActionCode);
			});
		}

		public override void TestGetMessageBuilder()
		{
			AssertEquals("GetMessageBuilder",
				typeof(B3CusdecMessageBuilder<B3Message>),
				((B3ImportMessageManagerForTesting)messageManager).GetMessageBuilder_Exposed(MessageSubTypes.Undefined).GetType());
		}

		[TestDate(2015, 1, 1)]
		public void TestSendMessage()
		{
			using (CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345"))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				Env.Security.CAACROSSMsgSend.IsAllowed = true;
				declaration.CA_ServiceOption = ServiceOptions.Codes.ReplaceRMDwithAQ;
				declaration.CA_AssesmentOption = AssessmentOptions.Codes.AppraisalQualityData;
				Factory.Save();

				var manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
				manager.SendMessage(MessageSubTypes.Create);
				var notification = manager.Notification;
				CombineAssertions("Notification After 'Create' message", () =>
				{
					Assert(notification.HasShowMessageInstructionFormBeenCalled);
					Assert("not IsWaitingForResponse", !notification.IsWaitingForResponse);
					Assert("ContainsValidationErrors", notification.ContainsValidationErrors);
					AssertContains("ValidationErrorsMessage", MessageSendingValidation.MessageErrorsExistHeaderText, notification.ValidationErrorsMessage);
					AssertContains("ValidationErrorsMessage", MessageSendingValidation.WarningWhenInTestModeText, notification.ValidationErrorsMessage);
					Assert("not ContainsAdditionalWarnings", notification.ContainsAdditionalWarnings);
					AssertEquals("AdditionalWarningsMessage", @"The following unusual situations have been detected. Please read the following carefully.
The Release Status indicates that this shipment has not yet been cleared.
Are you sure you wish to continue and send the B3 message at this time?", notification.AdditionalWarningsMessage);
				});
				AssertEquals(new ZDateTime(2015, 1, 1), declaration.B3EntrySubmittedDate);
				notification.Reset();
				manager.SendMessage(MessageSubTypes.Change);
				notification = manager.Notification;
				CombineAssertions("Notification After 'Change' message", () =>
				{
					Assert("IsWaitingForResponse", notification.IsWaitingForResponse);
					Assert("ContainsValidationErrors", notification.ContainsValidationErrors);
					AssertContains("ValidationErrorsMessage", MessageSendingValidation.MessageErrorsExistHeaderText, notification.ValidationErrorsMessage);
					AssertContains("ValidationErrorsMessage", MessageSendingValidation.WarningWhenInTestModeText, notification.ValidationErrorsMessage);
					Assert("not ContainsAdditionalWarnings", notification.ContainsAdditionalWarnings);
					AssertEquals("AdditionalWarningsMessage", @"The following unusual situations have been detected. Please read the following carefully.
The Release Status indicates that this shipment has not yet been cleared.
Are you sure you wish to continue and send the B3 message at this time?", notification.AdditionalWarningsMessage);
				});
			}
		}

		[TestDate(2012, 7, 22)]
		public void TestB3Message()
		{
			using (CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345"))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2012, 7, 21);
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, string.Empty);
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.CL_CustomsValue = 1m;
				var invoice = declaration.Invoices.AddNew();
				JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
				line.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
				line.CA_PageNumber = 1;

				var manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
				var notification = manager.Notification;
				Env.Security.CAB3MsgSend.IsAllowed = true;
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
				declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;

				var importer = Factory.New<OrgHeader>();
				importer.FillWithValidTestData();
				declaration.JE_OH_Importer = importer.PK;
				declaration.ImporterAddInfo.ZO_IsCSAApprovedImporter = false;
				Factory.Save();

				manager.SetNextAnswer(false);
				notification.Reset();
				manager.SendMessage(MessageSubTypes.Create);
				Assert(notification.HasShowMessageInstructionFormBeenCalled);
				Assert(notification.ContainsAdditionalWarnings);
				AssertEquals("Release status text", @"
The following unusual situations have been detected. Please read the following carefully.
The Release Status indicates that this shipment has not yet been cleared.
The due date for accounting of this Low Value Shipment (released on 21-Jul-12) appears to fall after the Monthly Statement cut-off date of 24-Jul-12, based on the system settings the Entry message will be deferred until the next pay period.
Are you sure you wish to continue and schedule the B3 message at this time?".Trim(), notification.AdditionalWarningsMessage);

				manager.DeferInstruction_Exposed.DeferActionCode = DeferredB3SendActionList.Codes.Now;
				manager.SetNextAnswer(false);
				notification.Reset();
				manager.SendMessage(MessageSubTypes.Create);
				Assert(notification.HasShowMessageInstructionFormBeenCalled);
				Assert(notification.ContainsAdditionalWarnings);
				AssertEquals("Release status text", @"
The following unusual situations have been detected. Please read the following carefully.
The Release Status indicates that this shipment has not yet been cleared.
The due date for accounting of this Low Value Shipment (released on 21-Jul-12) appears to fall after the Monthly Statement cut-off date of 24-Jul-12.
Are you sure you wish to continue and send the B3 message at this time?".Trim(), notification.AdditionalWarningsMessage);

				declaration.ImporterAddInfo.ZO_IsCSAApprovedImporter = true;
				Factory.Save();

				manager.SetNextAnswer(false);
				notification.Reset();
				manager.SendMessage(MessageSubTypes.Create);
				Assert(notification.HasShowMessageInstructionFormBeenCalled);
				Assert(notification.ContainsAdditionalWarnings);
				AssertEquals("Release status text", @"
The following unusual situations have been detected. Please read the following carefully.
The Release Status indicates that this shipment has not yet been cleared.
Are you sure you wish to continue and send the B3 message at this time?".Trim(), notification.AdditionalWarningsMessage);

				declaration.ImporterAddInfo.ZO_IsCSAApprovedImporter = false;
				Factory.Save();
				entryLine.CL_CustomsValue = 10000m;
				entryHeader.ResetTotalsAndCachedValues();
				manager.DeferInstruction_Exposed.DeferActionCode = DeferredB3SendActionList.Codes.Defer;

				manager.SetNextAnswer(false);
				notification.Reset();
				manager.SendMessage(MessageSubTypes.Create);
				Assert(notification.HasShowMessageInstructionFormBeenCalled);
				Assert(notification.ContainsAdditionalWarnings);
				AssertEquals("Release status text", @"
The following unusual situations have been detected. Please read the following carefully.
The Release Status indicates that this shipment has not yet been cleared.
The due date for accounting of this job (released on 21-Jul-12) appears to fall after the Monthly Statement cut-off date of 24-Jul-12, based on the system settings the Entry message will be deferred until the next pay period.
Are you sure you wish to continue and schedule the B3 message at this time?".Trim(), notification.AdditionalWarningsMessage);

				manager.DeferInstruction_Exposed.DeferActionCode = DeferredB3SendActionList.Codes.Now;
				manager.SetNextAnswer(false);
				notification.Reset();
				manager.SendMessage(MessageSubTypes.Create);
				Assert(notification.HasShowMessageInstructionFormBeenCalled);
				Assert(notification.ContainsAdditionalWarnings);
				AssertEquals("Release status text", @"
The following unusual situations have been detected. Please read the following carefully.
The Release Status indicates that this shipment has not yet been cleared.
The due date for accounting of this job (released on 21-Jul-12) appears to fall after the Monthly Statement cut-off date of 24-Jul-12.
Are you sure you wish to continue and send the B3 message at this time?".Trim(), notification.AdditionalWarningsMessage);

				declaration.ImporterAddInfo.ZO_IsCSAApprovedImporter = true;
				Factory.Save();

				manager.SetNextAnswer(false);
				notification.Reset();
				manager.SendMessage(MessageSubTypes.Create);
				Assert(notification.HasShowMessageInstructionFormBeenCalled);
				Assert(notification.ContainsAdditionalWarnings);
				AssertEquals("Release status text", @"
The following unusual situations have been detected. Please read the following carefully.
The Release Status indicates that this shipment has not yet been cleared.
Are you sure you wish to continue and send the B3 message at this time?".Trim(), notification.AdditionalWarningsMessage);
			}
		}

		[TestDate(2012, 7, 22)]
		public void TestSendMessage_RationalityCheck()
		{
			using (CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345"))
			{
				Env.Security.CAB3MsgSend.IsAllowed = false;
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2012, 7, 21);
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, string.Empty);
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.CL_CustomsValue = 1m;
				var invoice = declaration.Invoices.AddNew();
				JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
				line.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
				line.CA_PageNumber = 1;

				var manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
				var notification = manager.Notification;
				Env.Security.CAB3MsgSend.IsAllowed = true;
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
				declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
				Factory.Save();

				manager.SetNextAnswer(false);
				notification.Reset();
				manager.SendMessage(MessageSubTypes.Create);
				Assert(notification.HasShowMessageInstructionFormBeenCalled);
				Assert(notification.ContainsAdditionalWarnings);
				AssertEquals("Release status text", @"
The following unusual situations have been detected. Please read the following carefully.
The Release Status indicates that this shipment has not yet been cleared.
The due date for accounting of this Low Value Shipment (released on 21-Jul-12) appears to fall after the Monthly Statement cut-off date of 24-Jul-12, based on the system settings the Entry message will be deferred until the next pay period.
Are you sure you wish to continue and schedule the B3 message at this time?".Trim(), notification.AdditionalWarningsMessage);

				notification.Reset();
				entryLine.CL_CustomsValue = 10000m;
				entryHeader.ResetTotalsAndCachedValues();
				manager.SetNextAnswer(false);
				manager.SendMessage(MessageSubTypes.Create);
				Assert(notification.HasShowMessageInstructionFormBeenCalled);
				Assert(notification.ContainsAdditionalWarnings);
				AssertEquals("Release status text", @"The following unusual situations have been detected. Please read the following carefully.
The Release Status indicates that this shipment has not yet been cleared.
The due date for accounting of this job (released on 21-Jul-12) appears to fall after the Monthly Statement cut-off date of 24-Jul-12, based on the system settings the Entry message will be deferred until the next pay period.
Are you sure you wish to continue and schedule the B3 message at this time?", notification.AdditionalWarningsMessage);

				notification.Reset();
				var ediReleaseEntryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.EDIRelease);
				ediReleaseEntryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.AuthorisedToDeliver;
				declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Importer;
				var importer = Factory.New<OrgHeader>();
				importer.FillWithValidTestData();
				declaration.JE_OH_Importer = importer.PK;
				declaration.ImporterAddInfo.ZO_PreventWarningOnSendingB3 = false;
				Factory.Save();
				manager.SendMessage(MessageSubTypes.Create);
				Assert(notification.HasShowMessageInstructionFormBeenCalled);
				Assert(notification.ContainsAdditionalWarnings);
				AssertEquals("Importer pays text", @"The following unusual situations have been detected. Please read the following carefully.
This job is flagged as 'Importer Pays' and so the Importer-Lodged-Security flag will be set in the message sent to CBSA.
The due date for accounting of this job (released on 21-Jul-12) appears to fall after the Monthly Statement cut-off date of 24-Jul-12, based on the system settings the Entry message will be deferred until the next pay period.
Are you sure you wish to continue and schedule the B3 message at this time?", notification.AdditionalWarningsMessage);

				declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Default;

				declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
				declaration.ImporterAddInfo.ZO_IsGSTDirectAutoRated = true;
				Factory.Save();
				manager.SendMessage(MessageSubTypes.Create);
				Assert(notification.HasShowMessageInstructionFormBeenCalled);
				Assert(notification.ContainsAdditionalWarnings);
				AssertEquals("Due date for accounting", @"The following unusual situations have been detected. Please read the following carefully.
The due date for accounting of this job (released on 21-Jul-12) appears to fall after the Monthly Statement cut-off date of 24-Jul-12, based on the system settings the Entry message will be deferred until the next pay period.
Are you sure you wish to continue and schedule the B3 message at this time?", notification.AdditionalWarningsMessage);

				notification.Reset();
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2012, 7, 15);
				Factory.Save();
				manager.SendMessage(MessageSubTypes.Create);
				Assert(notification.HasShowMessageInstructionFormBeenCalled);
				Assert(notification.ContainsValidationErrors);
				Assert(!notification.ContainsAdditionalWarnings);
				AssertEquals(string.Empty, notification.AdditionalWarningsMessage);

				notification.Reset();
				declaration.ImporterAddInfo.ZO_IsGSTDirectAutoRated = false;
				Factory.Save();
				manager.SendMessage(MessageSubTypes.Create);
				Assert(notification.HasShowMessageInstructionFormBeenCalled);
				AssertEquals("GST Direct text", @"The following unusual situations have been detected. Please read the following carefully.
This client is flagged as 'GST Direct' and GST will not be auto-rated for this job.
Are you sure you wish to continue and send the B3 message at this time?", notification.AdditionalWarningsMessage);

				notification.Reset();
				declaration.ImporterAddInfo.ZO_IsGSTDirectAutoRated = true;
				entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
				Factory.Save();
				manager.SendMessage(MessageSubTypes.Create);
				Assert(!notification.HasShowMessageInstructionFormBeenCalled);

				notification.Reset();
				entryHeader.CH_EntryStatus = ZString.Empty;
				declaration.Invoices.AddNew().JZ_ValuationDateOverride = ZDateTime.Now.AddDays(2);
				Factory.Save();
				manager.SendMessage(MessageSubTypes.Create);
				Assert(notification.HasShowMessageInstructionFormBeenCalled);
				AssertEquals("GST Direct text", @"The following unusual situations have been detected. Please read the following carefully.
The direct shipment date of some invoice(s) is in the future.
Are you sure you wish to continue and send the B3 message at this time?", notification.AdditionalWarningsMessage);
			}
		}

		[TestDate(2012, 7, 4)]
		public void TestSendMessage_CancelDeferredMessage()
		{
			var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty,
				Guid.Empty, "CLIENTIDXX");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000;
			var line = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 3, 1, 1000, 1000, 666, 333, Core.Constants.Weight.Kilograms);
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Mexico;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			line.DutiesAndTaxes.DeleteAll();
			Factory.Save();
			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			var testMessage1 = entryHeader.Messages.AddNew();
			testMessage1.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage1.EM_Status = EDIMessage.Status.Queued;
			testMessage1.EM_HeldUntilDate = ZDateTime.UtcNow.AddDays(3);
			Factory.Save();

			B3ImportMessageManagerForTesting manager;
			CombineAssertions("Defer Message", () =>
			{
				Assert(declaration.HasScheduledB3Message);
				var b3Header = new B3ImportMessageWrapper(entryHeader);
				manager = new B3ImportMessageManagerForTesting(b3Header,
					new TestUserNotificationWithB3ScheduleSupport());
				AssertNull("defer instruction not exists before CanSendThisMessage", manager.DeferInstruction_Exposed);
				manager.SendMessage(MessageSubTypes.Create, false);
				Assert("Declaration.HasScheduledB3", declaration.HasScheduledB3Message);
				Assert("Message Active Status", !testMessage1.EM_IsActive);
				AssertEquals("MessageStatus", EDIMessageStatusList.Codes.Cancelled, testMessage1.EM_Status);
				AssertEquals("AWO", b3Header.MessageStatus);
				AssertEquals("AWO", entryHeader.CH_Status);
			});

			entryHeader.CH_Status = MessageStatusList.Codes.ClearOriginal;
			var testMessage2 = entryHeader.Messages.AddNew();
			testMessage2.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage2.EM_Status = EDIMessage.Status.Queued;
			testMessage2.EM_HeldUntilDate = ZDateTime.UtcNow.AddDays(3);
			Factory.Save();
			CombineAssertions("Resend Deferred Message", () =>
			{
				Assert(declaration.HasScheduledB3Message);
				var b3Header = new B3ImportMessageWrapper(entryHeader);
				manager = new B3ImportMessageManagerForTesting(b3Header,
					new TestUserNotificationWithB3ScheduleSupport());
				AssertNull("defer instruction not exists before CanSendThisMessage", manager.DeferInstruction_Exposed);
				manager.SendMessage(MessageSubTypes.Create, false);
				Assert("Declaration.HasScheduledB3", declaration.HasScheduledB3Message);
				Assert("Message Active Status", !testMessage1.EM_IsActive);
				AssertEquals("MessageStatus", EDIMessageStatusList.Codes.Cancelled, testMessage1.EM_Status);
				AssertEquals("AWO", b3Header.MessageStatus);
				AssertEquals("AWO", entryHeader.CH_Status);
			});
		}

		[TestDate(2012, 7, 22)]
		public void TestSendMessage_AQtoFollowWarning()
		{
			using (CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345"))
			{
				Env.Security.CAB3MsgSend.IsAllowed = false;
				ZString messageText;
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_AssesmentOption = AssessmentOptions.Codes.AQtoFollow;
				declaration.CA_ServiceOption = ServiceOptions.Codes.PARS;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, string.Empty);
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.CL_CustomsValue = 1m;

				var manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
				var notification = manager.Notification;
				Env.Security.CAB3MsgSend.IsAllowed = true;
				Factory.Save();
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
				declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;

				manager.SetNextAnswer(false);
				notification.Reset();
				Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
				manager.SendMessage(MessageSubTypes.Create);
				Assert(notification.HasShowMessageInstructionFormBeenCalled);
				Assert(notification.ContainsAdditionalWarnings);
				AssertEquals("Release status text", @"
The following unusual situations have been detected. Please read the following carefully.
The Release Status indicates that this shipment has not yet been cleared.
AQ to follow was specified but AQ data does not appear to have been transmitted.
Are you sure you wish to continue and send the B3 message at this time?".Trim(), notification.AdditionalWarningsMessage);
			}
		}

		public override void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "B3 Message for Declaration", messageManager.MessageFriendlyName);
		}

		public override void TestPopulateMessages()
		{
			((B3ImportMessageManagerForTesting)messageManager).PopulateMessage_Exposed(MessageSubTypes.Create);
			AssertEquals("1 message", 1, entryHeader.Messages.Count);
			AssertEquals("EM_MessageSubType", MessageSubTypeCodes.Codes.Original, entryHeader.Messages[0].EM_MessageSubType);
			AssertEquals("Message status", MessageStatusList.Codes.AwaitingOriginal, entryHeader.CH_Status);
		}

		public void TestPopulateMessagesClearScheduledMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			var entryHedader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var testMessage1 = entryHeader.Messages.AddNew();
			testMessage1.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage1.EM_Status = EDIMessage.Status.Queued;
			var testMessage2 = entryHeader.Messages.AddNew();
			testMessage2.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage2.EM_Status = EDIMessage.Status.Queued;
			testMessage2.EM_HeldUntilDate = ZDateTime.Now;
			Factory.Save();
			var manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
			var testMessageManager = (B3ImportMessageManagerForTesting)messageManager;

			testMessageManager.PopulateMessage_Exposed(MessageSubTypes.Create);
			AssertEquals("3 message", 3, entryHeader.Messages.Count);
			AssertEquals(true, testMessage1.EM_IsActive);
			AssertEquals(false, testMessage2.EM_IsActive);
			AssertEquals(EDIMessage.Status.Queued, testMessage1.EM_Status);
			AssertEquals(EDIMessage.Status.Cancelled, testMessage2.EM_Status);
		}

		[TestDate(2012, 7, 4)]
		public void TestSendMessagesSettingHeldUntilTime()
		{
			var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000;
			var line = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 3, 1, 1000, 1000, 666, 333, Core.Constants.Weight.Kilograms);
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Mexico;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			line.DutiesAndTaxes.DeleteAll();
			Factory.Save();
			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			var manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
			manager.DeferInstruction_Exposed = new B3DeferInstruction(declaration, ZDate.Today);

			manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader), true, true);
			manager.SetNextAnswer(true);
			using (declaration.GetValidationSuspender())
			{
				manager.SendMessage(MessageSubTypes.Create);
			}
			AssertEquals("1 message", 1, entryHeader.Messages.Count);
			AssertEquals(EDIMessage.Status.Queued, entryHeader.Messages[0].EM_Status);
			AssertEquals(new ZDateTime(2012, 07, 25, 04, 00, 00), entryHeader.Messages[0].EM_HeldUntilDate);
			AssertEquals(new ZDateTime(2012, 07, 25, 04, 00, 00), declaration.B3EntrySubmittedDate);
		}

		[TestDate(2017, 1, 1)]
		public void TestSendMessagesSlientAndUseOriginalDeferredB3MessageTime()
		{
			JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();

			using (CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX"))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_EntryAuthorisationDate = ZDateTime.Now;

				var invoice = declaration.Invoices.AddNew();
				var line = invoice.JobComInvoiceLines.AddNew();
				JobComInvoiceLineTestHelper.FillInvoiceLine(line, 3, 1, 1000, 1000, 666, 333, Core.Constants.Weight.Kilograms);
				line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Mexico;

				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				line.DutiesAndTaxes.DeleteAll();
				Factory.Save();

				var b3EntryHeader = declaration.B3EntryHeader;
				b3EntryHeader.NeedResendDeferredB3CADMessage = true;
				b3EntryHeader.NeedResendB3CADMessageSilent = true;
				b3EntryHeader.OriginalDeferredB3CADMessageTime = ZDateTime.Now.AddDays(2);

				var messageManager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(b3EntryHeader))
				{
					DeferInstruction_Exposed = new B3DeferInstruction(declaration, ZDate.Today)
				};

				messageManager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(b3EntryHeader), true);
				messageManager.SetNextAnswer(true);

				using (declaration.GetValidationSuspender())
				{
					messageManager.SendMessage(MessageSubTypes.Create);
				}

				AssertEquals("Should create a message", 1, b3EntryHeader.Messages.Count);
				AssertEquals(EDIMessageStatusList.Codes.Queued, b3EntryHeader.Messages[0].EM_Status);
			}
		}

		public void TestValidationTest()
		{
			var manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
			AssertEquals("ValidationType", ValidateForMessageType.B3CUSDEC, manager.ValidateTypeForTesting);
		}

		public void TestSendMessage_WHS()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				BondedWarehousingHelperTest.CreateWarehouse(Factory, helper.Warehouse, "WH1");
				var inwardDeclaration = BondedWarehousingHelperTest.CreateWHSDeclaration(Factory, "B000000001", "00000001", B3EntryTypeList.Codes.Warehouse10, helper.Importer, helper.Warehouse);
				BondedWarehousingHelperTest.SetupInvoiceLinesForWHS(inwardDeclaration, helper.Part.OP_PartNum, 10m);
				inwardDeclaration.DoMerge();
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);

				var outwardDeclaration = BondedWarehousingHelperTest.CreateWHSDeclaration(Factory, "B000000002", "00000002", B3EntryTypeList.Codes.ExWarehouse20, helper.Importer, helper.Warehouse);
				BondedWarehousingHelperTest.SetupInvoiceLinesForWHS(outwardDeclaration, helper.Part.OP_PartNum, 2m);
				outwardDeclaration.DoMerge();
				var invoiceLine = outwardDeclaration.InvoiceLines[0];
				invoiceLine.JI_PreviousEntryNumber = "12345000000012-1";
				Factory.Save();

				var manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(outwardDeclaration.B3EntryHeader));
				manager.SendMessage(MessageSubTypes.Create);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);
			}
		}

		protected override IEDIFACTMessageAttachee GetDataWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return new B3ImportMessageWrapper(entryHeader);
		}

		protected override EDIFACTMessageManager GetMessageManager()
		{
			return new B3ImportMessageManagerForTesting((IB3Header)dataWrapper);
		}

		CusEntryHeader entryHeader;

		IDisposable asecSetup;

		protected override void SetUp()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var taxOrFee = universalHelper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 1000, Core.Constants.CountryCodes.Canada, new ZDateTime(2011, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest("12345");
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345");

			var data = (RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var customsDefaultToCurrentLoginDeptRegistry = (BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
			customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CACustomsDataRegistry.Instance.EnableCreditCheckForCLVS.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		protected override void AssertCanSendThisMessage(CusEntryHeader entryHeader, ZString expectedMessage)
		{
			var messageWrapper = new B3ImportMessageWrapper(entryHeader);
			var manager = new B3ImportMessageManagerForTesting(messageWrapper);
			ZString messageText;
			manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText);
			AssertEquals(expectedMessage, messageText);
			if (!expectedMessage.IsEmpty)
			{
				AssertEquals("Submit message with credit restriction", UnitTestUserNotification.Instance.LastMessage.Caption);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				using (Globals.SetIsUserInteractiveForTest(false))
				{
					manager = new B3ImportMessageManagerForTesting(messageWrapper);
					manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText);
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Caption);
				}
			}
		}

		protected override void AssertResetDeclaration(CAMessageManager manager, Action resetDeclaration, bool securityAllowed)
		{
			Assert(true);
		}
	}
}
