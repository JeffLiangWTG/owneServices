using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	[TestsSubclassesOf(typeof(ExitControlMessageSender))]
	public abstract class ExitControlMessageSenderTest<T> : TestCaseWithFactory
		where T : ExitControlMessageSender
	{
		public virtual void TestSendMessage()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode, VersionNumber = AESVersionNumberList.Codes._30 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				ExportDeclarationSender.Send();
				CombineAssertions(() =>
				{
					AssertEquals(1, cusExitReport.Messages.Count);
					var message = cusExitReport.Messages[0];
					AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsAesSystem, message.EM_ApplicationCode);
					AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.AES, message.EM_MessageType);
					AssertEquals("EM_ApplicationReference", ExpectedMessageType, message.EM_ApplicationReference);
					AssertEquals("EM_MessageSubType", ExpectedMessageSubType, message.EM_MessageSubType);
					AssertEquals("LinkedObject", cusExitReport.PK, message.EM_LinkedObject.PK);
					AssertEquals("CER_MessageStatus", LogicalStatusList.Codes.Sent, cusExitReport.CER_MessageStatus);
					AssertEquals("LogbookEORIBranchSuffix exists", "EBS1", message.GetLogbookEORIBranchSuffix());
					AssertEquals("LogbookLocalReferenceNumber exists", LocalReferenceNumberExpected ? (ZString)"WTG1234" : ZString.Empty, message.GetLogbookLocalReferenceNumber());
					AssertEquals("LogbookRegistrationNumber exists", "MRN4TEST", message.GetLogbookRegistrationNumber());
				});
			}
		}

		protected ExitControlMessageSender ExportDeclarationSender => exportDeclarationSender ?? (exportDeclarationSender = (ExitControlMessageSender)Activator.CreateInstance(typeof(T), sendingObject));
		ExitControlMessageSender exportDeclarationSender;

		protected abstract ZString ExpectedMessageType { get; }

		string ExpectedMessageSubType => ExportMessageSubTypeList.Codes.EXT;

		protected abstract bool LocalReferenceNumberExpected { get; }

		protected override void SetUp()
		{
			base.SetUp();
			var carrierOrgAddress = EORIHelperTest.GetOrgWithEORNumberAndEORIBranchAndAPINumber(Factory, "EOR1", "EBS1", "1111111111111111");
			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.CXH_OA_Carrier = carrierOrgAddress.PK;
			var consignment = exitHeader.CusExitConsignments.AddNew();
			cusExitReport = exitHeader.CusExitReports.AddNew();
			cusExitReport.CER_CXC_Consignment = consignment.PK;
			cusExitReport.Consignment.CXC_LocalReference = "WTG1234";
			cusExitReport.Consignment.CXC_MovementReference = "MRN4TEST";
			sendingObject = new ExitControlMessageSendingObject(cusExitReport);
		}

		CusExitReport cusExitReport;
		ExitControlMessageSendingObject sendingObject;
	}
}
