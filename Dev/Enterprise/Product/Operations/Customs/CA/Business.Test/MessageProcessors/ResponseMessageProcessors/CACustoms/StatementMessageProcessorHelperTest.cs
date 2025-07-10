using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	public class StatementMessageProcessorHelperTest : TestCaseWithFactory
	{
		public void TestCreateLineGroupIfNeed()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			var lineGroup = StatementMessageProcessorHelper.CreateLineGroupIfNeed(statementHeader, "123456789", 100m, 200m);
			AssertEquals(1, statementHeader.LineGroupCollection.Count);
			AssertEquals(100m, statementHeader.LineGroupCollection[0].FinancialDetailCollection.Cast<CusStatementLineGroupFinancialDetail>().First(x => x.B11_Type == PostingJournalTypeList.Codes.Refund).B11_Amount);
			AssertEquals(200m, statementHeader.LineGroupCollection[0].FinancialDetailCollection.Cast<CusStatementLineGroupFinancialDetail>().First(x => x.B11_Type == PostingJournalTypeList.Codes.TotalPaymentReceived).B11_Amount);

			lineGroup = StatementMessageProcessorHelper.CreateLineGroupIfNeed(statementHeader, "123456789", 300m, 400m);
			AssertEquals(1, statementHeader.LineGroupCollection.Count);
			AssertEquals(300m, statementHeader.LineGroupCollection[0].FinancialDetailCollection.Cast<CusStatementLineGroupFinancialDetail>().First(x => x.B11_Type == PostingJournalTypeList.Codes.Refund).B11_Amount);
			AssertEquals(400m, statementHeader.LineGroupCollection[0].FinancialDetailCollection.Cast<CusStatementLineGroupFinancialDetail>().First(x => x.B11_Type == PostingJournalTypeList.Codes.TotalPaymentReceived).B11_Amount);
		}

		public void TestProperties()
		{
			AssertEquals("Message to Recipient (English)", StatementMessageProcessorHelper.EnglishMessageToRecipient);
			AssertEquals("Message to Recipient (French)", StatementMessageProcessorHelper.FrenchMessageToRecipient);
			AssertEquals(CACustomsDataRegistry.Instance.SendK84ReportNotificationsToGroup.Value, StatementMessageProcessorHelper.AcknowledgementEmailGroup);
			AssertEquals("from the CBSA ", StatementMessageProcessorHelper.MessageSender);
		}

		public void TestGetOrCreateCusStatementHeader()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var query = new ZQuery();
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, "DN-100023258RM0001-220825");
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, CusStatementHeaderTypes.Codes.Importer);
			query.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, false);
			var statementHeader = Factory.LoadTop1<CusStatementHeader>(query);
			AssertNull(statementHeader);

			StatementMessageProcessorHelper.GetOrCreateCusStatementHeader(Factory, "DN-100023258RM0001-220825", CusStatementHeaderTypes.Codes.Importer, "100023258RM0001", importer.PK, false);
			statementHeader = Factory.LoadTop1<CusStatementHeader>(query);
			AssertNotNull(statementHeader);
			AssertEquals("100023258", statementHeader.B2_ImporterCustomsID);
			AssertEquals("0001", statementHeader.B2_RMNumber);
			AssertEquals(importer.PK, statementHeader.B2_OH_Importer);

			var query1 = new ZQuery();
			query1.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, "123456789012345");
			query1.AddToFilter(CusStatementHeaderSchema.B2_StatementType, CusStatementHeaderTypes.Codes.Importer);
			query1.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, false);
			var statementHeader1 = Factory.LoadTop1<CusStatementHeader>(query1);
			AssertNull(statementHeader1);

			StatementMessageProcessorHelper.GetOrCreateCusStatementHeader(Factory, "123456789012345", CusStatementHeaderTypes.Codes.Importer, "100023258RM0001", importer.PK, false);
			statementHeader1 = Factory.LoadTop1<CusStatementHeader>(query1);
			AssertNotNull(statementHeader1);
			AssertEquals("100023258RM0001", statementHeader1.B2_ImporterCustomsID);
			AssertEquals(ZString.Empty, statementHeader1.B2_RMNumber);
			AssertEquals(importer.PK, statementHeader1.B2_OH_Importer);

			var query2 = new ZQuery();
			query2.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, "DN-100023258-220825");
			query2.AddToFilter(CusStatementHeaderSchema.B2_StatementType, CusStatementHeaderTypes.Codes.Broker);
			query2.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, false);
			var statementHeader2 = Factory.LoadTop1<CusStatementHeader>(query2);
			AssertNull(statementHeader2);

			StatementMessageProcessorHelper.GetOrCreateCusStatementHeader(Factory, "DN-100023258-220825", CusStatementHeaderTypes.Codes.Broker, "100023258", importer.PK, false);
			statementHeader2 = Factory.LoadTop1<CusStatementHeader>(query2);
			AssertNotNull(statementHeader2);
			AssertEquals("100023258", statementHeader2.B2_ImporterCustomsID);
			AssertEquals(ZString.Empty, statementHeader2.B2_RMNumber);
			AssertEquals(importer.PK, statementHeader2.B2_OH_Importer);
		}

		public void TestSetValue()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			StatementMessageProcessorHelper.SetValue(statementHeader, CusStatementHeaderSchema.B2_StatementNumber, "123456789012345678901234567890");
			AssertEquals("1234567890123456789012345", statementHeader.B2_StatementNumber);
		}

		public void TestFindImporter()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "100023258RM0001", Core.Constants.CountryCodes.Canada);
			Factory.Save();
			var importerPK = StatementMessageProcessorHelper.FindImporter(Factory, "100023258RM0001", CusStatementHeaderTypes.Codes.Importer);
			AssertEquals(importer.PK, importerPK);

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DCA";
			company.GC_Name = "DCA TEST";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			GlbCompany.CurrentCompany.GC_Code = "DCA";

			var broker = Factory.NewWithValidTestData<OrgHeader>();
			company.GC_OH_OrgProxy = broker.PK;
			broker.OH_IsBroker = true;
			broker.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "100023258RM0001", Core.Constants.CountryCodes.Canada);
			Factory.Save();
			var brokerPK = StatementMessageProcessorHelper.FindImporter(Factory, "100023258RM0001", CusStatementHeaderTypes.Codes.Broker);
			AssertEquals(broker.PK, brokerPK);
		}

		public void TestUpdateDates()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			StatementMessageProcessorHelper.UpdateDates(new ZDateTime(2023, 03, 01), new ZDateTime(2023, 03, 02), true, declaration);
			var k84ReportAttachee = (IK84ReportAttachee)declaration;
			AssertEquals(new ZDateTime(2023, 03, 01), k84ReportAttachee.StatementDate);
			AssertEquals(new ZDateTime(2023, 03, 02), k84ReportAttachee.AccountingDate);
			AssertEquals(new ZDateTime(2023, 03, 02), k84ReportAttachee.B2AcceptedDate);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			StatementMessageProcessorHelper.UpdateDates(new ZDateTime(2023, 03, 01), new ZDateTime(2023, 03, 02), false, declaration);
			k84ReportAttachee = declaration;
			AssertEquals(new ZDateTime(2023, 03, 02), k84ReportAttachee.ConfirmedDate);
		}

		public void TestAddMessageInfosIfRequired()
		{
			var tableCreator = new HtmlTableCreator();
			StatementMessageProcessorHelper.AddMessageInfosIfRequired(tableCreator, "ABC", "EDF");
			var expectedValue = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>&lt;table border=&quot;1&quot; cellpadding=&quot;1&quot; cellspacing=&quot;0&quot; width=&quot;100%&quot; class=&quot;table&quot;&gt;&lt;thead&gt;&lt;tr class=&quot;tableheadings&quot;&gt;&lt;th&gt;Message to Recipient (English)&lt;/th&gt;&lt;th&gt;Message to Recipient (French)&lt;/th&gt;&lt;/tr&gt;&lt;/thead&gt;&lt;tr align=&quot;center&quot;&gt;&lt;td&gt;ABC&lt;/td&gt;&lt;td&gt;EDF&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;</td></tr></table>";
			AssertEquals(expectedValue, tableCreator.ToHtml());
		}
	}
}
