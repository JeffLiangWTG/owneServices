using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.xTMessaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(JPCInterchangeUnpacker))]
	sealed class JPCInterchangeUnpackerTest : InterchangeUnpackerTest<JPCInterchangeUnpacker>
	{
		protected override string[] ApplicationCodes => new[] { EDIInterchange.ApplicationCodes.JPCustoms };

		public void TestXERMessageCreated_FindBranchFromAttribute()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			company.Branches.DeleteAll();

			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_GC = company.PK;

			var receiveInterchange = Factory.New<EDIInterchange>();
			receiveInterchange.EI_InterchangeNum = "JR000000002";
			receiveInterchange.EI_InterchangeType = JPMessageTypes.Codes.XER;
			receiveInterchange.EI_SessionGUID = ZGuid.NewZGuid();
			receiveInterchange.EI_From = "WTLEDI_JP";
			receiveInterchange.EI_To = "WTLDJPEDI";
			receiveInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.JPCustoms;
			receiveInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			receiveInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
			receiveInterchange.EI_BodyData = Encoding.ASCII.GetBytes("ReceiveMessage");
			receiveInterchange.EI_Status = EDIInterchange.Status.Queued;
			receiveInterchange.EI_IsActive = true;

			receiveInterchange.SetHeaderTextWithAttributeDictionary(new Dictionary<string, string>
			{
				{ Constants.DirectxT.ProtocolTypeAttribute, Constants.ProtocolType.POP3 },
				{ Constants.DirectxT.CompanyCodeAttribute, company.GC_Code }
			});

			Factory.Save();

			var logger = new LoggingInformation();
			var result = new JPCInterchangeUnpacker().Unpack(receiveInterchange, null, null, logger);

			CombineAssertions(() =>
			{
				Assert("Should create a message.", result.IsSuccess);

				var message = receiveInterchange.ContainedMessages.Cast<EDIMessage>().SingleOrDefault();

				AssertNotNull("Should have been 1 message extracted from interchange", message);
				AssertEquals("The new message is linked to interchange", receiveInterchange.PK, message.EM_EI);
				AssertEquals("Should update the branch of interchange", branch.PK, receiveInterchange.EI_GB);
				AssertEquals("Should update the branch of message", branch.PK, message.EM_GB);
				AssertEquals("Should not update the LinkUniqueID", ZGuid.Empty, message.EM_LinkUniqueID);
				AssertEquals(nameof(EDIMessage.EM_ApplicationCode), EDIInterchange.ApplicationCodes.JPCustoms, message.EM_ApplicationCode);
				AssertEquals(nameof(EDIMessage.EM_MessageData), "ReceiveMessage", JPMessageUtils.ConvertMessageToString(message.EM_MessageData));
				AssertEquals(nameof(EDIMessage.EM_ReceiveTransmit), EDIInterchange.Direction.Receive, message.EM_ReceiveTransmit);
				AssertEquals(nameof(EDIMessage.EM_Status), EDIInterchange.Status.Queued, message.EM_Status);
				AssertEquals(nameof(EDIMessage.EM_MessageType), JPMessageTypes.Codes.XER, message.EM_MessageType);
			});
		}

		public void TestXERMessageCreated_FindBranchFromHubID()
		{
			var entryHeader = CreateEntryHeader();
			var branch = entryHeader.Branch;

			var transmitMessage = Factory.New<EDIMessage>();
			transmitMessage.EM_MessageNum = "IDA00000000001";
			transmitMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			transmitMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmitMessage.EM_LinkedObject = entryHeader;

			var transmitInterchange = Factory.New<EDIInterchange>();
			transmitInterchange.EI_InterchangeNum = "IDA00000000001";
			transmitInterchange.EI_From = "WTLDJPEDI";
			transmitInterchange.EI_To = "WTLEDI_JP";
			transmitInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.JPCustoms;
			transmitInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			transmitInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
			transmitInterchange.EI_BodyData = Encoding.ASCII.GetBytes("SendMessage");
			transmitInterchange.ContainedMessages.Add(transmitMessage);

			var receiveInterchange = Factory.New<EDIInterchange>();
			receiveInterchange.EI_InterchangeNum = "JR000000002";
			receiveInterchange.EI_InterchangeType = JPMessageTypes.Codes.XER;
			receiveInterchange.EI_From = "WTLEDI_JP";
			receiveInterchange.EI_To = "WTLDJPEDI";
			receiveInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.JPCustoms;
			receiveInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			receiveInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
			receiveInterchange.EI_BodyData = Encoding.ASCII.GetBytes("ReceiveMessage");
			receiveInterchange.EI_Status = EDIInterchange.Status.Queued;
			receiveInterchange.EI_IsActive = true;

			transmitInterchange.EI_SessionGUID = receiveInterchange.EI_SessionGUID = ZGuid.NewZGuid();

			Factory.Save();

			var logger = new LoggingInformation();
			var result = new JPCInterchangeUnpacker().Unpack(receiveInterchange, transmitInterchange, transmitMessage, logger);

			CombineAssertions(() =>
			{
				Assert("Should create a message.", result.IsSuccess);

				var message = receiveInterchange.ContainedMessages.Cast<EDIMessage>().SingleOrDefault();

				AssertNotNull("Should have been 1 message extracted from interchange", message);
				AssertEquals("The new message is linked to interchange", receiveInterchange.PK, message.EM_EI);
				AssertEquals("Should update the branch of interchange", branch.PK, receiveInterchange.EI_GB);
				AssertEquals("Should update the branch of message", branch.PK, message.EM_GB);
				AssertEquals("Should update the LinkUniqueID", entryHeader.PK, message.EM_LinkUniqueID);
				AssertEquals(nameof(EDIMessage.EM_ApplicationCode), EDIInterchange.ApplicationCodes.JPCustoms, message.EM_ApplicationCode);
				AssertEquals(nameof(EDIMessage.EM_MessageData), "ReceiveMessage", JPMessageUtils.ConvertMessageToString(message.EM_MessageData));
				AssertEquals(nameof(EDIMessage.EM_ReceiveTransmit), EDIInterchange.Direction.Receive, message.EM_ReceiveTransmit);
				AssertEquals(nameof(EDIMessage.EM_Status), EDIInterchange.Status.Queued, message.EM_Status);
				AssertEquals(nameof(EDIMessage.EM_MessageType), JPMessageTypes.Codes.XER, message.EM_MessageType);
			});
		}

		public void TestNACCSMessageCreated()
		{
			var mimeHeader = @"Date: Tue, 02 Jan 2018 10:50:28 +0900
From: NACCS@MAIL.TEST.NACCS6
To: xxx@MAIL.TEST.NACCS6
Message-ID: <xxxx@MAIL.TEST.NACCS6>
Mime-Version: 1.0
Content-Type: Text/plain;charset=""EUC-JP""
Content-Transfer-Encoding: 8bit

";

			var mimeBody = @"XXXIDA  *SIDA  202203302232  XXXXX                 xxx@MAIL.TEST.NACCS6                                            00000-0000-0000 PCLUPUS02418706                                                               3037973340                          000ER * 0000002024";
			var mimeBody_EXC = @"XXXIDA  SAT0471202203302232  XXXXX                 xxx@MAIL.TEST.NACCS6"
+ new string(' ', 44)
+ "JPtest78901";

			var entryHeader = CreateEntryHeader();
			var exportControlEntryNum = CusEntryNumber.LoadOrCreate(entryHeader, JobMessageTypeList.Codes.Import, Core.Constants.CountryCodes.Japan);
			exportControlEntryNum.CE_EntryNum = "JPtest78901";

			var branch = entryHeader.Branch;

			var interchangeWithMimeContent = CreateInterchange(Encoding.ASCII.GetBytes(mimeHeader + mimeBody.PadRight(398)));
			var interchangeWithNormalContent = CreateInterchange(Encoding.ASCII.GetBytes(mimeBody.PadRight(398)));
			var interchangeWithExcMessageData = CreateInterchange(Encoding.ASCII.GetBytes(mimeBody_EXC.PadRight(398)));

			Factory.Save();

			CombineAssertions(() =>
			{
				var logger = new LoggingInformation();
				var unpacker = new JPCInterchangeUnpacker();

				var result = unpacker.Unpack(interchangeWithMimeContent, null, null, logger);

				Assert("Should create a message.", result.IsSuccess);
				AssertInterchange(interchangeWithMimeContent, mimeBody);

				result = unpacker.Unpack(interchangeWithNormalContent, null, null, logger);

				Assert("Should create a message.", result.IsSuccess);
				AssertInterchange(interchangeWithNormalContent, mimeBody);

				result = unpacker.Unpack(interchangeWithExcMessageData, null, null, logger);

				Assert("Should create a message.", result.IsSuccess);
				AssertInterchange(interchangeWithExcMessageData, mimeBody_EXC);
			});

			EDIInterchange CreateInterchange(byte[] bodyData)
			{
				var interchange = Factory.New<EDIInterchange>();
				interchange.EI_From = "XXX";
				interchange.EI_To = "YYY";
				interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.JPCustoms;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_IsActive = true;
				interchange.EI_BodyData = bodyData;
				interchange.EI_GB = GlbBranch.CurrentBranch.PK;

				return interchange;
			}

			void AssertInterchange(EDIInterchange interchange, string plainBodyData)
			{
				var message = interchange.ContainedMessages.Cast<EDIMessage>().SingleOrDefault();

				AssertNotNull("Should have been 1 message extracted from interchange", message);
				AssertEquals("The new message is linked to interchange", interchange.PK, message.EM_EI);
				AssertEquals("Should update the branch of interchange", branch.PK, interchange.EI_GB);
				AssertEquals("Should update the branch of message", branch.PK, message.EM_GB);
				AssertEquals("Should update the LinkUniqueID", entryHeader.PK, message.EM_LinkUniqueID);
				AssertEquals(nameof(EDIMessage.EM_ApplicationCode), EDIInterchange.ApplicationCodes.JPCustoms, message.EM_ApplicationCode);
				AssertEquals(nameof(EDIMessage.EM_MessageData), plainBodyData, JPMessageUtils.ConvertMessageToString(message.EM_MessageData).TrimEnd());
				AssertEquals(nameof(EDIMessage.EM_ReceiveTransmit), EDIInterchange.Direction.Receive, message.EM_ReceiveTransmit);
				AssertEquals(nameof(EDIMessage.EM_Status), EDIInterchange.Status.Queued, message.EM_Status);
				AssertEquals(nameof(EDIMessage.ProcedureCode), "IDAXXX", message.ProcedureCode);
			}
		}

		public void TestInterchangeWithoutParent()
		{
			var messageContent = @"   IDA  *SIDA  20240701      1AABC"
										+ new string(' ', 17)
										+ "test@mail.com".PadRight(64, ' ')
										+ new string(' ', 104)
										+ "IDA00000002024".PadRight(26, ' ')
										+ new string(' ', 8).PadRight(398, ' ');

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "JR000000002";
			interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.JPCustoms;
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.JPCustoms;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_TransportType = EDIInterchange.TransportType.xT;
			interchange.EI_BodyData = JPMessageUtils.ConvertStringToMessage(messageContent);
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			interchange.EI_IsActive = true;

			Factory.Save();

			var logger = new LoggingInformation();
			var unpacker = new JPCInterchangeUnpacker();
			var result = unpacker.Unpack(interchange, null, null, logger);

			Assert("Should create a message.", result.IsSuccess);

			var message = interchange.ContainedMessages.Cast<EDIMessage>().SingleOrDefault();

			AssertNotNull("Should have been 1 message extracted from interchange", message);
			AssertEquals("The new message is linked to interchange", interchange.PK, message.EM_EI);
			AssertEquals(nameof(EDIMessage.EM_Status), EDIInterchange.Status.Discarded, message.EM_Status);
		}

		public void TestInterchangeWithMultipleParents()
		{
			var messageContent = @"XXXIDA  SAT0471202203302232  XXXXX                 xxx@MAIL.TEST.NACCS6"
										+ new string(' ', 44)
										+ "12345678901".PadRight(398, ' ');

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "JR000000002";
			interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.JPCustoms;
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.JPCustoms;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_TransportType = EDIInterchange.TransportType.xT;
			interchange.EI_BodyData = JPMessageUtils.ConvertStringToMessage(messageContent);
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			interchange.EI_IsActive = true;

			var entryHeader = CreateEntryHeader();

			var exportControlEntryNum = CusEntryNumber.LoadOrCreate(entryHeader, JobMessageTypeList.Codes.Import, Core.Constants.CountryCodes.Japan);
			exportControlEntryNum.CE_EntryNum = "12345678901";

			var duplicatedEntryHeader = entryHeader.Declaration.ActiveEntryHeaders.AddNew();
			duplicatedEntryHeader.FillWithValidTestData();

			var duplicatedEntryNum = CusEntryNumber.LoadOrCreate(duplicatedEntryHeader, JobMessageTypeList.Codes.Import, Core.Constants.CountryCodes.Japan);
			duplicatedEntryNum.CE_EntryNum = "12345678901";

			Factory.Save();

			var logger = new LoggingInformation();
			var unpacker = new JPCInterchangeUnpacker();
			var result = unpacker.Unpack(interchange, null, null, logger);

			Assert("Should create a message as the FindParentFromSubject return the latest message parent.", result.IsSuccess);
			var message = interchange.ContainedMessages.Cast<EDIMessage>().SingleOrDefault();

			AssertNotNull("Should have been 1 message extracted from interchange", message);
			AssertEquals("The new message is linked to interchange", interchange.PK, message.EM_EI);
			AssertEquals(nameof(EDIMessage.EM_Status), EDIInterchange.Status.Queued, message.EM_Status);
		}

		CusEntryHeader CreateEntryHeader()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();

			var password = Factory.New<GlbExternalPasswordNMC>();
			password.GP_MailBoxID = "xxx";
			password.CurrentDecryptedPassword = "123";
			password.GP_GC = company.PK;
			password.ShouldReceive = true;

			var proxyOrg = Factory.NewWithValidTestData<OrgHeader>();
			company.GC_OH_OrgProxy = proxyOrg.PK;
			var customsCode = proxyOrg.CustomsCodes.AddNew();
			customsCode.OK_CodeType = OrgCusCode.JapanCodeTypes.NUC;
			customsCode.OK_CustomsRegNo = "XXXXX";

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.JP.IJobDeclaration>();
			declaration.JE_GB = branch.PK;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();
			entryHeader.CH_BGMReference = "0000002024";

			return entryHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var configType = helper.CreateOrGetExistingRefSysConfigType("NACCSMailT", "NACCS Mail Test", "NACCS Mail Test");
			helper.CreateOrUpdateExistingRefSysConfig(configType.ZRT_ConfigCode, "NACCS@Mail.Test.NACCS6", ZDateTime.Now.AddYears(-1), ZDateTime.Now.AddYears(1));
			Factory.Save();
		}
	}
}
