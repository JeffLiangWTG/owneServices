using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryHeaderCustomsLinkedObjectAdapterConstructorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("entryHeader required", () => new CusEntryHeaderCustomsLinkedObjectAdapter(null));
		AssertExceptionThrown<ArgumentNullException>("entryHeader.Declaration required", () => new CusEntryHeaderCustomsLinkedObjectAdapter(Factory.New<CusEntryHeader>()));
		AssertNoExceptionThrown("Valid entryHeader", () => new CusEntryHeaderCustomsLinkedObjectAdapter(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew()));
	}
}

sealed class CusEntryHeader_CustomsLinkedObjectAdapterTest : CustomsLinkedObjectAdapterTest<CusEntryHeader>
{
	public override void TestAddMessage()
	{
		AssertExceptionThrown<ArgumentNullException>(() => Adapter.AddMessage(null));

		AssertEquals("PRE-CONDITION", 0, Adaptee.Messages.Count);

		Adapter.AddMessage(Factory.New<ITEDIMessage>());
		AssertEquals("POST-CONDITION", 1, Adaptee.Messages.Count);
	}

	public override void TestGetLastSuccessfullySentMessage()
	{
		CombineAssertions("When declaration is not UCC6", () =>
		{
			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				AssertNull("No sent idoc messages", Adapter.GetLastSuccessfullySentMessage());

				var message1 = Adaptee.Messages.AddNew();
				message1.IsTransmitMessage = false;
				AssertNull("No sent idoc transmit messages", Adapter.GetLastSuccessfullySentMessage());

				message1.IsTransmitMessage = true;
				message1.EM_Status = EDIMessageStatusList.Codes.Queued;
				AssertNull("One queued idoc transmit message", Adapter.GetLastSuccessfullySentMessage());

				message1.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
				message1.EM_Status = EDIMessageStatusList.Codes.Sent;
				message1.EM_SystemCreateTimeUtc = new ZDateTime(2021, 01, 01, 10, 10, 10);
				AssertNotNull("One sent idoc transmit message", Adapter.GetLastSuccessfullySentMessage());
				AssertSame($"Expected {nameof(message1)}", message1, Adapter.GetLastSuccessfullySentMessage());

				var message2 = Adaptee.Messages.AddNew();
				message2.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
				message2.IsTransmitMessage = true;
				message2.EM_Status = EDIMessageStatusList.Codes.Sent;
				message2.EM_SystemCreateTimeUtc = new ZDateTime(2021, 01, 31, 10, 10, 10);

				var messageSWR = Adaptee.Messages.AddNew();
				messageSWR.EM_MessageType = MessageProcessorConstants.InterchangeTypes.SingleWindowRequest;
				message2.IsTransmitMessage = true;
				message2.EM_Status = EDIMessageStatusList.Codes.Sent;
				message2.EM_SystemCreateTimeUtc = new ZDateTime(2021, 01, 31, 11, 10, 10);
				AssertNotNull("Last sent idoc transmit message", Adapter.GetLastSuccessfullySentMessage());
				AssertSame($"Expected {nameof(message2)}", message2, Adapter.GetLastSuccessfullySentMessage());
			}
		});

		CombineAssertions("When declaration is UCC6", () =>
		{
			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1;
				Adaptee.CH_CEI_Instruction = entryInstruction.PK;

				AssertNull("No sent H1 messages", Adapter.GetLastSuccessfullySentMessage());

				var message1 = Adaptee.Messages.AddNew();
				message1.IsTransmitMessage = false;
				AssertNull("No sent H1 transmit messages", Adapter.GetLastSuccessfullySentMessage());

				message1.IsTransmitMessage = true;
				message1.EM_Status = EDIMessageStatusList.Codes.Queued;
				AssertNull("One queued transmit message", Adapter.GetLastSuccessfullySentMessage());

				message1.EM_MessageSubType = ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1;
				message1.EM_Status = EDIMessageStatusList.Codes.Sent;
				message1.EM_SystemCreateTimeUtc = new ZDateTime(2021, 01, 01, 10, 10, 10);
				AssertNotNull("One sent H1 transmit message", Adapter.GetLastSuccessfullySentMessage());
				AssertSame($"Expected {nameof(message1)}", message1, Adapter.GetLastSuccessfullySentMessage());

				entryInstruction.CEI_Style = ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeDepositoDoganaleH2;
				AssertNull("No sent H2 transmit messages", Adapter.GetLastSuccessfullySentMessage());
			}
		});
	}

	public override void TestEntryReferenceNumber()
	{
		Adaptee.CH_BGMReference = "BBB";
		AssertEquals(Adaptee.CH_BGMReference, Adapter.EntryReferenceNumber);
	}

	public override void TestJobReferenceNumber()
	{
		Adaptee.Declaration.JE_DeclarationReference = "CCC";
		AssertEquals(Adaptee.Declaration.JE_DeclarationReference, Adapter.JobReferenceNumber);
	}

	public override void TestFactory()
	{
		AssertNotNull("Not null", Adapter.Factory);
		AssertSame("Same", Adaptee.Factory, Adapter.Factory);
	}

	public override void TestCustomsProfile()
	{
		Adaptee.Declaration.JE_CustomsProfile = "12345";
		AssertEquals(Adaptee.Declaration.JE_CustomsProfile, Adapter.CustomsProfile);
	}

	public void TestLogClearedEventForAmendedEntry()
	{
		Adaptee.CH_EntryStatus = "AMD";
		Adaptee.CH_Status = "ACO";

		Adaptee.Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var adapter = Adapter as IXmlCustomsLinkedObjectAdapter;
		adapter.SetStatusAsCleared(ZDateTime.Now);
		AssertEquals("CH_EntryStatus does not change from AMD to ICC, AMD expected", "AMD", Adaptee.CH_EntryStatus);

		Factory.Save();

		Assert("ICC line for Entry Header expected", Adaptee.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CES" && x.SL_Reference == "ICC"));
	}

	public void TestLogClearedEventIsWrittenOnlyOnce()
	{
		Adaptee.CH_EntryStatus = "REG";
		Adaptee.CH_Status = "ACO";

		declaration.JE_MessageType = "EXP";
		var adapter = Adapter as IXmlCustomsLinkedObjectAdapter;

		Factory.Save();
		AssertEquals("No ECC line for Entry Header expected", 0, GetEccLogsCount());

		adapter.SetStatusAsCleared(ZDateTime.Now);
		Factory.Save();

		AssertEquals("CH_EntryStatus ECC expected", "ECC", Adaptee.CH_EntryStatus);
		AssertEquals("One only ECC line for Entry Header expected", 1, GetEccLogsCount());

		int GetEccLogsCount() => Adaptee.Logs
			.Find(x => x.SL_SE_NKEvent == "CES" && x.SL_Reference == "ECC")
			.Count();
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;

	protected override CusEntryHeader GetAdaptee() => declaration.CustomsEntryHeaders.AddNew();

	protected override ICustomsLinkedObjectAdapter GetAdapter() => new CusEntryHeaderCustomsLinkedObjectAdapter(Adaptee);
}

sealed class CusEntryHeader_SadCustomsLinkedObjectAdapterTest : SadCustomsLinkedObjectAdapterTest<CusEntryHeader>
{
	public override void TestCustomsLines()
	{
		AssertEquals("No customs lines", 0, Adapter.CustomsLines.Count());

		Adaptee.MergedLines.AddNew();
		Adaptee.MergedLines.AddNew();
		AssertEquals("Two customs lines", 2, Adapter.CustomsLines.Count());
		AssertSame("Cached", Adapter.CustomsLines, Adapter.CustomsLines);
		AssertType<CusEntryLineCustomsLineLinkedObjectAdapter>("Type", Adapter.CustomsLines.First());
	}

	public override void TestEntryCustomsStatus()
	{
		Adaptee.CH_EntryStatus = "BBB";
		AssertEquals(Adaptee.CH_EntryStatus, Adapter.EntryCustomsStatus);
	}

	public override void TestStatusProvider()
	{
		CombineAssertions(() =>
		{
			var provider = Adapter.StatusProvider;
			AssertType<CusEntryHeaderCustomsStatusProvider>($"{nameof(Adapter.StatusProvider)} type", provider);
			AssertSame($"{nameof(Adapter.StatusProvider)} cached", provider, Adapter.StatusProvider);
		});
	}

	public override void TestFactory()
	{
		AssertSame(Adaptee.Factory, Adapter.Factory);
	}

	public override void TestGetEntryNumbers()
	{
		var entryNumbers = Adapter.GetEntryNumbers();
		AssertEquals("PRE-CONDITION", 0, entryNumbers.Count());

		var entryNumber1 = Adapter.GetNewCusEntryNumber();
		entryNumber1.CE_SystemCreateTimeUtc = new ZDateTime(2021, 02, 01);

		var entryNumber2 = Adapter.GetNewCusEntryNumber();
		entryNumber2.CE_SystemCreateTimeUtc = new ZDateTime(2020, 01, 01);

		entryNumbers = Adapter.GetEntryNumbers();
		AssertArrayEqualsByElements("POST-CONDITION: expected 2 elements ordered by CE_SystemCreateTimeUtc", new CusEntryNumber[] { entryNumber2, entryNumber1 }, entryNumbers.ToArray());
	}

	public override void TestGetNewCusEntryNumber()
	{
		var cusEntryNumber = Adapter.GetNewCusEntryNumber();

		AssertNotNull("Not null", cusEntryNumber);
		CombineAssertions(() =>
		{
			AssertEquals("CE_ParentID", Adaptee.PK, cusEntryNumber.CE_ParentID);
			AssertEquals("CE_ParentTable", Adaptee.TableName, cusEntryNumber.CE_ParentTable);
			AssertEquals("CE_Category", CusEntryNumber.Categories.CustomsPermitClearanceNumber, cusEntryNumber.CE_Category);
			AssertEquals("CE_RN_NKCountryCode", Core.Constants.CountryCodes.Italy, cusEntryNumber.CE_RN_NKCountryCode);
		});
	}

	public override void TestInsertOrUpdateA93Numbers()
	{
		AssertEquals("PRE-CONDITION", 0, Adaptee.EntryPayInfos.Count);

		var entryLine = Adaptee.MergedLines.AddNew();
		entryLine.Fees.AddOrUpdate("A00", 100m).CF_MethodOfPayment = "A";
		entryLine.Fees.AddOrUpdate("A10", 25m).CF_MethodOfPayment = "A";
		entryLine.Fees.AddOrUpdate("A20", 34.54m).CF_MethodOfPayment = "B";

		var regCusEntryNum = CreateCusEntryNumber(Adaptee, CusEntryNumberConstants.EntryTypes.RegistrationNumber);
		regCusEntryNum.CE_EntryNum = "4";

		var entryPaymentsMock = new Mock<ISadPositiveResponseMessageA93EntryPayments>();
		entryPaymentsMock.Setup(m => m.A93Number).Returns("123456");
		entryPaymentsMock.Setup(m => m.HasA93FirstPayment).Returns(true);
		entryPaymentsMock.Setup(m => m.FirstPaymentDueDate).Returns(new ZDate(2021, 01, 01));
		entryPaymentsMock.Setup(m => m.FirstPaymentMethod).Returns("A");
		entryPaymentsMock.Setup(m => m.HasA93SecondPayment).Returns(true);
		entryPaymentsMock.Setup(m => m.SecondPaymentDueDate).Returns(new ZDate(2021, 01, 02));
		entryPaymentsMock.Setup(m => m.SecondPaymentMethod).Returns("B");
		entryPaymentsMock.Setup(m => m.HasA93ThirdPayment).Returns(true);
		entryPaymentsMock.Setup(m => m.ThirdPaymentDueDate).Returns(new ZDate(2021, 01, 03));
		entryPaymentsMock.Setup(m => m.ThirdPaymentMethod).Returns("C");
		Adapter.InsertOrUpdateA93Numbers(entryPaymentsMock.Object);

		var a93EntryNumbers = Adaptee.EntryPayInfos.Cast<CusEntryPayInfo>().OrderBy(x => x.C9_PaymentParty).ToArray();
		AssertEquals("EntryPayInfo Count", 3, a93EntryNumbers.Length);

		CombineAssertions("EntryPayInfo at 0", () => a93EntryNumbers[0].AssertEntryPayInfo("123456", "A", 125m, "4", new ZDateTime(2021, 01, 01), Customs.Business.CusEntryPayInfoStatusList.Codes.Pending));
		CombineAssertions("EntryPayInfo at 1", () => a93EntryNumbers[1].AssertEntryPayInfo("123456", "B", 34.54m, "4", new ZDateTime(2021, 01, 02), Customs.Business.CusEntryPayInfoStatusList.Codes.Pending));
		CombineAssertions("EntryPayInfo at 2", () => a93EntryNumbers[2].AssertEntryPayInfo("123456", "C", 0m, "4", new ZDateTime(2021, 01, 03), Customs.Business.CusEntryPayInfoStatusList.Codes.Pending));
	}

	public override void TestIrildesCusEntryNum()
	{
		AssertNull("PRE-CONDITION", Adapter.IrildesCusEntryNum);

		CreateCusEntryNumber(Adaptee, CusEntryNumberConstants.EntryTypes.Irildes);
		AssertNotNull("POST-CONDITION", Adapter.IrildesCusEntryNum);
	}

	public override void TestIsEntryRegisteredOrNbRejected()
	{
		Adaptee.CH_EntryStatus = ZString.Empty;
		Assert("Empty", !Adapter.IsEntryRegisteredOrNbRejected);

		Adaptee.CH_EntryStatus = ITEntryStatusList.Codes.Registered;
		Assert(ITEntryStatusList.Codes.Registered, Adapter.IsEntryRegisteredOrNbRejected);

		Adaptee.CH_EntryStatus = ITEntryStatusList.Codes.ImportCleared;
		Assert(ITEntryStatusList.Codes.ImportCleared, !Adapter.IsEntryRegisteredOrNbRejected);

		Adaptee.CH_EntryStatus = ITEntryStatusList.Codes.NbRejected;
		Assert(ITEntryStatusList.Codes.NbRejected, Adapter.IsEntryRegisteredOrNbRejected);
	}

	public override void TestIsExport()
	{
		Adaptee.Declaration.JE_MessageType = ZString.Empty;
		Assert("Empty", !Adapter.IsExport);

		Adaptee.Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		Assert(EUJobMessageTypeList.Codes.Export, Adapter.IsExport);

		Adaptee.Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		Assert(EUJobMessageTypeList.Codes.Import, !Adapter.IsExport);

		Adaptee.Declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		Assert(EUJobMessageTypeList.Codes.MiscellaneousCustoms, !Adapter.IsExport);

		Adaptee.Declaration.JE_MessageType = "XXX";
		Assert("Invalid Type", !Adapter.IsExport);
	}

	public override void TestIsImport()
	{
		Adaptee.Declaration.JE_MessageType = ZString.Empty;
		Assert("Empty", !Adapter.IsImport);

		Adaptee.Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		Assert(EUJobMessageTypeList.Codes.Export, !Adapter.IsImport);

		Adaptee.Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		Assert(EUJobMessageTypeList.Codes.Import, Adapter.IsImport);

		Adaptee.Declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		Assert(EUJobMessageTypeList.Codes.MiscellaneousCustoms, !Adapter.IsImport);

		Adaptee.Declaration.JE_MessageType = "XXX";
		Assert("Invalid Type", !Adapter.IsImport);
	}

	public override void TestIsIncomingMessageAlreadyLinked()
	{
		Assert("Not linked", !Adapter.IsIncomingMessageAlreadyLinked("A"));

		Adaptee.Messages.AddNew().EM_MessageType = "A";
		Assert("Already linked", Adapter.IsIncomingMessageAlreadyLinked("A"));
	}

	public override void TestIvistoCusEntryNum()
	{
		AssertNull("PRE-CONDITION", Adapter.IvistoCusEntryNum);

		CreateCusEntryNumber(Adaptee, CusEntryNumberConstants.EntryTypes.Ivisto);
		AssertNotNull("POST-CONDITION", Adapter.IvistoCusEntryNum);
	}

	public override void TestMessageStatus()
	{
		Adaptee.CH_Status = "AAA";
		AssertEquals(Adaptee.CH_Status, Adapter.MessageStatus);
	}

	public override void TestMrn()
	{
		Adaptee.MovementReferenceNumberSetter("XXX");
		AssertEquals(Adaptee.MovementReferenceNumber, Adapter.Mrn);
	}

	public override void TestSetEntryReleaseDate()
	{
		AssertEquals("PRE-CONDITION", ZDateTime.Empty, Adaptee.CH_EntryReleaseDate);

		var testDate = ZDateTime.Now;

		Adapter.SetEntryReleaseDate(testDate);
		AssertEquals("POST-CONDITION", testDate, Adaptee.CH_EntryReleaseDate);
	}

	public override void TestSetEntryCustomsStatus()
	{
		AssertEquals("PRE-CONDITION", ZString.Empty, Adaptee.CH_EntryStatus);

		Adapter.SetEntryCustomsStatus("AAA");
		AssertEquals("POST-CONDITION", "AAA", Adaptee.CH_EntryStatus);
	}

	public override void TestSetMessageStatus()
	{
		AssertEquals("PRE-CONDITION", ZString.Empty, Adaptee.CH_Status);

		Adapter.SetMessageStatus("AAA");
		AssertEquals("POST-CONDITION", "AAA", Adaptee.CH_Status);
	}

	public override void TestSingleWindowRequestDataProvider()
	{
		var singleWindowRequestDataProvider = Adapter.SingleWindowRequestDataProvider;
		AssertNotNull("Not null", singleWindowRequestDataProvider);
		AssertType<CusEntryHeader>("Type", singleWindowRequestDataProvider);
	}

	public override void TestUpdatePendingGuaranteeTransactions()
	{
		AssertNoExceptionThrown("Doing nothing and should not throw any exception", () => Adapter.UpdatePendingGuaranteeTransactions(transactionsNewStatus: ""));
	}

	public override void TestWriteOffGuarantee()
	{
		AssertNoExceptionThrown("Doing nothing and should not throw any exception", () => Adapter.WriteOffGuarantee(applicationId: "", transactionDate: ZDate.Empty));
	}

	protected override CusEntryHeader GetAdaptee() => Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();

	protected override ISadCustomsLinkedObjectAdapter GetAdapter() => new CusEntryHeaderCustomsLinkedObjectAdapter(Adaptee);

	CusEntryNumber CreateCusEntryNumber(CusEntryHeader entryHeader, ZString entryType)
	{
		var cusEntryNumber = Factory.New<CusEntryNumber>();
		cusEntryNumber.CE_EntryType = entryType;
		cusEntryNumber.CE_ParentID = entryHeader.PK;
		cusEntryNumber.CE_ParentTable = entryHeader.TableName;
		cusEntryNumber.CE_Category = "CUS";
		return cusEntryNumber;
	}
}

sealed class CusEntryHeader_SingleWindowCustomsLinkedObjectAdapterTest : SingleWindowCustomsLinkedObjectAdapterTest<CusEntryHeader>
{
	public override void TestAddLog()
	{
		AssertNull("PRE-CONDITION", GetStatusUpdatedLog());

		var eventParameters = new KeyValuePair<string, string>[]
		{
			new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Service, "CCC"),
			new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "XXX"),
		};

		Adapter.AddLog(Events.StatusUpdated, new ZDateTime(2021, 01, 01), eventParameters);

		var statusUpdatedLog = GetStatusUpdatedLog();
		AssertEquals("Log Reference", $"|SER=CCC|TYP=XXX", statusUpdatedLog.SL_Reference);

		StmALog GetStatusUpdatedLog() => Adaptee.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
	}

	public override void TestDocManagerInfo()
	{
		AssertNotNull("Not null", Adapter.DocManagerInfo);
		AssertSame("Same", Adaptee.Declaration.DocManagerInfo, Adapter.DocManagerInfo);
	}

	public override void TestInsertOrUpdateReleaseCode()
	{
		AssertNull("PRE-CONDITION", GetCLREntryNumber());

		Adapter.InsertOrUpdateReleaseCode("ABCDEF", new ZDateTime(2021, 01, 01));
		CombineAssertions("POST-CONDITION", () =>
		{
			var releaseCodeEntryNumber = GetCLREntryNumber();
			AssertEquals("ReleaseCode", "ABCDEF", releaseCodeEntryNumber.CE_EntryNum);
			AssertEquals("ReleaseDate", new ZDateTime(2021, 01, 01), releaseCodeEntryNumber.CE_IssueDate);
		});

		CusEntryNumber GetCLREntryNumber()
		{
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, Adaptee.PK);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberConstants.EntryTypes.ClereanceCode);
			return Factory.LoadTop1<CusEntryNumber>(query);
		}
	}

	public override void TestSetEntryCustomsChannel()
	{
		AssertEquals("PRE-CONDITION", ZString.Empty, Adaptee.CustomsChannel);

		Adapter.SetEntryCustomsChannel("AA");
		AssertEquals("POST-CONDITION", "AA", Adaptee.CustomsChannel);
	}

	public override void TestSetEntryAsCleared()
	{
		Adaptee.Declaration.JE_MessageType = ZString.Empty;
		Adapter.SetEntryAsCleared(ZDateTime.Empty);
		AssertEquals("Empty", ITEntryStatusList.Codes.ExportCleared, Adaptee.CH_EntryStatus);
		AssertReleaseDate("Empty", ZDateTime.Empty);

		Adaptee.CH_EntryStatus = "";
		var testDate = ZDateTime.Now;
		Adaptee.Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		Adapter.SetEntryAsCleared(testDate);
		AssertEquals(EUJobMessageTypeList.Codes.Import, ITEntryStatusList.Codes.ImportCleared, Adaptee.CH_EntryStatus);
		AssertReleaseDate(EUJobMessageTypeList.Codes.Import, testDate);

		Adaptee.CH_EntryStatus = "";
		Adaptee.Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		Adapter.SetEntryAsCleared(testDate);
		AssertEquals(EUJobMessageTypeList.Codes.Export, ITEntryStatusList.Codes.ExportCleared, Adaptee.CH_EntryStatus);
		AssertReleaseDate(EUJobMessageTypeList.Codes.Export, testDate);

		Adaptee.CH_EntryStatus = "";
		Adaptee.Declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		Adapter.SetEntryAsCleared(testDate);
		AssertEquals(EUJobMessageTypeList.Codes.MiscellaneousCustoms, ITEntryStatusList.Codes.ExportCleared, Adaptee.CH_EntryStatus);
		AssertReleaseDate(EUJobMessageTypeList.Codes.MiscellaneousCustoms, testDate);

		Adaptee.CH_EntryStatus = "";
		Adaptee.Declaration.JE_MessageType = "XXX";
		Adapter.SetEntryAsCleared(testDate);
		AssertEquals("Invalid declaration type", ITEntryStatusList.Codes.ExportCleared, Adaptee.CH_EntryStatus);
		AssertReleaseDate("Invalid declaration type", testDate);
	}

	public override void TestIsEntryCleared()
	{
		Adaptee.CH_Status = ZString.Empty;
		Assert("Empty", !Adapter.IsEntryCleared);

		Adaptee.CH_Status = ITMessageStatusList.Codes.NotSent;
		Assert(ITMessageStatusList.Codes.NotSent, !Adapter.IsEntryCleared);

		Adaptee.CH_Status = ITMessageStatusList.Codes.ErrorOriginal;
		Assert(ITMessageStatusList.Codes.ErrorOriginal, !Adapter.IsEntryCleared);

		Adaptee.CH_Status = ITMessageStatusList.Codes.AcknowledgedOriginal;
		Assert(ITMessageStatusList.Codes.AcknowledgedOriginal, !Adapter.IsEntryCleared);

		Adaptee.CH_Status = ITMessageStatusList.Codes.AwaitingOriginal;
		Assert(ITMessageStatusList.Codes.AwaitingOriginal, !Adapter.IsEntryCleared);

		Adaptee.CH_Status = ITMessageStatusList.Codes.ClearOriginal;
		Assert(ITMessageStatusList.Codes.ClearOriginal, Adapter.IsEntryCleared);

		Adaptee.CH_Status = "XXX";
		Assert("Invalid type", !Adapter.IsEntryCleared);
	}

	void AssertReleaseDate(ZString assertMessagePrefix, ZDateTime releaseDateTime)
	{
		AssertEquals($"{assertMessagePrefix} Release Date", releaseDateTime, Adaptee.CH_EntryReleaseDate);
	}

	protected override CusEntryHeader GetAdaptee() => Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();

	protected override ISingleWindowCustomsLinkedObjectAdapter GetAdapter() => new CusEntryHeaderCustomsLinkedObjectAdapter(Adaptee);
}

sealed class CusEntryHeader_Ucc6CustomsLinkedObjectAdapterTest : XmlCustomsLinkedObjectAdapterTest<CusEntryHeader>
{
	public override void TestCustomsOfficeOfPresentation()
	{
		Adaptee.Declaration.JE_CustomsOffice = "IT12345";
		AssertEquals(nameof(Adapter.CustomsOfficeOfPresentation), "IT12345", Adapter.CustomsOfficeOfPresentation);
	}

	public override void TestGetAllEntryLines()
	{
		var expectedEntryLines = new[] { Adaptee.MergedLines.AddNew(), Adaptee.MergedLines.AddNew() };
		AssertArrayEqualsByElements(nameof(Adapter.GetAllEntryLines), expectedEntryLines, Adapter.GetAllEntryLines().ToArray());
	}

	public override void TestGetAllRelatedEntryNumbers()
	{
		var entryNumbers = Adapter.GetAllRelatedEntryNumbers();
		AssertEquals("PRE-CONDITION", 0, entryNumbers.Count());

		var entryNumber1 = Factory.New<CusEntryNumber>();
		entryNumber1.CE_ParentID = Adaptee.PK;
		entryNumber1.CE_ParentTable = Adaptee.TableName;
		entryNumber1.CE_SystemCreateTimeUtc = new ZDateTime(2021, 02, 01);

		var entryNumber2 = Factory.New<CusEntryNumber>();
		entryNumber2.CE_ParentID = Adaptee.PK;
		entryNumber2.CE_ParentTable = Adaptee.TableName;
		entryNumber2.CE_SystemCreateTimeUtc = new ZDateTime(2020, 01, 01);

		entryNumbers = Adapter.GetAllRelatedEntryNumbers();
		AssertArrayEqualsByElements("POST-CONDITION: expected 2 elements ordered by CE_SystemCreateTimeUtc", new CusEntryNumber[] { entryNumber2, entryNumber1 }, entryNumbers.ToArray());
	}

	public override void TestIsAwaitingMessage()
	{
		Adaptee.CH_Status = "";
		AssertEquals(nameof(Adapter.IsAwaitingMessage), false, Adapter.IsAwaitingMessage);

		Adaptee.CH_Status = "AWO";
		AssertEquals(nameof(Adapter.IsAwaitingMessage), true, Adapter.IsAwaitingMessage);
	}

	public override void TestSetStatusAsCleared()
	{
		CombineAssertions("[PRE-CONDITION]", () =>
		{
			AssertEquals("Entry Status", "", Adaptee.CH_EntryStatus);
			AssertEquals("Status", "", Adaptee.CH_Status);
			AssertEquals("Entry Release Date", ZDateTime.Empty, Adaptee.CH_EntryReleaseDate);
		});

		var now = ZDateTime.Now;
		var nextYear = now.AddYears(1);

		Adaptee.Declaration.JE_MessageType = "IMP";
		Adapter.SetStatusAsCleared(now);
		CombineAssertions(() =>
		{
			AssertEquals("Entry Status", "ICC", Adaptee.CH_EntryStatus);
			AssertEquals("Status", "CLO", Adaptee.CH_Status);
			AssertEquals("Entry Release Date", now, Adaptee.CH_EntryReleaseDate);
		});

		Adapter.SetStatusAsCleared(nextYear);
		CombineAssertions("Entry Status Safe Set", () =>
		{
			AssertEquals("Entry Status", "ICC", Adaptee.CH_EntryStatus);
			AssertEquals("Status", "CLO", Adaptee.CH_Status);
			AssertEquals("Entry Release Date", nextYear, Adaptee.CH_EntryReleaseDate);
		});
	}

	public void TestSetStatusAsClearedForAutomaticIvistoMessageWithUCC6()
	{
		var declaration = Adaptee.Declaration;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		Adaptee.MovementReferenceNumberSetter("MRN12345");
		Factory.NewCusEntryNumber(Adaptee, "CLR", "XYZ123", ZDateTime.Now);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			Adapter.SetStatusAsCleared(ZDateTime.Now);

			Adaptee.Messages.Reload(reLoadExistingRows: false);
			var ivistoMessage = Adaptee.Messages.GetLastMessageByType("IVI");
			AssertNotNull("Ivisto Message", ivistoMessage);
		}
	}

	public void TestSetStatusAsClearedForAutomaticIvistoMessageWithoutUCC6()
	{
		var declaration = Adaptee.Declaration;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		Adaptee.MovementReferenceNumberSetter("MRN12345");
		Factory.NewCusEntryNumber(Adaptee, "CLR", "XYZ123", ZDateTime.Now);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			Adapter.SetStatusAsCleared(ZDateTime.Now);

			Adaptee.Messages.Reload(reLoadExistingRows: false);
			var ivistoMessage = Adaptee.Messages.GetLastMessageByType("IVI");
			AssertNull("Ivisto Message", ivistoMessage);
		}
	}

	public override void TestSetStatusAsRegistered()
	{
		AssertEquals("[PRE-CONDITION] Entry Status", "", Adaptee.CH_EntryStatus);

		Adapter.SetStatusAsRegistered(ZDateTime.BrettsBirthday);
		AssertEquals("Entry Status", "REG", Adaptee.CH_EntryStatus);

		Adaptee.CH_EntryStatus = "UCL";
		Adapter.SetStatusAsRegistered(ZDateTime.BrettsBirthday);
		AssertEquals("Entry Status Safe Set", "UCL", Adaptee.CH_EntryStatus);
	}

	public override void TestSetStatusAsAcknowledged()
	{
		AssertEquals("[PRE-CONDITION] Status", "", Adaptee.CH_Status);

		Adapter.SetStatusAsAcknowledged();
		AssertEquals("Entry Status", "ACO", Adaptee.CH_Status);
	}

	public override void TestSetStatusAsFailedForTransmission()
	{
		AssertEquals("[PRE-CONDITION] Status", "", Adaptee.CH_Status);

		Adapter.SetStatusAsFailedForTransmission();
		AssertEquals("Entry Status", "FFT", Adaptee.CH_Status);
	}

	public override void TestSetStatusAsError()
	{
		AssertEquals("[PRE-CONDITION] Status", "", Adaptee.CH_Status);

		Adapter.SetStatusAsError();
		AssertEquals("Entry Status", "ERO", Adaptee.CH_Status);
	}

	public override void TestSetStatusAsCancelled()
	{
		AssertEquals("[PRE-CONDITION] Status", "", Adaptee.CH_EntryStatus);

		Adapter.SetStatusAsCancelled();
		AssertEquals(nameof(Adaptee.CH_EntryStatus), ITEntryStatusList.Codes.Canceled, Adaptee.CH_EntryStatus);
	}

	public override void TestSetStatusAsDeposited()
	{
		AssertEquals("[PRE-CONDITION] Status", "", Adaptee.CH_EntryStatus);

		Adapter.SetStatusAsDeposited();
		AssertEquals(nameof(Adaptee.CH_EntryStatus), ITEntryStatusList.Codes.Deposited, Adaptee.CH_EntryStatus);

		Adaptee.CH_EntryStatus = "REG";
		Adapter.SetStatusAsDeposited();
		AssertEquals("Entry Status Safe Set", "REG", Adaptee.CH_EntryStatus);
	}

	public override void TestSetStatusAsUnderControl()
	{
		AssertEquals("[PRE-CONDITION] Status", "", Adaptee.CH_EntryStatus);

		Adapter.SetStatusAsUnderControl();
		AssertEquals(nameof(Adaptee.CH_EntryStatus), ITEntryStatusList.Codes.UnderControl, Adaptee.CH_EntryStatus);

		Adaptee.CH_EntryStatus = "ECC";
		Adapter.SetStatusAsUnderControl();
		AssertEquals("Entry Status Safe Set", "ECC", Adaptee.CH_EntryStatus);
	}

	public override void TestSetStatusAsExitCompleted()
	{
		AssertEquals("[PRE-CONDITION] Status", "", Adaptee.CH_EntryStatus);

		Adapter.SetStatusAsExitCompleted();
		AssertEquals(nameof(Adaptee.CH_EntryStatus), ITEntryStatusList.Codes.Exit, Adaptee.CH_EntryStatus);

		Adaptee.CH_EntryStatus = "CNC";
		Adapter.SetStatusAsExitCompleted();
		AssertEquals("Entry Status Safe Set", "CNC", Adaptee.CH_EntryStatus);
	}

	public override void TestSetStatusAsGoodsWrittenOffClosed()
	{
		Adaptee.CH_BGMReference = "BGM";
		AssertExceptionThrown<CustomsMessageProcessorException>(
			"Trying call SetStatusAsGoodsWrittenOffClosed in CusEntryHeader",
			"Irildes is not supported by Entry Header [BGMReference: BGM]",
			() => Adapter.SetStatusAsGoodsWrittenOffClosed());
	}

	public void TestSetSentEntryLinesCount()
	{
		AssertEquals("Pre:ZG_SentEntryLines no lines added", 0, Adaptee.ZG_SentEntryLinesCount);

		Adaptee.MergedLines.AddNew();
		Adaptee.MergedLines.AddNew();
		Adapter.SetSentEntryLinesCount();
		AssertEquals("ZG_SentEntryLines", 2, Adaptee.ZG_SentEntryLinesCount);

		Adaptee.MergedLines.AddNew();
		Adapter.SetSentEntryLinesCount();
		AssertEquals("ZG_SentEntryLines", 3, Adaptee.ZG_SentEntryLinesCount);
	}

	public override void TestGetAllPaymentInfo()
	{
		var paymentInfos = Adapter.GetAllPaymentInfo();
		AssertEquals("PRE-CONDITION", 0, paymentInfos.Count);

		var payInfoOne = Adaptee.EntryPayInfos.AddNew();
		payInfoOne.C9_IncomingPayResponseNo = "123";

		var payInfoTwo = Adaptee.EntryPayInfos.AddNew();
		payInfoTwo.C9_IncomingPayResponseNo = "345";

		paymentInfos = Adapter.GetAllPaymentInfo();
		AssertArrayEqualsByElements("POST-CONDITION: expected 2 elements", new CusEntryPayInfo[] { payInfoOne, payInfoTwo }, paymentInfos.ToArray());
	}

	public override void TestSetStatusAsAmended()
	{
		AssertEquals("[PRE-CONDITION] Status", "", Adaptee.CH_EntryStatus);

		Adapter.SetStatusAsAmended();
		AssertEquals(nameof(Adaptee.CH_EntryStatus), ITEntryStatusList.Codes.Amended, Adaptee.CH_EntryStatus);
	}

	public override void TestUpdateOrInsertEntryNumber()
	{
		var entryNumbers = Adapter.GetAllRelatedEntryNumbers();
		AssertEquals("PRE-CONDITION", 0, entryNumbers.Count());

		var entryNumber = Adapter.UpdateOrInsertEntryNumber(CusEntryNumberConstants.EntryTypes.RegistrationNumber,
			"123",
			"TEST",
			ZDateTime.Now,
			null);

		var insertedEntry = CusEntryNumber.Load(Adaptee, CusEntryNumberConstants.EntryTypes.RegistrationNumber, "IT");
		AssertNotNull(nameof(CusEntryNumber), entryNumber);
		AssertNotNull(nameof(CusEntryNumber), insertedEntry);
		AssertEquals(nameof(CusEntryNumber.CE_EntryNum), insertedEntry.CE_EntryNum, entryNumber.CE_EntryNum);

		Adapter.UpdateOrInsertEntryNumber(CusEntryNumberConstants.EntryTypes.RegistrationNumber,
			"345",
			"TEST",
			ZDateTime.Now,
			null);

		var finalEntry = CusEntryNumber.Load(Adaptee, CusEntryNumberConstants.EntryTypes.RegistrationNumber, "IT");
		AssertEquals(nameof(CusEntryNumber.CE_EntryNum), "345", finalEntry.CE_EntryNum);

		AssertExceptionThrown<CustomsMessageProcessorException>("Unable to create a new Entry Number",
			codeToRun: () => Adapter.UpdateOrInsertEntryNumber(CusEntryNumberConstants.EntryTypes.RegistrationNumber,
				"19311",
				"ABCD",
				ZDateTime.Now,
				34));
	}

	public override void TestAddA93Number()
	{
		AssertEquals("[PRE-CONDITION] PaymentInfo Count", 0, Adaptee.EntryPayInfos.Count);

		var paymentDate = ZDateTime.Today;
		var calculatorMock = new Mock<IUcc6A93NumberAmountCalculator>();
		calculatorMock.Setup(u => u.CalculateAmount()).Returns(50m);
		var ucc6A93NumberPayment = new Mock<IUcc6A93NumberPayment>();
		ucc6A93NumberPayment.Setup(p => p.AmountCalculator).Returns(calculatorMock.Object);
		ucc6A93NumberPayment.Setup(p => p.PaymentDate).Returns(paymentDate);
		ucc6A93NumberPayment.Setup(p => p.PaymentResponseNo).Returns("34-1");
		ucc6A93NumberPayment.Setup(p => p.PaymentType).Returns("E");

		Adapter.AddA93Number(ucc6A93NumberPayment.Object);

		AssertEquals("[POST-CONDITION] PaymentInfo Count", 1, Adaptee.EntryPayInfos.Count);
		var payInfo = Adaptee.EntryPayInfos[0];
		CombineAssertions("[POST-CONDITION] Generated PaymentInfo", () =>
		{
			AssertEquals(nameof(payInfo.C9_IncomingPayResponseNo), "34-1", payInfo.C9_IncomingPayResponseNo);
			AssertEquals(nameof(payInfo.C9_PaymentDate), paymentDate, payInfo.C9_PaymentDate);
			AssertEquals(nameof(payInfo.C9_PaymentAmount), 50m, payInfo.C9_PaymentAmount);
			AssertEquals(nameof(payInfo.C9_TransactionType), "", payInfo.C9_TransactionType);
			AssertEquals(nameof(payInfo.C9_PaymentReference), "", payInfo.C9_PaymentReference);
			AssertEquals(nameof(payInfo.C9_PaymentParty), "E", payInfo.C9_PaymentParty);
			AssertEquals(nameof(payInfo.C9_PaymentStatus), "PEN", payInfo.C9_PaymentStatus);
			AssertEquals(nameof(payInfo.C9_CH), Adaptee.PK, payInfo.C9_CH);
		});

		Adaptee.EntryPayInfos.RemoveAndDeleteAll();
		Adapter.UpdateOrInsertEntryNumber("REG", "4 T-123456", "", ZDateTime.Now, null);
		Adapter.AddA93Number(ucc6A93NumberPayment.Object);

		AssertEquals("[POST-CONDITION 1] PaymentInfo Count", 1, Adaptee.EntryPayInfos.Count);
		payInfo = Adaptee.EntryPayInfos[0];
		AssertEquals($"[POST-CONDITION 1] {nameof(payInfo.C9_TransactionType)} when entry has REG CusEntryNum", "4 T", Adaptee.EntryPayInfos[0].C9_TransactionType);
	}

	public override void TestGetAllFees()
	{
		SetupDataForFees();
		var fees = Adapter.GetAllFees(f => f.MethodOfPayment == "E");
		AssertNotNull("Fees", fees);
		AssertEquals("Fees Count", 3, fees.Count);

		fees = Adapter.GetAllFees(f => f.MethodOfPayment == "F");
		AssertNotNull("Fees", fees);
		AssertEquals("Fees Count", 2, fees.Count);

		fees = Adapter.GetAllFees(f => f.MethodOfPayment == "G");
		AssertNotNull("Fees", fees);
		AssertEquals("Fees Count", 1, fees.Count);

		fees = Adapter.GetAllFees(null);
		AssertNotNull("Fees", fees);
		AssertEquals("Fees Count", 7, fees.Count);
	}

	public override void TestGetFeesForLine()
	{
		SetupDataForFees();

		var fees = Adapter.GetFeesForLine(10, f => f.MethodOfPayment == "E");
		AssertNotNull("Fees", fees);
		AssertEquals("Fees Count", 2, fees.Count);

		fees = Adapter.GetFeesForLine(10, f => f.MethodOfPayment == "F");
		AssertNotNull("Fees", fees);
		AssertEquals("Fees Count", 0, fees.Count);

		fees = Adapter.GetFeesForLine(20, f => f.MethodOfPayment == "F");
		AssertNotNull("Fees", fees);
		AssertEquals("Fees Count", 2, fees.Count);

		fees = Adapter.GetFeesForLine(20, f => f.MethodOfPayment == "E");
		AssertNotNull("Fees", fees);
		AssertEquals("Fees Count", 1, fees.Count);

		fees = Adapter.GetFeesForLine(20, null);
		AssertNotNull("Fees", fees);
		AssertEquals("Fees Count", 3, fees.Count);

		AssertExceptionThrown<CustomsMessageProcessorException>("Invalid Entry Line Number",
			expectedExceptionMessage: "Not able to find an Entry Line with a Line Number: 99",
			codeToRun: () => Adapter.GetFeesForLine(99, null));
	}

	public override void TestSetCustomsChannel()
	{
		AssertNullOrEmpty("[PRE-CONDITION] CustomsChannel", Adaptee.CustomsChannel);

		Adapter.SetCustomsChannel("VM");
		AssertEquals(nameof(Adaptee.CustomsChannel), "VM", Adaptee.CustomsChannel);
	}

	public override void TestIsDeposited()
	{
		Adaptee.CH_EntryStatus = "REG";
		AssertEquals("When CH_EntryStatus is not DEP, IsDeposited", false, Adapter.IsDeposited);

		Adaptee.CH_EntryStatus = "DEP";
		AssertEquals("When CH_EntryStatus is DEP, IsDeposited", true, Adapter.IsDeposited);
	}

	public override void TestGetUniqueTransactionIdentifierRequestContext()
	{
		AssertType<EntryHeaderUniqueTransactionIdentifierRequestContext>("Type", Adapter.GetUniqueTransactionIdentifierRequestContext("1234", "ABCDEF123456"));
	}

	public override void TestGetLastSuccessfullySentMessageForDepositedStatus()
	{
		AssertNull(nameof(Adapter.GetLastSuccessfullySentMessageForDepositedStatus), Adapter.GetLastSuccessfullySentMessageForDepositedStatus());

		var message1 = Adaptee.Messages.AddNew();
		message1.EM_MessageSubType = "H1";
		message1.EM_MessageType = "NEW";
		message1.IsTransmitMessage = true;
		message1.EM_Status = EDIMessageStatusList.Codes.Sent;
		AssertSame(nameof(Adapter.GetLastSuccessfullySentMessageForDepositedStatus), message1, Adapter.GetLastSuccessfullySentMessageForDepositedStatus());
	}

	public override void TestSetStatusAsAcceptedBySystem()
	{
		AssertEquals("[PRE-CONDITION] CH_Status", "", Adaptee.CH_Status);

		Adapter.SetStatusAsAcceptedBySystem();
		AssertEquals(nameof(Adaptee.CH_Status), "ACS", Adaptee.CH_Status);
	}

	public override void TestUpdateOrInsertIvistoEntryNumber()
	{
		var ivistoEntryNumber = Adaptee.EntryNumbersProvider.Ivisto;
		AssertNull("Ivisto EntryNumber not found", ivistoEntryNumber);

		Adapter.UpdateOrInsertIvistoEntryNumber(new ZDateTime(2023, 01, 01), "IT123456", "EXC");
		ivistoEntryNumber = Adaptee.EntryNumbersProvider.Ivisto;
		CombineAssertions("Ivisto EntryNumber is created", () => AssertEntryNumber(ivistoEntryNumber, "IVI", "IT123456", new ZDateTime(2023, 01, 01), "EXC"));

		Adapter.UpdateOrInsertIvistoEntryNumber(new ZDateTime(2023, 01, 02), "IT999999", "EXR");
		ivistoEntryNumber = Adaptee.EntryNumbersProvider.Ivisto;
		CombineAssertions("Ivisto EntryNumber is updated (as it is newer than the current)", () => AssertEntryNumber(ivistoEntryNumber, "IVI", "IT999999", new ZDateTime(2023, 01, 02), "EXR"));

		Adapter.UpdateOrInsertIvistoEntryNumber(new ZDateTime(2022, 12, 31), "IT888888", "EXD");
		ivistoEntryNumber = Adaptee.EntryNumbersProvider.Ivisto;
		CombineAssertions("Ivisto EntryNumber is not updated (as it is older than the current)", () => AssertEntryNumber(ivistoEntryNumber, "IVI", "IT999999", new ZDateTime(2023, 01, 02), "EXR"));

		Adapter.UpdateOrInsertIvistoEntryNumber(ZDateTime.Empty, "IT111111", "EXC");
		ivistoEntryNumber = Adaptee.EntryNumbersProvider.Ivisto;
		CombineAssertions("Ivisto EntryNumber is not updated (as new date is empty)", () => AssertEntryNumber(ivistoEntryNumber, "IVI", "IT999999", new ZDateTime(2023, 01, 02), "EXR"));

		ivistoEntryNumber.CE_IssueDate = ZDateTime.Empty;
		Adapter.UpdateOrInsertIvistoEntryNumber(new ZDateTime(2022, 12, 31), "IT888888", "EXD");
		ivistoEntryNumber = Adaptee.EntryNumbersProvider.Ivisto;
		CombineAssertions("Ivisto EntryNumber is updated (as previous date was empty)", () => AssertEntryNumber(ivistoEntryNumber, "IVI", "IT888888", new ZDateTime(2022, 12, 31), "EXD"));
	}

	public override void TestUpdateOrInsertIrildesEntryNumber()
	{
		Adaptee.CH_BGMReference = "BGM";
		AssertExceptionThrown<CustomsMessageProcessorException>(
			"Trying call UpdateOrInsertIrildesEntryNumber in CusEntryHeader",
			"Irildes is not supported by Entry Header [BGMReference: BGM]",
			() => Adapter.UpdateOrInsertIrildesEntryNumber(ZString.Empty, ZDateTime.Empty));
	}

	public override void TestGetOriginalSentMessageByUniqueTransactionIdentifier()
	{
		var sentMessage = Adaptee.Messages.AddNew();
		sentMessage.IsTransmitMessage = true;
		sentMessage.EM_Status = "SNT";
		var sentInterchange = Factory.New<EDIInterchange>();
		sentInterchange.ContainedMessages.Add(sentMessage);
		sentInterchange.EI_SessionGUID = new ZGuid("EADC205E-BFD7-44AD-B547-14C4B3C02177");

		var acknowledgmentMessage = Adaptee.Messages.AddNew();
		acknowledgmentMessage.EM_MessageType = "ACK";
		acknowledgmentMessage.EM_MessageText = "<IUT>20220307D11000328189</IUT>";
		var acknowledgmentInterchange = Factory.New<EDIInterchange>();
		acknowledgmentInterchange.ContainedMessages.Add(acknowledgmentMessage);
		acknowledgmentInterchange.EI_SessionGUID = new ZGuid("EADC205E-BFD7-44AD-B547-14C4B3C02177");

		CombineAssertions(() =>
		{
			AssertSame("When the passed IUT matches the ACK and a sent message", sentMessage, Adapter.GetOriginalSentMessageByUniqueTransactionIdentifier("20220307D11000328189"));
			AssertNull("When the passed IUT does not match an ACK nor a sent message", Adapter.GetOriginalSentMessageByUniqueTransactionIdentifier("ABCDEFGH"));
		});
	}

	public void TestProcessBondedWarehouseIfRequired()
	{
		SetupDataForBondedWarehouse();

		AssertEquals("PRE-CONDITION, IsIntoWarehouseWarehousing", true, Adaptee.InvoiceLines.ElementAt(0).IsIntoWarehouseWarehousing);

		Adapter.ProcessBondedWarehouseIfRequired(logger, Adaptee.Messages[0]);
		Factory.Save();
		AssertNull("Cancel Warehouse log", Adaptee.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob));

		Adaptee.CH_WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreationHeld;
		Adapter.ProcessBondedWarehouseIfRequired(logger, Adaptee.Messages[0]);
		Factory.Save();
		var cancelTheWarehouseJobLog = Adaptee.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);
		AssertNotNull("Cancel Warehouse log", cancelTheWarehouseJobLog);
		AssertEquals("Cancel Warehouse Log, IsInDatabase", true, cancelTheWarehouseJobLog.IsInDatabase);
	}

	public void TestProcessBondedWarehouseIfRequired_WhenStatusDoesNotRequireUpdate()
	{
		SetupDataForBondedWarehouse();

		Assert("CH_EntryStatus is empty, CH_Status is empty", Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated, string.Empty, string.Empty);
		Assert("CH_EntryStatus is empty, CH_Status is ACO", Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated, string.Empty, ITMessageStatusList.Codes.AcknowledgedOriginal);
		Assert("CH_EntryStatus is REG, CH_Status is ACO", Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated, ITEntryStatusList.Codes.Registered, ITMessageStatusList.Codes.AcknowledgedOriginal);
		Assert("CH_EntryStatus is UCL, CH_Status is UNK", Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated, ITEntryStatusList.Codes.UnderControl, ITMessageStatusList.Codes.Unknown);
		Assert("CH_EntryStatus is CNG, CH_Status is ACO", Customs.Business.WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, ITEntryStatusList.Codes.Canceling, ITMessageStatusList.Codes.AcknowledgedOriginal);
		Assert("CH_EntryStatus is CNG, CH_Status is ACS", Customs.Business.WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, ITEntryStatusList.Codes.Canceling, ITMessageStatusList.Codes.AcceptedBySystem);
		Assert("CH_EntryStatus is AMG, CH_Status is empty", Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated, ITEntryStatusList.Codes.Amending, string.Empty);
		Assert("CH_EntryStatus is AMG, CH_Status is ACO", Customs.Business.WarehouseTransactionStatusList.Codes.InwardUpdatedPending, ITEntryStatusList.Codes.Amending, ITMessageStatusList.Codes.AcknowledgedOriginal);
		Assert("CH_EntryStatus is AMG, CH_Status is ACS", Customs.Business.WarehouseTransactionStatusList.Codes.InwardUpdatedPending, ITEntryStatusList.Codes.Amending, ITMessageStatusList.Codes.AcceptedBySystem);

		void Assert(string message, string whsTransactionStatus, string entryStatus, string status)
		{
			Adaptee.CH_WarehouseTransactionStatus = whsTransactionStatus;
			Adaptee.CH_EntryStatus = entryStatus;
			Adaptee.CH_Status = status;
			Factory.Save();

			var logsCount = Adaptee.Logs.GetAllLogs().Count;
			Adapter.ProcessBondedWarehouseIfRequired(logger, Adaptee.Messages[0]);
			Factory.Save();

			CombineAssertions(message, () =>
			{
				AssertEquals("CH_WarehouseTransactionStatus", whsTransactionStatus, Adaptee.CH_WarehouseTransactionStatus);
				AssertEquals("Logs", logsCount, Adaptee.Logs.GetAllLogs().Count);
			});
		}
	}

	void SetupDataForBondedWarehouse()
	{
		var declaration = Adaptee.Declaration;

		declaration.JE_ApplicationCode = "BLT";
		declaration.JE_MessageType = "";
		Adaptee.EntryNumber = "ENT001";
		Adaptee.CH_WarehouseTransactionStatus = ZString.Empty;
		Adaptee.CH_Status = Common.Shared.MessageStatusList.Codes.ErrorOriginal;

		var whsDataTestHelper = new WhsDataTestHelper(Factory);
		declaration.JE_OH_Importer = whsDataTestHelper.Importer.PK;
		declaration.WarehouseDocAddress.E2_OA_Address = whsDataTestHelper.Warehouse.MainAddress.PK;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_OA_Warehouse2 = whsDataTestHelper.WhsWarehouse.WW_OA_WarehouseAddress;
		Adaptee.CH_CEI_Instruction = entryInstruction.PK;

		var receivedMessage = Factory.NewWithValidTestData<ITEDIMessage>();
		Adaptee.Messages.Add(receivedMessage);

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var mergedLine = Adaptee.MergedLines.AddNew();
		invoiceLine.JI_CL = mergedLine.PK;
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_PartNo = whsDataTestHelper.Part.OP_PartNum;
		invoiceLine.JI_InvoiceQuantity = 1;
		invoiceLine.JI_CustomsQuantity = 1;
		invoiceLine.JI_ValuationCode = MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;

		var procedure = whsDataTestHelper.InwardCusProcedure;
		invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;
	}

	void SetupDataForFees()
	{
		var lineOne = Adaptee.MergedLines.AddNew();
		lineOne.CL_LineNumber = 10;
		var lineTwo = Adaptee.MergedLines.AddNew();
		lineTwo.CL_LineNumber = 20;

		AddFeeLine(lineOne, "E");
		AddFeeLine(lineOne, "E");
		AddFeeLine(lineOne, "G");
		AddFeeLine(lineOne, "Q");

		AddFeeLine(lineTwo, "F");
		AddFeeLine(lineTwo, "F");
		AddFeeLine(lineTwo, "E");

		void AddFeeLine(CusEntryLine line, string methodOfPayment)
		{
			var lineFee = line.Fees.AddNew();
			lineFee.CF_MethodOfPayment = methodOfPayment;
		}
	}

	void AssertEntryNumber(CusEntryNumber entryNumber, ZString expectedEntryType, ZString expectedEntryLineReference, ZDateTime expectedIssueDate, ZString expectedEntryStatus)
	{
		AssertEquals("CE_EntryType", expectedEntryType, entryNumber.CE_EntryType);
		AssertEquals("CE_EntryLineReference", expectedEntryLineReference, entryNumber.CE_EntryLineReference);
		AssertEquals("CE_IssueDate", expectedIssueDate, entryNumber.CE_IssueDate);
		AssertEquals("CE_EntryStatus", expectedEntryStatus, entryNumber.CE_EntryStatus);
		AssertEquals("CE_RN_NKCountryCode", "IT", entryNumber.CE_RN_NKCountryCode);
		AssertEquals("CE_Category", "CUS", entryNumber.CE_Category);
		AssertEquals("CE_EntryIsSystemGenerated", ZBool.True, entryNumber.CE_EntryIsSystemGenerated);
	}

	public void TestSetLocalReferenceNumberImport()
	{
		Adaptee.Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var entryInstruction = Adaptee.Declaration.CustomsEntryInstructions.AddNew();
		Adaptee.CH_CEI_Instruction = entryInstruction.PK;

		AssertEquals("PreviousDocuments empty", 0, Adaptee.EntryInstruction.PreviousDocuments.Count);

		var lrn = "2024EDIDAT000000000001";
		Adapter.SetLocalReferenceNumber(lrn);

		AssertEquals("PreviousDocument added", 1, Adaptee.EntryInstruction.PreviousDocuments.Count);
		CombineAssertions(() =>
		{
			var previousDocument = Adaptee.EntryInstruction.PreviousDocuments[0];
			AssertEquals("CSI_Procedure", "NUM", previousDocument.CSI_Procedure);
			AssertEquals("CSI_Code", "ZZZ", previousDocument.CSI_Code);
			AssertEquals("CSI_ReferenceNumber", lrn, previousDocument.CSI_ReferenceNumber);
		});
	}

	public void TestSetLocalReferenceNumberExport()
	{
		Adaptee.Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var entryInstruction = Adaptee.Declaration.CustomsEntryInstructions.AddNew();
		Adaptee.CH_CEI_Instruction = entryInstruction.PK;

		AssertEquals("PreviousDocuments empty", 0, Adaptee.EntryInstruction.PreviousDocuments.Count);

		var lrn = "2024EDIDAT000000000001";
		Adapter.SetLocalReferenceNumber(lrn);

		AssertEquals("PreviousDocument empty", 0, Adaptee.EntryInstruction.PreviousDocuments.Count);
	}

	public void TestSetLocalReferenceNumberWithoutEntryInstruction()
	{
		AssertNoExceptionThrown(() => Adapter.SetLocalReferenceNumber("2024EDIDAT000000000001"));
	}

	public override void TestAddEDoc()
	{
		var documentData = new byte[] { 0x20, 0x40 };

		Adapter.AddEDoc(documentData, "Test12.pdf", "CLR");
		var eDocs = Adaptee.DocManagerInfo().AllEDocs;
		var uniqueEdoc = eDocs[0];

		AssertEquals("eDocs count", 1, eDocs.Count);
		AssertEquals("DocType is CLR", "CLR", uniqueEdoc.DocType);
		AssertEquals("FileName", "Test12.pdf", uniqueEdoc.FileName);
	}

	public override void TestCustomsProfile()
	{
		Adaptee.Declaration.JE_CustomsProfile = "12345";
		AssertEquals(Adaptee.Declaration.JE_CustomsProfile, Adapter.CustomsProfile);
	}

	public override void TestCreateIvistoRequestMessage()
	{
		Adaptee.Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		Adaptee.Declaration.MessageVersion = MessageVersionList.Codes.XML;
		Adapter.CreateIvistoRequestMessage();
		Adaptee.Messages.Reload(reLoadExistingRows: false);
		var ivistoMessage = Adaptee.Messages.GetLastMessageByType("IVI");
		AssertNotNull("IVI message", ivistoMessage);
	}

	public override void TestCreateIrildesRequestMessage()
	{
		AssertExceptionThrown<CustomsMessageProcessorException>(Adapter.CreateIrildesRequestMessage);
	}

	protected override void SetUp()
	{
		base.SetUp();
		logger = new LoggingInformationForTesting();
	}

	LoggingInformationForTesting logger;

	protected override CusEntryHeader GetAdaptee() => Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();

	protected override IXmlCustomsLinkedObjectAdapter GetAdapter() => new CusEntryHeaderCustomsLinkedObjectAdapterForTest(Adaptee);

	sealed class CusEntryHeaderCustomsLinkedObjectAdapterForTest : CusEntryHeaderCustomsLinkedObjectAdapter
	{
		public CusEntryHeaderCustomsLinkedObjectAdapterForTest(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected override IvistoRequestMessageFactory GetNewIvistoRequestMessageFactory()
		{
			return new IvistoRequestMessageFactoryForTest();
		}
	}
}
