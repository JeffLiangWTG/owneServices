using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusStatementHeader))]
	sealed class CusStatementHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Deleting object should have an exception/error because of a trigger.", true);
		}

		public void TestPeriod()
		{
			var statement = Factory.New<CusStatementHeader>();
			AssertEquals(ZString.Empty, statement.PeriodFrom);

			statement.B2_PeriodEndDate = new ZDate(2021, 03, 31);
			AssertEquals(ZString.Empty, statement.PeriodFrom);

			statement.B2_PeriodStartDate = new ZDate(2021, 03, 1);
			AssertEquals("03/2021", statement.PeriodFrom);
		}

		[TestedType(typeof(CusStatementHeader.Loader))]
		class Test : LoaderTestCase
		{
			public void TestLoader()
			{
				CusStatementHeader statementHeader1 = Factory.New<CusStatementHeader>();
				statementHeader1.B2_StatementNumber = "0127030012000018260";
				statementHeader1.B2_GC = GlbCompany.CurrentCompany.PK;
				statementHeader1.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;

				var result1 = new CusStatementHeader.Loader(Factory).Load("0127030012000018260", GlbCompany.CurrentCompany.PK.ToGuid(), StatementHeaderTypeList.Codes.Invoice);
				AssertEquals(statementHeader1, result1);

				var newCompany = Factory.New<GlbCompany>();

				CusStatementHeader statementHeader2 = Factory.New<CusStatementHeader>();
				statementHeader2.B2_StatementNumber = "0127030012000018260";
				statementHeader2.B2_GC = newCompany.PK;
				statementHeader2.B2_StatementType = StatementHeaderTypeList.Codes.IndividualCollectionReceipt;

				var result2 = new CusStatementHeader.Loader(Factory).Load("0127030012000018260", newCompany.PK, StatementHeaderTypeList.Codes.IndividualCollectionReceipt);
				AssertEquals(statementHeader2, result2);
			}

			protected override BusinessObject.Loader GetNewLoaderToTest()
			{
				return new CusStatementHeader.Loader(Factory);
			}
		}

		public void TestIControllerIDProvider()
		{
			var statement = Factory.New<CusStatementHeader>();
			var iStatement = (IControllerIDProvider)statement;
			AssertEquals(statement.PK, iStatement.BusinessObjectPK);
			AssertEquals(ControllerIDs.Customs.CustomsStatement, iStatement.ControllerID);
		}

		public void TestMessagesParent()
		{
			var statement = Factory.New<CusStatementHeader>();
			var iStatement = (IEDIMessageCollectionProvider)statement;
			AssertEquals(Factory, iStatement.Factory);
			AssertEquals(statement.Messages, iStatement.Messages);
		}

		public void TestUNIPASSDeclarantID()
		{
			var statement = Factory.New<CusStatementHeader>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(statement.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "40084");
			AssertEquals("40084", statement.UNIPASSDeclarantID);

			statement.B2_GC = Guid.Empty;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "40084");
			AssertNullOrEmpty(statement.UNIPASSDeclarantID);
		}
		public void TestFormatted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.EntryNumber = "1234524123456";

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.MonthlyReceipt;
			statement.B2_StatementNumber = "0402080011634";
			statement.B2_ImporterCustomsID = "1168103897";

			var line = statement.StatementLines.AddNew();
			line.B3_EntryNum = "1234524123456";
			line.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			AssertEquals("80011634", statement.FormattedTaxInvoiceNumber);
			AssertEquals("116-81-03897", statement.FormattedImporterID);

			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			statement.B2_StatementNumber = "0127012012100000649";

			AssertEquals("0127-012-01-21-0-000064-9", statement.FormattedPaymentNumber);
		}

		public void TestFirstLine()
		{
			var statement = Factory.New<CusStatementHeader>();
			var lineData = statement.StatementLines.AddNew();
			var lineData2 = statement.StatementLines.AddNew();

			AssertEquals(lineData, statement.FirstLine);

			AssertNotEquals(lineData2, statement.FirstLine);
			AssertEquals(lineData2, statement.StatementLines[1]);
		}

		public void TestBaseAmountAndVAT()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.MonthlyReceipt;
			var line = statement.StatementLines.AddNew();
			line.B3_CustomsFeesTotal = 760570m;
			var charge = line.Charges.AddNew();
			charge.B4_ChargeAmount = 7605791m;
			charge.B4_ChargeType = ChargeTypeList.Codes.ValueForVAT;

			var line2 = statement.StatementLines.AddNew();
			line2.B3_CustomsFeesTotal = 1000000m;
			var charge2 = line2.Charges.AddNew();
			charge2.B4_ChargeAmount = 10000000m;
			charge2.B4_ChargeType = ChargeTypeList.Codes.ValueForVAT;

			AssertEquals(1760570m, statement.TotalVAT);
			AssertEquals(17605791m, statement.TotalVATBaseAmountForVATReport);

			AssertEquals(5, statement.TotalVATBaseAmountBlankCount);
			AssertNullOrEmpty(statement.TotalVATBaseAmountDigits[0]);
			AssertNullOrEmpty(statement.TotalVATBaseAmountDigits[1]);
			AssertNullOrEmpty(statement.TotalVATBaseAmountDigits[2]);
			AssertNullOrEmpty(statement.TotalVATBaseAmountDigits[3]);
			AssertEquals("1", statement.TotalVATBaseAmountDigits[4]);
			AssertEquals("7", statement.TotalVATBaseAmountDigits[5]);
			AssertEquals("6", statement.TotalVATBaseAmountDigits[6]);
			AssertEquals("0", statement.TotalVATBaseAmountDigits[7]);
			AssertEquals("5", statement.TotalVATBaseAmountDigits[8]);
			AssertEquals("7", statement.TotalVATBaseAmountDigits[9]);
			AssertEquals("9", statement.TotalVATBaseAmountDigits[10]);
			AssertEquals("1", statement.TotalVATBaseAmountDigits[11]);

			AssertNullOrEmpty(statement.TotalVATDigits[0]);
			AssertNullOrEmpty(statement.TotalVATDigits[1]);
			AssertNullOrEmpty(statement.TotalVATDigits[2]);
			AssertNullOrEmpty(statement.TotalVATDigits[3]);
			AssertEquals("1", statement.TotalVATDigits[4]);
			AssertEquals("7", statement.TotalVATDigits[5]);
			AssertEquals("6", statement.TotalVATDigits[6]);
			AssertEquals("0", statement.TotalVATDigits[7]);
			AssertEquals("5", statement.TotalVATDigits[8]);
			AssertEquals("7", statement.TotalVATDigits[9]);
			AssertEquals("0", statement.TotalVATDigits[10]);

			var statement5FY = Factory.New<CusStatementHeader>();
			statement5FY.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			var line3 = statement5FY.StatementLines.AddNew();
			line3.B3_CustomsFeesTotal = 760570m;
			var charge3 = line3.Charges.AddNew();
			charge3.B4_ChargeAmount = 7605791m;
			charge3.B4_ChargeType = ChargeTypeList.Codes.ValueForVAT;

			AssertEquals(0m, statement5FY.TotalVAT);
			AssertEquals(0m, statement5FY.TotalVATBaseAmountForVATReport);

			AssertEquals(13, statement5FY.TotalVATBaseAmountBlankCount);
			AssertNullOrEmpty(statement5FY.TotalVATBaseAmountDigits[11]);
			AssertNullOrEmpty(statement5FY.TotalVATDigits[10]);
		}

		public void TestPayer()
		{
			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "한국아이비엠(주)");
			TestOrgDataSetUpHelper.AddOrgContact(payer, "송기홍", true);
			TestOrgDataSetUpHelper.AddOrgAddress(payer.MainAddress, "서울특별시 영등포구 국제금융로 10", "(여의도동, 서울 국제금융 센터)");
			var payerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1168103897", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(payer, payerCodes);

			var orgHeaderWrapper = OrgHeaderWrapper.New(payer);
			orgHeaderWrapper.ZO_TypeOfBusiness = "도매, 써비스";
			orgHeaderWrapper.ZO_ItemOfBusiness = "전산기 임대 및 판매";

			Factory.Save();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_OH_Importer = payer.PK;

			AssertEquals(orgHeaderWrapper, statement.Payer);
		}

		public void TestCustoms()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "040", "인천세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(codeList.PK, Constants.ZZ.CodeListAttributeNames.BusinessNumber, "1218300561");
			helper.CreateCusCodeListAttribute(codeList.PK, Constants.ZZ.CodeListAttributeNames.Address, "인천광역시 중구 서해대로 339 (항동7가)");
			helper.CreateCusCodeListAttribute(codeList.PK, Constants.ZZ.CodeListAttributeNames.BankAccountID, "110288");

			Factory.Save();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_ProcessPort = "030";

			AssertNull(statement.CustomsOffice);
			AssertNullOrEmpty(statement.CustomsAddress);
			AssertNullOrEmpty(statement.FormattedCustomsOfficeBusinessNumber);
			AssertNullOrEmpty(statement.BankAccountID);

			statement.B2_ProcessPort = "040";

			AssertEquals("인천세관", statement.CustomsOffice.ZZD_Description);
			AssertEquals("인천광역시 중구 서해대로 339 (항동7가)", statement.CustomsAddress);
			AssertEquals("121-83-00561", statement.FormattedCustomsOfficeBusinessNumber);
			AssertEquals("110288", statement.BankAccountID);
		}

		public void TestTotalTax()
		{
			GlbGroup importGroup = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "T1";
			staff1.GS_LoginName = "Test1";
			staff1.GS_EmailAddress = "staff1@wisetechglobal.com";
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = importGroup.PK;
			link1.GK_GS = staff1.PK;

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "T2";
			staff2.GS_LoginName = "Test2";
			staff2.GS_EmailAddress = "staff2@wisetechglobal.com";
			var link2 = Factory.New<GlbGroupLink>();
			link2.GK_GG = importGroup.PK;
			link2.GK_GS = staff2.PK;

			var fileReader = new TestFileReader(typeof(CusStatementHeaderTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5FY_0.xml");

			var incomingMessage = Factory.New<EDIMessage>();

			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5FY;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;

			Factory.Save();

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			incomingMessage.Reload();

			var statementHeader = new CusStatementHeader.Loader(Factory).Load("0127030012000018260", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.Invoice);

			AssertEquals("0127030012000018260", statementHeader.B2_StatementNumber);
			AssertEquals(12605040m, statementHeader.TotalDutyAmount);
			AssertEquals(17016810m, statementHeader.TotalVAT);
			AssertEquals(123m, statementHeader.TotalLiquorTax);
			AssertEquals(456m, statementHeader.TotalAgricultureTax);
			AssertEquals(789m, statementHeader.TotalSpecialConsumptionTax);
			AssertEquals(987m, statementHeader.TotalTransportationTax);
			AssertEquals(654m, statementHeader.TotalEducationTax);
			AssertEquals(321m, statementHeader.TotalPenaltyAndInterest);
			AssertEquals(159m, statementHeader.TotalPenaltyForLatePayment);
		}

		public void TestKoreanCustomsIDAndPaymentReferenceNumber()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = ZString.Empty;
			statement.B2_StatementNumber = "0127012012100000649";
			AssertEquals("", statement.KoreanCustomsID);
			AssertEquals("0127012012100000649", statement.FormattedPaymentReferenceNumber);

			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			AssertEquals("0127", statement.KoreanCustomsID);
			AssertEquals("012-01-21-0-000064-9", statement.FormattedPaymentReferenceNumber);
		}

		public void TestOverdueAmount()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("FLA", "Flat");
			helper.CreateTaxOrFee(ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.DailyRateAfterSixMonths, 0.00022m, Core.Constants.CountryCodes.KoreaSouth, 0m, 0m, "FLA", new ZDateTime("2022-02-15"), ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementAmount = 1534567m;
			statement.B2_DueDate = new ZDateTime(2024, 10, 28);
			var statementLine = statement.StatementLines.AddNew();
			var charge = statementLine.Charges.AddNew();
			charge.B4_ChargeType = ChargeTypeList.Codes.Duty;
			charge.B4_ChargeAmount = 1534567m;
			AssertEquals("StatementAmount >= 1500000", 1580930m, statement.OverdueAmount);

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementAmount = 1400000m;
			statement2.B2_DueDate = new ZDateTime(2024, 10, 28);
			var statementLine2 = statement2.StatementLines.AddNew();
			var charge2 = statementLine2.Charges.AddNew();
			charge2.B4_ChargeType = ChargeTypeList.Codes.Duty;
			charge2.B4_ChargeAmount = 1400000m;
			AssertEquals("StatementAmount < 1500000", 1442000m, statement2.OverdueAmount);
		}

		public void TestTotalAmountPayableOneDayAfterDueDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("FLA", "Flat");
			helper.CreateTaxOrFee(ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.DailyRateAfterSixMonths, 0.00022m, Core.Constants.CountryCodes.KoreaSouth, 0m, 0m, "FLA", new ZDateTime("2022-02-15"), ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementAmount = 1400000m;
			statement.B2_DueDate = new ZDateTime(2024, 10, 28);
			SetLineCharges(statement);

			var fileReader = new TestFileReader(typeof(PenaltyExemptionRequestMessageSendingObjectTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5FK_ANT.xml");
			var message5FK = statement.Messages.AddNew();
			message5FK.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message5FK.EM_MessageText = messageText;
			message5FK.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5FK.EM_MessageType = ElectronicDocumentTypeList.Codes._5FK;
			message5FK.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
			message5FK.EM_ApplicationReference = "1";

			statement.B2_Status = StatementHeaderStatusList.Codes.A;
			AssertEquals("Status is not 'Z' and StatementAmount < 1500000", 5099950m, statement.OverdueAmount);

			statement.B2_Status = StatementHeaderStatusList.Codes.Z;
			AssertEquals("Status is 'Z' and StatementAmount < 1500000", 5099950m, statement.OverdueAmount);

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_Status = StatementHeaderStatusList.Codes.Z;
			statement2.B2_StatementAmount = 1500000m;
			statement2.B2_DueDate = new ZDateTime(2024, 10, 28);
			SetLineCharges(statement2);
			AssertEquals("Status is 'Z' and StatementAmount >= 1500000", 5100650m, statement2.OverdueAmount);
		}

		public void TestFormattedImporterID()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_ImporterCustomsID = "1234567890";
			AssertEquals("123-45-67890", statement.FormattedImporterID);

			statement.B2_ImporterCustomsID = "1234567890123";
			AssertEquals("123456-7890123", statement.FormattedImporterID);
		}

		public void TestBrokerDeclarantID()
		{
			var statement = Factory.New<CusStatementHeader>();
			AssertEquals("If CusStatementHeader.Lines is null, return ZString.Empty.", ZString.Empty, statement.BrokerDeclarantID);

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1234567890123";
			statement.B2_StatementType = "X";
			AssertEquals("If CusStatementHeader.B2_StatementType is not in ('I', 'R', 'C'), return ZString.Empty.", ZString.Empty, statement.BrokerDeclarantID);

			statement.B2_StatementType = "I";
			AssertEquals("12345", statement.BrokerDeclarantID);
		}

		public void Test5JGDocumentProperties()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "110", "울산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(codeList.PK, Constants.ZZ.CodeListAttributeNames.BankAccountID, "160034");

			GlbGroup importGroup = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "T1";
			staff1.GS_LoginName = "Test1";
			staff1.GS_EmailAddress = "staff1@wisetechglobal.com";
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = importGroup.PK;
			link1.GK_GS = staff1.PK;

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "T2";
			staff2.GS_LoginName = "Test2";
			staff2.GS_EmailAddress = "staff2@wisetechglobal.com";
			var link2 = Factory.New<GlbGroupLink>();
			link2.GK_GG = importGroup.PK;
			link2.GK_GS = staff2.PK;

			var fileReader = new TestFileReader(typeof(CusStatementHeaderTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5JG_TypeCodeIs17.xml");
			var incomingMessage = Factory.New<EDIMessage>();

			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5JG;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;

			Factory.Save();

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			incomingMessage.Reload();

			var statementHeader = new CusStatementHeader.Loader(Factory).Load("110512000000880", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.Normal);

			//			AssertEquals("79002792961104", statementHeader.B2_AccountNo);
			AssertEquals(new ZDate(2020, 10, 01), statementHeader.B2_ProcessDate);
			AssertEquals("160034", statementHeader.BankAccountID);
			AssertEquals("110-51-20-00000880", statementHeader.FormattedStatementNumber);
			AssertEquals("790-02-792961104", statementHeader.FormattedAccountNumber);
			AssertEquals(7000m, statementHeader.B2_StatementAmount);
			AssertContains("쑹까오신밍", statementHeader.PayerFromCustoms);
			AssertContains("(주)엠아이씨텍글로벌", statementHeader.PayerFromCustoms);
			AssertContains("경기 성남시 분당구 황새울로240번길3 (현대오피스빌딩)", statementHeader.PayerFromCustoms);
			AssertEquals("울산세관", statementHeader.CustomsOffice.ZZD_Description);
			AssertEquals(3500m, statementHeader.TemporaryOpeningFee);
			AssertEquals(3500m, statementHeader.InspectionFee);
			AssertEquals(0m, statementHeader.PermissionApplicationFee);
		}

		public void Test5AJDocumentProperties()
		{
			GlbGroup importGroup = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "T1";
			staff1.GS_LoginName = "Test1";
			staff1.GS_EmailAddress = "staff1@wisetechglobal.com";
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = importGroup.PK;
			link1.GK_GS = staff1.PK;

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "T2";
			staff2.GS_LoginName = "Test2";
			staff2.GS_EmailAddress = "staff2@wisetechglobal.com";
			var link2 = Factory.New<GlbGroupLink>();
			link2.GK_GG = importGroup.PK;
			link2.GK_GS = staff2.PK;

			var fileReader = new TestFileReader(typeof(CusStatementHeaderTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5AJ_5AC.xml");
			var incomingMessage = Factory.New<EDIMessage>();

			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5AJ;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;

			var statement5JG = Factory.New<CusStatementHeader>();
			statement5JG.B2_StatementNumber = "030511900081007";
			statement5JG.B2_GC = GlbBranch.CurrentBranch.Company.PK;
			statement5JG.B2_StatementType = StatementHeaderTypeList.Codes.Normal;
			Factory.Save();

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			incomingMessage.Reload();

			#region StatementHeader
			var statementHeader = new CusStatementHeader.Loader(Factory).Load("030190079807", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.NormalReport);

			AssertEquals("030190079807", statementHeader.B2_StatementNumber);
			AssertEquals("030-19-0079807", statementHeader.FormattedStatementNumber);
			AssertEquals(39600m, statementHeader.B2_StatementAmount);
			AssertEquals("PYC", statementHeader.B2_PaymentStatus);
			AssertEquals("B", statementHeader.B2_StatementType);
			AssertEquals("030", statementHeader.B2_ProcessPort);
			AssertEquals(true, statementHeader.B2_IsMonthlyStatement);
			AssertEquals("030511900081007", statementHeader.B2_AccountNo);
			AssertEquals("030-51-19-00081007", statementHeader.FormattedAccountNumber);
			AssertEquals("", statementHeader.B2_PaymentParty);
			AssertEquals("6108500065", statementHeader.B2_ImporterCustomsID);
			AssertEquals(GlbBranch.CurrentBranch.Company.PK, statementHeader.B2_GC);
			#endregion
		}

		public void TestTotalAmount()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementAmount = 1234567m;
			var statementLine1 = statement.StatementLines.AddNew();
			statementLine1.B3_CustomsFeesTotal = 100m;
			var statementLine2 = statement.StatementLines.AddNew();
			statementLine2.B3_CustomsFeesTotal = 200m;
			var statementLine3 = statement.StatementLines.AddNew();
			statementLine3.B3_CustomsFeesTotal = 300m;

			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			AssertEquals(1234567m, statement.TotalAmount);

			statement.B2_StatementType = StatementHeaderTypeList.Codes.IndividualCollectionReceipt;
			AssertEquals(600m, statement.TotalAmount);

			statement.B2_StatementType = StatementHeaderTypeList.Codes.NormalReport;
			AssertEquals(1234567m, statement.TotalAmount);

			statement.B2_StatementType = StatementHeaderTypeList.Codes.MonthlyReceipt;
			AssertEquals(600m, statement.TotalAmount);

			statement.B2_StatementType = StatementHeaderTypeList.Codes.Normal;
			AssertEquals(1234567m, statement.TotalAmount);
		}

		public void TestFormattedStatementNumber()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "0127010012100011441";
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			AssertEquals("0127-010-01-21-00011441", statement.FormattedStatementNumber);

			statement.B2_StatementNumber = "0102180004497";
			statement.B2_StatementType = StatementHeaderTypeList.Codes.IndividualCollectionReceipt;
			AssertEquals("010-21-80004497", statement.FormattedStatementNumber);

			statement.B2_StatementType = StatementHeaderTypeList.Codes.MonthlyReceipt;
			AssertEquals("010-21-80004497", statement.FormattedStatementNumber);

			statement.B2_StatementNumber = "030190079807";
			statement.B2_StatementType = StatementHeaderTypeList.Codes.NormalReport;
			AssertEquals("030-19-0079807", statement.FormattedStatementNumber);

			statement.B2_StatementNumber = "030511900081007";
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Normal;
			AssertEquals("030-51-19-00081007", statement.FormattedStatementNumber);
		}
		public void TestFormattedAccountNumber()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_AccountNo = "0127010012100011441";
			statement.B2_StatementType = StatementHeaderTypeList.Codes.MonthlyReceipt;
			AssertEquals("0127-010-01-21-00011441", statement.FormattedAccountNumber);

			statement.B2_StatementType = StatementHeaderTypeList.Codes.IndividualCollectionReceipt;
			AssertEquals("0127-010-01-21-00011441", statement.FormattedAccountNumber);

			statement.B2_AccountNo = "0102180004497";
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Normal;
			AssertEquals("010-21-80004497", statement.FormattedAccountNumber);

			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			AssertEquals("010-21-80004497", statement.FormattedAccountNumber);

			statement.B2_AccountNo = "030511900081007";
			statement.B2_StatementType = StatementHeaderTypeList.Codes.NormalReport;
			AssertEquals("030-51-19-00081007", statement.FormattedAccountNumber);
		}
		public void TestTotalCustomsValue()
		{
			var statement = Factory.New<CusStatementHeader>();
			var statementLine1 = statement.StatementLines.AddNew();
			var charge1 = statementLine1.Charges.AddNew();
			charge1.B4_ChargeAmount = 100m;
			charge1.B4_ChargeType = ChargeTypeList.Codes.ValueForVAT;
			var charge2 = statementLine1.Charges.AddNew();
			charge2.B4_ChargeAmount = 200m;
			charge2.B4_ChargeType = ChargeTypeList.Codes.TOF;
			var statementLine2 = statement.StatementLines.AddNew();
			var charge3 = statementLine2.Charges.AddNew();
			charge3.B4_ChargeAmount = 400m;
			charge3.B4_ChargeType = ChargeTypeList.Codes.DIF;
			var charge4 = statementLine2.Charges.AddNew();
			charge4.B4_ChargeAmount = 800m;
			charge4.B4_ChargeType = ChargeTypeList.Codes.ValueForVAT;
			var statementLine3 = statement.StatementLines.AddNew();
			var charge5 = statementLine3.Charges.AddNew();
			charge5.B4_ChargeAmount = 1600m;
			charge5.B4_ChargeType = ChargeTypeList.Codes.ValueForVAT;
			var charge6 = statementLine3.Charges.AddNew();
			charge6.B4_ChargeAmount = 3200m;
			charge6.B4_ChargeType = ChargeTypeList.Codes.VAT;

			statement.B2_StatementType = StatementHeaderTypeList.Codes.MonthlyReceipt;
			AssertEquals(2500m, statement.TotalVATBaseAmountForVATReport);

			statement.B2_StatementType = StatementHeaderTypeList.Codes.IndividualCollectionReceipt;
			AssertEquals(2500m, statement.TotalVATBaseAmountForVATReport);

			statement.B2_StatementType = StatementHeaderTypeList.Codes.Normal;
			AssertEquals(0m, statement.TotalVATBaseAmountForVATReport);

			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			AssertEquals(0m, statement.TotalVATBaseAmountForVATReport);

			statement.B2_StatementType = StatementHeaderTypeList.Codes.NormalReport;
			AssertEquals(0m, statement.TotalVATBaseAmountForVATReport);
		}

		public void TestTaxProxyFields()
		{
			var statement = Factory.New<CusStatementHeader>();
			SetLineCharges(statement);

			AssertEquals(111111m, statement.TotalDutyAmount);
			AssertEquals(222222m, statement.TotalLiquorTax);
			AssertEquals(333333m, statement.TotalAgricultureTax);
			AssertEquals(444444m, statement.TotalTransportationTax);
			AssertEquals(555555m, statement.TotalEducationTax);
			AssertEquals(666666m, statement.TotalPenaltyAndInterest);
			AssertEquals(777777m, statement.TotalSpecialConsumptionTax);
			AssertEquals(888888m, statement.TotalVAT);
			AssertEquals(999999m, statement.TotalPenaltyForLatePayment);
		}

		void SetLineCharges(CusStatementHeader statement)
		{
			var statementLine1 = statement.StatementLines.AddNew();
			statementLine1.B3_EntryNum = "1";
			statementLine1.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			var dtyAmount = 111111m;
			var lqtAmount = 222222m;
			var agtAmount = 333333m;
			var trtAmount = 444444m;
			var edtAmount = 555555m;
			var pltAmount = 666666m;
			var sctAmount = 777777m;
			var vatAmount = 888888m;
			var pmtAmount = 999999m;
			var chargeDTY = statementLine1.Charges.AddNew();
			chargeDTY.B4_ChargeAmount = dtyAmount;
			chargeDTY.B4_ChargeType = ChargeTypeList.Codes.Duty;
			var chargeLQT = statementLine1.Charges.AddNew();
			chargeLQT.B4_ChargeAmount = lqtAmount;
			chargeLQT.B4_ChargeType = ChargeTypeList.Codes.LiquorTax;
			var chargeAGT = statementLine1.Charges.AddNew();
			chargeAGT.B4_ChargeAmount = agtAmount;
			chargeAGT.B4_ChargeType = ChargeTypeList.Codes.AgricultureTax;
			var chargeTRT = statementLine1.Charges.AddNew();
			chargeTRT.B4_ChargeAmount = trtAmount;
			chargeTRT.B4_ChargeType = ChargeTypeList.Codes.TransportationTax;
			var chargeEDT = statementLine1.Charges.AddNew();
			chargeEDT.B4_ChargeAmount = edtAmount;
			chargeEDT.B4_ChargeType = ChargeTypeList.Codes.EducationTax;
			var chargePLT = statementLine1.Charges.AddNew();
			chargePLT.B4_ChargeAmount = pltAmount;
			chargePLT.B4_ChargeType = ChargeTypeList.Codes.PenaltyAndInterest;
			var chargeSCT = statementLine1.Charges.AddNew();
			chargeSCT.B4_ChargeAmount = sctAmount;
			chargeSCT.B4_ChargeType = ChargeTypeList.Codes.SpecialConsumptionTax;
			var chargeVAT = statementLine1.Charges.AddNew();
			chargeVAT.B4_ChargeAmount = vatAmount;
			chargeVAT.B4_ChargeType = ChargeTypeList.Codes.VAT;
			var chargePMT = statementLine1.Charges.AddNew();
			chargePMT.B4_ChargeAmount = pmtAmount;
			chargePMT.B4_ChargeType = ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration;
		}

		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
