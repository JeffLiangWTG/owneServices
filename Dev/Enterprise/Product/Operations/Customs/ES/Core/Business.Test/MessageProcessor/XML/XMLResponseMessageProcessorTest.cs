using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using CusTempStorageRegHeader = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader;
using CusTempStorageRegLine = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine;
using CusTempStorageRegLineTransaction = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction;
using CusTempStorageRegPremises = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises;

namespace Enterprise.Customs.ES.Business.Testing;

public abstract class XMLResponseMessageProcessorTest<TResponse, TPrettyMessage, TResponseProvider> : ESResponseMessageProcessorTest<TResponse, TResponseProvider>
	where TResponse : XMLResponseMessageProcessor<TResponseProvider, TPrettyMessage>
	where TResponseProvider : class, ICommonServiceSegment, IResponseCode
	where TPrettyMessage : IMessagePrettyFormatter
{
	public void TestProcessMessageWrongXML()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, WrongXMLTestFile, InterchangeID);

		ProcessMessageForTest(message);

		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
			AssertEquals("CH_Status", CHStatusWhenWrongXMLOrMessageTextEmpty, entryHeader.CH_Status);
			AssertLoggerMessagesWhenProcessMessageWrongXML();
		});
	}

	public void TestMessageProcessingError()
	{
		var message = CreateNewEDIMessage("AAAAAAAA", ZString.Empty, InterchangeID, false);

		ProcessMessageForTest(message);

		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
			AssertLoggerMessagesWhenMessageProcessingError();
		});
	}

	public void TestMessageProcessingErrorNoReference()
	{
		var message = CreateNewEDIMessage(ZString.Empty, ZString.Empty, InterchangeID, false);

		Factory.Save();

		ProcessMessageForTest(message);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
			AssertContains("Log has error", "No Application Reference found for message", logger.UserLogStrings[0]);
		});
	}

	public void TestProcessMessageAcceptedWithSegmentIdTooLong()
	{
		var messageText = GetAcceptanceTestFileWithLongSegmentId();
		if (!messageText.IsNullOrEmpty())
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, messageText, InterchangeID);

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("No exception expected since SementId is cut to 35 chars", () => ProcessMessageForTest(message));

				AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			});
		}
		else
		{
			Assert(true);
		}
	}

	protected virtual ZString CHStatusWhenWrongXMLOrMessageTextEmpty => "FAL";

	protected abstract string GetAcceptanceTestFileWithLongSegmentId();

	protected virtual void AssertLoggerMessagesWhenProcessMessageWrongXML()
	{
		AssertContains("logger", "Unable to read message text from message", GetAllConcatenatedUserLogStrings());
	}

	protected virtual void AssertLoggerMessagesWhenMessageProcessingError()
	{
		AssertContains("Log has error", "Unable to find business object for message", GetAllConcatenatedUserLogStrings());
	}

	protected override void AssertLoggerMessagesWhenProcessMessageWrongText(TestEdiMessage message, BusinessObject businessObject)
	{
		var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
		AssertContains("logger", "Unable to read message text from message ", concatenatedUserLogStrings);
		AssertContains("logger exception", "There is an error in XML document ", concatenatedUserLogStrings);
		AssertContains("EM_MessageInterpretation",
				string.Format("<H3>Processor Failure</H3><br>" +
				"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
				"<H4>Exception: There is an error in XML document ", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference)
				, message.EM_MessageInterpretation);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_DataModel = Core.Constants.CountryCodes.Spain;
		entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

		Factory.Save();
	}
	protected JobDeclaration declaration;
	protected CusEntryHeader entryHeader;

	protected const string GuaranteeReference = "16ESAGL9990000096";
	protected const string OtherGuaranteeReference = "17ESAGL9990000097";

	protected const string DeclarantId = "NIF22222222";
	protected const string DeclarantName = "Declarant Full Name";

	protected void AssertDocument(CusSupportingInfo doc, ZString code, ZString reference, ZString subtype, ZString status)
	{
		AssertEquals("Doc with reference " + reference + " has correct CSI_Code", code, doc.CSI_Code);
		AssertEquals("Doc with reference " + reference + " has correct CSI_SubType", subtype, doc.CSI_SubType);
		AssertEquals("Doc with reference " + reference + " has correct CSCSI_StatusI_Code", status, doc.CSI_Status);
	}

	protected void AddGuarantees(ZGuid entryInstructionPK, string mrnCode, string entryReference, bool addTransactions = false, bool addOBLTransaction = true, bool setPositiveTranAmount = false, string guaranteeReference = GuaranteeReference, string otherGuaranteeReference = OtherGuaranteeReference)
	{
		var balance = addTransactions ? 1000m + (-50m - 200) : 1000m;
		var transactionAmount = setPositiveTranAmount ? 150m : -50m;
		SetUpGuarantee(guaranteeReference, EUGuaranteeTypeList.Codes.IMP, mrnCode, entryReference, transactionAmount, 1000m, balance, addTransactions, addOBLTransaction);

		balance = addTransactions ? 1000m + (-540m - 200) : 1200m;
		transactionAmount = setPositiveTranAmount ? 640m : -540m;
		SetUpGuarantee(otherGuaranteeReference, EUGuaranteeTypeList.Codes.IMP, mrnCode, entryReference, transactionAmount, 1200m, balance, addTransactions, addOBLTransaction);

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;

		GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstructionPK, guaranteeReference), (entryInstructionPK, otherGuaranteeReference), (entryInstruction2.PK, guaranteeReference));
		declaration.Guarantees[0].PW_BondAmount = 1000m;
		declaration.Guarantees[1].PW_BondAmount = 1200m;
		declaration.Guarantees[2].PW_BondAmount = 1500m;
	}

	protected TestEdiMessage AddSentMessageAndEDocs()
	{
		var sentInterchange = SetSentInterchange(entryHeader, InterchangeID);
		sentInterchange.EI_To = SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub;
		var sentMessage = (TestEdiMessage)sentInterchange.ContainedMessages[0];

		var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "sentDoc1.txt", "CIV");
		var pivot1 = entryHeader.EDocPivotCollection.AddNew();
		pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;
		var messagePivot1 = Factory.New<GenPivot>();
		messagePivot1.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
		messagePivot1.XX_Relation1ID = pivot1.PK;
		messagePivot1.XX_Relation1TableCode = pivot1.TablePrefix;
		messagePivot1.XX_Relation2ID = sentMessage.PK;
		messagePivot1.XX_Relation2TableCode = sentMessage.TablePrefix;

		var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "sentDoc2.txt", "CIV");
		var pivot2 = entryHeader.EDocPivotCollection.AddNew();
		pivot2.CSD_StorageDocReference = eDoc2.UniqueKey;
		var messagePivot2 = Factory.New<GenPivot>();
		messagePivot2.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
		messagePivot2.XX_Relation1ID = pivot2.PK;
		messagePivot2.XX_Relation1TableCode = pivot2.TablePrefix;
		messagePivot2.XX_Relation2ID = sentMessage.PK;
		messagePivot2.XX_Relation2TableCode = sentMessage.TablePrefix;

		Factory.Save();
		declaration.DocManagerInfo.Save();
		entryHeader.EDocPivotCollection.Reload(true);

		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		Factory.Save();

		return sentMessage;
	}

	protected (CusTempStorageRegLineTransaction transaction1, CusTempStorageRegLineTransaction transaction2, CusTempStorageRegLineTransaction transaction3) SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption(ZString internalReferenceType)
	{
		var entryReference = entryHeader.CH_BGMReference;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = "reference";
		var regLine = Factory.New<CusTempStorageRegLine>();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_SRH = regHeader.PK;
		var regLineTransaction1 = (CusTempStorageRegLineTransaction)regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransaction1.SRT_InternalReferenceNumber = entryReference;
		regLineTransaction1.SRT_InternalReferenceType = internalReferenceType;
		var regLineTransaction2 = (CusTempStorageRegLineTransaction)regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_InternalReferenceNumber = entryReference;
		regLineTransaction2.SRT_InternalReferenceType = internalReferenceType;
		var regLineTransaction3 = (CusTempStorageRegLineTransaction)regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransaction3.SRT_InternalReferenceNumber = entryReference;
		regLineTransaction3.SRT_InternalReferenceType = ZString.Empty;

		return (regLineTransaction1, regLineTransaction2, regLineTransaction3);
	}

	protected void SetUpPremises(CusEntryHeader entryHeader, OrgAddress orgAddress, CusTempStorageRegHeader regHeader, string premiseType = CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility)
	{
		entryHeader.Declaration.CustomsEntryInstructions[0].GoodsLocation.Address.AuthorisationNumber = "9999000002";

		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Type = premiseType;
		premises.SRP_CustomsLocation = "9999000002";
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		regHeader.SRH_SRP_Premises = premises.PK;
	}

	protected (OrgHeader, OrgAddress) SetUpOrganization()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		return (orgHeader, orgAddress);
	}

	protected void SetUpGuaranteeForRegHeader(OrgHeader orgHeader, CusTempStorageRegHeader regHeader)
	{
		var cusGuarantee = Factory.New<CusGuaranteeHeader>();
		cusGuarantee.CPH_Number = "Test1";
		cusGuarantee.CPH_OH_PermitHolder = orgHeader.PK;
		cusGuarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		cusGuarantee.CPH_SubType = "1";
		cusGuarantee.CPH_StartDate = ZDate.BrettsBirthday;

		var commonGuarantee = Factory.New<EU.Business.Declaration.CommonGuarantee>();
		commonGuarantee.Parent = regHeader;
		commonGuarantee.PW_BondNumber = "Test1";
		commonGuarantee.PW_CPH_Guarantee = cusGuarantee.PK;

		var guarantee = regHeader.Guarantee.CusGuarantee;
		var guaranteeLineTransaction = guarantee.CusGuaranteeLineTransactions.AddNew();
		guaranteeLineTransaction.CPL_Reference = "reference";
		guaranteeLineTransaction.CPL_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		guaranteeLineTransaction.CPL_TranValue = 2.0m;
	}

	protected void SetUpDataForConfirmTemporaryStorageGoodsConsumptionLoggerWriteOffTransactionError(CusEntryHeader entryHeader, ZString internalReferenceType)
	{
		(var orgHeader, var orgAddress) = SetUpOrganization();

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = "reference";

		SetUpPremises(entryHeader, orgAddress, regHeader, CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse);

		SetUpGuaranteeForRegHeader(orgHeader, regHeader);

		var regLine = Factory.New<CusTempStorageRegLine>();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_SRH = regHeader.PK;

		var openingRegLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
		openingRegLineTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		openingRegLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		openingRegLineTransaction.SRT_BondAmount = 1.0m;
		openingRegLineTransaction.SRT_GrossWeight = 6;

		var regLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransaction.SRT_InternalReferenceNumber = entryHeader.CH_BGMReference;
		regLineTransaction.SRT_InternalReferenceType = internalReferenceType;
		regLineTransaction.SRT_GrossWeight = 6;
	}

	protected IDisposable SetTemporaryStorageEnabled(bool enabled)
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		return registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enabled);
	}

	protected const string TransactionsAESCommentPrefix = "DUE: ";
}
