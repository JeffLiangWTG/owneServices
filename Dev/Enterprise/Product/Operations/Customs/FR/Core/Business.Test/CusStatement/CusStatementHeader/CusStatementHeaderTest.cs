using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	[TestedType(typeof(CusStatementHeader))]
	sealed class CusStatementHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetEDocsProviderSupporter()
		{
			AssertType<JobInvoicingEDocsProviderSupporter>((statement as IEDocsProvider).GetEDocsProviderSupporter());
		}

		public void TestDocumentSupporter()
		{
			var supporter = (statement as IEDocsProvider).DocumentSupporter;
			AssertType<CusStatementHeaderDocumentSupporter>("Type", supporter);
		}

		public void TestDocManagerInfo()
		{
			var manager = (statement as IEDocsProvider).DocManagerInfo;
			AssertType<DocManagerInfo>("Type", manager);
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		public void TestLoadOrCreateDCGStatementHeaderWhenResponseHasAnomalies()
		{
			var factory = new BusinessObjectFactory();

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithAnomalies.xml");
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;
			factory.Save();
			var statementHeader = CusStatementHeader.LoadOrCreateDCGStatementHeader(factory, GlbCompany.CurrentCompany.PK, dataProvider, false);
			AssertNull(statementHeader);
		}

		public void TestLoadOrCreateDCGStatementHeaderWhenResponseHasErrors()
		{
			var factory = new BusinessObjectFactory();

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithErrors.xml");
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;
			factory.Save();
			var statementHeader = CusStatementHeader.LoadOrCreateDCGStatementHeader(factory, GlbCompany.CurrentCompany.PK, dataProvider, false);
			AssertNull(statementHeader);
		}

		public void TestLoadOrCreateDCGStatementHeader()
		{
			var factory = new BusinessObjectFactory();

			var dec1 = factory.New<JobDeclaration>();
			dec1.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			dec1.JE_DeclarationReference = "0738/19";
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			var entryNumber1 = CusEntryNumber.New(entry1, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber1.CE_EntryNum = "1906142283";
			factory.Save();

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;

			var statementHeader = CusStatementHeader.LoadOrCreateDCGStatementHeader(factory, GlbCompany.CurrentCompany.PK, dataProvider, true);
			factory.Save();

			AssertNotNull("In this case, a new CusStatementHeader should have been created.", statementHeader);
			AssertEquals("New statement header company should be the current one.", GlbCompany.CurrentCompany.PK, statementHeader.B2_GC);
			AssertEquals("New statement header B2_StatementType should have been defaulted to Unknown", StatementPeriodicityList.Codes.Unknown, statementHeader.B2_StatementType);
			AssertEquals("New statement header period start date should have been inferred from message", "01-Jul-19", statementHeader.B2_PeriodStartDate.ToString());
			AssertEquals("New statement header period end date should have been inferred from message", "31-Jul-19", statementHeader.B2_PeriodEndDate.ToString());
			AssertEquals("New statement header B2_BranchDesignation should be set to entry direction.", EU.Business.MessageTypeList.Codes.Import, statementHeader.B2_BranchDesignation);
			AssertEquals("New statement header ChargesDetail B3_BrokerReference should have been defaulted to 'DCG_REFERENCE'", "DCG_REFERENCE", statementHeader.ChargesDetail.B3_BrokerReference);

			factory.Save();

			AssertSame("In this case, the just created CusStatementHeader should have been loaded.", statementHeader, CusStatementHeader.LoadOrCreateDCGStatementHeader(factory, GlbCompany.CurrentCompany.PK, dataProvider, true));
		}

		public void TestMessages()
		{
			var message = Factory.New<FREDIMessage>();
			message.EM_LinkedObject = statement;
			AssertContainsExactElementsInAnyOrder(new[] { message }, statement.Messages);
		}

		public void TestB2_StatementNumber()
		{
			AssertEquals("Statement Number", DataBoundResourceStrings.GetDataForProperty(statement.B2_StatementNumberInfo).Caption);
			AssertEquals(true, statement.B2_StatementNumberInfo.ReadOnly);
		}

		public void TestB2_StatementNumber_IsGeneratedByNumberFountain()
		{
			Factory.Save();
			AssertEquals("L00000001", statement.B2_StatementNumber);

			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_StatementType = StatementPeriodicityList.Codes.Day;
			Factory.Save();
			AssertEquals("L00000002", statement1.B2_StatementNumber);

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementType = StatementPeriodicityList.Codes.Day;
			Factory.Save();
			AssertEquals("L00000003", statement2.B2_StatementNumber);
		}

		public void TestEntryNumber()
		{
			AssertEquals("Entry Number", DataBoundResourceStrings.GetDataForProperty(statement.EntryNumberInfo).Caption);
			AssertEquals(true, statement.EntryNumberInfo.ReadOnly);

			statement.ChargesDetail.B3_EntryNum = "DCG004800";
			AssertEquals("DCG004800", statement.EntryNumber);
		}

		public void TestB2_Status()
		{
			AssertEquals("Status", DataBoundResourceStrings.GetDataForProperty(statement.B2_StatusInfo).Caption);
			AssertEquals(true, statement.B2_StatusInfo.ReadOnly);
		}

		public void TestStatusDescription()
		{
			var resStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusStatementHeader), nameof(CusStatementHeader.StatusDescription));
			AssertEquals("Status Description", resStringData.Caption);
			AssertEquals("Status Desc.", resStringData.ShortCaption);

			statement.B2_Status = ZString.Empty;
			AssertEquals(ZString.Empty, statement.StatusDescription);

			statement.B2_Status = StatementStatusList.Codes.Complete;
			AssertEquals(StatementStatusList.Descriptions.Complete, statement.StatusDescription);

			statement.B2_Status = "XXX";
			AssertEquals(ZString.Empty, statement.StatusDescription);
		}

		public void TestB2_PaymentType()
		{
			AssertEquals("Payment Method", DataBoundResourceStrings.GetDataForProperty(statement.B2_PaymentTypeInfo).Caption);
			AssertEquals("B2_PaymentType should have default value 'R'.", MethodOfPaymentList.Codes.R, Factory.New<CusStatementHeader>().B2_PaymentType);
			AssertPropertyIsReadOnlyWhenEntryNumberIsNotEmpty(statement.B2_PaymentTypeInfo);
		}

		public void TestPaymentTypeDescription()
		{
			var resStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusStatementHeader), nameof(CusStatementHeader.PaymentTypeDescription));
			AssertEquals("Payment Method Description", resStringData.Caption);
			AssertEquals("Payment Method Desc.", resStringData.ShortCaption);

			statement.B2_PaymentType = ZString.Empty;
			AssertEquals(ZString.Empty, statement.PaymentTypeDescription);

			statement.B2_PaymentType = MethodOfPaymentList.Codes.A;
			AssertEquals(MethodOfPaymentList.Descriptions.A, statement.PaymentTypeDescription);

			statement.B2_PaymentType = "XXX";
			AssertEquals(ZString.Empty, statement.PaymentTypeDescription);
		}

		public void TestB2_BranchDesignation()
		{
			AssertEquals("Type", DataBoundResourceStrings.GetDataForProperty(statement.B2_BranchDesignationInfo).Caption);
			AssertPropertyIsReadOnlyWhenEntryNumberIsNotEmpty(statement.B2_BranchDesignationInfo);
		}

		public void TestBranchDesignationDescription()
		{
			var resStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusStatementHeader), nameof(CusStatementHeader.BranchDesignationDescription));
			AssertEquals("Type Description", resStringData.Caption);
			AssertEquals("Type Desc.", resStringData.ShortCaption);

			statement.B2_BranchDesignation = ZString.Empty;
			AssertEquals(ZString.Empty, statement.BranchDesignationDescription);

			statement.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			AssertEquals(StatementEntryTypeList.Descriptions.Import, statement.BranchDesignationDescription);

			statement.B2_BranchDesignation = "XXX";
			AssertEquals(ZString.Empty, statement.BranchDesignationDescription);
		}

		public void TestB2_StatementType()
		{
			AssertEquals("Frequency", DataBoundResourceStrings.GetDataForProperty(statement.B2_StatementTypeInfo).Caption);
			AssertEquals("The default B2_StatementType should be empty.", ZString.Empty, Factory.New<CusStatementHeader>().B2_StatementType);
			AssertPropertyIsReadOnlyWhenEntryNumberIsNotEmpty(statement.B2_StatementTypeInfo);
		}

		public void TestStatementTypeDescription()
		{
			var resStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusStatementHeader), nameof(CusStatementHeader.StatementTypeDescription));
			AssertEquals("Frequency Description", resStringData.Caption);
			AssertEquals("Frequency Desc.", resStringData.ShortCaption);

			statement.B2_StatementType = ZString.Empty;
			AssertEquals(ZString.Empty, statement.StatementTypeDescription);

			statement.B2_StatementType = StatementPeriodicityList.Codes.Decade;
			AssertEquals(StatementPeriodicityList.Descriptions.Decade, statement.StatementTypeDescription);

			statement.B2_StatementType = "%";
			AssertEquals(ZString.Empty, statement.StatementTypeDescription);
		}

		public void TestB2_EntryFilerCode()
		{
			AssertEquals("Profile", DataBoundResourceStrings.GetDataForProperty(statement.B2_EntryFilerCodeInfo).Caption);
			AssertPropertyIsReadOnlyWhenEntryNumberIsNotEmpty(statement.B2_EntryFilerCodeInfo);
		}

		public void TestB2_AccountNo()
		{
			AssertEquals("Account", DataBoundResourceStrings.GetDataForProperty(statement.B2_AccountNoInfo).Caption);
		}

		public void TestB2_ImporterCustomsID()
		{
			AssertEquals("Representative ID", DataBoundResourceStrings.GetDataForProperty(statement.B2_ImporterCustomsIDInfo).Caption);
			AssertPropertyIsReadOnlyWhenEntryNumberIsNotEmpty(statement.B2_ImporterCustomsIDInfo);
		}

		public void TestB2_OH_Importer()
		{
			AssertEquals("Representative", DataBoundResourceStrings.GetDataForProperty(statement.B2_OH_ImporterInfo).Caption);
			AssertPropertyIsReadOnlyWhenEntryNumberIsNotEmpty(statement.B2_OH_ImporterInfo);
		}

		public void TestImporterFullName()
		{
			AssertEquals("Representative", DataBoundResourceStrings.GetDataForProperty(typeof(CusStatementHeader), nameof(CusStatementHeader.ImporterFullName)).Caption);

			statement.B2_OH_Importer = ZGuid.Empty;
			AssertEquals(ZString.Empty, statement.ImporterFullName);

			statement.B2_OH_Importer = ZGuid.NewZGuid();
			AssertEquals(ZString.Empty, statement.ImporterFullName);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "HG";
			importer.OH_FullName = "Highgarden";
			statement.B2_OH_Importer = importer.PK;
			AssertEquals("HG Highgarden", statement.ImporterFullName);
		}

		public void TestB2_DueDate()
		{
			AssertEquals("Due Date", DataBoundResourceStrings.GetDataForProperty(statement.B2_DueDateInfo).Caption);
		}

		public void TestB2_PeriodStartDate()
		{
			AssertPropertyIsReadOnlyWhenEntryNumberIsNotEmpty(statement.B2_PeriodStartDateInfo);
		}

		public void TestB2_PeriodEndDate()
		{
			AssertPropertyIsReadOnlyWhenEntryNumberIsNotEmpty(statement.B2_PeriodEndDateInfo);
		}

		public void TestB2_CheckNo()
		{
			statement.EntryNumber = "ABC123";
			AssertEquals("B2_CheckNo should be read only when EntryNumber is not empty.", true, statement.B2_CheckNoInfo.ReadOnly);

			statement.EntryNumber = ZString.Empty;
			statement.B2_PaymentType = MethodOfPaymentList.Codes.A;
			AssertEquals("B2_CheckNo should be read only when EntryNumber is empty, but B2_PaymentType is not R or M.", true, statement.B2_CheckNoInfo.ReadOnly);

			statement.B2_PaymentType = MethodOfPaymentList.Codes.R;
			AssertEquals("B2_CheckNo should be editable when EntryNumber is empty, and B2_PaymentType is R or M.", false, statement.B2_CheckNoInfo.ReadOnly);
		}

		public void TestCorrelationID()
		{
			AssertEquals("Reference Number", DataBoundResourceStrings.GetDataForProperty(statement.CorrelationIDInfo).Caption);
			AssertEquals(10, CusStatementHeader.Schema.CorrelationMaxLength);
			AssertEquals(true, statement.CorrelationIDInfo.ReadOnly);
			AssertEquals(ZString.Empty, statement.CorrelationID);
			Factory.Save();
			AssertNotNullOrEmpty(statement.CorrelationID);
			AssertEquals(statement.ChargesDetail.B3_BrokerReference, statement.CorrelationID);
		}

		public void TestCorrelationID_ShouldSkipExistingOne()
		{
			statement.B2_StatementType = StatementPeriodicityList.Codes.Decade;
			Factory.Save();
			AssertEquals("0000000001", statement.CorrelationID);

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementType = StatementPeriodicityList.Codes.Decade;
			Factory.Save();
			AssertEquals("0000000002", statement2.CorrelationID);

			var statementFromCustoms = Factory.New<CusStatementHeader>();
			statementFromCustoms.B2_StatementType = StatementPeriodicityList.Codes.Decade;
			statementFromCustoms.CorrelationID = "0000000003";
			Factory.Save();

			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_StatementType = StatementPeriodicityList.Codes.Decade;
			Factory.Save();
			AssertEquals("Since 0000000003 exists, we should generate a new one instead.", "0000000004", statement3.CorrelationID);

			var newFactory = new BusinessObjectFactory();
			var statement4 = newFactory.New<CusStatementHeader>();
			statement4.B2_StatementType = StatementPeriodicityList.Codes.Decade;
			newFactory.Save();
			AssertEquals("The next number should be 0000000005.", "0000000005", statement4.CorrelationID);
		}

		public void TestCorrelationIDPrefix()
		{
			statement.B2_StatementType = StatementPeriodicityList.Codes.Decade;
			AssertEquals(ZString.Empty, statement.CorrelationIDPrefix);
		}

		public void TestHumanReadableName()
		{
			statement.B2_StatementNumber = "002244";
			AssertEquals("Liquidation 002244", statement.HumanReadableName);
		}

		public void TestDeltaAgreementAccountRepresentativeID()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ReportingPeriodList.Codes.DAY, "AD49C94A");
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI002", ZString.Empty, ReportingPeriodList.Codes.TEN, "B92F8A8B");
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "DGE001", ZString.Empty, ReportingPeriodList.Codes.DAY, "D34C0059");
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G2, "DGE002", ZString.Empty, ReportingPeriodList.Codes.TEN, "E7A1CC18");

			statement.B2_BranchDesignation = StatementEntryTypeImpExpList.Codes.Import;
			statement.B2_OH_Importer = importer.PK;
			statement.B2_EntryFilerCode = "DGI002";
			AssertEquals("B92F8A8B", statement.DeltaAgreementAccountRepresentativeID);

			statement.B2_BranchDesignation = StatementEntryTypeImpExpList.Codes.Export;
			statement.B2_EntryFilerCode = "DGE002";
			AssertEquals("E7A1CC18", statement.DeltaAgreementAccountRepresentativeID);

			statement.B2_BranchDesignation = ZString.Empty;
			AssertEquals(ZString.Empty, statement.DeltaAgreementAccountRepresentativeID);
		}

		public void TestDefaultingOfChargesDetail()
		{
			var chargesDetail = CusStatementChargesDetail.LoadOrCreate(statement);
			AssertNotNull(chargesDetail);
			AssertEquals(StatementEntryTypeList.Codes.DCG, chargesDetail.B3_EntryType);
		}

		public void TestCollectionProperties()
		{
			statement.Entries.AddNew();
			AssertEquals(1, statement.Entries.Count);
			statement.Entries.AddNew();
			AssertEquals(2, statement.Entries.Count);
		}

		public void TestLookups()
		{
			AssertType<CusStatementHeaderLookups>(statement.Lookups);
		}

		void AssertPropertyIsReadOnlyWhenEntryNumberIsNotEmpty(ZPropertyInfo propertyInfo)
		{
			statement.EntryNumber = ZString.Empty;
			AssertEquals($"Property {propertyInfo.Name} should be editable when EntryNumber is empty.", false, propertyInfo.ReadOnly);

			statement.EntryNumber = "ABC123";
			AssertEquals($"Property {propertyInfo.Name} should be read only when EntryNumber is not empty.", true, propertyInfo.ReadOnly);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Deleting object should have an exception/error because of a trigger.", true);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => statement;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => statement;

		protected override void SetUp()
		{
			base.SetUp();
			statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementPeriodicityList.Codes.Day;
		}

		CusStatementHeader statement;
	}
}
