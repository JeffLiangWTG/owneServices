using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	public class CARMDailyNoticeMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessCARMDNoticeBrokerMessage()
		{
			#region Create Test Data

			var newGroup1 = Factory.New<GlbGroup>();
			newGroup1.GG_Code = "NG1";
			var newStaff1 = newGroup1.Staff.AddNew();
			newStaff1.GS_Code = "NS1";
			newStaff1.GS_LoginName = "NS1";
			newStaff1.GS_EmailAddress = "ns1@cargowise.com";

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DCA";
			company.GC_Name = "DCA TEST";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			GlbCompany.CurrentCompany.GC_Code = "DCA";

			var broker = Factory.NewWithValidTestData<OrgHeader>();
			company.GC_OH_OrgProxy = broker.PK;
			broker.OH_IsBroker = true;
			broker.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "178234732", Core.Constants.CountryCodes.Canada);

			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "178234732RM0001", Core.Constants.CountryCodes.Canada);
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "178234732RM0002", Core.Constants.CountryCodes.Canada);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "1";
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = declaration.PK;
			entryNumber.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			entryNumber.CE_EntryType = CusEntryNumber.EntryType.CATransactionNumber;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			entryNumber.CE_EntryNum = "20220203RT1140";

			var b2dec = Factory.NewWithValidTestData<JobDeclaration>();
			b2dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var entryNumber1 = Factory.New<CusEntryNumber>();
			entryNumber1.CE_ParentID = b2dec.PK;
			entryNumber1.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			entryNumber1.CE_EntryType = CusEntryNumber.EntryType.CATransactionNumber;
			entryNumber1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			entryNumber1.CE_EntryNum = "99999999991234";
			Factory.Save();

			#endregion

			using (CACustomsDataRegistry.Instance.SendK84ReportNotificationsToGroup.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGroup1.PK.ToGuid()))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var query = new ZQuery();
				query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, "DN-178234732-220825");
				query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, CusStatementHeaderTypes.Codes.Broker);
				query.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, false);
				var statementHeader = Factory.LoadTop1<CusStatementHeader>(query);
				AssertNull(statementHeader);

				var text = CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeBrokerMessageText();

				var message = Factory.New<CARMDailyNoticeMessage>();
				message.EM_MessageText = text;
				message.EM_MessageType = MessageTypeList.Codes.CARMDailyNotice;
				message.EM_MessageSubType = CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeBroker;
				var cacMessageProcessor = new CACustomsMessageProcessor(logger);
				AssertNoExceptionThrown(() =>
				{
					cacMessageProcessor.ProcessMessage(message);
				});

				statementHeader = Factory.LoadTop1<CusStatementHeader>(query);
				AssertNotNull(statementHeader);
				AssertEquals(statementHeader, message.EM_LinkedObject);
				AssertEquals(CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeBroker, message.EM_MessageSubType);
				AssertEquals(EDIMessageStatusList.Codes.Received, message.EM_Status);
				AssertEquals("Part of file_name", ZString.Empty, statementHeader.B2_RMNumber);
				AssertEquals("HEADER.DN_DATE", new ZDateTime(2022, 02, 03), statementHeader.B2_PrintDate);
				AssertEquals("HEADER.PARTY.BN9", "178234732", statementHeader.B2_ImporterCustomsID);
				AssertEquals("HEADER.PARTY.BN9", broker.PK, statementHeader.B2_OH_Importer);
				AssertEquals("SUMMARY.IMP_DEC", ZDecimal.Zero, statementHeader.B2_StatementAmount);
				AssertEquals("SUMMARY.PAYMENTS", ZDecimal.Zero, statementHeader.B2_PaidAmount);
				AssertEquals("SUMMARY.DISB", ZDecimal.Zero, statementHeader.B2_RefundAmount);
				AssertEquals("Should be empty", ZDate.Empty, statementHeader.B2_DueDate);
				AssertEquals("Min Accounting Date of lines", new ZDateTime(2022, 2, 3), statementHeader.B2_ProcessDate);
				AssertEquals("RandomLine.ATN_NUM.Left(5)", "99999", statementHeader.B2_EntryFilerCode);
				var noteEN = statementHeader.Notes.FindByDescription(StatementMessageProcessorHelper.EnglishMessageToRecipient).FirstOrDefault();
				AssertEquals("NOTES.Message", "DNCB - The CARM Client Portal is now live. Check the CBSA Website for more information.", noteEN.ST_NoteDataAsText);
				var noteFR = statementHeader.Notes.FindByDescription(StatementMessageProcessorHelper.FrenchMessageToRecipient).FirstOrDefault();
				AssertEquals("NOTES.Message", "DNCB - Le Portail client de la GCRA est maintenant disponible. Verifier le site Web de lASFC pour plus dinformation.", noteFR.ST_NoteDataAsText);

				AssertCusStatementLine(statementHeader, b2dec.JE_DeclarationReference, new ZDate(2022, 02, 03), "B2", "010000053358", new ZDate(2022, 03, 31), 100000m, 0m, 0m, 0m, 10000m, 0m, 0m, 0m, 0m, 0m, 0m, 110000m, "178234732RM0001", new ZDate(2020, 02, 22), PaymentPartyCodeDescriptionList.Codes.Broker, ZString.Empty, "00002", "178234732", ZString.Empty, ZString.Empty, 2);
				AssertCusStatementLine(statementHeader, ZString.Empty, new ZDate(2022, 02, 03), "B2", "010000053999", new ZDate(2022, 03, 31), 100000m, 1000m, 0m, 0m, 0m, 300m, 0m, 0m, 0m, 0m, 0m, 101300m, "178234732RM0001", new ZDate(2020, 02, 23), PaymentPartyCodeDescriptionList.Codes.Broker, ZString.Empty, "00002", "178234732", ZString.Empty, ZString.Empty, 3);
				AssertCusStatementLine(statementHeader, declaration.JE_DeclarationReference, new ZDate(2023, 03, 11), "B3", "010000066778", new ZDate(2022, 03, 31), 100000m, 751.13m, 41.39m, 0m, 10000m, 0m, 0m, 0m, 0m, 0m, 0m, 110000m, "178234732RM0002", new ZDate(2023, 03, 10), PaymentPartyCodeDescriptionList.Codes.Broker, ZString.Empty, "00002", "178234732", ZString.Empty, ZString.Empty, 4);

				AssertEquals("Statement Date", new ZDateTime(2022, 02, 03), ((IK84ReportAttachee)b2dec).StatementDate);
				AssertEquals("Accounting Date", new ZDate(2022, 02, 03), ((IK84ReportAttachee)b2dec).AccountingDate);
				AssertEquals("B2 Accepted Date", new ZDate(2022, 02, 03), ((IK84ReportAttachee)b2dec).B2AcceptedDate);
				AssertEquals("Statement Date", new ZDateTime(2022, 02, 03), ((IK84ReportAttachee)entryHeader).StatementDate);
				AssertEquals("Accounting Date", new ZDate(2023, 03, 11), ((IK84ReportAttachee)entryHeader).AccountingDate);

				AssertCusStatementLineGroup(statementHeader, "178234732RM0001", importer1.PK, 0m, 99m);
				AssertCusStatementLineGroup(statementHeader, "178234732RM0002", importer2.PK, 0m, 88m);

				var expectedHtml = ZString.Empty;
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.CA.Business.Test.MessageProcessors.ResponseMessageProcessors.CACustoms.TestFiles.ExpectedEmailHtmlForBroker.txt"))
				using (var sr = new StreamReader(stream))
				{
					expectedHtml = sr.ReadToEnd();
				}
				AssertContains(expectedHtml, message.EM_MessageInterpretation);

				var email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "CARM Daily Notice Broker for 03-Feb-22");
				AssertNotNull(email);
				AssertEquals("One recipient", 1, email.CCRecipients.Count);
				AssertEquals("ns1@cargowise.com", email.CCRecipients[0].Email);
				AssertContains(expectedHtml, email.Body);
			}
		}

		public void TestNoExceptionThrownWhenCusStatementLineGroupIsNull()
		{
			var newGroup1 = Factory.New<GlbGroup>();
			newGroup1.GG_Code = "NG1";
			var newStaff1 = newGroup1.Staff.AddNew();
			newStaff1.GS_Code = "NS1";
			newStaff1.GS_LoginName = "NS1";
			newStaff1.GS_EmailAddress = "ns1@cargowise.com";

			using (CACustomsDataRegistry.Instance.SendK84ReportNotificationsToGroup.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGroup1.PK.ToGuid()))
			{
				var text = CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeBrokerMessageTextWithPartyAccountElementIsNotPresent();

				var message = Factory.New<CARMDailyNoticeMessage>();
				message.EM_MessageText = text;
				message.EM_MessageSubType = CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeBroker;
				var cacMessageProcessor = new CACustomsMessageProcessor(logger);
				AssertNoExceptionThrown(() =>
				{
					cacMessageProcessor.ProcessMessage(message);
				});
			}
		}

		public void TestProcessCARMDNoticeImporterMessage_Successed()
		{
			#region Creat Test Data

			var newGroup1 = Factory.New<GlbGroup>();
			newGroup1.GG_Code = "NG1";
			var newStaff1 = newGroup1.Staff.AddNew();
			newStaff1.GS_Code = "NS1";
			newStaff1.GS_LoginName = "NS1";
			newStaff1.GS_EmailAddress = "ns1@cargowise.com";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "100023258RM0001", Core.Constants.CountryCodes.Canada);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "1";
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = declaration.PK;
			entryNumber.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			entryNumber.CE_EntryType = CusEntryNumber.EntryType.CATransactionNumber;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			entryNumber.CE_EntryNum = "20220203RT1140";

			var lvsDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			lvsDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var lvsEntryHeader = lvsDeclaration.ActiveEntryHeaders.AddNew();
			lvsEntryHeader.CH_BGMReference = "1";
			lvsEntryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var lvsEntryNumber = Factory.New<CusEntryNumber>();
			lvsEntryNumber.CE_ParentID = lvsDeclaration.PK;
			lvsEntryNumber.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			lvsEntryNumber.CE_EntryType = CusEntryNumber.EntryType.CATransactionNumber;
			lvsEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			lvsEntryNumber.CE_EntryNum = "12345679991234";

			var b2dec = Factory.NewWithValidTestData<JobDeclaration>();
			b2dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var entryNumber1 = Factory.New<CusEntryNumber>();
			entryNumber1.CE_ParentID = b2dec.PK;
			entryNumber1.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			entryNumber1.CE_EntryType = CusEntryNumber.EntryType.CATransactionNumber;
			entryNumber1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			entryNumber1.CE_EntryNum = "99999999991234";
			Factory.Save();

			#endregion

			var linePK1 = ZGuid.Empty;
			var linePK2 = ZGuid.Empty;
			var linePK3 = ZGuid.Empty;
			using (CACustomsDataRegistry.Instance.SendK84ReportNotificationsToGroup.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGroup1.PK.ToGuid()))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var query = new ZQuery();
				query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, "DN-100023258RM0001-220825");
				query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, CusStatementHeaderTypes.Codes.Importer);
				query.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, false);
				var statementHeader = Factory.LoadTop1<CusStatementHeader>(query);
				AssertNull(statementHeader);

				var text = CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeImporterMessageText();

				var message = Factory.New<CARMDailyNoticeMessage>();
				message.EM_MessageText = text;
				message.EM_MessageSubType = CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeImporter;
				var cacMessageProcessor = new CACustomsMessageProcessor(logger);
				AssertNoExceptionThrown(() =>
				{
					cacMessageProcessor.ProcessMessage(message);
				});

				statementHeader = Factory.LoadTop1<CusStatementHeader>(query);
				AssertStatementHeaderAndLine(statementHeader, message);

				var expectedHtml = ZString.Empty;
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.CA.Business.Test.MessageProcessors.ResponseMessageProcessors.CACustoms.TestFiles.ExpectedEmailHtmlForImporter.txt"))
				using (var sr = new StreamReader(stream))
				{
					expectedHtml = sr.ReadToEnd();
				}
				AssertContains(expectedHtml, message.EM_MessageInterpretation);

				var email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "CARM Daily Notice Importer for 02-Feb-22");
				AssertNotNull(email);
				AssertEquals("One recipient", 1, email.CCRecipients.Count);
				AssertEquals("ns1@cargowise.com", email.CCRecipients[0].Email);
				AssertContains(expectedHtml, email.Body);

				var message1 = Factory.New<CARMDailyNoticeMessage>();
				message1.EM_MessageText = text;
				message1.EM_MessageSubType = CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeImporter;
				AssertNoExceptionThrown(() =>
				{
					cacMessageProcessor.ProcessMessage(message1);
				});

				statementHeader = Factory.LoadTop1<CusStatementHeader>(query);
				AssertStatementHeaderAndLine(statementHeader, message1);

				var message2 = Factory.New<CARMDailyNoticeMessage>();
				message2.EM_MessageText = text;
				message2.EM_MessageSubType = ZString.Empty;
				AssertNoExceptionThrown(() =>
				{
					cacMessageProcessor.ProcessMessage(message2);
				});

				statementHeader = Factory.LoadTop1<CusStatementHeader>(query);
				AssertStatementHeaderAndLine(statementHeader, message2);
			}

			void AssertStatementHeaderAndLine(CusStatementHeader statementHeader, EDIMessage message)
			{
				AssertNotNull(statementHeader);
				AssertEquals(statementHeader, message.EM_LinkedObject);
				AssertEquals(CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeImporter, message.EM_MessageSubType);
				AssertEquals(EDIMessageStatusList.Codes.Received, message.EM_Status);
				AssertEquals("Part of file_name", "0001", statementHeader.B2_RMNumber);
				AssertEquals("HEADER.DN_DATE", new ZDateTime(2022, 02, 02), statementHeader.B2_PrintDate);
				AssertEquals("HEADER.PARTY.BN9", "100023258", statementHeader.B2_ImporterCustomsID);
				AssertEquals("HEADER.PARTY.ACCOUNT", importer.PK, statementHeader.B2_OH_Importer);
				AssertEquals("SUMMARY.IMP_DEC", 337837.53m, statementHeader.B2_StatementAmount);
				AssertEquals("SUMMARY.PAYMENTS", 11m, statementHeader.B2_PaidAmount);
				AssertEquals("SUMMARY.DISB", 22m, statementHeader.B2_RefundAmount);
				AssertEquals("SUMMARY.REV_DIST.PAYMENT_DUE_DATE", new ZDateTime(2021, 12, 31), statementHeader.B2_DueDate);
				AssertEquals("Min Accounting Date of lines", new ZDateTime(2022, 2, 3), statementHeader.B2_ProcessDate);
				var noteEN = statementHeader.Notes.FindByDescription(StatementMessageProcessorHelper.EnglishMessageToRecipient).FirstOrDefault();
				AssertEquals("NOTES.Message", "DNAH - The CARM Client Portal is now live. Check the CBSA Website for more information.", noteEN.ST_NoteDataAsText);
				var noteFR = statementHeader.Notes.FindByDescription(StatementMessageProcessorHelper.FrenchMessageToRecipient).FirstOrDefault();
				AssertEquals("NOTES.Message", "DNAH - Le Portail client de la GCRA est maintenant disponible. Verifier le site Web de lASFC pour plus dinformation.", noteFR.ST_NoteDataAsText);

				AssertEquals(3, statementHeader.StatementLines.Count);
				AssertCusStatementLine(statementHeader, b2dec.JE_DeclarationReference, new ZDate(2022, 02, 03), "B2", "010000053358", new ZDate(2022, 03, 31), 100000m, 0m, 0m, 0m, 10000m, 0m, 0m, 0m, 0m, 0m, 0m, 110000m, "100023258RM0001", new ZDate(2020, 02, 21), PaymentPartyCodeDescriptionList.Codes.Importer, "Reassessment (B2-1)", "00002", "178234732", ZString.Empty, "ABC", 2);
				AssertCusStatementLine(statementHeader, lvsDeclaration.JE_DeclarationReference, new ZDate(2022, 02, 04), "B3", "010110053359", new ZDate(2022, 03, 30), 50000m, 0m, 0m, 0m, 5000m, 0m, 0m, 0m, 0m, 0m, 0m, 55000m, "100023258RM0001", new ZDate(2020, 02, 22), PaymentPartyCodeDescriptionList.Codes.Importer, "Reassessment (B2-1)", "00003", "CBSA", ZString.Empty, ZString.Empty, 2);
				AssertCusStatementLine(statementHeader, declaration.JE_DeclarationReference, new ZDate(2022, 02, 05), "B3", "010000053388", new ZDate(2021, 12, 31), 102408m, 10m, 1242.03m, -20m, 69187.5m, 30m, -40m, 50m, -60m, 70m, -80m, 172837.53m, "100023258RM0001", new ZDate(2021, 12, 16), PaymentPartyCodeDescriptionList.Codes.Importer, "Assessment (B3)", "00001", "100023258", "L", ZString.Empty, 11);

				AssertEquals("Statement Date", new ZDateTime(2022, 02, 02), ((IK84ReportAttachee)b2dec).StatementDate);
				AssertEquals("Accounting Date", new ZDate(2022, 02, 03), ((IK84ReportAttachee)b2dec).AccountingDate);
				AssertEquals("B2 Accepted Date", new ZDate(2022, 02, 03), ((IK84ReportAttachee)b2dec).B2AcceptedDate);
				AssertEquals("Statement Date", new ZDateTime(2022, 02, 02), ((IK84ReportAttachee)entryHeader).StatementDate);
				AssertEquals("Accounting Date", new ZDate(2022, 02, 05), ((IK84ReportAttachee)entryHeader).AccountingDate);

				AssertCusStatementLineGroup(statementHeader, "100023258RM0001", importer.PK, 22m, 11m);
			}
		}

		public void TestProcessCARMDNoticeImporterMessage_Failed()
		{
			var text = CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeImporterMessageText();
			text = text.Replace("DN-100023258RM0001-202208251", "DN-100023258RM-202208251");
			var message = Factory.New<CARMDailyNoticeMessage>();
			message.EM_MessageText = text;
			message.EM_MessageSubType = CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeImporter;
			var cacMessageProcessor = new CACustomsMessageProcessor(logger);
			AssertNoExceptionThrown(() =>
			{
				cacMessageProcessor.ProcessMessage(message);
			});
			AssertEquals(EDIMessageStatusList.Codes.Failed, message.EM_Status);
			AssertEquals(2, logger.Logs.Count());
			AssertContains("The file_name of message does not match the rule, it should start with 'DN-BN9(BN15)-'.", logger.Logs.ToArray()[0].Message);
			AssertContains("The file_name of message does not match the rule, it should start with 'DN-BN9(BN15)-'.", logger.Logs.ToArray()[1].Message);
			logger.ClearLogs();
		}

		public void TestEmailGroupAndMode()
		{
			var newGroupNN = Factory.New<GlbGroup>();
			newGroupNN.GG_Code = "NN1";
			Factory.Save();

			using (CACustomsDataRegistry.Instance.SendK84ReportNotificationsToGroup.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGroupNN.PK.ToGuid()))
			{
				var cacMessageProcessor = new CARMDailyNoticeMessageProcessorForTest(logger);
				AssertEquals(newGroupNN.PK, cacMessageProcessor.AcknowledgementEmailGroup_Exposed);
				AssertEquals(Core.Constants.EmailTo.NominatedGroup, cacMessageProcessor.AcknowledgementEmailMode_Exposed);
				AssertEquals(ZGuid.Empty, cacMessageProcessor.ImpedimentEmailGroup_Exposed);
				AssertEquals(ZString.Empty, cacMessageProcessor.ImpedimentEmailMode_Exposed);
				AssertEquals(ZGuid.Empty, cacMessageProcessor.ErrorEmailGroup_Exposed);
				AssertEquals(ZString.Empty, cacMessageProcessor.ErrorEmailMode_Exposed);
			}
		}

		ZGuid AssertCusStatementLine(CusStatementHeader statementHeader, ZString b3BrokerReference, ZDate scheduledProcessDate, ZString entryType, ZString associatedEntry, ZDate dueDate,
			ZDecimal duties, ZDecimal exciseTax, ZDecimal exciseDuties, ZDecimal sIMA, ZDecimal goodsAndServicesTax, ZDecimal harmonizedSalesTax, ZDecimal provincialSalesTax, ZDecimal interest,
			ZDecimal penalties, ZDecimal payments, ZDecimal others, ZDecimal total, ZString importerCustomsID, ZDate releaseDate, ZString chargePaymentParty, ZString description, ZString cadVersion,
			ZString submittedBy, ZString status, ZString port, int chargesCount)
		{
			var result = ZGuid.Empty;
			CombineAssertions(() =>
			{
				var statementLine = statementHeader.StatementLines.Cast<CusStatementLine>().First(x => x.B3_BrokerReference == b3BrokerReference);
				AssertNotNull(associatedEntry, statementLine);
				result = statementLine.PK;
				AssertEquals(associatedEntry + " LINEITEM.ACC_DATE", scheduledProcessDate, statementLine.B3_ScheduledProcessDate);
				AssertEquals(associatedEntry + "LINEITEM.DOC_TYPE", entryType, statementLine.B3_EntryType);
				AssertEquals(associatedEntry + "LINEITEM.ATN_NUM", importerCustomsID, statementLine.B3_ImporterCustomsID);
				AssertEquals(associatedEntry + "LINEITEM.ATN_NUM", b3BrokerReference, statementLine.B3_BrokerReference);
				AssertEquals(associatedEntry + "LINEITEM.AMOUNTS.PAYMENT_DUE_DATE", dueDate, statementLine.B3_DueDate);
				AssertEquals(associatedEntry + "Charges Count", chargesCount, statementLine.Charges.Count);
				AssertEquals(associatedEntry + "LINEITEM.AMOUNTS.DUTIES", duties, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.Duties));
				AssertEquals(associatedEntry + "LINEITEM.AMOUNTS.EXCISE", exciseTax, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.ExciseTax));
				AssertEquals(associatedEntry + "LINEITEM.AMOUNTS.EXCISEDUTIES", exciseDuties, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.ExciseDuties));
				AssertEquals(associatedEntry + "LINEITEM.AMOUNTS.SIMA", sIMA, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.SIMA));
				AssertEquals(associatedEntry + "LINEITEM.AMOUNTS.GST", goodsAndServicesTax, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax));
				AssertEquals(associatedEntry + "LINEITEM.AMOUNTS.HST", harmonizedSalesTax, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.HarmonizedSalesTax));
				AssertEquals(associatedEntry + "LINEITEM.AMOUNTS.PST", provincialSalesTax, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.ProvincialSalesTax));
				AssertEquals(associatedEntry + "LINEITEM.AMOUNTS.INTEREST", interest, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.Interest));
				AssertEquals(associatedEntry + "LINEITEM.AMOUNTS.PENALTIES", penalties, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.Penalties));
				AssertEquals(associatedEntry + "LINEITEM.AMOUNTS.PAYMENTS", payments, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.Payments));
				AssertEquals(associatedEntry + "LINEITEM.AMOUNTS.OTHERS", others, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.Others));
				AssertEquals(associatedEntry + " LINEITEM.AMOUNTS.TOTALS", total, statementLine.CARMTotal);
				AssertEquals(associatedEntry + "ZCARMDNOTICECBDETAILSIMPORTER.PARTY.ACCOUNT", importerCustomsID, statementLine.B3_ImporterCustomsID);
				AssertEquals(associatedEntry + "DailyNoticeLineItemTypeLINEITEM.REL_DATE", releaseDate, statementLine.B3_EntryDate);
				AssertEquals(associatedEntry + " DailyNoticeLineItemTypeLINEITEM.TRANS_DESC", description, statementLine.CARMTransactionDescription);
				AssertEquals(associatedEntry + " DailyNoticeLineItemTypeLINEITEM.CAD_VERSION", cadVersion, statementLine.CARMCADVersion);
				AssertEquals(associatedEntry + " DailyNoticeLineItemTypeLINEITEM.SUB_BY", submittedBy, statementLine.CARMSubmittedBy);
				AssertEquals(associatedEntry + " DailyNoticeLineItemTypeLINEITEM.STATUS", status, statementLine.CARMStatus);
				AssertEquals(associatedEntry + " DailyNoticeLineItemTypeLINEITEM.Port", port, statementLine.CARMPort);
				Assert(associatedEntry + "Charge Payment Party", statementLine.Charges.Cast<CusStatementLineCharge>().All(x => x.B4_PaymentParty == chargePaymentParty));
			});
			return result;
		}

		void AssertCusStatementLineGroup(CusStatementHeader statementHeader, ZString importerCustomsID, ZGuid importerPK, ZDecimal refundAmount, ZDecimal paymentReceivedAmount)
		{
			var statementLineGroup = statementHeader.LineGroupCollection.Cast<CusStatementLineGroup>().First(x => x.B10_ImporterCustomsID == importerCustomsID);
			AssertEquals(importerCustomsID + " Importer", importerPK, statementLineGroup.B10_OH_Importer);
			var expectedRefundAmount = statementLineGroup.FinancialDetailCollection.Cast<CusStatementLineGroupFinancialDetail>().FirstOrDefault(x => x.B11_Type == PostingJournalTypeList.Codes.Refund)?.B11_Amount ?? ZDecimal.Zero;
			AssertEquals(importerCustomsID + " Refund Amound", expectedRefundAmount, refundAmount);
			var expectedPaymentReceivedAmount = statementLineGroup.FinancialDetailCollection.Cast<CusStatementLineGroupFinancialDetail>().FirstOrDefault(x => x.B11_Type == PostingJournalTypeList.Codes.TotalPaymentReceived)?.B11_Amount ?? ZDecimal.Zero;
			AssertEquals(importerCustomsID + " Payment Received Amound", expectedPaymentReceivedAmount, paymentReceivedAmount);
		}

		ZDecimal GetChargeAmount(CusStatementLine line, ZString chargeType)
		{
			var result = ZDecimal.Zero;
			var charge = line.Charges.Cast<CusStatementLineCharge>().FirstOrDefault(x => x.B4_ChargeType == chargeType);
			if (charge != null)
			{
				result = charge.B4_ChargeAmount;
			}
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new LoggingInformation();
		}

		LoggingInformation logger;

		class CARMDailyNoticeMessageProcessorForTest : CARMDailyNoticeMessageProcessor
		{
			public CARMDailyNoticeMessageProcessorForTest(LoggingInformation logger)
				: base(logger)
			{
			}

			public ZGuid AcknowledgementEmailGroup_Exposed => base.AcknowledgementEmailGroup;

			public ZString AcknowledgementEmailMode_Exposed => base.AcknowledgementEmailMode;

			public ZGuid ImpedimentEmailGroup_Exposed => base.ImpedimentEmailGroup;

			public ZString ImpedimentEmailMode_Exposed => base.ImpedimentEmailMode;

			public ZGuid ErrorEmailGroup_Exposed => base.ErrorEmailGroup;

			public ZString ErrorEmailMode_Exposed => base.ErrorEmailMode;
		}
	}
}
