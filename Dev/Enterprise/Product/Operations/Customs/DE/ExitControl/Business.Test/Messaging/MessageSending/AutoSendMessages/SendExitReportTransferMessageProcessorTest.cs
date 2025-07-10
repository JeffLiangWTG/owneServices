using System;
using System.IO;
using System.Xml.Serialization;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	sealed class SendExitReportTransferMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			PrepareToSatisfyInformationTypeLVCondition();

			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode, VersionNumber = AESVersionNumberList.Codes._30 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				new SendExitReportTransferMessageProcessor(exitReport).Process(new NotificationBuffer());

				CombineAssertions(() =>
				{
					AssertEquals("Messages count", 1, exitReport.Messages.Count);
					var message = exitReport.Messages[0];
					AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsAesSystem, message.EM_ApplicationCode);
					AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.AES, message.EM_MessageType);
					AssertEquals("EM_ApplicationReference", nameof(DEXTIF), message.EM_ApplicationReference);
					AssertEquals("EM_MessageSubType", ExportMessageSubTypeList.Codes.EXT, message.EM_MessageSubType);
					AssertEquals("LinkedObject", exitReport.PK, message.EM_LinkedObject.PK);
					AssertEquals("CER_MessageStatus", LogicalStatusList.Codes.Sent, exitReport.CER_MessageStatus);
					AssertEquals("LogbookEORIBranchSuffix exists", "EBS1", message.GetLogbookEORIBranchSuffix());
					AssertEquals("LogbookLocalReferenceNumber exists", (ZString)"WTG1234", message.GetLogbookLocalReferenceNumber());
					AssertEquals("LogbookRegistrationNumber exists", "MRN4TEST", message.GetLogbookRegistrationNumber());

					DEXTIF result = null;
					var serializer = new XmlSerializer(typeof(DEXTIF));
					using (TextReader reader = new StringReader(message.EM_MessageText))
					{
						result = (DEXTIF)serializer.Deserialize(reader);
					}
					AssertEquals("InformationType", DEXTIFExportOperationInformationType.LV, result.ExportOperation.informationType);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var carrierOrgAddress = EORIHelperTest.GetOrgWithEORNumberAndEORIBranchAndAPINumber(Factory, "EOR1", "EBS1", "1111111111111111");
			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_OA_Carrier = carrierOrgAddress.PK;
			consignment = exitHeader.CusExitConsignments.AddNew();
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			consignment.CXC_LocalReference = "WTG1234";
			consignment.CXC_MovementReference = "MRN4TEST";
			exitReport = exitHeader.CusExitReports.AddNew();
			exitReport.CER_CXC_Consignment = consignment.PK;
			exitReport.CER_Calc_Discrepancies = false;
		}
		CusExitReport exitReport;
		CusExitConsignment consignment;

		void PrepareToSatisfyInformationTypeLVCondition()
		{
			var consignmentItem1 = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem1.CCI_LineNumber = 1;
			consignmentItem1.CCI_GrossMass = 10;
			consignmentItem1.CCI_NetMass = 5;
			var package1 = consignmentItem1.CusExitConsignmentPackagePivots.AddNew().Package;
			package1.CXP_Quantity = 10;
			var consignmentItem2 = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem2.CCI_LineNumber = 1;
			consignmentItem2.CCI_GrossMass = 30;
			consignmentItem2.CCI_NetMass = 15;
			var package2 = consignmentItem2.CusExitConsignmentPackagePivots.AddNew().Package;
			package2.CXP_Quantity = 20;
		}
	}
}
