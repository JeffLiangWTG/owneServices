using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusStatementHeaderDocumentSupporter))]
	sealed class CusStatementHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestCSARevenueSummaryFormDocumentWrapperIsSupported()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			var supporter = new CusStatementHeaderDocumentSupporter(statementHeader);
			var wrappers = supporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null);
			AssertEquals(1, wrappers.Length);
			var wrapperType = wrappers[0].ParentBusinessObject.GetType();
			AssertEquals(typeof(CSARevenueSummaryFormDocumentWrapper), wrapperType);
		}

		public void TestCARMSOABillingPeriodDocumentWrapperIsSupported()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				var statementHeader = Factory.New<CusStatementHeader>();
				statementHeader.B2_StatementType = CARMStatementOfAccountStatementTypeList.ShortCodes.LegalEntiry;
				var supporter = new CusStatementHeaderDocumentSupporter(statementHeader);
				var wrappers = supporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null);
				AssertEquals(1, wrappers.Length);
				var wrapperType = wrappers[0].ParentBusinessObject.GetType();
				AssertNotEquals(typeof(CARMSOABillingPeriodDocumentWrapper), wrapperType);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var statementHeader = Factory.New<CusStatementHeader>();
				statementHeader.B2_StatementType = "X";
				var supporter = new CusStatementHeaderDocumentSupporter(statementHeader);
				var wrappers = supporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null);
				AssertEquals(1, wrappers.Length);
				var wrapperType = wrappers[0].ParentBusinessObject.GetType();
				AssertNotEquals(typeof(CARMSOABillingPeriodDocumentWrapper), wrapperType);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var statementHeader = Factory.New<CusStatementHeader>();
				statementHeader.B2_StatementType = CARMStatementOfAccountStatementTypeList.ShortCodes.LegalEntiry;
				var supporter = new CusStatementHeaderDocumentSupporter(statementHeader);
				var wrappers = supporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null);
				AssertEquals(1, wrappers.Length);
				var wrapperType = wrappers[0].ParentBusinessObject.GetType();
				AssertEquals(typeof(CARMSOABillingPeriodDocumentWrapper), wrapperType);
			}
		}

		public void TestCARMDailyNoticeFormDocumentWrapperIsSupported()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				var statementHeader = Factory.New<CusStatementHeader>();
				statementHeader.B2_StatementNumber = "DN-123456789-1";
				var supporter = new CusStatementHeaderDocumentSupporter(statementHeader);
				var wrappers = supporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null);
				AssertEquals(1, wrappers.Length);
				var wrapperType = wrappers[0].ParentBusinessObject.GetType();
				AssertNotEquals(typeof(CARMDailyNoticeDocumentWrapper), wrapperType);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var statementHeader = Factory.New<CusStatementHeader>();
				statementHeader.B2_StatementNumber = "DN-123456789-1";
				var supporter = new CusStatementHeaderDocumentSupporter(statementHeader);
				var wrappers = supporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null);
				AssertEquals(1, wrappers.Length);
				var wrapperType = wrappers[0].ParentBusinessObject.GetType();
				AssertEquals(typeof(CARMDailyNoticeDocumentWrapper), wrapperType);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var statementHeader = Factory.New<CusStatementHeader>();
				statementHeader.B2_StatementNumber = "DN-1";
				var supporter = new CusStatementHeaderDocumentSupporter(statementHeader);
				var wrappers = supporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null);
				AssertEquals(1, wrappers.Length);
				var wrapperType = wrappers[0].ParentBusinessObject.GetType();
				AssertNotEquals(typeof(CARMDailyNoticeDocumentWrapper), wrapperType);
			}
		}

		public void TestNoExceptionWhenLastMessageIsNotEitherDNorSOA()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "8804P04001";

			var message = statement.Messages.AddNew();
			statement.B2_IsMonthlyStatement = false;
			message.EM_MessageText = string.Format(EDIMessageText, "XNN");
			AssertNoExceptionThrown(() => statement.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null)[0].ParentBusinessObject.GetType());

			message = statement.Messages.AddNew();
			statement.B2_IsMonthlyStatement = true;
			message.EM_MessageText = string.Format(EDIMessageText, "SOA");
			AssertNoExceptionThrown(() => statement.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null)[0].ParentBusinessObject.GetType());

			message.EM_MessageText = string.Format(EDIMessageText, "DN");
			statement.B2_IsMonthlyStatement = false;
			AssertNoExceptionThrown(() => statement.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null)[0].ParentBusinessObject.GetType());

			statement.Messages.RemoveAll();
			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = "Daily Notice";
			AssertNoExceptionThrown(() => statement.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), menuItem));
		}

		public void TestShowReasonForNotPrinting()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "8804P04001";
			statement.B2_IsMonthlyStatement = true;
			AssertEquals(false, statement.DocumentSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));

			var message = statement.Messages.AddNew();
			message.EM_MessageText = string.Format(EDIMessageText, "SOA");
			message.EM_MessageSubType = ARLMessageTypes.Codes.StatementOfAccount;
			statement.B2_IsMonthlyStatement = true;
			AssertEquals(typeof(ARLStatementOfAccountDocumentWrapper), statement.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null)[0].ParentBusinessObject.GetType());

			message.EM_MessageText = string.Format(EDIMessageText, "DN");
			message.EM_MessageSubType = ARLMessageTypes.Codes.DailyNotice;
			statement.B2_IsMonthlyStatement = false;
			AssertEquals(typeof(ARLDailyNoticeDocumentWrapper), statement.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null)[0].ParentBusinessObject.GetType());

			statement.Messages.RemoveAll();
			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = "Daily Notice";

			AssertNull(statement.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), menuItem));
		}

		public void TestBOProviderForDocumentSupport()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "8804P04001";
			statement.B2_IsMonthlyStatement = true;

			AssertEquals(typeof(CusStatementHeader), statement.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null)[0].ParentBusinessObject.GetType());
		}

		public void TestCusStatementHeaderIsSupported()
		{
			var statementHeader = Factory.New<CusStatementHeader>();

			var supporter = new CusStatementHeaderDocumentSupporter(statementHeader);
			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValue(".CusStatementHeader")));

			var result = supporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null);

			AssertEquals(1, result.Length);
			AssertEquals(statementHeader, BODocDataProvider.GetBusinessObject(result[0]));
		}

		public void TestGetContactOrganisation()
		{
			var importer = Factory.New<OrgHeader>();
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_OH_Importer = importer.PK;
			AssertEquals(importer.PK, statement.DocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY).OrgHeader.PK);
		}

		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "8804P04001";
			statement.B2_IsMonthlyStatement = false;

			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = "Daily Notice";

			AssertEquals("Daily Notice message cannot be found.", statement.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.SGPrintPermit), menuItem));

			statement.B2_IsMonthlyStatement = true;

			menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = "CARM Statement Of Account";

			AssertEquals("CARM Statement Of Account message cannot be found.", statement.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.SGPrintPermit), menuItem));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var statement = Factory.New<CusStatementHeader>();

			var message = statement.Messages.AddNew();
			message.EM_MessageSubType = ARLMessageTypes.Codes.DailyNotice;
			message.EM_MessageText = string.Format(EDIMessageText, "DN");

			var messageSOA = statement.Messages.AddNew();
			messageSOA.EM_MessageSubType = ARLMessageTypes.Codes.StatementOfAccount;
			statement.B2_IsMonthlyStatement = true;
			messageSOA.EM_MessageText = string.Format(EDIMessageText, "SOA");

			return statement;
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return documentCommand.SU_MenuName == "-";
		}

		const string EDIMessageText = @"<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CAAccountsReceivableLedger</Type>
					<Key></Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<BatchType>
			<Code>{0}</Code>
		</BatchType>
		<TransactionCollection>
			<Transaction>
				<Category>SUM</Category>
				<CreateTime>2014-11-18T00:00:00</CreateTime>
				<PostDate>2014-11-17T00:00:00</PostDate>
				<PostingJournalCollection>
					<PostingJournal>
						<Description>TotalPaymentReceived</Description>
						<LocalAmount>0</LocalAmount>
					</PostingJournal>
				</PostingJournalCollection>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";
	}
}
