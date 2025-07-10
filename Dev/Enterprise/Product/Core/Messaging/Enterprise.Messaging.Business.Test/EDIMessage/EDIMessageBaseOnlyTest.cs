using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Edifact;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Messaging.Testing
{
	[TestedType(typeof(TestEDIMessage))]
	sealed class EDIMessageBaseOnlyTest : EDIMessageTest
	{
		public void TestMultipleInstancesAroundARow_DoesItAffectStream()
		{
			var b1 = Factory.New<TestEDIMessage>();
			var b2 = Factory.Load<XmlEDIMessage>(b1.PK);
			AssertNotEquals(b1.GetType(), b2.GetType());
			var text = new string('a', 15000);
			b2.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			b2.EM_ReceiveTransmit = "TRX";
			using (var stream = new MemoryStream())
			{
				var blob = ZBlob.FromUTF8(text);
				for (int i = 0; i < blob.Length; i++)
				{
					stream.WriteByte(blob[i]);
				}
				b1.SetEM_MessageTextOrDataSource(stream);
				Factory.Save();
			}

			var newNewFactory = new BusinessObjectFactory();
			var b3 = newNewFactory.Load<XmlEDIMessage>(b1.PK);
			using (var resultReader = b3.GetEM_MessageTextReader())
			{
				var result2 = resultReader.ReadToEnd();
				AssertEquals(text, result2);
			}
		}

		public void TestReloadMessageAndBinStream_DoesItSaveTheStream()
		{
			var text = new string('a', 15000);
			var b1 = Factory.New<XmlEDIMessage>();
			b1.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			b1.EM_ReceiveTransmit = "TRX";
			b1.EM_MessageText = text;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var b2 = newFactory.Load<XmlEDIMessage>(b1.PK);
			using (var reader = b2.GetEM_MessageTextReader())
			{
				var result1 = reader.ReadToEnd();
				AssertEquals(text, result1);
				newFactory.Save();
			}

			var newNewFactory = new BusinessObjectFactory();
			var b3 = newNewFactory.Load<XmlEDIMessage>(b1.PK);
			using (var resultReader = b3.GetEM_MessageTextReader())
			{
				var result2 = resultReader.ReadToEnd();
				AssertEquals(text, result2);
			}
			newNewFactory.Save();

			var newNewNewFactory = new BusinessObjectFactory();
			var b4 = newNewNewFactory.Load<XmlEDIMessage>(b1.PK);
			using (var resultReader = b4.GetEM_MessageTextReader())
			{
				var result3 = resultReader.ReadToEnd();
				AssertEquals(text, result3);
			}
		}

		public void TestPreventSaveOfUDM_TRX_WithNoData()
		{
			var b1 = Factory.New<XmlEDIMessage>();
			b1.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			b1.EM_ReceiveTransmit = "TRX";
			b1.EM_MessageText = "Buying cheese from a noodle factory";
			Factory.Save();

			Db.Connection.ExecuteNonQuery("update dbo.EDIMessage set EM_MessageData = null where EM_PK = @pk", p => p.AddParameter("@pk", SqlDbType.UniqueIdentifier, b1.PK.ToGuid()));
			b1.EM_SystemLastEditTimeUtc = b1.EM_SystemLastEditTimeUtc.AddHours(1);

			AssertExceptionThrown<ZSaveConcurrencyException>(Factory.Save);

			AssertEquals(false, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			ErrorReporter.Clear();
		}

		public void TestDeactivateEDIMessage_WithNoData()
		{
			var b1 = Factory.New<XmlEDIMessage>();
			b1.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			b1.EM_IsActive = false;

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestReaderFromStreamSource()
		{
			var sampleText = "Charles Mingus";
			var b2 = Factory.New<XmlEDIMessage>();
			b2.EM_ReceiveTransmit = "RCV";
			b2.EM_MessageNum = "FOO";
			using (var stream = new MemoryStream())
			{
				var blob = ZBlob.FromUTF8(sampleText);
				for (int i = 0; i < blob.Length; i++)
				{
					stream.WriteByte(blob[i]);
				}
				b2.SetEM_MessageDataSource(new StreamSource(stream));
				Factory.Save();
			}

			var bizo2 = new BusinessObjectFactory().Load<XmlEDIMessage>(b2.PK);
			AssertEquals(sampleText, bizo2.EM_MessageDataAsText);
		}

		class StreamSource : IStreamSource
		{
			public StreamSource(Stream stream)
			{
				this.stream = stream;
			}
			readonly Stream stream;
			public Stream GetStream() => stream;
		}

		class DummyEDIMessage : EDIMessage
		{
			public DummyEDIMessage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
			protected override string GetMessageReferenceNumber()
			{
				return "111";
			}

			public Dictionary<string, Func<Type>> RegisteredLinkedObjectTypesExposed => base.RegisteredLinkedObjectTypes;
		}

		public void TestRegisteredTypeLoad()
		{
			var dummy = Factory.New<DummyEDIMessage>();
			var dummyBO = Factory.New<CargoWise.EntityFramework.Testing.DummyBusinessObject>();

			dummy.EM_LinkedObject = dummyBO;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedMsg = newFactory.Load<DummyEDIMessage>(dummy.PK);

			var loadedBO = (CargoWise.EntityFramework.Testing.DummyBusinessObject)loadedMsg.EM_LinkedObject;

			AssertEquals(dummyBO.PK, loadedBO.PK);
		}

		public void TestRegisteredTypeMappings()
		{
			var msg = Factory.New<DummyEDIMessage>();

			var expectedMappings = new Dictionary<string, Type>()
			{
				{ AsycudaBillSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.ManifestBase.IAsycudaBill>() },
				{ AsycudaManifestHeaderSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.ManifestBase.IAsycudaManifestHeader>() },
				{ CusSCAContainerSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusSCAContainer>() },
				{ CusSCAHouseSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusSCAHouse>() },
				{ CusSCAOceanBillSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusSCAOceanBill>() },
				{ JobDeclarationSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>() },
				{ JobContainerSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonContainer>() },
				{ CusEntryHeaderSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.ICusEntryHeader>() },
				{ CusHAWBSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.ICusHAWB>() },
				{ CusISFHeaderSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.ICusISFHeader>() },
				{ CusMAWBSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.ICusMAWB>() },
				{ CusOutturnHeaderSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusOutturnHeader>() },
				{ CusPermitHeaderSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.ICommonCusPermitHeader>() },
				{ CusReconDeclarationSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseCusReconDeclaration>() },
				{ CusTempStorageDecSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.EU.ICusTempStorageDec>() },
				{ CusUnderbondSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.ICusUnderbond>() },
				{ EDIInterchangeSchema.Constants.TableName, typeof(EDIInterchange) },
				{ ExportAWBHeaderSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Freight.Integration.AWB.IExportAWBHeader>() },
				{ JobConsolSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>() },
				{ CusInBondHeaderSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader>() },
				{ CusInBondMoveHeaderSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.ICusInBondMoveHeader>() },
				{ QuarantineExDocHeaderSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IQuarantineExdocHeader>() },
				{ QuarantineColsHeaderSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IQuarantineColsHeader>() },
				{ CusStorageDocPivotSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusStorageDocPivot>() },
				{ DummyBizoSchema.Constants.TableName, typeof(CargoWise.EntityFramework.Testing.DummyBusinessObject) },
				{ CusExitReportSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.EUExitControl.ICusExitReport>() },
				{ GlbCompanySchema.Constants.TableName, ObjectFactory.GetType<IGlbCompany>() },
				{ CusMiscRequestHeaderSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.KR.ICusMiscRequestHeader>() },
				{ CusPollingTransactionSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.ICusPollingTransaction>() },
				{ GlbExternalPasswordSchema.Constants.TableName, ObjectFactory.GetType<IGlbExternalPassword>() },
				{ OrgRelatedPartySchema.Constants.TableName, typeof(OrgRelatedParty) },
				{ CusGoodsCatalogSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.ICusGoodsCatalog>() },
				{ HVLVConsignmentSchema.Constants.TableName, ObjectFactory.GetType<eTail.Integration.IHVLVConsignment>() },
				{ EDIMessageSchema.Constants.TableName, ObjectFactory.GetType<IEDIMessage>() },
				{ JobShipmentSchema.Constants.TableName, ObjectFactory.GetType<Forwarding.IForwardingShipment>() },
				{ CusBRForeignOperatorSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Customs.BR.ICusBRForeignOperator>() },
				{ JobVoyageSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Freight.IJobVoyage>() },
				{ OrgHeaderSchema.Constants.TableName, typeof(OrgHeader) },
			};

			var registered = msg.RegisteredLinkedObjectTypesExposed;

			AssertEquals(expectedMappings.Count, registered.Count);

			foreach (var mapping in expectedMappings)
			{
				Assert($"Mapping for {mapping.Key} not found", registered.ContainsKey(mapping.Key));
				var bizoType = registered[mapping.Key].Invoke();
				AssertEquals($"Mapping type for {mapping.Key}", mapping.Value, bizoType);
				AssertEquals($"Table from type {bizoType.Name} does not match {mapping.Key} according to BusinessObjectFactory", mapping.Key, BusinessObjectFactory.GetTableNameFromType(bizoType));
			}
		}

		public void TestExceptionOnSave()
		{
			var message = EDIMessageTestFactory.New(Factory);
			var stream = (SubStreamableStream)new MemoryStream();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.NativeDataMessaging;
			message.MessageNumberStrategy = new MockMessageNumberStrategy();
			message.SetEM_MessageTextOrDataSource(stream);
			stream.Dispose();
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			ErrorReporter.Clear();
			AssertEquals(false, message.IsDeleted);
		}

		public void TestDisposeSubStreamableStreamAfterSettingMessageData()
		{
			var message = EDIMessageTestFactory.New(Factory);
			var stream = (SubStreamableStream)new MemoryStream();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.NativeDataMessaging;
			message.MessageNumberStrategy = new MockMessageNumberStrategy();
			message.SetEM_MessageTextOrDataSource(stream);
			stream.Dispose();
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			var errorMessageReported = ErrorReporter.LastMessageReported;
			AssertContains("EDI Message Creation Stack Trace:", errorMessageReported);
			AssertContains("Dispose Stack Trace:", errorMessageReported);
			AssertNull("Don't hardcode a key as this error report is too generic and we don't want everything on the same issue",
				ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestRegistryDefault()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = "XMS";
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageText = new string('a', 500000);
			Factory.Save();
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIMessage), new ZQuery(EDIMessageSchema.EM_MessageData, SQLComparisonOperator.NotEqual, null)));
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIMessage), new ZQuery(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Equal, "")));
		}

		public void TestRegistryAlternate()
		{
			SystemDataRegistry.Instance.EMMessageDataActive.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = "XMS";
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageText = new string('a', 500000);
			Factory.Save();
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIMessage), new ZQuery(EDIMessageSchema.EM_MessageData, SQLComparisonOperator.Equal, null)));
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIMessage), new ZQuery(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.NotEqual, "")));
		}

		public void TestIsXMLMessage()
		{
			CombineAssertions(() =>
			{
				AssertIsXMLMessage(ApplicationCodeList.Codes.UAECustoms, false);
				AssertIsXMLMessage(ApplicationCodeList.Codes.Unknown, false);
				AssertIsXMLMessage(ApplicationCodeList.Codes.SGCustomsCMD, false);

				AssertIsXMLMessage(ApplicationCodeList.Codes.XMS, true);
				AssertIsXMLMessage(ApplicationCodeList.Codes.UniversalDataMessaging, true);
				AssertIsXMLMessage(ApplicationCodeList.Codes.UniversalDataQuery, true);
				AssertIsXMLMessage(ApplicationCodeList.Codes.NativeDataMessaging, true);
				AssertIsXMLMessage(ApplicationCodeList.Codes.NativeDataQuery, true);
				AssertIsXMLMessage(ApplicationCodeList.Codes.SYS, true);
				AssertIsXMLMessage(ApplicationCodeList.Codes.CustomsWare, true);
				AssertIsXMLMessage(ApplicationCodeList.Codes.DECustomsAtlasSystem, true);
				AssertIsXMLMessage(ApplicationCodeList.Codes.DECustomsAesSystem, true);
				AssertIsXMLMessage(ApplicationCodeList.Codes.DECustomsEmcsSystem, true);
				AssertIsXMLMessage(ApplicationCodeList.Codes.AsycudaIncoming_GMD_Message, true);
				AssertIsXMLMessage(ApplicationCodeList.Codes.CNCustomsSingleWindow, true);
				AssertIsXMLMessage(ApplicationCodeList.Codes.GbCustomsEMCS, true);
				AssertIsXMLMessage(ApplicationCodeList.Codes.AirCargoAdvanceScreening, true);
			});
		}

		void AssertIsXMLMessage(string applicationCode, bool expectedResult)
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = applicationCode;
			AssertEquals("message.IsXMLMessage for " + applicationCode, expectedResult, message.IsXMLMessage);
		}

		public void TestNTextFieldGetsBlankedOutWhenUpodatedViaEM_MessageText()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.ForceDeprecatedNTextUsageForTesting = true;
			message.EM_MessageNText = LargeMessageTestHelper.BigChunkOfXML;
			Factory.Save();

			AssertEquals("message.EM_MessageData.dbdatalength()", 0, LargeMessageTestHelper.GetLengthStoredInDB(message, EDIMessageSchema.EM_MessageData));
			AssertEquals("message.EM_MessageText.dbdatalength()", 0, LargeMessageTestHelper.GetLengthStoredInDB(message, EDIMessageSchema.EM_MessageText));
			AssertNotEquals("message.EM_MessageNText.dbdatalength()", 0, LargeMessageTestHelper.GetLengthStoredInDB(message, EDIMessageSchema.EM_MessageNText));

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedMessage = reloadingFactory.Load<EDIMessage>(message.PK);
			AssertEquals("reloadedMessage.EM_MessageText", LargeMessageTestHelper.BigChunkOfXML, reloadedMessage.EM_MessageText);
			reloadedMessage.EM_MessageText = LargeMessageTestHelper.BigChunkOfXML + ".";
			reloadingFactory.Save();

			AssertNotEquals("reloadedMessage.EM_MessageData.dbdatalength()", 0, LargeMessageTestHelper.GetLengthStoredInDB(reloadedMessage, EDIMessageSchema.EM_MessageData));
			AssertEquals("reloadedMessage.EM_MessageText.dbdatalength()", 0, LargeMessageTestHelper.GetLengthStoredInDB(reloadedMessage, EDIMessageSchema.EM_MessageText));
			AssertEquals("reloadedMessage.EM_MessageNText.dbdatalength()", 0, LargeMessageTestHelper.GetLengthStoredInDB(reloadedMessage, EDIMessageSchema.EM_MessageNText));

			AssertEquals("reloadedMessage.EM_MessageText", LargeMessageTestHelper.BigChunkOfXML + ".", reloadedMessage.EM_MessageText);
		}

		public void TestXMLMessageTextUsingPropertyGoesToCompressedFormInMessageData()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = ApplicationCodeList.Codes.NativeDataMessaging;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			AssertEquals("EM_MessageData.Length", 0, message.EM_MessageData.Length);
			message.EM_MessageText = LargeMessageTestHelper.BigChunkOfXML;
			AssertNotEquals("EM_MessageData.Length", 0, message.EM_MessageData.Length);
			Factory.Save();

			var dataLengthInDB = LargeMessageTestHelper.GetLengthStoredInDB(message, EDIMessageSchema.EM_MessageData);
			Assert("message.EM_MessageData.dbdatalength() > 0", dataLengthInDB > 0);
			Assert("message.EM_MessageData.dbdatalength() < (bigChunkOfXML.Length / 5) - ie: Compression kicked in. dataLengthInDB = " + dataLengthInDB.ToString() + ", bigChunkOfXML.Length = " + LargeMessageTestHelper.BigChunkOfXML.Length.ToString(), dataLengthInDB < (LargeMessageTestHelper.BigChunkOfXML.Length / 5));
			var textLengthInDB = LargeMessageTestHelper.GetLengthStoredInDB(message, EDIMessageSchema.EM_MessageText);
			AssertEquals("message.EM_MessageText.dbdatalength()", 0, textLengthInDB);
			var ntextLengthInDB = LargeMessageTestHelper.GetLengthStoredInDB(message, EDIMessageSchema.EM_MessageNText);
			AssertEquals("message.EM_MessageNText.dbdatalength()", 0, ntextLengthInDB);

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedMessage = reloadingFactory.Load<EDIMessage>(message.PK);
			AssertEquals("reloadedMessage.EM_MessageText", LargeMessageTestHelper.BigChunkOfXML, reloadedMessage.EM_MessageText);
		}

		public void TestVeryLargeXMLMessageTextUsingTextReaderGoesToCompressedFormInMessageData()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			long xmlLength;
			using (var textSource = new MemoryStream(MessageEncoding.UTF8WithoutBOM.GetBytes(LargeMessageTestHelper.BiggerChunkOfXML)))
			{
				xmlLength = textSource.Length;
				message.SetEM_MessageTextSource(new TextReaderSource(textSource));
				Factory.Save();
			}

			var dataLengthInDB = LargeMessageTestHelper.GetLengthStoredInDB(message, EDIMessageSchema.EM_MessageData);
			Assert("message.EM_MessageData.dbdatalength() > 0", dataLengthInDB > 0);
			Assert("message.EM_MessageData.dbdatalength() < (biggerChunkOfXML.Length / 5) - ie: Compression kicked in. dataLengthInDB = " + dataLengthInDB.ToString() + ", biggerChunkOfXML.Length = " + xmlLength.ToString(), dataLengthInDB < (xmlLength / 5));
			var textLengthInDB = LargeMessageTestHelper.GetLengthStoredInDB(message, EDIMessageSchema.EM_MessageText);
			AssertEquals("message.EM_MessageText.dbdatalength()", 0, textLengthInDB);
			var ntextLengthInDB = LargeMessageTestHelper.GetLengthStoredInDB(message, EDIMessageSchema.EM_MessageNText);
			AssertEquals("message.EM_MessageNText.dbdatalength()", 0, ntextLengthInDB);

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedMessage = reloadingFactory.Load<EDIMessage>(message.PK);
			using (var textReader = reloadedMessage.GetEM_MessageTextReader())
			{
				var messageText = textReader.ReadToEnd();
				AssertEquals("reloadedMessage.GetEM_MessageTextReader().ReadToEnd()", LargeMessageTestHelper.BiggerChunkOfXML, messageText);
			}
		}

		public void TestMidSizeXMLMessageTextUsingTextReaderStoresUnCompressedInMessageData()
		{
			// This is not a business requirement for us in this scope, test is
			// here to exhibit current behaviour to anyone debugging compression issues.
			// Happy to have everything compressed in the future.
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			long xmlLength;
			using (var textSource = new MemoryStream(MessageEncoding.UTF8WithoutBOM.GetBytes(LargeMessageTestHelper.BigChunkOfXML)))
			{
				xmlLength = textSource.Length;
				message.SetEM_MessageTextSource(new TextReaderSource(textSource));
				Factory.Save();
			}

			var dataLengthInDB = LargeMessageTestHelper.GetLengthStoredInDB(message, EDIMessageSchema.EM_MessageData);
			AssertEquals("message.EM_MessageData.dbdatalength()", xmlLength, dataLengthInDB);
			var textLengthInDB = LargeMessageTestHelper.GetLengthStoredInDB(message, EDIMessageSchema.EM_MessageText);
			AssertEquals("message.EM_MessageText.dbdatalength()", 0, textLengthInDB);
			var ntextLengthInDB = LargeMessageTestHelper.GetLengthStoredInDB(message, EDIMessageSchema.EM_MessageNText);
			AssertEquals("message.EM_MessageNText.dbdatalength()", 0, ntextLengthInDB);

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedMessage = reloadingFactory.Load<EDIMessage>(message.PK);
			using (var textReader = reloadedMessage.GetEM_MessageTextReader())
			{
				var messageText = textReader.ReadToEnd();
				AssertEquals("reloadedMessage.GetEM_MessageTextReader().ReadToEnd()", LargeMessageTestHelper.BigChunkOfXML, messageText);
			}
		}

		public void TestUploadLargeMessage()
		{
			string intputFilePath = LargeMessageTestHelper.CreateTestFile(LargeMessageTestHelper.FileType.Text, 100 * 1024 * 1024);
			long inputFileLength = (new FileInfo(intputFilePath)).Length;
			LargeFileHolder tester = new LargeFileHolder(intputFilePath);

			var message = Factory.New<TestEdiMessage>();
			message.SetEM_MessageTextSource(tester);
			Factory.Save();
			message.Refresh();

			AssertEquals(LargeMessageHelper.ShortTextSizeLimit, message.EM_MessageTextShort.Length);
			AssertEquals(LargeMessageHelper.DetailTextSizeLimit, message.EM_MessageTextDetail.Length);

			string outputBodyPath = Temp.GetTempFileName();
			using (StreamWriter writer = new StreamWriter(outputBodyPath))
			{
				using (TextReader reader = message.GetEM_MessageTextReader())
				{
					writer.AddStream(reader);
				}
			}

			AssertEquals(inputFileLength, new FileInfo(outputBodyPath).Length);
			File.Delete(intputFilePath);
			File.Delete(outputBodyPath);
		}

		public void TestBranch()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_GB = GlbBranch.CurrentBranch.PK;

			var iediMessage = message as IEDIMessage;
			AssertNotNull(iediMessage);
			AssertEquals(GlbBranch.CurrentBranch.PK, iediMessage.Branch.PK);
			AssertEquals(GlbCompany.CurrentCompany.PK, iediMessage.Branch.CompanyPK);
		}

		public void TestCompanySetToBranchCompany()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_GB = GlbBranch.CurrentBranch.PK;

			AssertEquals(GlbCompany.CurrentCompany.PK, message.EM_GC);

			message.EM_GB = Guid.Empty;

			Assert(message.EM_GC.IsEmpty);
		}

		public void TestResetToQueuedOnReceivedUniversalXMLMessageShouldNotRemoveMessageTypeAndSubType()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_Status = EDIMessage.Status.Failed;
			Factory.Save();

			message.ResetToQueuedStatus();

			CombineAssertions(delegate
			{
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
			});
		}

		public void TestEDIMessageWithXmlContentIsNotFormattedWithTheEDIMessageFormatter()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;

			string messageText = @"<UniversalShipment>
  <Shipment>
	<SomeStuff>l:'sk?'dq'+++</SomeStuff>
  </Shipment>
</UniversalShipment>";

			message.EM_MessageText = messageText;

			AssertEquals("message.EM_MessageTextDetail", messageText, message.EM_MessageTextDetail);

			message.EM_MessageType = EDIMessageTypeList.Codes.XMS;

			AssertEquals("message.EM_MessageTextDetail", messageText, message.EM_MessageTextDetail);
		}

		public void TestApplicationCodeWithDescription()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			AssertEquals("message.ApplicationCodeWithDescription", "UDM - Universal Data Messaging", message.ApplicationCodeWithDescription);
		}

		public void TestStatusWithDescription()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			AssertEquals("message.MessageStatusWithDescription", "QUE - Queued", message.MessageStatusWithDescription);
		}

		public void TestMessageTypeWithDescription()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			AssertEquals("message.MessageTypeWithDescription", "XDC - XML Data Content", message.MessageTypeWithDescription);
		}

		public void TestMessageStreamNotDisposedBeforeSave()
		{
			using (SystemDataRegistry.Instance.EMMessageDataActive.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var message = Factory.NewWithValidTestData<TestEDIMessage>();
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

				using (var stream = new CargoWise.IO.Shim.SubStreamableStream())
				{
					var str = "ABCDEFGHIJKLMNOPQ";
					stream.Write(new UTF8Encoding().GetBytes(str), 0, str.Length);
					message.SetEM_MessageTextOrDataSource(stream);

					using (message.GetEM_MessageTextReader())
					{
						//By default the underlying stream should not be disposed, it gets disposed on save;
					}
				}

				AssertExceptionThrown<ZSaveException>(Factory.Save);
			}
		}

		public void TestEmptyMessageData()
		{
			var message1 = Factory.New<TestEDIMessage>();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_MessageType = EDIMessageTypeList.Codes.FHL;
			message1.EM_MessageSubType = EDIMessageSubTypeList.Codes.Consols;

			AssertExceptionThrown<ZSaveConcurrencyException>(Factory.Save);

			AssertMultilineASCIIEquals($@"EM_MessageData cannot be empty when EM_ReceiveTransmit = TRX and EM_ApplicationCode = UDM.
Type: Enterprise.Messaging.Testing.EDIMessageBaseOnlyTest+TestEDIMessage
Message Number: test
Message Type: FHL
Message SubType: CON
Branch: BNE
Company: EDI
EM_MessageNText Length: 0
Organisation: ", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var message2 = new BusinessObjectFactory().NewWithValidTestData<TestEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_MessageText = "BLAH";
			AssertNoExceptionThrown(message2.Factory.Save);

			void SaveMessageWithStream(string messageText)
			{
				var message3 = new BusinessObjectFactory().New<TestEDIMessage>();
				message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
				message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				using (var stream = new CargoWise.IO.Shim.SubStreamableStream())
				{
					stream.Write(new UTF8Encoding().GetBytes(messageText), 0, messageText.Length);
					message3.SetEM_MessageTextOrDataSource(stream);
					if (string.IsNullOrEmpty(messageText))
					{
						AssertExceptionThrown<ZSaveConcurrencyException>(message3.Factory.Save);
					}
					else
					{
						AssertNoExceptionThrown(message3.Factory.Save);
					}

					var errorReports = string.IsNullOrEmpty(messageText) ? 1 : 0;
					AssertEquals(errorReports, ErrorReporter.TotalErrorCount);
					ErrorReporter.Clear();
				}
			}

			using (SystemDataRegistry.Instance.EMMessageDataActive.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				SaveMessageWithStream("ABCDEFGHIJKLMNOPQ");
				SaveMessageWithStream("");
			}

			using (SystemDataRegistry.Instance.EMMessageDataActive.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				SaveMessageWithStream("ABCDEFGHIJKLMNOPQ");
				SaveMessageWithStream("");
			}
		}

		public void TestMessageSubTypeWithDescription()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Consols;
			AssertEquals("message.MessageSubTypeWithDescription", "CON - Consols", message.MessageSubTypeWithDescription);
		}

		public void TestMessageSubTypeDescription()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Consols;
			AssertEquals("message.EM_MessageSubTypeDescription for Consols", EDIMessageSubTypeList.Descriptions.Consols, message.EM_MessageSubTypeDescription);

			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			AssertEquals("message.EM_MessageSubTypeDescription for XmlUniversalEvent", EDIMessageSubTypeList.Descriptions.XmlUniversalEvent, message.EM_MessageSubTypeDescription);

			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			AssertEquals("message.EM_MessageSubTypeDescription for XmlUniversalShipment", EDIMessageSubTypeList.Descriptions.XmlUniversalShipment, message.EM_MessageSubTypeDescription);

			var testMessage = Factory.New<TestEdiMessage>();
			testMessage.EM_MessageSubType = "TTT";
			AssertEquals("Test Message", "TEST MESSAGE", testMessage.EM_MessageSubTypeDescription);

			testMessage.EM_MessageSubType = "INV";
			AssertEquals("Test Message", "INV", testMessage.EM_MessageSubTypeDescription);
		}

		public void TestEmptyMessageData_ConcurrencyError()
		{
			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var job1 = factory1.New<Forwarding.IForwardingShipment>();
			factory1.Save();
			var message = factory2.New<TestEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			//cause concurrency error in factory2 on IsValid column
			{
				var job2 = factory2.Load<Forwarding.IForwardingShipment>(job1.PK);
				((ILightValidationInternals)job2).IsValid = true;
				((ILightValidationInternals)job1).IsValid = false;
				job1.JS_A_RCV = ZDateTime.Now.AddDays(2);
				factory1.Save();
			}

			var previousMaxChunkSize = ZLargeColumnSaver.MaxChunkSize;
			try
			{
				ZLargeColumnSaver.MaxChunkSize = 3;
				var messageText = "ABCDEFG";
				using (var stream = new CargoWise.IO.Shim.SubStreamableStream())
				{
					stream.Write(new UTF8Encoding().GetBytes(messageText), 0, messageText.Length);
					message.SetEM_MessageTextOrDataSource(stream);
					AssertNoExceptionThrown(message.Factory.Save);
					AssertEquals("Error report when the ZBlob for EM_MessageData has not been saved", 0, ErrorReporter.TotalErrorCount);
				}
			}
			finally
			{
				ZLargeColumnSaver.MaxChunkSize = previousMaxChunkSize;
			}
		}

		public void TestLargeMessageText_ConcurrencyError()
		{
			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var job1 = factory1.New<Forwarding.IForwardingShipment>();
			factory1.Save();
			var message = factory2.New<TestEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			var job2 = factory2.Load<Forwarding.IForwardingShipment>(job1.PK);
			((ILightValidationInternals)job2).IsValid = true;
			((ILightValidationInternals)job1).IsValid = false;
			job1.JS_A_RCV = ZDateTime.Now.AddDays(2);
			factory1.Save();

			var previousMaxChunkSize = ZLargeColumnSaver.MaxChunkSize;
			try
			{
				ZLargeColumnSaver.MaxChunkSize = 3;
				message.EM_MessageText = "ABCDEFG";
				AssertNoExceptionThrown(message.Factory.Save);
				AssertEquals("Error report when the ZBlob for EM_MessageText has not been saved", 0, ErrorReporter.TotalErrorCount);
			}
			finally
			{
				ZLargeColumnSaver.MaxChunkSize = previousMaxChunkSize;
			}
		}

		public void TestResetStatusToQueuedForReceiveMessage()
		{
			var message1 = Factory.NewWithValidTestData<EDIMessage>();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CIM;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_LinkUniqueID = Guid.NewGuid();
			message1.EM_LinkTable = "BLAH";
			message1.EM_MessageType = EDIMessageTypeList.Codes.FHL;
			message1.EM_MessageSubType = EDIMessageSubTypeList.Codes.Consols;
			message1.EM_Status = EDIMessage.Status.Failed;
			message1.EM_RetryCount = 2;
			Factory.Save();

			message1.ResetToQueuedStatus();

			AssertEquals("EM_Status should be Queued", EDIMessage.Status.Queued, message1.EM_Status);

			AssertEquals("EM_LinkUniqueID should be empty", ZGuid.Empty, message1.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable should be empty", ZString.Empty, message1.EM_LinkTable);
			AssertEquals("EM_MessageType should be empty", ZString.Empty, message1.EM_MessageType);
			AssertEquals("EM_MessageSubType should be empty", ZString.Empty, message1.EM_MessageSubType);
			AssertEquals("EM_RetryCount should be 0", 0, message1.EM_RetryCount.ToZInt());

			var message2 = Factory.NewWithValidTestData<EDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.XMS;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_LinkUniqueID = Guid.NewGuid();
			message2.EM_LinkTable = "BLAH";
			message2.EM_MessageType = EDIMessageTypeList.Codes.XMS;
			message2.EM_MessageSubType = EDIMessageSubTypeList.Codes.Consols;
			message2.EM_Status = EDIMessage.Status.Failed;
			message2.EM_RetryCount = 2;
			Factory.Save();

			message2.ResetToQueuedStatus();

			AssertEquals("EM_Status should be Queued", EDIMessage.Status.Queued, message2.EM_Status);

			AssertNotEquals("EM_LinkUniqueID should not be empty", ZGuid.Empty, message2.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable should not be empty", "BLAH", message2.EM_LinkTable);
			AssertEquals("EM_MessageType should not be empty", EDIMessageTypeList.Codes.XMS, message2.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be empty", EDIMessageSubTypeList.Codes.Consols, message2.EM_MessageSubType);
			AssertEquals("EM_RetryCount should be 0", 0, message2.EM_RetryCount.ToZInt());
		}

		public void TestResetStatusToQueuedForTransmitMessage()
		{
			EDIMessage message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.MessageNumberStrategy = new MockMessageNumberStrategy();
			message.EM_LinkUniqueID = Guid.NewGuid();
			message.EM_LinkTable = "BLAH";
			message.EM_MessageType = "BAR";
			message.EM_MessageSubType = "ETC";
			message.EM_Status = "FOO";
			message.EM_RetryCount = 2;
			Factory.Save();

			message.ResetToQueuedStatus();

			AssertEquals("EM_Status should be Queued", EDIMessage.Status.Queued, message.EM_Status);

			AssertNotEquals("EM_LinkUniqueID should not be empty", ZGuid.Empty, message.EM_LinkUniqueID);
			AssertNotEquals("EM_LinkTable should not be empty", ZString.Empty, message.EM_LinkTable);
			AssertNotEquals("EM_MessageType should not be empty", ZString.Empty, message.EM_MessageType);
			AssertNotEquals("EM_MessageSubType should not be empty", ZString.Empty, message.EM_MessageSubType);
			AssertEquals("EM_RetryCount should be 0", 0, message.EM_RetryCount.ToZInt());
		}

		public void TestCheckCorrectUseOfLinkTableDeveloperError()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();

			var containedMessage = interchange.ContainedMessages.AddNew();
			var acknowledgementMessage = interchange.InterchangeAcknowledgementMessages.AddNew();

			containedMessage.EM_LinkedObject = interchange;
			AssertContains("Developer error reported", "EM_EI and EM_LinkUniqueID have the same GUID.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			containedMessage.EM_LinkUniqueID = ZGuid.Empty;
			containedMessage.EM_LinkTable = "";

			acknowledgementMessage.EM_EI = interchange.PK;
			AssertContains("Developer error reported", "EM_EI and EM_LinkUniqueID have the same GUID.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			acknowledgementMessage.EM_EI = ZGuid.Empty;

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = "BAR";
			message.EM_MessageSubType = "ETC";
			message.EM_Status = "FAL";
			message.EM_LinkUniqueID = Guid.NewGuid();
			message.EM_LinkTable = "BLAH";

			message.EM_EI = interchange.PK;
			AssertNotContains("Developer error reported", "EM_EI and EM_LinkUniqueID have the same GUID.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestResetStatusToQueuedForInterchangeMessageThatHasBeenSentToeHub()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_BodyText = "BODY";
			interchange.EI_FooterText = "FOOTER";
			interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.XMS;
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			interchange.EI_Status = "HQU";
			Factory.Save();

			interchange.EI_Status = "FAL";
			Factory.Save();

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.MessageNumberStrategy = new MockMessageNumberStrategy();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_LinkUniqueID = Guid.NewGuid();
			message.EM_LinkTable = "BLAH";
			message.EM_MessageType = "BAR";
			message.EM_MessageSubType = "ETC";
			message.EM_Status = "HQU";
			Factory.Save();

			message.EM_Status = "FAL";
			message.EM_EI = interchange.PK;
			Factory.Save();

			message.ResetToQueuedStatus();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("EM_Status should be Sent", EDIMessage.Status.Sent, message.EM_Status);
				AssertEquals("EI_Status should be eHub Queued", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
				AssertEquals("EI_TransportType should be eHub", EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
				AssertEquals($"Looking for 'Updated status: FAL->{EDIInterchange.Status.eHubQueued}' in interchange.Logs", true, interchange.Logs.GetAllLogs().ToArray<StmALog>().ToList<StmALog>().Exists(x => x.SL_Reference.Contains("|NEW=HQU|OLD=FAL")));

				AssertNotEquals("EM_LinkUniqueID should not be empty", ZGuid.Empty, message.EM_LinkUniqueID);
				AssertNotEquals("EM_LinkTable should not be empty", ZString.Empty, message.EM_LinkTable);
				AssertNotEquals("EM_MessageType should not be empty", ZString.Empty, message.EM_MessageType);
				AssertNotEquals("EM_MessageSubType should not be empty", ZString.Empty, message.EM_MessageSubType);
				AssertEquals($"Looking for 'Updated status: FAL->{EDIMessage.Status.Sent}' in message.Logs", true, message.Logs.GetAllLogs().ToArray<StmALog>().ToList<StmALog>().Exists(x => x.SL_Reference.Contains("|NEW=SNT|OLD=FAL")));
			});
		}

		public void TestResetStatusToQueuedForInterchangeMessageThatHasBeenSentToeAdaptor()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_BodyText = "BODY";
			interchange.EI_FooterText = "FOOTER";
			interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.XMS;
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			interchange.EI_Status = "FOO";
			Factory.Save();

			interchange.EI_Status = "AQU";
			Factory.Save();

			interchange.EI_Status = "FAL";
			Factory.Save();

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.MessageNumberStrategy = new MockMessageNumberStrategy();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_LinkUniqueID = Guid.NewGuid();
			message.EM_LinkTable = "BLAH";
			message.EM_MessageType = "BAR";
			message.EM_MessageSubType = "ETC";
			message.EM_Status = "FOO";
			Factory.Save();

			message.EM_EI = interchange.PK;
			message.EM_Status = "AQU";
			Factory.Save();

			message.EM_Status = "FAL";
			Factory.Save();

			message.ResetToQueuedStatus();

			AssertEquals("EM_Status should be Sent", EDIMessage.Status.Sent, message.EM_Status);
			AssertEquals("EI_Status should be eAdaptor Queued", EDIInterchange.Status.eAdaptorQueued, interchange.EI_Status);
			AssertEquals("EI_TransportType should be eAdaptor", EDIInterchange.TransportType.eAdaptor, interchange.EI_TransportType);

			AssertNotEquals("EM_LinkUniqueID should not be empty", ZGuid.Empty, message.EM_LinkUniqueID);
			AssertNotEquals("EM_LinkTable should not be empty", ZString.Empty, message.EM_LinkTable);
			AssertNotEquals("EM_MessageType should not be empty", ZString.Empty, message.EM_MessageType);
			AssertNotEquals("EM_MessageSubType should not be empty", ZString.Empty, message.EM_MessageSubType);
		}

		public void TestResetStatusToQueuedForUniversalMessage()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_BodyText = "BODY";
			interchange.EI_FooterText = "FOOTER";
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XDC;
			interchange.EI_Status = "FOO";
			Factory.Save();

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.MessageNumberStrategy = new MockMessageNumberStrategy();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_LinkUniqueID = Guid.NewGuid();
			message.EM_LinkTable = "BLAH";
			message.EM_MessageType = "BAR";
			message.EM_MessageSubType = "ETC";
			message.EM_Status = "FOO";
			message.EM_EI = interchange.PK;
			Factory.Save();

			message.ResetToQueuedStatus();

			AssertEquals("EM_Status should not be changed", "FOO", message.EM_Status);

			interchange.EI_Status = "QUE";
			Factory.Save();

			interchange.EI_Status = "FOO";
			Factory.Save();

			message.ResetToQueuedStatus();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("EM_Status should be Sent", EDIMessage.Status.Sent, message.EM_Status);
				AssertEquals($"Looking for 'Updated status: FOO->{EDIMessage.Status.Sent}' in message.Logs", true, message.Logs.GetAllLogs().ToArray<StmALog>().ToList<StmALog>().Exists(x => x.SL_Reference.Contains("|NEW=SNT|OLD=FOO")));
			});
		}

		public void TestResetStatusToPreviousQueuedForInterchangeMessage()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_BodyText = "BODY";
			interchange.EI_FooterText = "FOOTER";
			interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.XMS;
			interchange.EI_Status = EDIInterchange.Status.eHubQueued;
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			interchange.EI_TransportType = EDIInterchange.TransportType.eHub;
			Factory.Save();

			var message = Factory.NewWithValidTestData<DummyEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_LinkUniqueID = Guid.NewGuid();
			message.EM_LinkTable = "BLAH";
			message.EM_MessageType = "BAR";
			message.EM_MessageSubType = "ETC";
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_EI = interchange.PK;
			Factory.Save();

			message.ResetToQueuedStatus();
			AssertEquals("EM_Status should not be Changed", EDIMessage.Status.Sent, message.EM_Status);
			AssertEquals("EI_Status should not be Changed", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			AssertEquals("EI_TransportType should not be Changed", EDIInterchange.TransportType.eHub, interchange.EI_TransportType);

			interchange.EI_Status = EDIMessage.Status.Sent;
			Factory.Save();

			message.ResetToQueuedStatus();
			AssertEquals("EM_Status should be SNT", EDIMessage.Status.Sent, message.EM_Status);
			AssertEquals("EI_Status should be HQU", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			AssertEquals("EI_TransportType should not be Changed", EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
		}

		public void TestStatusChangeWithoutSaveIsNotLogged()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_Status = EDIMessage.Status.Failed;
			AssertEquals("Count of 'Updated status:' in message.Logs", 0, message.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode));
		}

		public void TestStatusChangeDoesNotLogIntermidiateValues()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_Status = EDIMessage.Status.Failed;
			message.EM_Status = EDIMessage.Status.Acknowledged;
			message.EM_Status = EDIMessage.Status.Error;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_Status = EDIMessage.Status.Cancelled;
			Factory.Save();

			AssertEquals("Count of 'Updated status:' in message.Logs", 1, message.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode));
		}

		[TestDate(2009, 1, 2, 13, 20, 20)]
		[TestUtcOffset(8, 0, 0)]
		public void TestEM_MessageDateTime()
		{
			var mock = Factory.NewMoq<EDIMessage>();
			mock.Protected()
				.Setup<string>("GetMessageReferenceNumber")
				.Returns("2312");
			var message = mock.Object;
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			Factory.Save();
			Assert(message.EM_SystemCreateTimeUtc.IsValid);

			message.ResetMessageDateTimeForTesting();
			var time = message.EM_MessageDateTime;
			var utcTime = message.EM_SystemCreateTimeUtc;
			AssertEquals(utcTime.AddHours(8), time);
			mock.VerifyAll();
		}

		public void TesteDocs()
		{
			var mock = Factory.NewMoq<EDIMessage>();
			mock.Protected()
				.Setup<string>("GetMessageReferenceNumber")
				.Returns("2312");
			var message = mock.Object;
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;

			var addedEDoc = message.DocManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes("Test Blob Thing"), "dodgy.pdf", "MCD");
			addedEDoc.Description = "Something";
			Factory.Save();
			message.DocManagerInfo.Save();

			var anotherFactory = new BusinessObjectFactory();
			var messageReloaded = anotherFactory.Load<EDIMessage>(message.PK);
			var eDocs = messageReloaded.DocManagerInfo.AllEDocs;
			AssertEquals(1, eDocs.Count);
			var eDoc = eDocs[0];
			AssertEquals("eDoc.FileName", "dodgy.pdf", eDoc.FileName);
			AssertEquals("eDoc.Description", "Something", eDoc.Description);
			AssertEquals("eDoc.DocType", "MCD", eDoc.DocType);
			using (var streamReader = new StreamReader(eDoc.GetImageDataReader()))
			{
				AssertEquals("Encoding.UTF8.GetString(eDoc.ImageData)", "Test Blob Thing", streamReader.ReadToEnd());
			}
			mock.VerifyAll();
		}

		public void TestOnSavedEventIsCalled()
		{
			var mock = Factory.NewMoq<EDIMessage>();
			mock.Protected()
				.Setup<string>("GetMessageReferenceNumber")
				.Returns("2312");
			var message = mock.Object;
			var message_SavedCalled = 0;
			var message_SaveSucceeded = false;
			message.Saved += new EDIMessage.SavedEventHandler((m, b) =>
			{
				message_SavedCalled++;
				message_SaveSucceeded = b;
			});
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			Factory.Save();
			AssertEquals(1, message_SavedCalled);
			AssertEquals(true, message_SaveSucceeded);

			Factory.Save();
			AssertEquals(1, message_SavedCalled);
			AssertEquals(true, message_SaveSucceeded);

			message.EM_MessageText = "H";
			Factory.Save();
			AssertEquals(2, message_SavedCalled);
			AssertEquals(true, message_SaveSucceeded);

			message.Saving +=
				new SavingEventHandler<EDIMessage>((m) => throw new ZSaveException(new ZDataException(new Exception(), ((IBusinessObjectInternals)message).Row, Db.Connection), Factory));
			message.EM_MessageText = ZString.Empty;
			AssertExceptionThrown(typeof(ZSaveException), delegate
			{ Factory.Save(); });
			AssertEquals(3, message_SavedCalled);
			AssertEquals(false, message_SaveSucceeded);
			mock.VerifyAll();
		}

		public void TestOnSavingEventIsCalled()
		{
			var mock = Factory.NewMoq<EDIMessage>();
			mock.Protected()
				.Setup<string>("GetMessageReferenceNumber")
				.Returns("2312");
			var message = mock.Object;
			var message_SavingCalled = 0;
			message.Saving += new SavingEventHandler<EDIMessage>((e) => message_SavingCalled++);
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			Factory.Save();
			AssertEquals(1, message_SavingCalled);

			Factory.Save();
			AssertEquals(1, message_SavingCalled);

			message.EM_MessageText = "H";
			Factory.Save();
			AssertEquals(2, message_SavingCalled);
			mock.VerifyAll();
		}

		public void TestDiagnosticDetails()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_MessageText = "";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_Status = EDIMessage.Status.Failed;

			Factory.Save();

			var expected = string.Format("{{EDIMessage:{{Application Code: UDM, Communication Party Config: 00000000-0000-0000-0000-000000000000, EM_EI: 00000000-0000-0000-0000-000000000000, Request Message: 00000000-0000-0000-0000-000000000000, EM_GB: {2}, EM_GE: {3}, EM_GP: {4}, Is Active: Y, Is Test Message: N, Link Unique ID: 00000000-0000-0000-0000-000000000000, Message Data: System.Byte[], Message Sub Type: XUS, Message Type: XDC, Receive Transmit: RCV, Retry Count: 0, Send With Message Errors: N, Status: FAL, System Create Time Utc: {1}, System Create User: E, System Last Edit Time Utc: {0}, System Last Edit User: E}}}}", message.EM_SystemLastEditTimeUtc, message.EM_SystemCreateTimeUtc, message.EM_GB, message.EM_GE, ZGuid.Empty);
			AssertEquals("Diagnostic Details: ", expected, message.DiagnosticDetails);
		}

		public void TestISourceInfoMembers()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "sender";

			var message = interchange.ContainedMessages.AddNew();
			message.EM_MessageNum = "00023643";
			message.Notes.AddNew(true, "File Name", "blah.txt");

			var info = message as ISourceInfo;
			AssertNotNull(info);
			AssertEquals(BillingDataSource.None.ToString(), info.DataSource);
			AssertEquals("eServices", info.InterfaceName);
			AssertEquals(message.PK, info.EDIMessagePK);
			AssertEquals("blah.txt", info.FileName);
			AssertEquals("sender", info.SenderId);
		}

		public void TestGetAutoEdifactMessageUsingNamedFactory()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ApplicationCode = ""; // to prove we no longer care about it in GetAutoEdifactMessageUsingNamedFactory

			var un_1_912 = new Enterprise.Edifact.D04A.D04AMessageFactory();
			un_1_912.AddRegisteredMessage(
					 new Enterprise.Edifact.MessageFactory.MessageRegistration(
						 typeof(Enterprise.Edifact.D04A.Messages.CONTRL.CONTRLMessage), "UN", "1", "912", "CONTRL"));
			var messageFactory = new MessageFactory(new Enterprise.Edifact.D00A.EdifactD00AMessageFactory(), un_1_912);

			// Look ->->--------------------V------V-V---V
			message.EM_MessageText = "UNH+1+CUSRES:D:00A:UN'BGM+:::687+8010S00001244D+11'DTM+9:200902131330:203'GIS+1'UNT+5+1'";
			var edi = message.GetAutoEdifactMessageUsingNamedFactory(messageFactory);
			AssertNotNull("Should be able to understand a UN:04A:D:CUSRES message, since our factory registered it", edi);
			AssertType(typeof(Enterprise.Edifact.D00A.Messages.CUSRES.CUSRESMessage), edi);

			// Look ->->---------------------------------V------V-V---V
			message.EM_MessageText = "UNH+41627169960100+CONTRL:1:912:UN+<<SYSCAR>>'UCI+2+CNSCHIEFEDI::21+CNSJJB+4'UCM+21+CUSDEC:D:04A:UN:109730+4+C3'FTX+AAI+++CNS CCMI authorisation failure. User not authorised for Agent role'UNT+5+41627169960100'";
			edi = message.GetAutoEdifactMessageUsingNamedFactory(messageFactory);
			AssertNotNull("Should be able to understand a UN:912:1:CONTRL message, since our factory registered it", edi);
			AssertType(typeof(Enterprise.Edifact.D04A.Messages.CONTRL.CONTRLMessage), edi);

			// Look ->->---------------------------------V------V-V---V
			message.EM_MessageText = "UNH+41627169960100+CONTRL:9:999:XX+<<SYSCAR>>'UCI+2+CNSCHIEFEDI::21+CNSJJB+4'UCM+21+CUSDEC:D:04A:UN:109730+4+C3'FTX+AAI+++CNS CCMI authorisation failure. User not authorised for Agent role'UNT+5+41627169960100'";
			edi = message.GetAutoEdifactMessageUsingNamedFactory(messageFactory);
			AssertNull("Should not be able to understand an XX:999:9:CONTRL message using a factory without that registration", edi);
		}

		public void TestEM_LinkedObjectAsCusMAWB()
		{
			var mawbAU = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.AU.ICusMAWB)));
			var messageAU = Factory.NewWithValidTestData<EDIMessage>();
			messageAU.EM_LinkTable = mawbAU.TableName;
			messageAU.EM_LinkUniqueID = mawbAU.PK;

			var mawbNZ = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.NZ.ICusMAWB)));
			var messageNZ = Factory.NewWithValidTestData<EDIMessage>();
			messageNZ.EM_LinkTable = mawbNZ.TableName;
			messageNZ.EM_LinkUniqueID = mawbNZ.PK;

			Factory.Save();

			var reFactory = new BusinessObjectFactory();

			var reMessageAU = reFactory.Load<EDIMessage>(messageAU.PK);
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusMAWB>(), reMessageAU.EM_LinkedObject.GetType());
			var reMessageNZ = reFactory.Load<EDIMessage>(messageNZ.PK);
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.ICusMAWB>(), reMessageNZ.EM_LinkedObject.GetType());
		}

		public void TestFactoryNewSequenceNumber()
		{
			var message1 = Factory.New<EDIMessage>();
			var message2 = Factory.New<EDIMessage>();
			AssertEquals("FactoryNewSequenceNumber should reflect the order in which EDIMessage was Factory.New'd", true, message1.FactoryNewSequenceNumber < message2.FactoryNewSequenceNumber);
		}

		public void TestFactoryNewSequenceNumber_WhenEDIMessageSavedToDb()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			Factory.Save();

			AssertExceptionThrown("FactoryNewSequenceNumber called EDIMessage was saved", typeof(InvalidOperationException), "FactoryNewSequenceNumber should not be called after EDIMessage is saved", () => { var something = message.FactoryNewSequenceNumber; });
		}

		public void TestEM_MessageInterpretation()
		{
			var message = Factory.New<TestEdiMessage>();

			AssertEquals("ShouldShowInterpretation Default", true, message.ShouldShowInterpretation);

			AssertEquals("Message.Notes.HasNotes", false, message.Notes.HasNotes);
			AssertEquals("Message.EM_MessageInterpretation", "", message.EM_MessageInterpretation);
			AssertEquals("Message.Notes.HasNotes", false, message.Notes.HasNotes);

			message.EM_MessageInterpretation = "YAYAYA";
			AssertEquals("Message.EM_MessageInterpretation", "YAYAYA", message.EM_MessageInterpretation);
			AssertEquals("Message.Notes.HasNotes", true, message.Notes.HasNotes);

			message.EM_MessageInterpretation = "NANANA";
			AssertEquals("Message.EM_MessageInterpretation", "NANANA", message.EM_MessageInterpretation);
			AssertEquals("Message.Notes.HasNotes", true, message.Notes.HasNotes);

			message.EM_MessageInterpretation = "";
			AssertEquals("Message.Notes.HasNotes", false, message.Notes.HasNotes);
			AssertEquals("Message.EM_MessageInterpretation", "", message.EM_MessageInterpretation);
			AssertEquals("Message.Notes.HasNotes", false, message.Notes.HasNotes);
		}

		public void TestEM_MessageInterpretationForUniversalXml()
		{
			var message = EDIMessageTestFactory.New(Factory);
			AssertEquals("", message.EM_MessageInterpretation);

			var inputPoop = "Poop";
			message.EM_MessageText = inputPoop;
			AssertEquals(inputPoop, message.EM_MessageInterpretation);

			var inputXml = "<?xml version='1.0'?><UniversalShipment>I am any XML but not a fragment<ChildNode>Baby</ChildNode></UniversalShipment>";
			message.EM_MessageText = inputXml;
			AssertEquals(inputXml, message.EM_MessageInterpretation);

			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageText = inputPoop;
			AssertEquals(inputPoop, message.EM_MessageInterpretation);

			message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;

			message.EM_MessageText = inputXml;
			AssertContains("<span class=\"start-tag\">UniversalShipment</span>", message.EM_MessageInterpretation);
			AssertContains("<div class=\"expander-content\">I am any XML but not a fragment<div>", message.EM_MessageInterpretation);

			message.EM_MessageText = inputPoop;
			AssertContains("<span class=\"start-tag\">UniversalShipment</span>", message.EM_MessageInterpretation); //Cached for re-use.
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestEM_InterchangeStatus()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			testMessage.EM_EI = interchange.PK;
			AssertEquals("Interchange Status", EDIInterchange.Status.Queued, testMessage.EM_InterchangeStatus);
		}

		public void TestEM_User()
		{
			var message = Factory.New<TestEdiMessage>();
			AssertEquals(ZString.Empty, message.EM_User);

			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			AssertEquals(ZString.Empty, message.EM_User);

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "Gummy Bears";
			message.EM_EI = interchange.PK;
			AssertEquals("Gummy Bears", message.EM_User);

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.Inttra;
			AssertEquals("Gummy Bears", message.EM_User);

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.XMS;
			AssertEquals("Gummy Bears", message.EM_User);

			message.EM_MessageType = EDIMessage.ApplicationCodes.XMS;
			AssertEquals("Gummy Bears", message.EM_User);

			message.EM_ApplicationCode = ZString.Empty;
			AssertEquals("Gummy Bears", message.EM_User);

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ContainerManagement;
			AssertEquals("Gummy Bears", message.EM_User);

			message.EM_MessageOwner = "IronMan";
			AssertEquals("Gummy Bears", message.EM_User);

			message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			AssertEquals("Gummy Bears", message.EM_User);
		}

		public void TestSendingUser()
		{
			var user = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "E");
			AssertNotNull("PRE: User", user);

			var message = Factory.New<TestEdiMessage>();
			AssertEquals(ZString.Empty, message.EM_SendingUser);

			message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			AssertEquals(ZString.Empty, message.EM_SendingUser);

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "HYEDAUTST";
			message.EM_EI = interchange.PK;
			AssertEquals(ZString.Empty, message.EM_SendingUser);

			message.EM_SystemCreateUser = "E";
			AssertEquals(user.GS_FullName, message.EM_SendingUser);

			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			AssertEquals(ZString.Empty, message.EM_SendingUser);
		}

		public void TestCreateUserFullName()
		{
			var user = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "E");
			CombineAssertions(() =>
			{
				AssertNotNull("PRE: User", user);

				var message = Factory.New<TestEdiMessage>();
				AssertEquals("CreateUserFullName is Empty", ZString.Empty, message.EM_CreateUserFullName);

				message.EM_SystemCreateUser = "E";
				AssertEquals("CreateUserFullName is valid", user.GS_FullName, message.EM_CreateUserFullName);
			});
		}

		public void TestSender()
		{
			var message = Factory.New<TestEdiMessage>();
			AssertEquals(ZString.Empty, message.EM_InterchangeSender);

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "HYEDAUTST";
			message.EM_EI = interchange.PK;
			AssertEquals("HYEDAUTST", message.EM_InterchangeSender);
		}

		public void TestReceiver()
		{
			var message = Factory.New<TestEdiMessage>();
			AssertEquals(ZString.Empty, message.EM_InterchangeReceiver);

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_To = "HYEDAUTST";
			message.EM_EI = interchange.PK;
			AssertEquals("HYEDAUTST", message.EM_InterchangeReceiver);
		}

		public void TestReceiveTransmitValidation()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			Assert("PreCondition", !testMessage.EM_ReceiveTransmitInfo.HasErrors());
			testMessage.EM_ReceiveTransmit = "XXX";
			Assert(testMessage.EM_ReceiveTransmitInfo.HasErrors());
			testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Assert(!testMessage.EM_ReceiveTransmitInfo.HasErrors());
			testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Assert(!testMessage.EM_ReceiveTransmitInfo.HasErrors());
			testMessage.EM_ReceiveTransmit = ZString.Empty;
			Assert(testMessage.EM_ReceiveTransmitInfo.HasErrors());
		}

		public void TestIsTransmitReceiveSetter()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			Assert("PreCondition", testMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit);
			testMessage.IsTransmitMessage = false;
			Assert(testMessage.EM_ReceiveTransmit == EDIMessage.Direction.Receive);
			testMessage.IsTransmitMessage = true;
			Assert(testMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit);
		}

		public void TestIsTransmitMessageGetter()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			AssertEquals("Initial State", true, testMessage.IsTransmitMessage);
			testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertEquals("State for RCV", false, testMessage.IsTransmitMessage);
			testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals("State for TRX", true, testMessage.IsTransmitMessage);
		}

		public void TestDefaults()
		{
			TestEdiMessage testMessage = Factory.New<TestEdiMessage>();
			Assert("PreCondition", testMessage.EM_MessageNum.IsEmpty);
			AssertEquals("IsActive set to true", true, testMessage.EM_IsActive);
			Assert("Current Branch is not Empty", !testMessage.EM_GB.IsEmpty);
			Assert("Current Department is not Empty", !testMessage.EM_GE.IsEmpty);
			AssertEquals("Default status", EDIMessage.Status.Queued, testMessage.EM_Status);
		}

		public void TestDontSetEscapeCharacterForUNOB()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			var interchange = Factory.New<EDIInterchange>();
			testMessage.EM_EI = interchange.PK;
			interchange.EI_HeaderText = "UNA.  UNBUNOB1060500000100230605010500007504051411134749EDIFICE1";
			AssertEquals("EscapeCharacter", testMessage.CharacterSet.EscapeCharacter, UNCharacterSet.NotDefined);
		}

		public void TestEM_DateTimeInterchangeSent()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "1";
			interchange.EI_To = "1";
			var testMessage = Factory.New<TestEdiMessage>();
			testMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			Assert(testMessage.EM_DateTimeInterchangeSent.IsEmpty);
			testMessage.EM_EI = interchange.PK;
			Assert(testMessage.EM_DateTimeInterchangeSent.IsEmpty);
			Factory.Save();
			AssertEquals(
				Env.Time.GetLocalTimeFromUtc(interchange.EI_SystemCreateTimeUtc.ToDateTime()),
				testMessage.EM_DateTimeInterchangeSent.ToDateTime());
		}

		public void TestUNOACharacterSetWithoutInterchange()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			testMessage.EM_MessageText = "UNH+123";
			AssertEquals("Element delimiter", "+", testMessage.CharacterSet.ElementDelimiter);
		}

		public void TestUNOBCharacterSetWithoutInterchange()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			testMessage.EM_MessageText = "UNH29492800CUSRESD97AUN1";  // hidden characters in this string
			AssertEquals("Element delimiter", new UNOBCharacterSet().ElementDelimiter, testMessage.CharacterSet.ElementDelimiter);
		}

		public void TestEdiMessageNumberPlaceHolderIsRestoredAfterFactoryFailedToSave()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			Assert("PreCondition Empty Message Number", testMessage.EM_MessageNum.IsEmpty);
			testMessage.EM_MessageText = TestMessageNumberPlaceHolderIsRestoredExampleText;
			Assert("PreCondition Place Holder Exists in Message Text", testMessage.EM_MessageText.IndexOf(EDIMessage.MessageNumberPlaceHolder) > -1);
			var testLOCO = Factory.New<RefUNLOCO>();
			testLOCO.RL_RN_NKCountryCode = new ZString();
			try
			{
				Factory.Save();
			}
			catch (ZSaveException)
			{
				AssertEquals("Message should not be in database", false, testMessage.IsInDatabase);
				AssertEquals("Message should be deleted. Users send an original message > save fails > click a menu to send it again and there are two original messages if we dont delete the first message before saving fails",
					true, testMessage.IsDeleted);
			}
		}

		public void TestEM_MessageNumberClearedAfterFactoryFailedToSave()
		{
			var testMessage = Factory.New<TestEDIMessage>();

			testMessage.ShouldClearMessageNumberOnFailureToSave = true;

			Assert("PreCondition Empty Message Number", testMessage.EM_MessageNum.IsEmpty);
			testMessage.EM_ECC_CommunicationPartyConfig = ZGuid.NewZGuid();

			AssertExceptionThrown<ZSaveException>(() => Factory.Save());

			AssertEquals(false, testMessage.IsInDatabase);
			AssertEquals(true, testMessage.EM_MessageNum.IsEmpty);
		}

		public void TestEM_MessageNumberNotClearedAfterFactoryFailedToSave()
		{
			var testMessage = Factory.New<TestEdiMessage>();

			Assert("PreCondition Empty Message Number", testMessage.EM_MessageNum.IsEmpty);
			testMessage.EM_ECC_CommunicationPartyConfig = ZGuid.NewZGuid();

			AssertExceptionThrown<ZSaveException>(() => Factory.Save());

			AssertEquals(false, testMessage.IsInDatabase);
			AssertEquals("Default Behavior, the message number is not cleared on failure to save.", "1", testMessage.EM_MessageNum);
		}

		public void TestClearMessageNumberOnFailureToSave()
		{
			var message = Factory.New<TestEdiMessage>();
			AssertEquals(false, message.ClearMessageNumberOnFailureToSave);
		}

		public void TestCopyPersistentValuesFrom()
		{
			var interchange = Factory.New<EDIInterchange>();
			var testMessage = Factory.New<TestEdiMessage>();
			interchange.ContainedMessages.Add(testMessage);
			testMessage.EM_MessageText = "MESSAGETEXT";
			var clonedMessage = Factory.New<EDIMessage>();
			clonedMessage.CopyPersistentValuesFrom(testMessage);
			AssertEquals(testMessage.EM_MessageText, clonedMessage.EM_MessageText);
			AssertEquals(testMessage.EM_EI, clonedMessage.EM_EI);
			AssertEquals(testMessage.EM_GB, clonedMessage.EM_GB);
		}

		public void TestIsAConfirmingEXDMessage()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			testMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			testMessage.EM_MessageType = "EXD";
			testMessage.EM_MessageText = "UNH+1+CUSDEC:D:99B:UN'BGM+830:::EXD+B00147618/1:3+5'LOC+9+AUSYD::6'LOC+12+NZAKL::6'LOC+28+NZ::6'DTM+129:20041007:102'GIS+N:79:95'GIS+N:107:95'RFF+AWH:A'RFF+ED:AAAAMEJHS'PAC+++N:67:95'PAC+++OT:146:95'TDT+20+++6'NAD+CN+++COATS INDUSTRIAL++AUCKLAND'NAD+GO+66015286036::95'MOA+39::NZD'MOA+63:1000:NZD'UNS+D'CST+1+I::95'FTX+AAA+++SOYA BEANS'LOC+27++AU-NS::6'MEA+WT++KG:100.000'MEA+ABW++KG:100.0000'MOA+63:1000'RFF+HS:12010000'UNS+S'CNT+11:1'CNT+36:0'UNT+29+1'";
			Assert(!testMessage.IsAConfirmingEXDMessage);

			testMessage.EM_MessageText = testMessage.EM_MessageText.Replace("GIS+N:79:95'", "GIS+Y:79:95'");

			Assert(testMessage.IsAConfirmingEXDMessage);
		}

		public void TestAssumeMessageClearIfAcknowledgedAndNoResponse()
		{
			var testMessage = Factory.New<EDIMessage>();
			Assert(!testMessage.AssumeMessageClearIfAcknowledgedAndNoResponse);
		}

		public void TestContainedChecksumPlaceHolder()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			Assert("PreCondition Empty Message Number", testMessage.EM_MessageNum.IsEmpty);
			testMessage.EM_MessageText = TestContainedChecksumPlaceHolderExampleText;
			Assert("PreCondition Place Holder Exists", testMessage.EM_MessageText.IndexOf(EDIMessage.ContainedChecksumPlaceHolder) > -1);
			Assert("PreCondition Resulting Data Not Exists", testMessage.EM_MessageText.IndexOf(TestEdiMessage.UniqueString) == -1);
			Factory.Save();
			Assert("PreCondition Place Holder Not Exists", testMessage.EM_MessageText.IndexOf(EDIMessage.ContainedChecksumPlaceHolder) == -1);
			Assert("PreCondition Resulting Data Exists", testMessage.EM_MessageText.IndexOf(TestEdiMessage.UniqueString) > -1);
		}

		public void TestEM_LinkedObjectAsCusUnderbond()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			var cusUnderbond = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusUnderbond>());
			testMessage.EM_LinkTable = cusUnderbond.TableName;
			testMessage.EM_LinkUniqueID = cusUnderbond.PK;
			AssertEquals("LinkedObject", cusUnderbond, testMessage.EM_LinkedObject);
		}

		public void TestEM_LinkedObjectAsZAAsycudaManifestHeader()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			var header = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.IAsycudaManifestHeader>());
			testMessage.EM_LinkTable = header.TableName;
			testMessage.EM_LinkUniqueID = header.PK;
			AssertEquals("LinkedObject", header, testMessage.EM_LinkedObject);
		}

		public void TestEM_LinkedObjectAsCusTempStorageDec()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			var header = Factory.New<Enterprise.Integration.Customs.EU.ICusTempStorageJobHeader>();
			var dec = Factory.New<Enterprise.Integration.Customs.EU.ICusTempStorageDec>();
			dec.STH_SJH = header.PK;
			testMessage.EM_LinkTable = CusTempStorageDecSchema.Constants.TableName;
			testMessage.EM_LinkUniqueID = dec.PK;
			AssertEquals("LinkedObject", dec, testMessage.EM_LinkedObject);
		}

		public void TestEM_LinkedObjectAsQuarantineExdocHeader()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			var quarantineExDocHeader = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.IQuarantineExdocHeader>();
			testMessage.EM_LinkTable = quarantineExDocHeader.TableName;
			testMessage.EM_LinkUniqueID = quarantineExDocHeader.PK;
			AssertEquals("LinkedObject", quarantineExDocHeader, testMessage.EM_LinkedObject);
		}

		public void TestOwnerReferenceFilledInOnSave()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			testMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + EDIMessage.OwnerReferencePlaceHolder;
			Factory.Save();
			Assert("MessageTextContainsOwnerReference", testMessage.EM_MessageText.Contains("GetOwnerReferenceResult"));
		}

		public void TestSystemCommonAccessReferencePkPlaceholderFilledInOnSave()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			testMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + EDIMessage.SystemCommonAccessReferencePkPlaceholder;
			Factory.Save();
			Assert("Contains PK", testMessage.EM_MessageText.Contains("PrimaryKeyAsString"));
		}

		public void TestAgentReferenceFilledInOnSave()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			testMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + EDIMessage.AgentReferencePlaceHolder;
			Factory.Save();
			Assert("MessageTextContainsAgentReference", testMessage.EM_MessageText.Contains("Agent Reference"));
		}

		public void TestEntryNumberFilledInOnSave()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			testMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + EDIMessage.EntryNumberPlaceHolder;
			Factory.Save();
			Assert("MessageTextContainsEntryNumber", testMessage.EM_MessageText.Contains("GetEntryNumberResult"));
		}

		public void TestDocumentMessageVersionFilledInOnSave()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			testMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + EDIMessage.DocumentMessageVersionPlaceHolder;
			Factory.Save();
			Assert("MessageTextContainsDocumentMessageVersion", testMessage.EM_MessageText.Contains("GetDocumentMessageVersion"));
		}

		public void TestBatchNumberFilledInOnSave()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			testMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + EDIMessage.UniqueBatchNumberPlaceHolder;
			Factory.Save();
			Assert("MessageTextContainsBatchNumber", testMessage.EM_MessageText.Contains("GetBatchNumberResult"));
			testMessage = Factory.New<TestEdiMessage>();
			testMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + EDIMessage.UniqueBatchNumberPlaceHolder;
			testMessage.ShouldReplaceUniqueBatchNumberExposed = false;
			Factory.Save();
			Assert("MessageTextNotContainsBatchNumber", testMessage.EM_MessageText.Contains(EDIMessage.UniqueBatchNumberPlaceHolder));
		}

		public void TestAgentReferenceFilledInOnSave_WhenAgentsRefIsNull()
		{
			var testMessage = Factory.New<TestEdiMessageWithNullAgentsRef>();
			testMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + EDIMessage.AgentReferencePlaceHolder;
			Factory.Save();
			AssertEquals("1" + EDIMessage.AgentReferencePlaceHolder, testMessage.EM_MessageText);
		}

		public void TestClone()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			testMessage.EM_MessageText = "MESSAGETEXT";
			var clonedMessage = (TestEdiMessage)testMessage.Clone();
			AssertEquals("MESSAGETEXT", "MESSAGETEXT", clonedMessage.EM_MessageText);
		}

		public void TestIsPending()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_Status = EDIMessage.Status.Pending;
			AssertEquals(true, message.IsPending);

			message.EM_Status = EDIMessage.Status.Cancelled;
			AssertEquals(false, message.IsPending);
		}

		public void TestMessageAttachments()
		{
			EDIMessage message = Factory.New<TestEdiMessage>();
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			EDIMessageAttach ediMessageAttach = message.MessageAttachments.AddNew();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			message = newFactory.Load<EDIMessage>(message.PK);
			AssertEquals(1, message.MessageAttachments.Count);
			AssertEquals(ediMessageAttach.PK, message.MessageAttachments[0].PK);
		}

		public void TestEM_MessageTextToGetEM_MessageTextReaderDataFlow()
		{
			const string testString = "This is a test string";

			var message = Factory.New<TestEDIMessage>();
			message.EM_MessageText = testString;
			using (var reader = message.GetEM_MessageTextReader(false))
			{
				AssertEquals(message.EM_MessageText, reader.ReadToEnd());
			}

			Factory.Save();
			message.Refresh();

			using (var reader = message.GetEM_MessageTextReader(false))
			{
				AssertEquals(message.EM_MessageText, reader.ReadToEnd());
			}
		}

		public void TestUploadLargeNTextMessage()
		{
			string intputFilePath = LargeMessageTestHelper.CreateTestFile(LargeMessageTestHelper.FileType.NText, 1024 * 1024);
			try
			{
				long inputFileLength = (new FileInfo(intputFilePath)).Length;
				var tester = new LargeFileHolder(intputFilePath);

				var message = Factory.New<TestEDIMessage>();
				message.EM_ApplicationCode = ApplicationCodeList.Codes.XMS;
				message.SetEM_MessageTextSource(tester);
				Factory.Save();
				message.Reload();

				string outputBodyPath = Temp.GetTempFileName();
				try
				{
					using (StreamWriter writer = new StreamWriter(outputBodyPath))
					{
						using (TextReader reader = message.GetEM_MessageTextReader())
						{
							writer.AddStream(reader);
						}
					}

					AssertEquals(inputFileLength, new FileInfo(outputBodyPath).Length);
				}
				finally
				{
					File.Delete(outputBodyPath);
				}
			}
			finally
			{
				File.Delete(intputFilePath);
			}
		}

		public void TestDetailMessageIsFormatted()
		{
			var message = Factory.New<TestEDIMessage>();
			message.EM_MessageText = "This \r is \n is \t a test ?' message'. This" + '\x1f' + "format" + '\x1d' + "I don't" + '\x1c' + " understand?";
			Factory.Save();
			message.Refresh();

			AssertEquals("This  is  is  a test   message\r\n. This:format+I don\r\nt\r\n understand?", message.EM_MessageTextDetail);
		}

		public void TestFormattedMessage()
		{
			var message = Factory.New<TestEDIMessage>();
			message.EM_MessageText = "This \r is \n is \t a test ?' message'. This" + '\x1f' + "format" + '\x1d' + "I don't" + '\x1c' + " understand?";
			Factory.Save();
			message.Refresh();

			AssertEquals("This  is  is  a test   message\r\n. This:format+I don\r\nt\r\n understand?", message.EM_FormattedMessageText);
			using (TextReader reader = message.EM_FormattedMessageTextReader)
			{
				string output = reader.ReadToEnd();
				AssertEquals("This  is  is  a test   message\r\n. This:format+I don\r\nt\r\n understand?", output);
			}

			using (TextReader reader = message.EM_FormattedMessageTextReader)
			{
				string output = reader.ReadToEnd();
				AssertEquals("This  is  is  a test   message\r\n. This:format+I don\r\nt\r\n understand?", output);
			}

			message.EM_MessageText = "This \r is \n is \t a test ?' message'. This" + '\x1f' + "format" + '\x1d' + "I don't" + '\x1c' + " understand";
			Factory.Save();
			using (TextReader reader = message.EM_FormattedMessageTextReader)
			{
				string output = reader.ReadToEnd();
				AssertEquals("This  is  is  a test   message\r\n. This:format+I don\r\nt\r\n understand", output);
			}
		}

		public void TestFormattedMessage_CreateFormattedMessageStream_SetStreamPositionToZero()
		{
			var message = Factory.New<TestEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.CMX;
			message.EM_MessageText = "This \r is \n is \t a test ?' message'. This" + '\x1f' + "format" + '\x1d' + "I don't" + '\x1c' + " understand?";
			Factory.Save();
			message.Refresh();
			AssertEquals("This  is  is  a test   message\r\n. This:format+I don\r\nt\r\n understand?", message.EM_FormattedMessageText);

			TextReader reader = message.EM_FormattedMessageTextReader;
			string output = reader.ReadToEnd();
			AssertEquals("This  is  is  a test   message\r\n. This:format+I don\r\nt\r\n understand?", output);

			message.EM_FormattedMessageTextReader.Dispose();
			reader = message.EM_FormattedMessageTextReader;
			output = reader.ReadToEnd();
			AssertEquals("This  is  is  a test   message\r\n. This:format+I don\r\nt\r\n understand?", output);

			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_FormattedMessageTextReader.Dispose();
			reader = message.EM_FormattedMessageTextReader;
			output = reader.ReadToEnd();
			message.EM_FormattedMessageTextReader.Dispose();
			AssertEquals("This \r is \n is \t a test ?' message'. ThisformatI don't understand?", output);
		}

		public void TestIndentedXMLMessage_NoDeclaration()
		{
			var message = Factory.New<TestEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.CMX;
			var input = @"<Node><Node2><Node3>aaa</Node3></Node2></Node>";
			message.EM_MessageText = input;

			var expected = @"<Node>
  <Node2>
    <Node3>aaa</Node3>
  </Node2>
</Node>";

			AssertEquals(expected, message.EM_MessageTextIndentedXml);
		}

		public void TestIndentedXMLMessage_Declaration()
		{
			var message = Factory.New<TestEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.CMX;
			var input = @"<?xml version=""1.0"" encoding=""UTF-8""?><Node><Node2><Node3>aaa</Node3></Node2></Node>";
			message.EM_MessageText = input;

			var expected = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Node>
  <Node2>
    <Node3>aaa</Node3>
  </Node2>
</Node>";

			AssertEquals(expected, message.EM_MessageTextIndentedXml);
		}

		public void TestStreamReaderDispose()
		{
			string intputFilePath = LargeMessageTestHelper.CreateTestFile(LargeMessageTestHelper.FileType.Text, 1 * 1024 * 1024);
			long inputFileLength = (new FileInfo(intputFilePath)).Length;
			TestLargeFileHolder tester = new TestLargeFileHolder(intputFilePath);

			var message = Factory.New<TestEDIMessage>();
			message.SetEM_MessageTextSource(tester);
			Factory.Save();
			AssertEquals(true, tester.Reader.WasDisposed);

			File.Delete(intputFilePath);
		}

		public void TestNoteTypesList()
		{
			string intputFilePath = LargeMessageTestHelper.CreateTestFile(LargeMessageTestHelper.FileType.Text, 1 * 1024 * 1024);
			try
			{
				long inputFileLength = (new FileInfo(intputFilePath)).Length;
				var tester = new TestLargeFileHolder(intputFilePath);
				var message = Factory.New<TestEDIMessage>();
				message.SetEM_MessageTextSource(tester);
				message.Notes.AddNew(false, PredefinedNoteTypes.Instance.DataImportLogNote.Description, "DataImportLogNote");
				message.Notes.AddNew(false, PredefinedNoteTypes.Instance.MessageInterpretation.Description, "MessageInterpretation");

				Factory.Save();
				CombineAssertions(() =>
				{
					foreach (var item in message.Notes.GetAllNotes().Cast<StmNote>())
					{
						var validator = new StmNoteValidation(item);
						validator.ValidateST_Description();
						AssertEquals("Should contain no error when validating Note: " + item.ST_Description, 0, item.GetErrors().Count());
					}
				});
			}
			finally
			{
				File.Delete(intputFilePath);
			}
		}

		public void TestHumanReadableName()
		{
			AssertEquals("EDI Message", EDIMessageTestFactory.New(Factory).HumanReadableName);
		}

		public void TestXMSMessage()
		{
			//Application Code is XMS
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "EDIEDIDAT";
			interchange.EI_To = "EDIEDIDAT";
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.XMS;

			var message = interchange.ContainedMessages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = "嘿MESSAGE";

			message.Validation.ValidateEM_MessageText();

			var errorMessage = " only accepts Western European languages characters.";
			Assert("MessageText is Western European validation succeeded.", !message.EM_MessageTextInfo.HasError("Message" + errorMessage));

			Factory.Save();

			var loadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertEquals("EM_MessageText", "嘿MESSAGE", loadedMessage.EM_MessageText);
			AssertEquals("EM_MessageDataAsText", "嘿MESSAGE", loadedMessage.EM_MessageDataAsText);

			//Application Code is something else
			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_From = "EDIEDIDAT";
			interchange1.EI_To = "EDIEDIDAT";
			interchange1.EI_ApplicationCode = EDIInterchange.ApplicationCodes.AirCargo;

			var message1 = interchange1.ContainedMessages.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageText = "嘿MESSAGE";

			message1.Validation.ValidateEM_MessageText();

			Assert("Error expected - EMessageText is Western European validation failed.", !message1.EM_MessageTextInfo.HasError("Message" + errorMessage));

			message1.EM_MessageText = "MESSAGE";

			Factory.Save();

			var loadedMessage1 = new BusinessObjectFactory().Load<EDIMessage>(message1.PK);
			AssertEquals("MessageText", "MESSAGE", loadedMessage1.EM_MessageText);
			AssertEquals("MessageText", "", loadedMessage1.EM_MessageDataAsText);
		}

		public void TestCWSMessage()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = ApplicationCodeList.Codes.CustomsWare;
			message.EM_MessageText = "嘿MESSAGE";

			AssertEquals("EM_MessageText", "嘿MESSAGE", message.EM_MessageText);
			AssertEquals("EM_MessageDataAsText", "嘿MESSAGE", message.EM_MessageDataAsText);
		}

		public void TestReadTextGEIMessage()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_MessageText = "MESSAGE";
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GlobalElectronicInvoice;

			AssertEquals("EM_MessageText", "MESSAGE", message.EM_MessageText);
		}

		public void TestReadBlobGEIMessage()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GlobalElectronicInvoice;
			message.EM_MessageText = "嘿MESSAGE";

			AssertEquals("EM_MessageText", "嘿MESSAGE", message.EM_MessageText);
		}

		public void TestAfterNewObjectIsLinkedIsCalledForCasesWhereEM_LinkedObjectIsLoadedInTheGetter()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "X";
			interchange.EI_To = "Y";
			interchange.EI_InterchangeNum = "1";

			var message = Factory.New<TestEDIMessage>();
			message.EM_LinkedObject = interchange;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var messageLoaded = factory2.Load<TestEDIMessage>(message.PK);
			Assert(!messageLoaded.AfterNewObjectIsLinkedCalled);

			var linkedObjectAccessed = messageLoaded.EM_LinkedObject;
			Assert(messageLoaded.AfterNewObjectIsLinkedCalled);
		}

		public void TestEDIMessageShouldSuspendValidation()
		{
			var msg = EDIMessageTestFactory.New(Factory);
			AssertEquals((msg as ISourceInfo).ShouldSuspendValidation, true);
		}

		public void TestDeleteEDIMessageWhichIsPreviouslyInDatabase()
		{
			var testMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			testMessage.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			testMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			testMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			Factory.Save();
			AssertEquals("Message.IsInDatabase should be true", true, testMessage.IsInDatabase);
			testMessage.Delete();
			AssertContains("Error regarding EDIMessage shouldn't be deleted should have been reported.", "The previously persisted EDIMessage shouldn't be deleted.", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestDeleteEDIMessageWhoseLinkedEDIInterchangeIsPreviouslyInDatabase()
		{
			var testInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			testInterchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XDC;
			Factory.Save();
			AssertEquals("Interchange.IsInDatabase should be true", true, testInterchange.IsInDatabase);
			var testMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			testMessage.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			testMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			testMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			testMessage.EM_EI = testInterchange.PK;
			testMessage.Delete();
			AssertContains("Error regarding EDIMessage shouldn't be deleted should have been reported.", "The EDIMessage linked to a previously persisted EDIInterchange shouldn't be deleted.", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestDeleteEDIMessageWhichIsATransmitAndSent()
		{
			var testMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			testMessage.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			testMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			testMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			testMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			testMessage.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();
			testMessage.Delete();
			AssertContains("Error regarding EDIMessage shouldn't be deleted should have been reported.", "The previously persisted EDIMessage shouldn't be deleted.", ErrorReporter.LastKeyReported);
			AssertContains("Error regarding EDIMessage shouldn't be deleted should have been reported with correct message.", "You are attempting to delete an EDIMessage that has been successfully sent. It is required for support purposes.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestEM_LinkedObjectAsCusPermitHeader()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			var permit = Factory.New<Enterprise.Integration.Customs.EU.ICusPermitHeader>();
			testMessage.EM_LinkTable = CusPermitHeaderSchema.Constants.TableName;
			testMessage.EM_LinkUniqueID = permit.PK;
			AssertEquals("LinkedObject", permit, testMessage.EM_LinkedObject);
		}

		public void TestEM_LinkedObjectAsCusReconDeclaration()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			var cusReconDeclaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusReconDeclaration>());
			testMessage.EM_LinkTable = cusReconDeclaration.TableName;
			testMessage.EM_LinkUniqueID = cusReconDeclaration.PK;
			AssertEquals("LinkedObject", cusReconDeclaration, testMessage.EM_LinkedObject);
		}

		public void TestEM_LinkedObjectAsConsolidatedDeclaration()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			var cusReconDeclaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IConsolidatedDeclaration>());
			cusReconDeclaration[CusReconDeclarationSchema.CRD_ApplicationCode.Name] = "TSW";
			testMessage.EM_LinkTable = cusReconDeclaration.TableName;
			testMessage.EM_LinkUniqueID = cusReconDeclaration.PK;
			Assert("LinkedObject", testMessage.EM_LinkedObject is Enterprise.Integration.Customs.IConsolidatedDeclaration);
		}

		public void TestEM_LinkedObjectAsCusExitReport()
		{
			var testMessage = Factory.New<TestEdiMessage>();
			var exitReport = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.EUExitControl.ICusExitReport>());
			testMessage.EM_LinkTable = exitReport.TableName;
			testMessage.EM_LinkUniqueID = exitReport.PK;
			AssertEquals("LinkedObject", exitReport, testMessage.EM_LinkedObject);
		}

		public void TestErrorReportedWhenMessageTextIsEmpty()
		{
			AssertLastKeyReportedWhenSaving(EDIMessage.ApplicationCodes.USCustomsImport);
			AssertLastKeyReportedWhenSaving(EDIMessage.ApplicationCodes.CAIMP);
			AssertLastKeyReportedWhenSaving(EDIMessage.ApplicationCodes.CMR);
			AssertLastKeyReportedWhenEmptyMessageText(EDIMessage.ApplicationCodes.USCustomsImport);
			AssertLastKeyReportedWhenEmptyMessageText(EDIMessage.ApplicationCodes.CAIMP);
			AssertLastKeyReportedWhenEmptyMessageText(EDIMessage.ApplicationCodes.CMR);
		}

		void AssertLastKeyReportedWhenSaving(string applicationCode)
		{
			try
			{
				var message = Factory.New<TestEDIMessage>();
				message.EM_ApplicationCode = applicationCode;
				message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				message.OnSaving();
				AssertEquals($"Invalid message text in customs message. EM_ApplicationCode: {applicationCode}", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		void AssertLastKeyReportedWhenEmptyMessageText(string applicationCode)
		{
			try
			{
				var message = Factory.New<TestEDIMessage>();
				message.EM_ApplicationCode = applicationCode;
				message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				message.EM_MessageText = ZString.Empty;
				AssertEquals($"Invalid message text in customs message. EM_ApplicationCode: {applicationCode}", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestErrorReportedWhenMessageDataIsEmptyForUSeBond()
		{
			try
			{
				var message = Factory.New<TestEDIMessage>();
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeBond;
				message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				message.EM_MessageText = ZString.Empty;
				AssertExceptionThrown<ZSaveConcurrencyException>(Factory.Save);
				AssertEquals($"Invalid values in EDIMessage. EM_ApplicationCode: {EDIMessage.ApplicationCodes.USeBond}", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestErrorReportedWhenMessageSubTypeNotConfiguredInMessagePurgeSettings()
		{
			try
			{
				var message = Factory.New<TestEDIMessage>();
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.GlobalElectronicInvoice;
				message.EM_MessageSubType = "!!!";
				Factory.Save();
				AssertEquals("Developer error should be reported", "Please add !!! to eHubMessagingRegistry where ApplicationCode is GEI", ErrorReporter.LastExceptionReported.Message);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		[ExpectNoExceptions]
		public void TestStatusChangeSuccessfulForLargeEBondMessage()
		{
			Db.Connection.ExecuteNonQuery(LargeEBondMessageInsertScript.Script);
			var reloadedMessage = Factory.Load<EDIMessage>(Guid.Parse(LargeEBondMessageInsertScript.EDIMessagePK));
			reloadedMessage.ResetToQueuedStatus();
			Factory.Save();
		}

		public void TestErrorReportedWhenIncorrectNumberAllocatedForAMSMessage()
		{
			try
			{
				var message = Factory.New<TestEDIMessage>();
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.AMS;
				message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				message.EM_LinkUniqueID = new ZGuid(Guid.NewGuid());
				message.EM_LinkTable = CusInBondMoveHeaderSchema.Constants.TableName;
				message.EM_MessageNum = "ABC123";
				Factory.Save();
				AssertEquals("ErrorReporter.LastKeyReported", "Incorrect number allocated for AMS message", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestQueuedMessagesQueryUsesIndexSeekAndNoLookup()
		{
			Db.Connection.ExecuteNonQuery(@"
				WITH
					t0(i) AS (SELECT 0 UNION ALL SELECT 0),
					t1(i) AS (SELECT 0 FROM t0 a, t0 b),
					t2(i) AS (SELECT 0 FROM t1 a, t1 b),
					t3(i) AS (SELECT 0 FROM t2 a, t2 b),
					t4(i) AS (SELECT 0 FROM t3 a, t3 b),
					t5(i) AS (SELECT 0 FROM t4 a, t4 b),
					tt(i) AS (select TOP 10000 0 from t5),
					Source(id) AS (select cast(row_number() over (order by i) as int) from tt)
				INSERT INTO dbo.EDIMessage(EM_PK, EM_GB, EM_GE, EM_Status, EM_ReceiveTransmit, EM_ApplicationCode, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser)
				SELECT newid(), @branch, @dept, 'SNT', 'TRX', 'CTR', GETUTCDATE(), 'DAT', GETUTCDATE(), 'DAT' from Source
				UNION ALL
				SELECT newid(), @branch, @dept, 'QUE', 'TRX', 'CTR', GETUTCDATE(), 'DAT', GETUTCDATE(), 'DAT' from Source WHERE id % 10 = 0
				UNION ALL
				SELECT newid(), @branch, @dept, 'PPS', 'RCV', 'TRC', GETUTCDATE(), 'DAT', GETUTCDATE(), 'DAT' from Source WHERE id % 10 = 0

				UPDATE STATISTICS EDIMessage WITH SAMPLE 2 ROWS;
			", p =>
			{
				p.AddParameter("@branch", SqlDbType.UniqueIdentifier, GlbBranch.CurrentBranch.PK.ToGuid());
				p.AddParameter("@dept", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK.ToGuid());
			});

			using (Db.Connection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var sql = @"
					SELECT COUNT(*) from dbo.EDIMessage
					WHERE (1=1)
					AND EM_Status='QUE'
					AND EM_ReceiveTransmit='TRX'
					AND EM_ApplicationCode='CTR'
					OPTION (RECOMPILE)
				";
				Db.Connection.ExecuteReader(sql, r => { });
				var query = Db.Connection.ExecutedCommandsAndQueryPlans.Single
				(
					q => q.Item1.Contains("SELECT COUNT(*) from dbo.EDIMessage")
				);
				var plan = new QueryPlanalyzer(query.Item2.Single());
				AssertEquals("no table scans", 0, plan.TableScans.Count());
				AssertEquals("no index scans", 0, plan.IndexScans.Count());
				AssertEquals("has 1 index seek", 1, plan.IndexSeeks.Count());
				AssertEquals("no lookups", 0, plan.RowIDLookups.Count());
			}
		}

		public void TestMessagePlaceholdersRestoredOnSaveFailure()
		{
			var message = Factory.New<TestEdiMessage>();
			message.EM_MessageText = $"MsgNo: {EDIMessage.MessageNumberPlaceHolder}";

			var saveDelegate = new SavingEventHandler<EDIMessage>((m) => { throw new ZSaveException(new ZDataException(new Exception(), ((IBusinessObjectInternals)message).Row, Db.Connection), Factory); });

			message.Saving += saveDelegate;

			CombineAssertions(() =>
			{
				AssertExceptionThrown(typeof(ZSaveException), () => { Factory.Save(); });
				AssertEquals("EM_MessageText should still have placeholder after error", "MsgNo: <<MSGNO PLACEHOLDER>>", message.EM_MessageText);
			});

			message.Saving -= saveDelegate;

			Factory.Save();
			AssertEquals("EM_MessageText should have new message Num", $"MsgNo: {message.EM_MessageNum}", message.EM_MessageText);
		}

		class MockMessageNumberStrategy : IMessageNumberStrategy
		{
			public string GetMessageReferenceNumber() => Guid.NewGuid().ToString("N");
		}

		public class TestEDIMessage : EDIMessage
		{
			public TestEDIMessage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override string GetMessageReferenceNumber()
			{
				return "test";
			}

			public bool AfterNewObjectIsLinkedCalled;
			protected override void AfterNewObjectIsLinked(BusinessObject newBizObj)
			{
				AfterNewObjectIsLinkedCalled = true;
				base.AfterNewObjectIsLinked(newBizObj);
			}

			protected override void ReportIfMessageTextIsEmpty(ZBool shouldReport)
			{
				base.ReportIfMessageTextIsEmpty(true);
			}

			protected override bool ClearMessageNumberOnFailureToSaveCore => ShouldClearMessageNumberOnFailureToSave;

			public bool ShouldClearMessageNumberOnFailureToSave { get; set; }
		}
	}
}
