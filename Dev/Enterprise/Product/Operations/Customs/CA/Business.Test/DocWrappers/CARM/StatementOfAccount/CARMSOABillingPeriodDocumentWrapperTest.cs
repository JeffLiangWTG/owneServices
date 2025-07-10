using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CARMSOABillingPeriodDocumentWrapperTest : TestCaseWithFactory
	{
		#region TestSourceIdentifierProvider

		public void TestISourceIdentifierProvider()
		{
			var header = CreateCusStatementHeaderWithMessages_LE(Factory);
			var wrapper = new CARMSOABillingPeriodDocumentWrapper(header);

			var supporter = wrapper as ISourceIdentifierProvider;
			AssertNotNull("CARMSOABillingPeriodDocumentWrapper should implement ISourceIdentifierProvider", supporter);
			AssertEquals("supporter.SourceIdentifier", header.PK, supporter?.SourceIdentifier);
		}

		#endregion

		public void TestTakeLastSOAMessageForLegalName()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_IsMonthlyStatement = true;
			header.B2_StatementType = CARMStatementOfAccountStatementTypeList.ShortCodes.LegalEntiry;
			header.B2_StatementNumber = "112358145-20231222114905";
			header.B2_ImporterCustomsID = "112358145";
			header.B2_PrintDate = new ZDateTime(2023, 10, 25);
			header.B2_DueDate = new ZDateTime(2023, 10, 31);
			header.B2_StatementAmount = 27280.0;
			header.B2_PeriodStartDate = new ZDate(2023, 09, 18);
			header.B2_PeriodEndDate = new ZDate(2023, 10, 17);

			var line1 = header.StatementLines.AddNew();
			line1.B3_ImporterCustomsID = "112358145RM0002";
			line1.B3_EntryDate = new ZDate(2023, 08, 02);
			line1.B3_DueDate = new ZDate(2023, 08, 02);
			line1.B3_ScheduledProcessDate = new ZDate(2023, 08, 02);
			CreateStatementCharges(line1, 10.2m, 20.2m, 30.2m, 40.2m, 50.2m, 60.2m, 70.2m, 80.2m, 90.2m, 100.2m, 0m, 552.0m);

			Factory.Save();

			var message1 = header.Messages.AddNew();
			message1.EM_MessageNum = "00000000000161841525";
			message1.EM_ApplicationCode = "NDM";
			message1.EM_MessageType = "XDC";
			message1.EM_MessageSubType = "XNN";
			message1.EM_ReceiveTransmit = "TRX";
			message1.EM_MessageData = Encoding.ASCII.GetBytes("00AA");

			var message3 = header.Messages.AddNew();
			message3.EM_MessageNum = "00000000000161841526";
			message3.EM_ApplicationCode = "CAC";
			message3.EM_MessageType = "SOA";
			message3.EM_ReceiveTransmit = "RCV";
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				message3.EM_MessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.DocWrappers.CARM.StatementOfAccount.TestFiles.CARMStatementOfAccount_LE.xml");
			}

			var message4 = header.Messages.AddNew();
			message4.EM_MessageNum = "00000000000161841527";
			message4.EM_ApplicationCode = "NDM";
			message4.EM_MessageType = "XDC";
			message4.EM_MessageSubType = "XNN";
			message4.EM_ReceiveTransmit = "TRX";
			message4.EM_MessageData = Encoding.ASCII.GetBytes("0000");

			AssertNoExceptionThrown(() => new CARMSOABillingPeriodDocumentWrapper(header));
		}

		public void TestProperties_LE()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var header = CreateCusStatementHeaderWithMessages_LE(Factory);
				var wrapper = new CARMSOABillingPeriodDocumentWrapper(header);
				AssertEquals("BillingPeriodStart", new ZDateTime(2023, 09, 18), wrapper.BillingPeriodStart);
				AssertEquals("BillingPeriodEnd", new ZDateTime(2023, 10, 17), wrapper.BillingPeriodEnd);
				AssertEquals("BusinessNumber", "112358145", wrapper.BusinessNumber);
				AssertEquals("LegalName", "SoA Card", wrapper.LegalName);
				AssertEquals("ProgramType", "LE", wrapper.ProgramType);
				AssertEquals("StatementDate", new ZDateTime(2023, 10, 25), wrapper.StatementDate);
				AssertEquals("DueDate", new ZDateTime(2023, 10, 31), wrapper.DueDate);
				AssertEquals("StatementAmount", 27280.0m, wrapper.StatementAmount);
				AssertEquals("PreviousStatementBalance", 1000.1m, wrapper.PreviousStatementBalance);
				AssertEquals("CorrectionsToPreviousStatementBalance", 2000.1m, wrapper.CorrectionsToPreviousStatementBalance);
				AssertEquals("PaymentsReceivedAfterPreviousSOA", 3000.1m, wrapper.PaymentsReceivedAfterPreviousSOA);
				AssertEquals("Disburesements", 4000.1m, wrapper.Disburesements);
				AssertEquals("InterestAndPenaltiesSumTotal", 5000.1m, wrapper.InterestAndPenaltiesSumTotal);
				AssertEquals("CurrentPeriodCharges", 6000.1m, wrapper.CurrentPeriodCharges);
				AssertEquals("CurrentPeriodCredit", 0m, wrapper.CurrentPeriodCredit);
				AssertEquals("CurrentStatementBalance", 21000.6m, wrapper.CurrentStatementBalance);

				AssertEquals("Duties", 100.1m, wrapper.Duties);
				AssertEquals("Excise", 200.1m, wrapper.Excise);
				AssertEquals("ExciseDuties", 300.1m, wrapper.ExciseDuties);
				AssertEquals("SIMA", 400.1m, wrapper.SIMA);
				AssertEquals("GST", 500.1m, wrapper.GST);
				AssertEquals("HST", 600.1m, wrapper.HST);
				AssertEquals("PST", 700.1m, wrapper.PST);
				AssertEquals("Payments", 1000.1m, wrapper.Payments);
				AssertEquals("Others", 0m, wrapper.Others);

				var programAccount = wrapper.ProgramAccount;
				AssertEquals("programAccount.Count", 2, programAccount.Count);
				AssertEquals("Bn15", "112358145RM0001", programAccount[0].Bn15);
				AssertEquals("Bn15", "112358145RM0002", programAccount[1].Bn15);
				AssertEquals("TotalDuties", 20.3m, programAccount[0].TotalDuties);
				AssertEquals("TotalExcise", 40.3m, programAccount[0].TotalExcise);
				AssertEquals("TotalExciseDuties", 60.3m, programAccount[0].TotalExciseDuties);
				AssertEquals("TotalSIMA", 80.3m, programAccount[0].TotalSIMA);
				AssertEquals("TotalGST", 100.3m, programAccount[0].TotalGST);
				AssertEquals("TotalHST", 120.3m, programAccount[0].TotalHST);
				AssertEquals("TotalPST", 140.3m, programAccount[0].TotalPST);
				AssertEquals("TotalPayments", 200.3m, programAccount[0].TotalPayments);
				AssertEquals("TotalOthers", 0m, programAccount[0].TotalOthers);
				AssertEquals("TotalTotalAmount", 762.4m, programAccount[0].TotalTotalAmount);

				var summaryByDay = programAccount[0].SummaryByDay;
				AssertEquals("summaryByDay.Count", 2, summaryByDay.Count);
				AssertEquals("ReleaseDate", new ZDateTime(2023, 08, 01), summaryByDay[0].ReleaseDate);
				AssertEquals("AccountingDate", new ZDateTime(2023, 08, 01), summaryByDay[0].AccountingDate);
				AssertEquals("ReleaseDate", new ZDateTime(2023, 08, 02), summaryByDay[1].ReleaseDate);
				AssertEquals("AccountingDate", new ZDateTime(2023, 08, 02), summaryByDay[1].AccountingDate);
				AssertEquals("Duties", 10.1m, summaryByDay[0].Duties);
				AssertEquals("Excise", 20.1m, summaryByDay[0].Excise);
				AssertEquals("ExciseDuties", 30.1m, summaryByDay[0].ExciseDuties);
				AssertEquals("SIMA", 40.1m, summaryByDay[0].SIMA);
				AssertEquals("GST", 50.1m, summaryByDay[0].GST);
				AssertEquals("HST", 60.1m, summaryByDay[0].HST);
				AssertEquals("PST", 70.1m, summaryByDay[0].PST);
				AssertEquals("Payments", 100.1m, summaryByDay[0].Payments);
				AssertEquals("Others", 0m, summaryByDay[0].Others);
				AssertEquals("TotalAmount", 380.8m, summaryByDay[0].TotalAmount);

				var notes = wrapper.SOANotes;
				AssertEquals("Message", "SoA - The CARM Client Portal is now live. Check the CBSA Website for more information.", notes);
			}
		}

		static CusStatementHeader CreateCusStatementHeaderWithMessages_LE(BusinessObjectFactory factory)
		{
			var header = factory.New<CusStatementHeader>();
			header.B2_IsMonthlyStatement = true;
			header.B2_StatementType = CARMStatementOfAccountStatementTypeList.ShortCodes.LegalEntiry;
			header.B2_StatementNumber = "112358145-20231222114905";
			header.B2_ImporterCustomsID = "112358145";
			header.B2_PrintDate = new ZDateTime(2023, 10, 25);
			header.B2_DueDate = new ZDateTime(2023, 10, 31);
			header.B2_StatementAmount = 27280.0;
			header.B2_PeriodStartDate = new ZDate(2023, 09, 18);
			header.B2_PeriodEndDate = new ZDate(2023, 10, 17);

			var lineGroup = header.LineGroupCollection.AddNew();
			lineGroup.B10_ImporterCustomsID = "112358145";
			UpdateFinancialDetailForLineGroup(lineGroup, 1000.1m, 2000.1m, 3000.1m, 4000.1m, 5000.1m, 6000.1m, 0m, 21000.6m);

			var lineGroupDIST = header.LineGroupCollection.AddNew();
			lineGroupDIST.B10_ImporterCustomsID = "112358145_DIST";
			UpdateFinancialDetailForLineGroupDist(lineGroupDIST, 100.1m, 200.1m, 300.1m, 400.1m, 500.1m, 600.1m, 700.1m, 800.1m, 900.1m, 1000.1m, 0m, 5501.0m);

			var line1 = header.StatementLines.AddNew();
			line1.B3_ImporterCustomsID = "112358145RM0001";
			line1.B3_EntryDate = new ZDate(2023, 08, 01);
			line1.B3_DueDate = new ZDate(2023, 08, 01);
			line1.B3_ScheduledProcessDate = new ZDate(2023, 08, 01);
			CreateStatementCharges(line1, 10.1m, 20.1m, 30.1m, 40.1m, 50.1m, 60.1m, 70.1m, 80.1m, 90.1m, 100.1m, 0m, 551.0m);

			var line2 = header.StatementLines.AddNew();
			line2.B3_ImporterCustomsID = "112358145RM0001";
			line2.B3_EntryDate = new ZDate(2023, 08, 02);
			line2.B3_DueDate = new ZDate(2023, 08, 02);
			line2.B3_ScheduledProcessDate = new ZDate(2023, 08, 02);
			CreateStatementCharges(line2, 10.2m, 20.2m, 30.2m, 40.2m, 50.2m, 60.2m, 70.2m, 80.2m, 90.2m, 100.2m, 0m, 552.0m);

			var line3 = header.StatementLines.AddNew();
			line3.B3_ImporterCustomsID = "112358145RM0002";
			line3.B3_EntryDate = new ZDate(2023, 08, 01);
			line3.B3_DueDate = new ZDate(2023, 08, 01);
			line3.B3_ScheduledProcessDate = new ZDate(2023, 08, 01);
			CreateStatementCharges(line3, 10.1m, 20.1m, 30.1m, 40.1m, 50.1m, 60.1m, 70.1m, 80.1m, 90.1m, 100.1m, 0m, 551.0m);

			var line4 = header.StatementLines.AddNew();
			line4.B3_ImporterCustomsID = "112358145RM0002";
			line4.B3_EntryDate = new ZDate(2023, 08, 02);
			line4.B3_DueDate = new ZDate(2023, 08, 02);
			line4.B3_ScheduledProcessDate = new ZDate(2023, 08, 02);
			CreateStatementCharges(line4, 10.2m, 20.2m, 30.2m, 40.2m, 50.2m, 60.2m, 70.2m, 80.2m, 90.2m, 100.2m, 0m, 552.0m);

			factory.Save();

			var message = header.Messages.AddNew();
			message.EM_ApplicationCode = "CAC";
			message.EM_MessageType = "SOA";
			message.EM_ReceiveTransmit = "RCV";
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				message.EM_MessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.DocWrappers.CARM.StatementOfAccount.TestFiles.CARMStatementOfAccount_LE.xml");
				header.CreateOrUpdateCustomNote(StatementMessageProcessorHelper.EnglishMessageToRecipient, "SoA - The CARM Client Portal is now live. Check the CBSA Website for more information.");
			}

			return header;
		}

		public void TestProperties_PA()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var header = CreateCusStatementHeaderWithMessages_PA(Factory);
				var wrapper = new CARMSOABillingPeriodDocumentWrapper(header);
				AssertEquals("BillingPeriodStart", new ZDateTime(2023, 09, 18), wrapper.BillingPeriodStart);
				AssertEquals("BillingPeriodEnd", new ZDateTime(2023, 10, 17), wrapper.BillingPeriodEnd);
				AssertEquals("BusinessNumber", "112358145", wrapper.BusinessNumber);
				AssertEquals("LegalName", "SoA Card", wrapper.LegalName);
				AssertEquals("ProgramType", "PA", wrapper.ProgramType);
				AssertEquals("StatementDate", new ZDateTime(2023, 10, 25), wrapper.StatementDate);
				AssertEquals("DueDate", new ZDateTime(2023, 10, 31), wrapper.DueDate);
				AssertEquals("StatementAmount", 12820.0m, wrapper.StatementAmount);
				AssertEquals("PreviousStatementBalance", 1000.1m, wrapper.PreviousStatementBalance);
				AssertEquals("CorrectionsToPreviousStatementBalance", 2000.1m, wrapper.CorrectionsToPreviousStatementBalance);
				AssertEquals("PaymentsReceivedAfterPreviousSOA", 3000.1m, wrapper.PaymentsReceivedAfterPreviousSOA);
				AssertEquals("Disburesements", 4000.1m, wrapper.Disburesements);
				AssertEquals("InterestAndPenaltiesSumTotal", 5000.1m, wrapper.InterestAndPenaltiesSumTotal);
				AssertEquals("CurrentPeriodCharges", 6000.1m, wrapper.CurrentPeriodCharges);
				AssertEquals("CurrentPeriodCredit", 0m, wrapper.CurrentPeriodCredit);
				AssertEquals("CurrentStatementBalance", 21000.6m, wrapper.CurrentStatementBalance);

				AssertEquals("Duties", 100.1m, wrapper.Duties);
				AssertEquals("Excise", 200.1m, wrapper.Excise);
				AssertEquals("ExciseDuties", 300.1m, wrapper.ExciseDuties);
				AssertEquals("SIMA", 400.1m, wrapper.SIMA);
				AssertEquals("GST", 500.1m, wrapper.GST);
				AssertEquals("HST", 600.1m, wrapper.HST);
				AssertEquals("PST", 700.1m, wrapper.PST);
				AssertEquals("Payments", 1000.1m, wrapper.Payments);
				AssertEquals("Others", 0m, wrapper.Others);

				var programAccount = wrapper.ProgramAccount;
				AssertEquals("programAccount.Count", 1, programAccount.Count);
				AssertEquals("Bn15", "112358145RM0004", programAccount[0].Bn15);

				AssertEquals("TotalDuties", 30.6m, programAccount[0].TotalDuties);
				AssertEquals("TotalExcise", 60.6m, programAccount[0].TotalExcise);
				AssertEquals("TotalExciseDuties", 90.6m, programAccount[0].TotalExciseDuties);
				AssertEquals("TotalSIMA", 120.6m, programAccount[0].TotalSIMA);
				AssertEquals("TotalGST", 150.6m, programAccount[0].TotalGST);
				AssertEquals("TotalHST", 180.6m, programAccount[0].TotalHST);
				AssertEquals("TotalPST", 210.6m, programAccount[0].TotalPST);
				AssertEquals("TotalPayments", 300.6m, programAccount[0].TotalPayments);
				AssertEquals("TotalOthers", 0m, programAccount[0].TotalOthers);
				AssertEquals("TotalTotalAmount", 1144.8m, programAccount[0].TotalTotalAmount);

				var summaryByDay = programAccount[0].SummaryByDay;
				AssertEquals("summaryByDay.Count", 3, summaryByDay.Count);
				AssertEquals("ReleaseDate", new ZDateTime(2023, 08, 01), summaryByDay[0].ReleaseDate);
				AssertEquals("AccountingDate", new ZDateTime(2023, 08, 01), summaryByDay[0].AccountingDate);
				AssertEquals("ReleaseDate", new ZDateTime(2023, 08, 02), summaryByDay[1].ReleaseDate);
				AssertEquals("AccountingDate", new ZDateTime(2023, 08, 02), summaryByDay[1].AccountingDate);
				AssertEquals("ReleaseDate", new ZDateTime(2023, 08, 03), summaryByDay[2].ReleaseDate);
				AssertEquals("AccountingDate", new ZDateTime(2023, 08, 03), summaryByDay[2].AccountingDate);

				AssertEquals("Duties", 10.1m, summaryByDay[0].Duties);
				AssertEquals("Excise", 20.1m, summaryByDay[0].Excise);
				AssertEquals("ExciseDuties", 30.1m, summaryByDay[0].ExciseDuties);
				AssertEquals("SIMA", 40.1m, summaryByDay[0].SIMA);
				AssertEquals("GST", 50.1m, summaryByDay[0].GST);
				AssertEquals("HST", 60.1m, summaryByDay[0].HST);
				AssertEquals("PST", 70.1m, summaryByDay[0].PST);
				AssertEquals("Payments", 100.1m, summaryByDay[0].Payments);
				AssertEquals("Others", 0m, summaryByDay[0].Others);
				AssertEquals("TotalAmount", 380.8m, summaryByDay[0].TotalAmount);

				var notes = wrapper.SOANotes;
				AssertEquals("Message", "SoA - The CARM Client Portal is now live. Check the CBSA Website for more information.", notes);
			}
		}

		static CusStatementHeader CreateCusStatementHeaderWithMessages_PA(BusinessObjectFactory factory)
		{
			var header = factory.New<CusStatementHeader>();
			header.B2_IsMonthlyStatement = true;
			header.B2_StatementType = CARMStatementOfAccountStatementTypeList.ShortCodes.ProgramAccount;
			header.B2_StatementNumber = "112358145-20240711120000";
			header.B2_ImporterCustomsID = "112358145";
			header.B2_PrintDate = new ZDateTime(2023, 10, 25);
			header.B2_DueDate = new ZDateTime(2023, 10, 31);
			header.B2_StatementAmount = 12820.0m;
			header.B2_PeriodStartDate = new ZDate(2023, 09, 18);
			header.B2_PeriodEndDate = new ZDate(2023, 10, 17);

			var lineGroup = header.LineGroupCollection.AddNew();
			lineGroup.B10_ImporterCustomsID = "112358145";
			UpdateFinancialDetailForLineGroup(lineGroup, 1000.1m, 2000.1m, 3000.1m, 4000.1m, 5000.1m, 6000.1m, 0m, 21000.6m);

			var lineGroupDIST = header.LineGroupCollection.AddNew();
			lineGroupDIST.B10_ImporterCustomsID = "112358145_DIST";
			UpdateFinancialDetailForLineGroupDist(lineGroupDIST, 100.1m, 200.1m, 300.1m, 400.1m, 500.1m, 600.1m, 700.1m, 800.1m, 900.1m, 1000.1m, 0m, 5501.0m);

			var line1 = header.StatementLines.AddNew();
			line1.B3_ImporterCustomsID = "112358145RM0004";
			line1.B3_EntryDate = new ZDate(2023, 08, 01);
			line1.B3_DueDate = new ZDate(2023, 08, 01);
			line1.B3_ScheduledProcessDate = new ZDate(2023, 08, 01);
			CreateStatementCharges(line1, 10.1m, 20.1m, 30.1m, 40.1m, 50.1m, 60.1m, 70.1m, 80.1m, 90.1m, 100.1m, 0m, 551.0m);

			var line2 = header.StatementLines.AddNew();
			line2.B3_ImporterCustomsID = "112358145RM0004";
			line2.B3_EntryDate = new ZDate(2023, 08, 02);
			line2.B3_DueDate = new ZDate(2023, 08, 02);
			line2.B3_ScheduledProcessDate = new ZDate(2023, 08, 02);
			CreateStatementCharges(line2, 10.2m, 20.2m, 30.2m, 40.2m, 50.2m, 60.2m, 70.2m, 80.2m, 90.2m, 100.2m, 0m, 552.0m);

			var line3 = header.StatementLines.AddNew();
			line3.B3_ImporterCustomsID = "112358145RM0004";
			line3.B3_EntryDate = new ZDate(2023, 08, 03);
			line3.B3_DueDate = new ZDate(2023, 08, 03);
			line3.B3_ScheduledProcessDate = new ZDate(2023, 08, 03);
			CreateStatementCharges(line3, 10.3m, 20.3m, 30.3m, 40.3m, 50.3m, 60.3m, 70.3m, 80.3m, 90.3m, 100.3m, 0m, 553.0m);

			factory.Save();

			var message = header.Messages.AddNew();
			message.EM_ApplicationCode = "CAC";
			message.EM_MessageType = "SOA";
			message.EM_ReceiveTransmit = "RCV";
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				message.EM_MessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.DocWrappers.CARM.StatementOfAccount.TestFiles.CARMStatementOfAccount_PA.xml");
				header.CreateOrUpdateCustomNote(StatementMessageProcessorHelper.EnglishMessageToRecipient, "SoA - The CARM Client Portal is now live. Check the CBSA Website for more information.");
			}

			return header;
		}

		public void TestProperties_PT()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var header = CreateCusStatementHeaderWithMessages_PT(Factory);
				var wrapper = new CARMSOABillingPeriodDocumentWrapper(header);
				AssertEquals("BillingPeriodStart", new ZDateTime(2023, 09, 18), wrapper.BillingPeriodStart);
				AssertEquals("BillingPeriodEnd", new ZDateTime(2023, 10, 17), wrapper.BillingPeriodEnd);
				AssertEquals("BusinessNumber", "112358145", wrapper.BusinessNumber);
				AssertEquals("LegalName", "SoA Card", wrapper.LegalName);
				AssertEquals("ProgramType", "PT", wrapper.ProgramType);
				AssertEquals("StatementDate", new ZDateTime(2023, 10, 25), wrapper.StatementDate);
				AssertEquals("DueDate", new ZDateTime(2023, 10, 31), wrapper.DueDate);
				AssertEquals("StatementAmount", 21640.0m, wrapper.StatementAmount);
				AssertEquals("PreviousStatementBalance", 1000.1m, wrapper.PreviousStatementBalance);
				AssertEquals("CorrectionsToPreviousStatementBalance", 2000.1m, wrapper.CorrectionsToPreviousStatementBalance);
				AssertEquals("PaymentsReceivedAfterPreviousSOA", 3000.1m, wrapper.PaymentsReceivedAfterPreviousSOA);
				AssertEquals("Disburesements", 4000.1m, wrapper.Disburesements);
				AssertEquals("InterestAndPenaltiesSumTotal", 5000.1m, wrapper.InterestAndPenaltiesSumTotal);
				AssertEquals("CurrentPeriodCharges", 6000.1m, wrapper.CurrentPeriodCharges);
				AssertEquals("CurrentPeriodCredit", 0m, wrapper.CurrentPeriodCredit);
				AssertEquals("CurrentStatementBalance", 21000.6m, wrapper.CurrentStatementBalance);

				AssertEquals("Duties", 100.1m, wrapper.Duties);
				AssertEquals("Excise", 200.1m, wrapper.Excise);
				AssertEquals("ExciseDuties", 300.1m, wrapper.ExciseDuties);
				AssertEquals("SIMA", 400.1m, wrapper.SIMA);
				AssertEquals("GST", 500.1m, wrapper.GST);
				AssertEquals("HST", 600.1m, wrapper.HST);
				AssertEquals("PST", 700.1m, wrapper.PST);
				AssertEquals("Payments", 1000.1m, wrapper.Payments);
				AssertEquals("Others", 0m, wrapper.Others);

				var programAccount = wrapper.ProgramAccount;
				AssertEquals("programAccount.Count", 2, programAccount.Count);
				AssertEquals("Bn15", "112358145RM0003", programAccount[0].Bn15);
				AssertEquals("Bn15", "112358145RM0004", programAccount[1].Bn15);

				AssertEquals("TotalDuties", 20.3m, programAccount[0].TotalDuties);
				AssertEquals("TotalExcise", 40.3m, programAccount[0].TotalExcise);
				AssertEquals("TotalExciseDuties", 60.3m, programAccount[0].TotalExciseDuties);
				AssertEquals("TotalSIMA", 80.3m, programAccount[0].TotalSIMA);
				AssertEquals("TotalGST", 100.3m, programAccount[0].TotalGST);
				AssertEquals("TotalHST", 120.3m, programAccount[0].TotalHST);
				AssertEquals("TotalPST", 140.3m, programAccount[0].TotalPST);
				AssertEquals("TotalPayments", 200.3m, programAccount[0].TotalPayments);
				AssertEquals("TotalOthers", 0m, programAccount[0].TotalOthers);
				AssertEquals("TotalTotalAmount", 762.4m, programAccount[0].TotalTotalAmount);

				var summaryByDay = programAccount[0].SummaryByDay;
				AssertEquals("summaryByDay.Count", 2, summaryByDay.Count);
				AssertEquals("ReleaseDate", new ZDateTime(2023, 08, 01), summaryByDay[0].ReleaseDate);
				AssertEquals("AccountingDate", new ZDateTime(2023, 08, 01), summaryByDay[0].AccountingDate);
				AssertEquals("ReleaseDate", new ZDateTime(2023, 08, 02), summaryByDay[1].ReleaseDate);
				AssertEquals("AccountingDate", new ZDateTime(2023, 08, 02), summaryByDay[1].AccountingDate);

				AssertEquals("Duties", 10.1m, summaryByDay[0].Duties);
				AssertEquals("Excise", 20.1m, summaryByDay[0].Excise);
				AssertEquals("ExciseDuties", 30.1m, summaryByDay[0].ExciseDuties);
				AssertEquals("SIMA", 40.1m, summaryByDay[0].SIMA);
				AssertEquals("GST", 50.1m, summaryByDay[0].GST);
				AssertEquals("HST", 60.1m, summaryByDay[0].HST);
				AssertEquals("PST", 70.1m, summaryByDay[0].PST);
				AssertEquals("Payments", 100.1m, summaryByDay[0].Payments);
				AssertEquals("Others", 0m, summaryByDay[0].Others);
				AssertEquals("TotalAmount", 380.8m, summaryByDay[0].TotalAmount);

				var notes = wrapper.SOANotes;
				AssertEquals("Message", "SoA - The CARM Client Portal is now live. Check the CBSA Website for more information.", notes);
			}
		}

		static CusStatementHeader CreateCusStatementHeaderWithMessages_PT(BusinessObjectFactory factory)
		{
			var header = factory.New<CusStatementHeader>();
			header.B2_IsMonthlyStatement = true;
			header.B2_StatementType = CARMStatementOfAccountStatementTypeList.ShortCodes.ProgramType;
			header.B2_StatementNumber = "112358145-20231222114854";
			header.B2_ImporterCustomsID = "112358145";
			header.B2_PrintDate = new ZDateTime(2023, 10, 25);
			header.B2_DueDate = new ZDateTime(2023, 10, 31);
			header.B2_StatementAmount = 21640.0m;
			header.B2_PeriodStartDate = new ZDate(2023, 09, 18);
			header.B2_PeriodEndDate = new ZDate(2023, 10, 17);

			var lineGroup = header.LineGroupCollection.AddNew();
			lineGroup.B10_ImporterCustomsID = "112358145";
			UpdateFinancialDetailForLineGroup(lineGroup, 1000.1m, 2000.1m, 3000.1m, 4000.1m, 5000.1m, 6000.1m, 0m, 21000.6m);

			var lineGroupDIST = header.LineGroupCollection.AddNew();
			lineGroupDIST.B10_ImporterCustomsID = "112358145_DIST";
			UpdateFinancialDetailForLineGroupDist(lineGroupDIST, 100.1m, 200.1m, 300.1m, 400.1m, 500.1m, 600.1m, 700.1m, 800.1m, 900.1m, 1000.1m, 0m, 5501.0m);

			var line1 = header.StatementLines.AddNew();
			line1.B3_ImporterCustomsID = "112358145RM0003";
			line1.B3_EntryDate = new ZDate(2023, 08, 01);
			line1.B3_DueDate = new ZDate(2023, 08, 01);
			line1.B3_ScheduledProcessDate = new ZDate(2023, 08, 01);
			CreateStatementCharges(line1, 10.1m, 20.1m, 30.1m, 40.1m, 50.1m, 60.1m, 70.1m, 80.1m, 90.1m, 100.1m, 0m, 551.0m);

			var line2 = header.StatementLines.AddNew();
			line2.B3_ImporterCustomsID = "112358145RM0003";
			line2.B3_EntryDate = new ZDate(2023, 08, 02);
			line2.B3_DueDate = new ZDate(2023, 08, 02);
			line2.B3_ScheduledProcessDate = new ZDate(2023, 08, 02);
			CreateStatementCharges(line2, 10.2m, 20.2m, 30.2m, 40.2m, 50.2m, 60.2m, 70.2m, 80.2m, 90.2m, 100.2m, 0m, 552.0m);

			var line3 = header.StatementLines.AddNew();
			line3.B3_ImporterCustomsID = "112358145RM0004";
			line3.B3_EntryDate = new ZDate(2023, 08, 01);
			line3.B3_DueDate = new ZDate(2023, 08, 01);
			line3.B3_ScheduledProcessDate = new ZDate(2023, 08, 01);
			CreateStatementCharges(line3, 10.1m, 20.1m, 30.1m, 40.1m, 50.1m, 60.1m, 70.1m, 80.1m, 90.1m, 100.1m, 0m, 551.0m);

			var line4 = header.StatementLines.AddNew();
			line4.B3_ImporterCustomsID = "112358145RM0004";
			line4.B3_EntryDate = new ZDate(2023, 08, 02);
			line4.B3_DueDate = new ZDate(2023, 08, 02);
			line4.B3_ScheduledProcessDate = new ZDate(2023, 08, 02);
			CreateStatementCharges(line4, 10.2m, 20.2m, 30.2m, 40.2m, 50.2m, 60.2m, 70.2m, 80.2m, 90.2m, 100.2m, 0m, 552.0m);

			factory.Save();

			var message = header.Messages.AddNew();
			message.EM_ApplicationCode = "CAC";
			message.EM_MessageType = "SOA";
			message.EM_ReceiveTransmit = "RCV";
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				message.EM_MessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.DocWrappers.CARM.StatementOfAccount.TestFiles.CARMStatementOfAccount_PT.xml");
				header.CreateOrUpdateCustomNote(StatementMessageProcessorHelper.EnglishMessageToRecipient, "SoA - The CARM Client Portal is now live. Check the CBSA Website for more information.");
			}

			return header;
		}

		static void UpdateFinancialDetailForLineGroup(CusStatementLineGroup lineGroup, ZDecimal psb, ZDecimal cps, ZDecimal prv, ZDecimal dis, ZDecimal ips, ZDecimal cpg, ZDecimal cpd, ZDecimal csb)
		{
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.PreviousStatementBalance, psb);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CorrectionsToPreviousStatementBalance, cps);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.PaymentsReceivedAfterPreviousSoA, prv);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.Disbursements, dis);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.InterestAndPenaltiesSumTotal, ips);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CurrentPeriodCharges, cpg);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CurrentPeriodCredits, cpd);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CurrentStatementBalance, csb);
		}

		static void UpdateFinancialDetailForLineGroupDist(CusStatementLineGroup lineGroupDIST, ZDecimal duties, ZDecimal exciseTax, ZDecimal exciseDuties,
			ZDecimal sima, ZDecimal gst, ZDecimal hst, ZDecimal pst, ZDecimal interest, ZDecimal penalties, ZDecimal payments, ZDecimal others, ZDecimal totals)
		{
			lineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Duties, duties);
			lineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.ExciseTax, exciseTax);
			lineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.ExciseDuties, exciseDuties);
			lineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.SIMA, sima);
			lineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax, gst);
			lineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.HarmonizedSalesTax, hst);
			lineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.ProvincialSalesTax, pst);
			lineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Interest, interest);
			lineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Penalties, penalties);
			lineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Payments, payments);
			lineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Others, others);
			lineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Totals, totals);
		}

		static void CreateStatementCharges(CusStatementLine line, ZDecimal duties, ZDecimal exciseTax, ZDecimal exciseDuties,
			ZDecimal sima, ZDecimal gst, ZDecimal hst, ZDecimal pst, ZDecimal interest, ZDecimal penalties, ZDecimal payments, ZDecimal others, ZDecimal totals)
		{
			StatementMessageProcessorHelper.CreateStatementCharges(line, duties, CARMDailyNoticeChargeTypeList.Codes.Duties, ZString.Empty);
			StatementMessageProcessorHelper.CreateStatementCharges(line, exciseTax, CARMDailyNoticeChargeTypeList.Codes.ExciseTax, ZString.Empty);
			StatementMessageProcessorHelper.CreateStatementCharges(line, exciseDuties, CARMDailyNoticeChargeTypeList.Codes.ExciseDuties, ZString.Empty);
			StatementMessageProcessorHelper.CreateStatementCharges(line, sima, CARMDailyNoticeChargeTypeList.Codes.SIMA, ZString.Empty);
			StatementMessageProcessorHelper.CreateStatementCharges(line, gst, CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax, ZString.Empty);
			StatementMessageProcessorHelper.CreateStatementCharges(line, hst, CARMDailyNoticeChargeTypeList.Codes.HarmonizedSalesTax, ZString.Empty);
			StatementMessageProcessorHelper.CreateStatementCharges(line, pst, CARMDailyNoticeChargeTypeList.Codes.ProvincialSalesTax, ZString.Empty);
			StatementMessageProcessorHelper.CreateStatementCharges(line, interest, CARMDailyNoticeChargeTypeList.Codes.Interest, ZString.Empty);
			StatementMessageProcessorHelper.CreateStatementCharges(line, penalties, CARMDailyNoticeChargeTypeList.Codes.Penalties, ZString.Empty);
			StatementMessageProcessorHelper.CreateStatementCharges(line, payments, CARMDailyNoticeChargeTypeList.Codes.Payments, ZString.Empty);
			StatementMessageProcessorHelper.CreateStatementCharges(line, others, CARMDailyNoticeChargeTypeList.Codes.Others, ZString.Empty);
			StatementMessageProcessorHelper.CreateStatementCharges(line, totals, CARMDailyNoticeChargeTypeList.Codes.Totals, ZString.Empty);
		}
	}
}
