using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.Business.Testing
{
	public abstract class ExportDeclarationSenderTest<T> : TestCaseWithFactory
		where T : ExportDeclarationSender
	{
		public virtual void TestSendMessage()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode, VersionNumber = AESVersionNumberList.Codes._30 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				ExportDeclarationSender.Send();
				CombineAssertions(() =>
				{
					AssertEquals(1, entryHeader.Messages.Count);
					var message = entryHeader.Messages[0];
					AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsAesSystem, message.EM_ApplicationCode);
					AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("EM_MessageType", Messaging.EDIMessageTypeList.Codes.AES, message.EM_MessageType);
					AssertEquals("EM_ApplicationReference", ExpectedMessageType, message.EM_ApplicationReference);
					AssertEquals("EM_MessageSubType", ExpectedMessageSubType, message.EM_MessageSubType);
					AssertEquals(entryHeader.PK, message.EM_LinkedObject.PK);
					AssertEquals(Common.Shared.MessageStatusList.Codes.Sent, entryHeader.CH_Status);
					AssertEquals("LogbookEORIBranchSuffix exists", "EBS1", message.GetLogbookEORIBranchSuffix());
					AssertEquals("LogbookLocalReferenceNumber exists", LocalReferenceNumberExpected ? (ZString)"WTG1234" : ZString.Empty, message.GetLogbookLocalReferenceNumber());
					AssertEquals("LogbookRegistrationNumber exists", "MRN4TEST", message.GetLogbookRegistrationNumber());
					AssertEquals("LockNumberOfEntryLines", expected: true, entryHeader.LockNumberOfEntryLines);
				});
			}
		}

		protected ExportDeclarationSender ExportDeclarationSender => exportDeclarationSender ?? (exportDeclarationSender = GetExportDeclarationSender());
		ExportDeclarationSender exportDeclarationSender;

		protected abstract ExportDeclarationSender GetExportDeclarationSender();

		protected abstract ZString ExpectedMessageType { get; }

		protected abstract ZString ExpectedMessageSubType { get; }

		protected abstract bool LocalReferenceNumberExpected { get; }

		protected override void SetUp()
		{
			base.SetUp();
			var declarantOrg = EORIHelperTest.GetOrgWithEORNumberAndEORIBranchAndAPINumber(Factory, "EOR1", "EBS1", "1111111111111111");

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OwnerRef = "WTG1234";
			declaration.JE_OA_DeclarantAddress = declarantOrg.PK;

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryNumber.LoadOrCreate(entryHeader, "MRN", CountryCodes.Germany).CE_EntryNum = "MRN4TEST";
		}
		protected JobDeclaration declaration;
		protected CusEntryHeader entryHeader;
	}
}

