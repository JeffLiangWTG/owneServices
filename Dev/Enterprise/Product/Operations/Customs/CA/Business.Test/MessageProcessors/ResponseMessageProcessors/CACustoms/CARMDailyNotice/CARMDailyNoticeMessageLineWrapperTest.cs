using System.Linq;
using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARM_TYPES;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	public class CARMDailyNoticeMessageLineWrapperTest : TestCaseWithFactory
	{
		public void TestProperties_AH()
		{
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

			var wrapper = CARMDailyNoticeMessageWrapperTest.GetCARMDailyNoticeMessageWrapper(Factory, CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeImporter, CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeImporterMessageText());
			CombineAssertions(() =>
			{
				AssertEquals("The count of line group should be 1", 1, wrapper.LineGroups.Count());
				var lineGroup = wrapper.LineGroups.ToList()[0];
				AssertEquals("Should be 3", 3, lineGroup.LineIteams.Count());

				var line = lineGroup.LineIteams.ToList()[0];
				AssertCARMDailyNoticeMessageLineWrapper(line, "1", "99999999991234", "B2", new ZDate(2020, 02, 21), new ZDateTime(2022, 02, 03), "ABC", 100000m, 0m, 0m, 0m, 10000m, 10000m, 0m, 0m, 0m, 0m, 0m, 0m, 110000m, "99999999991234", new ZDateTime(2022, 03, 31), b2dec.PK, b2dec.JE_DeclarationReference, "Reassessment (B2-1)", "00002", "178234732", null);

				line = lineGroup.LineIteams.ToList()[1];
				AssertCARMDailyNoticeMessageLineWrapper(line, "2", "12345679991234", "B3", new ZDate(2020, 02, 22), new ZDateTime(2022, 02, 04), "", 50000m, 0m, 0m, 0m, 5000m, 5000m, 0m, 0m, 0m, 0m, 0m, 0m, 55000m, "12345679991234", new ZDateTime(2022, 03, 30), lvsDeclaration.PK, lvsDeclaration.JE_DeclarationReference, "Reassessment (B2-1)", "00003", "CBSA", null);

				line = lineGroup.LineIteams.ToList()[2];
				AssertCARMDailyNoticeMessageLineWrapper(line, "3", "20220203RT1140", "B3", new ZDate(2021, 12, 16), new ZDateTime(2022, 02, 05), "", 102408m, -20m, 10m, 1242.03m, 69177.5m, 69187.5m, -40m, 30m, 50m, -60m, 70m, -80m, 172837.53m, "20220203RT1140", new ZDateTime(2021, 12, 31), declaration.PK, declaration.JE_DeclarationReference, "Assessment (B3)", "00001", "100023258", DailyNoticeLineItemTypeLineitemStatus.L);
			});
		}

		public void TestProperties_CB()
		{
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

			var wrapper = CARMDailyNoticeMessageWrapperTest.GetCARMDailyNoticeMessageWrapper(Factory, CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeBroker, CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeBrokerMessageText());
			CombineAssertions(() =>
			{
				AssertEquals("The count of line group should be 2", 2, wrapper.LineGroups.Count());
				var lineGroup = wrapper.LineGroups.ToList()[0];
				AssertEquals("Should be 2", 2, lineGroup.LineIteams.Count());
				var line = lineGroup.LineIteams.ToList()[0];
				AssertCARMDailyNoticeMessageLineWrapper(line, "1", "99999999991234", "B2", new ZDate(2020, 02, 22), new ZDateTime(2022, 02, 03), "", 100000m, 0m, 0m, 0m, 10000m, 10000m, 0m, 0m, 0m, 0m, 0m, 0m, 110000m, "99999999991234", new ZDateTime(2022, 03, 31), b2dec.PK, b2dec.JE_DeclarationReference, ZString.Empty, "00002", "178234732", null);
				line = lineGroup.LineIteams.ToList()[1];
				AssertCARMDailyNoticeMessageLineWrapper(line, "2", "99998888771234", "B2", new ZDate(2020, 02, 23), new ZDateTime(2022, 02, 03), "", 100000m, 0m, 1000m, 0m, 300m, 0m, 0m, 300m, 0m, 0m, 0m, 0m, 101300m, "99998888771234", new ZDateTime(2022, 03, 31), ZGuid.Empty, ZString.Empty, ZString.Empty, "00002", "178234732", null);

				lineGroup = wrapper.LineGroups.ToList()[1];
				AssertEquals("Should be 1", 1, lineGroup.LineIteams.Count());
				line = lineGroup.LineIteams.ToList()[0];
				AssertCARMDailyNoticeMessageLineWrapper(line, "3", "20220203RT1140", "B3", new ZDate(2023, 03, 10), new ZDateTime(2023, 03, 11), "", 100000m, 0m, 751.13m, 41.39m, 10000m, 10000m, 0m, 0m, 0m, 0m, 0m, 0m, 110000m, "20220203RT1140", new ZDateTime(2022, 03, 31), declaration.PK, declaration.JE_DeclarationReference, ZString.Empty, "00002", "178234732", null);
			});
		}

		public void TestITableInterpretation()
		{
			var wrapper = CARMDailyNoticeMessageWrapperTest.GetCARMDailyNoticeMessageWrapper(Factory, CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeImporter, CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeImporterMessageText());
			var lineGroup = wrapper.LineGroups.ToList()[0];
			var line = lineGroup.LineIteams.ToList()[0];
			var tableInterpretation = (ITableInterpretation)line;
			CombineAssertions(() =>
			{
				AssertEquals("Transactions", tableInterpretation.Caption);

				var titles = tableInterpretation.Titles.ToList();
				AssertEquals("Doc Type", titles[0]);
				AssertEquals("Release Date", titles[1]);
				AssertEquals("Accounting Date", titles[2]);
				AssertEquals("Port", titles[3]);
				AssertEquals("Duties", titles[4]);
				AssertEquals("SIMA", titles[5]);
				AssertEquals("Excise Tax", titles[6]);
				AssertEquals("Excise Duties", titles[7]);
				AssertEquals("GST/PST/HST", titles[8]);
				AssertEquals("Interest", titles[9]);
				AssertEquals("Others", titles[10]);
				AssertEquals("Totals", titles[11]);
				AssertEquals("Transaction Number", titles[12]);

				var values = tableInterpretation.Values.ToList();
				AssertEquals("Doc Type", "B2", values[0]);
				AssertEquals("Release Date", "2020-02-21", values[1]);
				AssertEquals("Accounting Date", "2022-02-03", values[2]);
				AssertEquals("Port", "ABC", values[3]);
				AssertEquals("Duties", "100,000.00", values[4]);
				AssertEquals("SIMA", "", values[5]);
				AssertEquals("Excise Tax", "", values[6]);
				AssertEquals("Excise Duties", "", values[7]);
				AssertEquals("GST/PST/HST", "10,000.00", values[8]);
				AssertEquals("Interest", "", values[9]);
				AssertEquals("Others", "", values[10]);
				AssertEquals("Totals", "110,000.00", values[11]);
				AssertEquals("Transaction Number", "99999999991234", values[12]);
			});
		}

		void AssertCARMDailyNoticeMessageLineWrapper(CARMDailyNoticeMessageLineWrapper line, ZString id, ZString transcationNumber, ZString docType, ZDate releaseDate, ZDateTime accountingDate, ZString port,
			ZDecimal duties, ZDecimal sima, ZDecimal exciseTax, ZDecimal exciseDuties, ZDecimal gSTAndPSTAndHST, ZDecimal gST, ZDecimal pST, ZDecimal hST, ZDecimal interest, ZDecimal penalties,
			ZDecimal payments, ZDecimal others, ZDecimal totals, ZString releatedDocumentNumber, ZDateTime? paymentDueDate, ZGuid jePK, ZString jobNumber, ZString description, ZString cadVersion,
			ZString submittedBy, object accountingStatus)
		{
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.ATN_NUM", transcationNumber, line.TransactionNumber);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.DOC_TYPE", docType, line.DocType);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.REL_DATE", releaseDate, line.ReleaseDate);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.ACC_DATE", accountingDate, line.AccountingDate);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.PORT", port, line.Port);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.AMOUNTS.DUTIES", duties, line.Duties);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.AMOUNTS.SIMA", sima, line.SIMA);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.AMOUNTS.EXCISE", exciseTax, line.ExciseTax);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.AMOUNTS.EXCISEDUTIES", exciseDuties, line.ExciseDuties);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.AMOUNTS.GST + PST + HST", gSTAndPSTAndHST, line.GSTAndPSTAndHST);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.AMOUNTS.GST", gST, line.GST);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.AMOUNTS.PST", pST, line.PST);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.AMOUNTS.HST", hST, line.HST);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.AMOUNTS.INTEREST", interest, line.Interest);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.AMOUNTS.PENALTIES", penalties, line.Penalties);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.AMOUNTS.PAYMENTS", payments, line.Payments);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.AMOUNTS.OTHERS", others, line.Others);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.AMOUNTS.TOTALS", totals, line.Totals);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.REL_DOC_NUMBER", releatedDocumentNumber, line.ReleatedDocumentNumber);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.PAYMENT_DUE_DATE", paymentDueDate, line.PaymentDueDate);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.TRANS_DESC", description, line.TransactionDescription);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.CAD_VERSION", cadVersion, line.CADVERSION);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.SUB_BY", submittedBy, line.SubmittedBy);
			AssertEquals(id + " DailyNoticeLineItemTypeLINEITEM.STATUS", accountingStatus, line.AccountingStatus);
			AssertEquals(id + " Declaration", jePK, line.ReleatedDeclaration?.PK ?? ZGuid.Empty);
			AssertEquals(id + " JobNumber", jobNumber, line.JobNumber);
		}
	}
}
