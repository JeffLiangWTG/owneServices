using System;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(NACCSMessageParentFinder))]
	sealed class NACCSMessageParentFinderTest : TestCaseWithFactory
	{
		public void TestFindParentFromHubID()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_GB = TestBranch.PK;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();

			var transmitMessage = Factory.New<EDIMessage>();
			transmitMessage.EM_MessageNum = "IDA00000000001";
			transmitMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			transmitMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmitMessage.EM_LinkedObject = entryHeader;
			transmitMessage.EM_GB = TestBranch.PK;

			var transmitInterchange = Factory.New<EDIInterchange>();
			transmitInterchange.EI_InterchangeNum = "IDA00000000001";
			transmitInterchange.EI_From = "WTLDJPEDI";
			transmitInterchange.EI_To = "WTLEDI_JP";
			transmitInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.JPCustoms;
			transmitInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			transmitInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
			transmitInterchange.ContainedMessages.Add(transmitMessage);

			var receiveInterchange = Factory.New<EDIInterchange>();
			receiveInterchange.EI_InterchangeNum = "JRIDA00SIDA000000001";
			receiveInterchange.EI_From = "NACCS";
			receiveInterchange.EI_To = "2024";
			receiveInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.JPCustoms;
			receiveInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			receiveInterchange.EI_TransportType = EDIInterchange.TransportType.xT;

			receiveInterchange.EI_SessionGUID = transmitInterchange.EI_SessionGUID = Guid.NewGuid();

			Factory.Save();

			using (var finder = new NACCSMessageParentFinder())
			{
				AssertSame("Should find the entry header by the hub ID.", entryHeader, finder.FindParentFromHubID(Factory, receiveInterchange.eHubID));
			}
		}

		public void TestFindParentFromMessageNum()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GB = TestBranch.PK;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var transmitMessage = Factory.New<EDIMessage>();
			transmitMessage.EM_MessageNum = "IDA00000000001";
			transmitMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			transmitMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmitMessage.EM_LinkedObject = entryHeader;

			var messageContent = @"   IDA  *SIDA  20240701      1AABC"
										+ new string(' ', 17)
										+ "test@mail.com".PadRight(64, ' ')
										+ new string(' ', 104)
										+ "IDA00000000001".PadRight(26, ' ');

			var messageData = JPMessageUtils.ConvertStringToMessage(messageContent.PadRight(398, ' '));

			using (var finder = new NACCSMessageParentFinder())
			{
				AssertSame("Should find the entry header by the message number.", entryHeader, finder.FindParentFromMessageData(Factory, messageData));
			}

			using (var finder = new NACCSMessageParentFinder())
			{
				transmitMessage.EM_MessageNum = "InvalidNum0001";
				AssertNull("Should not find the entry header as the message number is not matched.", finder.FindParentFromMessageData(Factory, messageData));
			}
		}

		public void TestFindParentFromEntryHeader()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.JP.IJobDeclaration>();
			declaration.JE_GB = TestBranch.PK;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();
			entryHeader.CH_BGMReference = "0000002024";

			Factory.Save();

			var messageContent = @"   IDA  *SIDA  20240701      1AABC"
										+ new string(' ', 17)
										+ "test@mail.com".PadRight(64, ' ')
										+ new string(' ', 104)
										+ "IDA00000002024".PadRight(26, ' ')
										+ new string(' ', 8)
										+ "0000002024";

			var messageData = JPMessageUtils.ConvertStringToMessage(messageContent.PadRight(398, ' '));

			using (var finder = new NACCSMessageParentFinder())
			{
				AssertSame("Should find the entry header by the input reference.", entryHeader, finder.FindParentFromMessageData(Factory, messageData));
			}

			using (var finder = new NACCSMessageParentFinder())
			{
				entryHeader.CH_BGMReference = "InvalidNum0001";
				Factory.Save();

				AssertNull("Should not find the entry header as the BGM Reference is not matched.", finder.FindParentFromMessageData(Factory, messageData));
			}
		}

		public void TestFindParentFromManifestHeader()
		{
			var manifestHeader = Factory.New<Integration.Customs.ASYCUDA.JPManifest.IAsycudaManifestHeader>();
			manifestHeader.AMA_GB = TestBranch.PK;
			manifestHeader.AMA_JobReference = "MAN0000024";
			manifestHeader.AMA_InputReference = "0000002024";

			var messageContent = @"   IDA  *SIDA  20240701      1AABC"
										+ new string(' ', 17)
										+ "test@mail.com".PadRight(64, ' ')
										+ new string(' ', 104)
										+ "IDA00000002024".PadRight(26, ' ')
										+ new string(' ', 8)
										+ "0000002024";

			var messageData = JPMessageUtils.ConvertStringToMessage(messageContent.PadRight(398, ' '));

			using (var finder = new NACCSMessageParentFinder())
			{
				AssertSame("Should find the manifest header by the input reference.", manifestHeader, finder.FindParentFromMessageData(Factory, messageData));
			}

			using (var finder = new NACCSMessageParentFinder())
			{
				manifestHeader.AMA_InputReference = "XXXXXX0001";
				AssertNull("Should not find the manifest header as the Job Reference is not matched.", finder.FindParentFromMessageData(Factory, messageData));
			}
		}

		public void TestFindParentBySubject_ExportControlNumberSubject()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.JP.IJobDeclaration>();
			declaration.JE_GB = TestBranch.PK;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var exportControlEntryNum = CusEntryNumber.LoadOrCreate(entryInstruction, CusEntryNumberTypes.JP.ExportControlNumber, Core.Constants.CountryCodes.Japan);
			exportControlEntryNum.CE_EntryNum = "JPtest78901234567890123456789012345";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			Factory.Save();

			var messageContent = JPMessageTestHelper.CreateCommonResponseHeader(businessCode: "CCL", responseCode: "SAT0051", subject: "JPtest78901234567890123456789012345", userMailAddress: "TEST1@MAIL.TEST.NACCS6", userCode: "USER1", inputReference: "JPABCTEST", messageReference: "JPTESTABC");
			var messageData = JPMessageUtils.ConvertStringToMessage(messageContent.PadRight(398, ' '));

			using (var finder = new NACCSMessageParentFinder())
			{
				var parent = finder.FindParentFromSubject(Factory, messageData);
				AssertSame("Should find the entry header by the subject.", entryHeader, parent);
			}
		}

		public void TestFindParentBySubject_DeclarationNumberSubject()
		{
			AsertFindParentBySubject_DeclarationNumber("SAT0471");
		}

		public void TestFindParentBySubject_DeclarationNumberAndAWBNumberSubject()
		{
			AsertFindParentBySubject_DeclarationNumber("AAE4751");
		}

		void AsertFindParentBySubject_DeclarationNumber(string responseCode)
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.JP.IJobDeclaration>();
			declaration.JE_GB = TestBranch.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var exportControlEntryNum = CusEntryNumber.LoadOrCreate(entryHeader, JobMessageTypeList.Codes.Export, Core.Constants.CountryCodes.Japan);
			exportControlEntryNum.CE_EntryNum = "JPtest78901";

			Factory.Save();

			var messageContent = JPMessageTestHelper.CreateCommonResponseHeader(businessCode: "1CC", responseCode: responseCode, subject: "JPtest78901", userMailAddress: "TEST1@MAIL.TEST.NACCS6", userCode: "USER1", inputReference: "JPABCTEST", messageReference: "JPTESTABC");
			var messageData = JPMessageUtils.ConvertStringToMessage(messageContent.PadRight(398, ' '));

			using (var finder = new NACCSMessageParentFinder())
			{
				var parent = finder.FindParentFromSubject(Factory, messageData);
				AssertSame("Should find the entry header by the subject.", entryHeader, parent);
			}
		}

		public void TestFindParentBySubject_AWBNumberSubject()
		{
			var manifestHeader = (ASYCUDA.Business.AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.JPManifest.IAsycudaManifestHeader>();
			manifestHeader.AMA_GB = TestBranch.PK;
			manifestHeader.AMA_MasterBill = "JPtest78901";
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;

			Factory.Save();

			var messageContent = JPMessageTestHelper.CreateCommonResponseHeader(businessCode: "CCL", responseCode: "AAS0180", subject: "JPtest78901", userMailAddress: "TEST1@MAIL.TEST.NACCS6", userCode: "USER1", inputReference: "JPABCTEST", messageReference: "JPTESTABC");
			var messageData = JPMessageUtils.ConvertStringToMessage(messageContent.PadRight(398, ' '));

			using (var finder = new NACCSMessageParentFinder())
			{
				var parent = finder.FindParentFromSubject(Factory, messageData);
				AssertEquals("Should find the manifest header by the subject.", manifestHeader.PK, parent?.PK ?? ZGuid.Empty);
			}
		}

		public void TestFindParentBySubject_IsMatchedUserCodeAndUserMailAddress()
		{
			var manifestHeader = (ASYCUDA.Business.AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.JPManifest.IAsycudaManifestHeader>();
			manifestHeader.AMA_GB = TestBranch.PK;
			manifestHeader.AMA_MasterBill = "JPtest78901";
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertIsMatched("USER1", "TEST1@MAIL.TEST.NACCS6", true);
				AssertIsMatched("USER1", "TEST0@MAIL.TEST.NACCS6", false);
				AssertIsMatched("USER0", "TEST1@MAIL.TEST.NACCS6", false);
			});

			void AssertIsMatched(string userCode, string userMailAddress, bool isMatched)
			{
				var messageContent = JPMessageTestHelper.CreateCommonResponseHeader(businessCode: "CCL", responseCode: "AAS0180", subject: "JPtest78901", userMailAddress: userMailAddress, userCode: userCode, inputReference: "JPABCTEST", messageReference: "JPTESTABC");
				var messageData = JPMessageUtils.ConvertStringToMessage(messageContent.PadRight(398, ' '));

				using (var finder = new NACCSMessageParentFinder())
				{
					var parent = finder.FindParentFromSubject(Factory, messageData);
					Assert("Should only find the linked by the subject when user code and user mail address are matched.", isMatched ? manifestHeader == parent : manifestHeader != parent);
				}
			}
		}

		public void TestFindParentBySubject_GetFirstOneByLastestMessage()
		{
			var manifestHeader = (ASYCUDA.Business.AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.JPManifest.IAsycudaManifestHeader>();
			manifestHeader.AMA_GB = TestBranch.PK;
			manifestHeader.AMA_MasterBill = "JPtest78901";
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;

			var message = manifestHeader.Messages.AddNew();
			message.EM_SystemCreateTimeUtc = DateTime.UtcNow;
			var message1 = manifestHeader.Messages.AddNew();
			message.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(-99);
			var message2 = manifestHeader.Messages.AddNew();
			message.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(-33);

			var manifestHeader2 = Factory.New<ASYCUDA.Business.AsycudaManifestHeader>();
			manifestHeader2.AMA_GB = TestBranch.PK;
			manifestHeader2.AMA_MasterBill = "JPtest78901";
			var message3 = manifestHeader.Messages.AddNew();
			message3.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(-1);
			var message4 = manifestHeader.Messages.AddNew();
			message4.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(-2);
			var message5 = manifestHeader.Messages.AddNew();
			message5.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(-3);

			Factory.Save();

			var messageContent = JPMessageTestHelper.CreateCommonResponseHeader(businessCode: "CCL", responseCode: "AAS0180", subject: "JPtest78901", userMailAddress: "TEST1@MAIL.TEST.NACCS6", userCode: "USER1", inputReference: "JPABCTEST", messageReference: "JPTESTABC");
			var messageData = JPMessageUtils.ConvertStringToMessage(messageContent.PadRight(398, ' '));

			using (var finder = new NACCSMessageParentFinder())
			{
				var parent = finder.FindParentFromSubject(Factory, messageData);
				Assert("Should find the manifest header by the subject.", manifestHeader.PK == parent.PK);
				Assert("Should find the manifest header by the subject.", manifestHeader2.PK != parent.PK);
			}
		}

		GlbBranch TestBranch
		{
			get
			{
				if (testBranch == null)
				{
					var company = Factory.NewWithValidTestData<GlbCompany>();
					company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

					var proxyOrg = Factory.NewWithValidTestData<OrgHeader>();
					company.GC_OH_OrgProxy = proxyOrg.PK;
					var customsCode = proxyOrg.CustomsCodes.AddNew();
					customsCode.OK_CodeType = OrgCusCode.JapanCodeTypes.NUC;
					customsCode.OK_CustomsRegNo = "USER1";

					testBranch = company.Branches.AddNew();
					testBranch.FillWithValidTestData();

					var password = Factory.New<GlbExternalPasswordNMC>();
					password.GP_MailBoxID = "TEST1";
					password.CurrentDecryptedPassword = "123";
					password.GP_GC = company.PK;
					password.ShouldReceive = true;
					password.GP_PasswordType = JPPasswordType.Codes.NMC;

					Factory.Save();
				}

				return testBranch;
			}
		}
		GlbBranch testBranch;

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
