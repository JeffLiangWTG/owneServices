using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsHeaderCustomsLinkedObjectAdapterConstructorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When nctsHeader is null", () => new NctsHeaderCustomsLinkedObjectAdapter(null));
		AssertExceptionThrown<NotSupportedException>("When nctsHeader is NOT a departure job", () => new NctsHeaderCustomsLinkedObjectAdapter(Factory.New<NctsHeader>()));
		AssertNoExceptionThrown("When nctsHeader is a departure job and its movement header is not null", () => new NctsHeaderCustomsLinkedObjectAdapter(Factory.NewDepartureNctsHeader()));
	}
}

sealed class NctsHeader_CustomsLinkedObjectAdapterTest : CustomsLinkedObjectAdapterTest<NctsHeader>
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
		AssertNull($"When there are no sent idoc messages, {nameof(Adapter.GetLastSuccessfullySentMessage)}", Adapter.GetLastSuccessfullySentMessage());

		var message1 = Adaptee.Messages.AddNew();
		message1.IsTransmitMessage = false;
		AssertNull($"When there are no sent idoc transmit messages, {nameof(Adapter.GetLastSuccessfullySentMessage)}", Adapter.GetLastSuccessfullySentMessage());

		message1.IsTransmitMessage = true;
		message1.EM_Status = EDIMessageStatusList.Codes.Queued;
		AssertNull($"When there is one queued idoc transmit message, {nameof(Adapter.GetLastSuccessfullySentMessage)}", Adapter.GetLastSuccessfullySentMessage());

		message1.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		message1.EM_Status = EDIMessageStatusList.Codes.Sent;
		message1.EM_SystemCreateTimeUtc = new ZDateTime(2021, 01, 01, 10, 10, 10);
		AssertNotNull($"When there is one sent idoc transmit message, {nameof(Adapter.GetLastSuccessfullySentMessage)}", Adapter.GetLastSuccessfullySentMessage());
		AssertSame($"{nameof(message1)} and {nameof(Adapter.GetLastSuccessfullySentMessage)}", message1, Adapter.GetLastSuccessfullySentMessage());

		var message2 = Adaptee.Messages.AddNew();
		message2.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		message2.IsTransmitMessage = true;
		message2.EM_Status = EDIMessageStatusList.Codes.Sent;
		message2.EM_SystemCreateTimeUtc = new ZDateTime(2021, 01, 31, 10, 10, 10);
		AssertNotNull($"When there are more than one sent idoc transmit messages, {nameof(Adapter.GetLastSuccessfullySentMessage)}", Adapter.GetLastSuccessfullySentMessage());
		AssertSame($"{nameof(message2)} and {nameof(Adapter.GetLastSuccessfullySentMessage)}", message2, Adapter.GetLastSuccessfullySentMessage());
	}

	public void TestGenerateTadDocumentWhenNctsHeaderIsNotCleared()
	{
		AssertEquals("[PRE-REQUISITE] CustomsStatus", "", Adaptee.MovementHeader.BM_CustomsStatus);
		Adapter.GenerateDocuments();
		AssertEquals("When Ncts has no Clearance, Should not have TAD document", 0, Factory.GetTadPrintedJobs(Adaptee).Length);
	}

	public void TestGenerateTadDocumentWhenNctsHeaderIsCleared()
	{
		GenerateTadAndAssertItIsInItalian(Adaptee);
	}

	public void TestGenerateTadDocumentWhenDocumentDeliveryDefaultLanguageRegistryIsOverridenToEN()
	{
		GlbBranch.CurrentBranch.OrgProxy.OH_Language = "EN-EN";
		var defaultLanguageSettings = new DocumentDeliveryDefaultLanguagesCollection
		{
			new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Branch, Order = 1 }
		};

		using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultLanguageSettings))
		{
			GenerateTadAndAssertItIsInItalian(Adaptee);
		}
	}

	void GenerateTadAndAssertItIsInItalian(NctsHeader nctsHeader)
	{
		nctsHeader.EntryNumbersProvider.InsertOrUpdateReleaseCode("VL9NSW", ZDateTime.Now);
		nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;
		Adapter.GenerateDocuments();

		var tadPrintedJobs = Factory.GetTadPrintedJobs(nctsHeader);
		AssertEquals("When Ncts is Cleared and it has no TAD Document, GenerateDocuments should generate one TAD document", 1, tadPrintedJobs.Length);
		using (var excelInterface = new ExcelInterface())
		{
			excelInterface.LoadExcelFile(tadPrintedJobs[0].SP_CustomProperties);
			AssertContains("Contains labels in italian", "DOCUMENTO ACCOMPAGNAMENTO TRANSITO", excelInterface.WorkSheets[0].ToString().ToUpper());
		}
	}

	public override void TestEntryReferenceNumber()
	{
		Adaptee.BH_JobReference = "BBB";
		AssertEquals(nameof(Adapter.EntryReferenceNumber), Adaptee.BH_JobReference, Adapter.EntryReferenceNumber);
	}

	public override void TestJobReferenceNumber()
	{
		Adaptee.BH_JobReference = "BBB";
		AssertEquals(nameof(Adapter.JobReferenceNumber), Adaptee.BH_JobReference, Adapter.JobReferenceNumber);
	}

	public override void TestFactory()
	{
		AssertNotNull("Not null", Adapter.Factory);
		AssertSame("Same", Adaptee.Factory, Adapter.Factory);
	}

	public override void TestCustomsProfile()
	{
		Adaptee.BH_CustomsProfile = "12345";
		AssertEquals(nameof(Adapter.CustomsProfile), Adaptee.BH_CustomsProfile, Adapter.CustomsProfile);
	}

	protected override NctsHeader GetAdaptee() => Factory.NewDepartureNctsHeader();

	protected override ICustomsLinkedObjectAdapter GetAdapter() => new NctsHeaderCustomsLinkedObjectAdapter(Adaptee);
}

sealed class NctsHeader_SadCustomsLinkedObjectAdapterTest : SadCustomsLinkedObjectAdapterTest<NctsHeader>
{
	public override void TestCustomsLines()
	{
		AssertEquals($"{nameof(Adapter.CustomsLines)} count", 0, Adapter.CustomsLines.Count());

		Adaptee.MovementHeader.GoodsItems.AddNew();
		Adaptee.MovementHeader.GoodsItems.AddNew();
		AssertEquals($"{nameof(Adapter.CustomsLines)} count", 2, Adapter.CustomsLines.Count());
		AssertSame($"{nameof(Adapter.CustomsLines)} cached", Adapter.CustomsLines, Adapter.CustomsLines);
		AssertType<NctsDepartureCargoDescCustomsLineLinkedObjectAdapter>("Type", Adapter.CustomsLines.First());
	}

	public override void TestEntryCustomsStatus()
	{
		Adaptee.MovementHeader.BM_CustomsStatus = "BBB";
		AssertEquals(nameof(Adapter.EntryCustomsStatus), Adaptee.MovementHeader.BM_CustomsStatus, Adapter.EntryCustomsStatus);
	}

	public override void TestStatusProvider()
	{
		CombineAssertions(() =>
		{
			var provider = Adapter.StatusProvider;
			AssertType<NctsHeaderCustomsStatusProvider>($"{nameof(Adapter.StatusProvider)} type", provider);
			AssertSame($"{nameof(Adapter.StatusProvider)} cached", provider, Adapter.StatusProvider);
		});
	}

	public override void TestFactory()
	{
		AssertSame($"{nameof(Adaptee.Factory)} and {nameof(Adapter.Factory)}", Adaptee.Factory, Adapter.Factory);
	}

	public override void TestGetEntryNumbers()
	{
		var entryNumbers = Adapter.GetEntryNumbers();
		AssertEquals($"PRE-CONDITION: {nameof(entryNumbers)} count", 0, entryNumbers.Count());

		var entryNumber1 = Adapter.GetNewCusEntryNumber();
		entryNumber1.CE_SystemCreateTimeUtc = ZDateTime.Now;

		var entryNumber2 = Adapter.GetNewCusEntryNumber();
		entryNumber2.CE_SystemCreateTimeUtc = entryNumber1.CE_SystemCreateTimeUtc.AddDays(-1);

		entryNumbers = Adapter.GetEntryNumbers();
		AssertArrayEqualsByElements($"POST-CONDITION: {nameof(entryNumbers)} should contain 2 elements ordered by CE_SystemCreateTimeUtc", new CusEntryNumber[] { entryNumber2, entryNumber1 }, entryNumbers.ToArray());
	}

	public override void TestGetNewCusEntryNumber()
	{
		var cusEntryNumber = Adapter.GetNewCusEntryNumber();

		AssertNotNull(nameof(cusEntryNumber), cusEntryNumber);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(cusEntryNumber.CE_ParentID), Adaptee.PK, cusEntryNumber.CE_ParentID);
			AssertEquals(nameof(cusEntryNumber.CE_ParentTable), Adaptee.TableName, cusEntryNumber.CE_ParentTable);
			AssertEquals(nameof(cusEntryNumber.CE_Category), CusEntryNumber.Categories.CustomsPermitClearanceNumber, cusEntryNumber.CE_Category);
			AssertEquals(nameof(cusEntryNumber.CE_RN_NKCountryCode), Core.Constants.CountryCodes.Italy, cusEntryNumber.CE_RN_NKCountryCode);
		});
	}

	public override void TestInsertOrUpdateA93Numbers()
	{
		var goodsItem = Adaptee.MovementHeader.GoodsItems.AddNew();
		AddNewFee("A", 100m);
		AddNewFee("A", 25m);
		AddNewFee("B", 34.54m);
		Factory.NewCusEntryNumber(Adaptee, CusEntryNumberConstants.EntryTypes.RegistrationNumber, "4", ZDateTime.Now);

		AssertEquals("PRE-CONDITION: InBondPayInfo Count", 0, Adaptee.MovementHeader.PayInfoCollection.Count);
		var entryPaymentsMock = new Mock<ISadPositiveResponseMessageA93EntryPayments>();
		entryPaymentsMock.Setup(x => x.A93Number).Returns("123456");
		entryPaymentsMock.Setup(x => x.HasA93FirstPayment).Returns(true);
		entryPaymentsMock.Setup(x => x.FirstPaymentDueDate).Returns(new ZDate(2021, 01, 01));
		entryPaymentsMock.Setup(x => x.FirstPaymentMethod).Returns("A");
		entryPaymentsMock.Setup(x => x.HasA93SecondPayment).Returns(true);
		entryPaymentsMock.Setup(x => x.SecondPaymentDueDate).Returns(new ZDate(2021, 01, 02));
		entryPaymentsMock.Setup(x => x.SecondPaymentMethod).Returns("B");
		entryPaymentsMock.Setup(x => x.HasA93ThirdPayment).Returns(true);
		entryPaymentsMock.Setup(x => x.ThirdPaymentDueDate).Returns(new ZDate(2021, 01, 03));
		entryPaymentsMock.Setup(x => x.ThirdPaymentMethod).Returns("C");
		Adapter.InsertOrUpdateA93Numbers(entryPaymentsMock.Object);

		var a93PayInfoCollection = Adaptee.MovementHeader.PayInfoCollection.Cast<NctsDeparturePayInfo>().OrderBy(x => x.BPI_MethodOfPayment).ToArray();
		AssertEquals("POST-CONDITION: InBondPayInfo Count", 3, a93PayInfoCollection.Length);

		CombineAssertions("InBondPayInfo at 0", () => AssertCusInBondPayInfo(a93PayInfoCollection[0], "123456", "A", 125m, "4", new ZDateTime(2021, 01, 01), Customs.Business.CusEntryPayInfoStatusList.Codes.Pending));
		CombineAssertions("InBondPayInfo at 1", () => AssertCusInBondPayInfo(a93PayInfoCollection[1], "123456", "B", 34.54m, "4", new ZDateTime(2021, 01, 02), Customs.Business.CusEntryPayInfoStatusList.Codes.Pending));
		CombineAssertions("InBondPayInfo at 2", () => AssertCusInBondPayInfo(a93PayInfoCollection[2], "123456", "C", 0m, "4", new ZDateTime(2021, 01, 03), Customs.Business.CusEntryPayInfoStatusList.Codes.Pending));

		void AddNewFee(ZString methodOfPayment, ZDecimal amount)
		{
			var fee = goodsItem.Fees.AddNew();
			fee.BFE_MethodOfPayment = methodOfPayment;
			fee.BFE_ChargeAmount = amount;
		}

		void AssertCusInBondPayInfo(NctsDeparturePayInfo nctsPayInfo, ZString incomingPayResponseNo, ZString methodOfPayment, ZDecimal paymentAmount, ZString declarationRegistry, ZDateTime expirationDate, ZString paymentStatus)
		{
			AssertEquals("BPI_IncomingPayResponseNo", incomingPayResponseNo, nctsPayInfo.BPI_IncomingPayResponseNo);
			AssertEquals("BPI_MethodOfPayment", methodOfPayment, nctsPayInfo.BPI_MethodOfPayment);
			AssertEquals("BPI_PaymentAmount", paymentAmount, nctsPayInfo.BPI_PaymentAmount);
			AssertEquals("BPI_TransactionType", declarationRegistry, nctsPayInfo.BPI_TransactionType);
			AssertEquals("BPI_PaymentDate", expirationDate, nctsPayInfo.BPI_PaymentDate);
			AssertEquals("BPI_PaymentStatus", paymentStatus, nctsPayInfo.BPI_PaymentStatus);
		}
	}

	public override void TestIrildesCusEntryNum()
	{
		AssertNull("PRE-CONDITION", Adapter.IrildesCusEntryNum);

		Factory.NewCusEntryNumber(Adaptee, CusEntryNumberConstants.EntryTypes.Irildes, ZString.Empty, ZDate.Today);
		AssertNotNull("POST-CONDITION", Adapter.IrildesCusEntryNum);
	}

	public override void TestIsEntryRegisteredOrNbRejected()
	{
		Adaptee.MovementHeader.BM_CustomsStatus = ZString.Empty;
		AssertEquals(GetAssertionMessage(), false, Adapter.IsEntryRegisteredOrNbRejected);

		Adaptee.MovementHeader.BM_CustomsStatus = "XXX";
		AssertEquals(GetAssertionMessage(), false, Adapter.IsEntryRegisteredOrNbRejected);

		Adaptee.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
		AssertEquals(GetAssertionMessage(), true, Adapter.IsEntryRegisteredOrNbRejected);

		Adaptee.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.NbRejected;
		AssertEquals(GetAssertionMessage(), true, Adapter.IsEntryRegisteredOrNbRejected);

		string GetAssertionMessage() => $"When {nameof(Adaptee.MovementHeader.BM_CustomsStatus)} = '{Adaptee.MovementHeader.BM_CustomsStatus}', {nameof(Adapter.IsEntryRegisteredOrNbRejected)}";
	}

	public override void TestIsExport()
	{
		AssertEquals(nameof(Adapter.IsExport), true, Adapter.IsExport);
	}

	public override void TestIsImport()
	{
		AssertEquals(nameof(Adapter.IsImport), false, Adapter.IsImport);
	}

	public override void TestIsIncomingMessageAlreadyLinked()
	{
		AssertEquals(nameof(Adapter.IsIncomingMessageAlreadyLinked), false, Adapter.IsIncomingMessageAlreadyLinked("A"));

		Adaptee.Messages.AddNew().EM_MessageType = "A";
		AssertEquals(nameof(Adapter.IsIncomingMessageAlreadyLinked), true, Adapter.IsIncomingMessageAlreadyLinked("A"));
	}

	public override void TestIvistoCusEntryNum()
	{
		AssertNull("PRE-CONDITION", Adapter.IvistoCusEntryNum);

		Factory.NewCusEntryNumber(Adaptee, CusEntryNumberConstants.EntryTypes.Ivisto, ZString.Empty, ZDate.Today);
		AssertNotNull("POST-CONDITION", Adapter.IvistoCusEntryNum);
	}

	public override void TestMessageStatus()
	{
		Adaptee.BH_MessageStatus = "AAA";
		AssertEquals(nameof(Adapter.MessageStatus), Adaptee.BH_MessageStatus, Adapter.MessageStatus);
	}

	public override void TestMrn()
	{
		AssertEquals("PRE-CONDITION", ZString.Empty, Adapter.Mrn);

		Adaptee.MovementReferenceEntryNumber.CE_EntryNum = "XYZ";
		AssertEquals("POST-CONDITION", "XYZ", Adapter.Mrn);
	}

	public override void TestSetEntryCustomsStatus()
	{
		AssertEquals("PRE-CONDITION", ZString.Empty, Adaptee.MovementHeader.BM_CustomsStatus);

		Adapter.SetEntryCustomsStatus("AAA");
		AssertEquals("POST-CONDITION", "AAA", Adaptee.MovementHeader.BM_CustomsStatus);
	}

	public override void TestSetEntryReleaseDate()
	{
		AssertNoExceptionThrown("Doing nothing and should not throw any exception", () => Adapter.SetEntryReleaseDate(ZDateTime.Empty));
	}

	public override void TestSetMessageStatus()
	{
		AssertEquals("PRE-CONDITION", ZString.Empty, Adaptee.BH_MessageStatus);

		Adapter.SetMessageStatus("AAA");
		AssertEquals("POST-CONDITION", "AAA", Adaptee.BH_MessageStatus);
	}

	public override void TestSingleWindowRequestDataProvider()
	{
		var singleWindowRequestDataProvider = Adapter.SingleWindowRequestDataProvider;
		AssertNotNull(nameof(singleWindowRequestDataProvider), singleWindowRequestDataProvider);
		AssertType<NctsHeader>($"{nameof(singleWindowRequestDataProvider)} type", singleWindowRequestDataProvider);
	}

	public override void TestUpdatePendingGuaranteeTransactions()
	{
		var messageNum = "123456";

		var permitHolder = Factory.New<OrgHeader>();
		permitHolder.OH_Code = "ORG";
		var guaranteeHeader = CusGuaranteeHeaderTestUtility.SetupGuarantee(Factory, permitHolder, "111", "XYZ");

		var sentMessage = Adaptee.Messages.AddNew();
		sentMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(messageNum);
		sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		sentMessage.EM_MessageType = "R";
		Factory.Save();

		var guaranteeTransaction1 = guaranteeHeader.AddTransaction(Adaptee.JobNumber, "", messageNum, "", 100m, 0m, PermitTransactionStatusList.Codes.Pending);
		var guaranteeTransaction2 = guaranteeHeader.AddTransaction(Adaptee.JobNumber, "", messageNum, "", 2m, 0m, PermitTransactionStatusList.Codes.Pending);
		var guaranteeTransaction3 = guaranteeHeader.AddTransaction(Adaptee.JobNumber, "", "111111", "", 50m, 0m, PermitTransactionStatusList.Codes.Pending);
		Factory.Save();

		Adapter.UpdatePendingGuaranteeTransactions("AAA");
		CombineAssertions(() =>
		{
			AssertEquals("GuaranteeTransaction1 CPL_TransactionStatus", "AAA", guaranteeTransaction1.CPL_TransactionStatus);
			AssertEquals("GuaranteeTransaction2 CPL_TransactionStatus", "AAA", guaranteeTransaction2.CPL_TransactionStatus);
			AssertEquals("GuaranteeTransaction3 CPL_TransactionStatus", PermitTransactionStatusList.Codes.Pending, guaranteeTransaction3.CPL_TransactionStatus);
		});
	}

	public override void TestWriteOffGuarantee()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
		var guaranteeHeader = CusGuaranteeHeaderTestUtility.SetupGuarantee(Factory, permitHolder, "111", "XYZ");
		Adaptee.BH_JobReference = "123ABC";
		Adaptee.Principal.E2_OA_Address = permitHolder.MainAddress.PK;
		Adaptee.Guarantees.RemoveAndDeleteAll();
		Adapter.WriteOffGuarantee("123456", new ZDate(2022, 01, 01));
		AssertEquals("When NctsHeader does not have any Guarantee added", 0, guaranteeHeader.CusGuaranteeLineTransactions.Count);

		var guarantee = Adaptee.Guarantees.AddNew();
		guarantee.PW_BondAmount = 999m;
		guarantee.PW_BondNumber = "AAA";
		guarantee.PW_BondType = "3";
		Adapter.WriteOffGuarantee("123456", new ZDate(2022, 01, 01));
		AssertEquals("When NctsHeader has a Guarantee added but does not match any registered GuaranteeHeader", 0, guaranteeHeader.CusGuaranteeLineTransactions.Count);

		guarantee.PW_BondNumber = "XYZ";
		Adapter.WriteOffGuarantee("123456", new ZDate(2022, 01, 01));
		AssertEquals("When NctsHeader has a Guarantee added and match a registered GuaranteeHeader", 1, guaranteeHeader.CusGuaranteeLineTransactions.Count);
		CombineAssertions("Created CusGuaranteeLineTransaction Fields", () =>
		{
			CusGuaranteeLineTestHelper.AssertGuaranteeLineTransaction(guaranteeHeader.CusGuaranteeLineTransactions[0]
				, appId: "123456"
				, comment: "NCTS write-off 123ABC"
				, transactionDate: new ZDate(2022, 01, 01)
				, transactionType: PermitTransactionTypeList.Codes.TRA
				, transactionCategory: PermitTransactionCategoryList.Codes.CUM
				, transactionStatus: PermitTransactionStatusList.Codes.Confirmed
				, reference: "123ABC"
				, transactionValue: 999m
			);
		});
	}

	#region Implementation

	protected override NctsHeader GetAdaptee() => Factory.NewDepartureNctsHeader();

	protected override ISadCustomsLinkedObjectAdapter GetAdapter() => new NctsHeaderCustomsLinkedObjectAdapter(Adaptee);

	#endregion
}

sealed class NctsHeader_SingleWindowCustomsLinkedObjectAdapterTest : SingleWindowCustomsLinkedObjectAdapterTest<NctsHeader>
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
		AssertSame("Same", Adaptee.DocManagerInfo, Adapter.DocManagerInfo);
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
		AssertEquals("PRE-CONDITION", ZString.Empty, Adaptee.MovementHeader.BM_ControlChannel);

		Adapter.SetEntryCustomsChannel("AAA");
		AssertEquals("POST-CONDITION", "AAA", Adaptee.MovementHeader.BM_ControlChannel);
	}

	public override void TestSetEntryAsCleared()
	{
		AssertEquals("PRE-CONDITION", ZString.Empty, Adaptee.MovementHeader.BM_CustomsStatus);

		Adapter.SetEntryAsCleared(ZDateTime.Empty);
		AssertEquals("POST-CONDITION", NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, Adaptee.MovementHeader.BM_CustomsStatus);
	}

	public override void TestIsEntryCleared()
	{
		Adaptee.MovementHeader.BM_CustomsStatus = ZString.Empty;
		Assert("Empty", !Adapter.IsEntryCleared);

		Adaptee.MovementHeader.BM_CustomsStatus = NctsMessageStatusList.Codes.Ok;
		Assert(NctsMessageStatusList.Codes.Ok, !Adapter.IsEntryCleared);

		Adaptee.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsNotReleasedForTransit;
		Assert(NctsTransitStatusList.Codes.GoodsNotReleasedForTransit, !Adapter.IsEntryCleared);

		Adaptee.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
		Assert(NctsTransitStatusList.Codes.DeclarationMrnAllocated, !Adapter.IsEntryCleared);

		Adaptee.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;
		Assert(NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, Adapter.IsEntryCleared);

		Adaptee.MovementHeader.BM_CustomsStatus = "XXX";
		Assert("Invalid type", !Adapter.IsEntryCleared);
	}

	protected override NctsHeader GetAdaptee() => Factory.NewDepartureNctsHeader();

	protected override ISingleWindowCustomsLinkedObjectAdapter GetAdapter() => new NctsHeaderCustomsLinkedObjectAdapter(Adaptee);
}
