using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ZHub;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NctsHeader = Enterprise.Customs.DE.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class DEICustomsAcknowledgementProcessorTest : TestCaseWithFactory
	{
		public void TestSuccessfullyProcessed()
		{
			var interchange = CreateEDIInterchange(LogBookTime, "DE90003480000956", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, "");
			processor.CreateMessagesForInterchange(interchange);
			CombineAssertions(() =>
			{
				AssertEquals("Interchange status", EDIInterchange.Status.Received, interchange.EI_Status);
				AssertEquals("Interchange DeliveredTime", "2020-04-05T10:17:58.2347048+02:00", relatedMessage.Interchange.EI_DeliveredTime.ToISO8601String());

				var docManagerSupport = (IDocManagerSupport)linkedObject.StorageHeader;
				AssertEquals("AllEDocs.Count", 1, docManagerSupport.DocManagerInfo.AllEDocs.Count);
				var eDoc = docManagerSupport.DocManagerInfo.AllEDocs[0];
				AssertEquals("FileName", "SVM-1-DE9000348-0001-DE005866_58660000003234841.pdf", eDoc.FileName);
				AssertEquals("DocType", "CAU", eDoc.DocType);
				AssertEquals("Description", "Report SCPRLI", eDoc.Description);
				AssertEquals("ImageData", Convert.FromBase64String(ImageData), eDoc.GetImageDataReader().ConvertToByteArrayAndCloseStream());
			});
		}

		public void TestEDocsAttachedForLinkedObjectNctsDepartureMovementHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var departureMovement = nctsHeader.MovementHeader;
			relatedMessage.EM_LinkedObject = departureMovement;

			var interchange = CreateEDIInterchange(LogBookTime, "DE90003480000956", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, "");
			processor.CreateMessagesForInterchange(interchange);
			CombineAssertions(() =>
			{
				AssertEquals("Interchange status", EDIInterchange.Status.Received, interchange.EI_Status);
				var docManagerSupport = (IDocManagerSupport)nctsHeader;
				AssertEquals("AllEDocs.Count", 1, docManagerSupport.DocManagerInfo.AllEDocs.Count);
				var eDoc = docManagerSupport.DocManagerInfo.AllEDocs[0];
				AssertEquals("FileName", "SVM-1-DE9000348-0001-DE005866_58660000003234841.pdf", eDoc.FileName);
				AssertEquals("DocType", "CAU", eDoc.DocType);
				AssertEquals("Description", "Report SCPRLI", eDoc.Description);
				AssertEquals("ImageData", Convert.FromBase64String(ImageData), eDoc.GetImageDataReader().ConvertToByteArrayAndCloseStream());
			});
		}

		public void TestEventsCreated()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryHeader = jobDeclaration.ActiveEntryHeaders.AddNew();
			relatedMessage.EM_LinkedObject = entryHeader;
			var interchange = CreateEDIInterchange(LogBookTime, "DE90003480000956", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, nameof(CUSINFReferencedMessageStatus.REJ));

			processor.CreateMessagesForInterchange(interchange);

			AssertEquals(UniversalReferenceConstants.EntryStatus.ERR, entryHeader.Logs.MostRecentLogByEventTime(Events.MessageRejected)?.SL_Reference);
		}

		public void TestSuccessfullyProcessed_MultipleRecordsWithDifferentFactories()
		{
			ErrorReporter.Clear();

			var interchange = CreateEDIInterchange(LogBookTime, "DE90003480000956", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, "");

			var newFactory = new BusinessObjectFactory();
			var linkedObject2 = CreateLinkedObject(newFactory);
			CreateRelatedMessage(newFactory, "DE90003480000957", linkedObject2);
			var interchange2 = CreateEDIInterchange(LogBookTime, "DE90003480000957", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, "", newFactory);

			processor.CreateMessagesForInterchange(interchange);
			processor.CreateMessagesForInterchange(interchange2);
			CombineAssertions(() =>
			{
				AssertEquals("LastMessageReported", ZString.Empty, ErrorReporter.LastMessageReported);
				AssertEquals("interchange status", EDIInterchange.Status.Received, interchange.EI_Status);
				AssertEquals("interchange2 status", EDIInterchange.Status.Received, interchange2.EI_Status);
			});
		}

		public void TestEmptyCustomsData()
		{
			var interchange = CreateEDIInterchange(LogBookTime, "DE90003480000956", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, "");
			interchange.EI_BodyText = ZString.Empty;
			processor.CreateMessagesForInterchange(interchange);
			CombineAssertions(() =>
			{
				AssertEquals("interchange status", EDIInterchange.Status.Error, interchange.EI_Status);
				AssertErrorLog(interchange, "NO DE CUSTOMS DATA");
			});
		}

		public void TestUnknownReferencedMessageIdentifier()
		{
			var interchange = CreateEDIInterchange(LogBookTime, "DE90003580000381", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, "");
			processor.CreateMessagesForInterchange(interchange);
			CombineAssertions(() =>
			{
				AssertEquals("interchange status", EDIInterchange.Status.Error, interchange.EI_Status);
				AssertErrorLog(interchange, "Interchange processing failed because related EDIMessage couldn't be located. Message Number: DE90003580000381. Application Code: DEA");
			});
		}

		public void TestPUnknownApplicationCode()
		{
			var interchange = CreateEDIInterchange(LogBookTime, "DE90003480000956", "XXX", "");
			processor.CreateMessagesForInterchange(interchange);
			CombineAssertions(() =>
			{
				AssertEquals("interchange status", EDIInterchange.Status.Error, interchange.EI_Status);
				AssertErrorLog(interchange, "Interchange processing failed because related EDIMessage couldn't be located. Message Number: DE90003480000956. Application Code: XXX");
			});
		}

		public void TestInvalidLogBookTime()
		{
			var interchange = CreateEDIInterchange("2020-13-05T10:17:58.2347048+02:00", "DE90003480000956", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, "");
			processor.CreateMessagesForInterchange(interchange);
			CombineAssertions(() =>
			{
				AssertEquals("interchange status", EDIInterchange.Status.Error, interchange.EI_Status);
				AssertErrorLog(interchange, $"Invalid LogBookTime. Interchange Number: {interchange.EI_InterchangeNum}.");
			});
		}

		public void TestMultipleMessagesWithSameMessageNumberDifferentCompanies()
		{
			relatedMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			var header = Factory.New<CusTempStorageJobHeader>();
			var messageOnDifferentBranch = Factory.New<AtlasEDIMessage>();
			messageOnDifferentBranch.EM_MessageNum = "DE90003480000956";
			messageOnDifferentBranch.EM_LinkedObject = CUSPRLCusTempStorageDec.New(header);
			messageOnDifferentBranch.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			messageOnDifferentBranch.EM_EI = Factory.New<EDIInterchange>().PK;
			messageOnDifferentBranch.EM_GB = Factory.New<GlbCompany>().Branches.AddNew().PK;
			messageOnDifferentBranch.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var interchange = CreateEDIInterchange(LogBookTime, "DE90003480000956", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, "");
			processor.CreateMessagesForInterchange(interchange);
			CombineAssertions(() =>
			{
				AssertEquals("Related Message Interchange DeliveredTime", "2020-04-05T10:17:58.2347048+02:00", relatedMessage.Interchange.EI_DeliveredTime.ToISO8601String());
				AssertEquals("Message On Different Branch Interchange DeliveredTime", "", messageOnDifferentBranch.Interchange.EI_DeliveredTime.ToISO8601String());
			});
		}

		public void TestReferenceStatusRejected_CusTempStorageDec()
		{
			AssertReferenceStatusRejected(linkedObject, () => linkedObject.STH_MessageStatus);
		}

		public void TestReferenceStatusRejected_JobDeclaration()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertReferenceStatusRejected(jobDeclaration, () => jobDeclaration.JE_MessageStatus);
		}

		public void TestReferenceStatusRejected_CusEntryHeader()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryHeader = jobDeclaration.ActiveEntryHeaders.AddNew();
			AssertReferenceStatusRejected(entryHeader, () => entryHeader.CH_Status);
		}

		public void TestReferenceStatusRejected_NctsHeader()
		{
			var nctsHeader = Factory.New<Integration.Customs.DE.ICusInBondHeader>();
			AssertReferenceStatusRejected((BusinessObject)nctsHeader, () => nctsHeader.EffectiveMessageStatus);
		}

		public void TestReferenceStatusRejected_NctsDepartureMovementHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var departureMovement = nctsHeader.MovementHeader;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_ApplicationCode = Common.Shared.CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader.CPH_Number = "GUA1";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
			guaranteeHeader.CPH_Type = "TRA";
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			guaranteeHeader.CPH_Balance = 1000.0m;
			guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
			var transaction = guaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123456789";

			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondType = NCTS.Business.NctsGuaranteeTypeList.Codes._1;
			guarantee.PW_BondNumber = "GUA1";
			guarantee.PW_BondAmount = 145.0m;
			guarantee.PW_CPH_Guarantee = guaranteeHeader.PK;

			guaranteeHeader.AddTransaction(nctsHeader.MovementHeader.BM_PaperlessInbondNum,
					"NCTS write-off " + nctsHeader.MovementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
					"messageNum",
					ZString.Empty,
					guarantee.PW_BondAmount * -1,
					0,
					status: Customs.Business.PermitTransactionStatusList.Codes.Pending);

			AssertReferenceStatusRejected(departureMovement, () => nctsHeader.EffectiveMessageStatus);

			AssertEquals("After processing GUA1", 2, guaranteeHeader.GetTransactions().Count());
			AssertEquals("After processing GUA1", Customs.Business.PermitTransactionStatusList.Codes.Deleted, guaranteeHeader.GetTransactions().Last().CPL_TransactionStatus);
		}

		public void TestReferenceStatusRejected_CusReconDeclaration()
		{
			var cusReconDeclaration = Factory.New<CusReconDeclaration>();
			AssertReferenceStatusRejected(cusReconDeclaration, () => cusReconDeclaration.CRD_MessageStatus);
		}

		public void TestReferenceStatusSent()
		{
			var interchange = CreateEDIInterchange(LogBookTime, "DE90003480000956", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, nameof(CUSINFReferencedMessageStatus.SNT));
			processor.CreateMessagesForInterchange(interchange);
			AssertEquals("Linked object status", ZString.Empty, linkedObject.STH_MessageStatus);
		}

		public void TestDeleteCusReconEntry()
		{
			AssertCusReconDeletion(true, nameof(CUSINFReferencedMessageStatus.REJ), false);
		}

		public void TestDeleteCusReconEntry_WhenReferencedMessageStatusNotREJ()
		{
			AssertCusReconDeletion(false, nameof(CUSINFReferencedMessageStatus.SNT), false);
		}

		public void TestDeleteCusReconEntry_WhenReferencedMessageStatusNotSpecified()
		{
			AssertCusReconDeletion(false, string.Empty, false);
		}

		public void TestDeleteCusReconEntry_WhenHasEntryNum()
		{
			AssertCusReconDeletion(false, nameof(CUSINFReferencedMessageStatus.REJ), true);
		}

		void AssertCusReconDeletion(bool expectedIsDeleted, string referencedMessageStatus, bool withCusEntryNum)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			if (withCusEntryNum)
			{
				var entryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				entryNumber.CE_EntryNum = "DE90003480000956";
			}

			var reconEntry = Factory.NewWithValidTestData<CusReconEntry>();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			var reconEntryLine = reconEntry.CusReconEntryLines.AddNew();
			var reconEntrySnapshot = reconEntry.CusReconSnapshots.AddNew();
			var reconEntryLineSnapshot = reconEntryLine.CusReconSnapshots.AddNew();
			relatedMessage.EM_LinkedObject = entryHeader;

			var interchange = CreateEDIInterchange(LogBookTime, "DE90003480000956", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, referencedMessageStatus);
			processor.CreateMessagesForInterchange(interchange);

			CombineAssertions(() =>
			{
				AssertEquals("CusReconEntry deleted", expectedIsDeleted, reconEntry.IsDeleted);
				AssertEquals("CusReconEntryLine deleted", expectedIsDeleted, reconEntryLine.IsDeleted);
				AssertEquals("CusReconSnapshot of CusReconEntry deleted", expectedIsDeleted, reconEntrySnapshot.IsDeleted);
				AssertEquals("CusReconSnapshot of CusReconEntryLine deleted", expectedIsDeleted, reconEntryLineSnapshot.IsDeleted);
			});
		}

		public void TestDeleteCusReconEntry_NoCusReconEntry()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			relatedMessage.EM_LinkedObject = entryHeader;

			var interchange = CreateEDIInterchange(LogBookTime, "DE90003480000956", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, nameof(CUSINFReferencedMessageStatus.REJ));
			AssertNoExceptionThrown(() => processor.CreateMessagesForInterchange(interchange));
		}

		protected override void SetUp()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "C0009 Desc");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eun);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, Core.Constants.CountryCodes.Germany, "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			base.SetUp();

			linkedObject = CreateLinkedObject(Factory);
			relatedMessage = CreateRelatedMessage(Factory, "DE90003480000956", linkedObject);
			processor = new DEICustomsAcknowledgementProcessor();
		}
		EDIMessage relatedMessage;
		CUSPRLCusTempStorageDec linkedObject;
		DEICustomsAcknowledgementProcessor processor;

		CUSPRLCusTempStorageDec CreateLinkedObject(BusinessObjectFactory factory)
		{
			var header = factory.New<CusTempStorageJobHeader>();
			return CUSPRLCusTempStorageDec.New(header);
		}

		EDIMessage CreateRelatedMessage(BusinessObjectFactory factory, ZString messageNum, CUSPRLCusTempStorageDec linkedObject)
		{
			relatedMessage = factory.New<AtlasEDIMessage>();
			relatedMessage.EM_MessageNum = messageNum;
			relatedMessage.EM_LinkedObject = linkedObject;
			relatedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			relatedMessage.EM_EI = factory.New<EDIInterchange>().PK;
			return relatedMessage;
		}

		void AssertErrorLog(EDIInterchange interchange, ZString expectedText) => AssertEquals("Erorr Log", expectedText, interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);

		EDIInterchange CreateEDIInterchange(ZString logBookTime, ZString messageIdentifier, ZString appCode, ZString referenceMessageStatus, BusinessObjectFactory factory = null)
		{
			var referenceMessageStatusAttribute = referenceMessageStatus.IsEmpty ? "" : $"<ReferencedMessageStatus>{referenceMessageStatus}</ReferencedMessageStatus>";
			var interchange = (factory ?? Factory).New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.DECustomsAcknowledgementSystem;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = "DEEAES";
			interchange.EI_To = "KDSER";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_BodyText = $@"
<DECustomsData>
	<LogbookTime>{logBookTime}</LogbookTime>
	<CustomsData>
		<CUSINF>
			<AppCode>{appCode}</AppCode>
			<ReferencedMessageIdentifier>{messageIdentifier}</ReferencedMessageIdentifier>
			{referenceMessageStatusAttribute}
		</CUSINF>
	</CustomsData>
	<AttachedDocumentCollection>
		<AttachedDocument>
			<FileName>SVM-1-DE9000348-0001-DE005866_58660000003234841.pdf</FileName>
			<Type>
				<Code>CAU</Code>
				<Description>Report SCPRLI</Description>
			</Type>
			<ImageData>{ImageData}</ImageData>
		</AttachedDocument>
	</AttachedDocumentCollection>
</DECustomsData>";
			return interchange;
		}

		void AssertReferenceStatusRejected(BusinessObject linkedObject, Func<ZString> linkedObjectStatus)
		{
			relatedMessage.EM_LinkedObject = linkedObject;

			var interchange = CreateEDIInterchange(LogBookTime, "DE90003480000956", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, nameof(CUSINFReferencedMessageStatus.REJ));
			processor.CreateMessagesForInterchange(interchange);
			CombineAssertions(() =>
			{
				AssertEquals("Interchange DeliveredTime", ZDateTimeOffset.Empty, relatedMessage.Interchange.EI_DeliveredTime);
				AssertEquals("Linked object status", nameof(CUSINFReferencedMessageStatus.REJ), linkedObjectStatus.Invoke());
				AssertEquals("Related Message status", nameof(CUSINFReferencedMessageStatus.REJ), relatedMessage.EM_Status);
			});
		}

		const string LogBookTime = "2020-04-05T10:17:58.2347048+02:00";
		const string ImageData = "MkE5UlZFQUMtQVpQMlU0RjItSkhKRVNDUkMtWllSSFpOUEYtTFREODQ2TFYtMzlRRDZGTk0tM1Y3VzZNRDgtUTdVMjk5VTI=";
	}
}
