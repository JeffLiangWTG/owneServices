using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(EDIInterchange))]
	public class EDIInterchangeTest : Enterprise.Messaging.Testing.EDIInterchangeTest
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<EDIInterchange>();

		public void TestStatusUpdateFromOtherValueToERROrFAL_CusEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertStatusUpdateFromQUEToERROrFAL_CusEntryHeader(declaration, EDIInterchangeStatusList.Codes.Queued, CustomsMessageStatusTypeList.Codes.OriginalSent, CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal);
			AssertStatusUpdateFromQUEToERROrFAL_CusEntryHeader(declaration, EDIInterchangeStatusList.Codes.Queued, CustomsMessageStatusTypeList.Codes.AmendmentSent, CustomsMessageStatusTypeList.Codes.ErrorSendingAmendment);
			AssertStatusUpdateFromQUEToERROrFAL_CusEntryHeader(declaration, EDIInterchangeStatusList.Codes.Queued, CustomsMessageStatusTypeList.Codes.CancellationSent, CustomsMessageStatusTypeList.Codes.ErrorSendingCancellation);

			AssertStatusUpdateFromSNTToERROrFAL_CusEntryHeader(declaration, EDIInterchangeStatusList.Codes.Sent, CustomsMessageStatusTypeList.Codes.OriginalSent, CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal);
			AssertStatusUpdateFromSNTToERROrFAL_CusEntryHeader(declaration, EDIInterchangeStatusList.Codes.Sent, CustomsMessageStatusTypeList.Codes.AmendmentSent, CustomsMessageStatusTypeList.Codes.ErrorSendingAmendment);
			AssertStatusUpdateFromSNTToERROrFAL_CusEntryHeader(declaration, EDIInterchangeStatusList.Codes.Sent, CustomsMessageStatusTypeList.Codes.CancellationSent, CustomsMessageStatusTypeList.Codes.ErrorSendingCancellation);
		}

		public void TestIfStatusValueIsAlreadyERROrFAL_CusEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertStatusUpdateFromQUEToERROrFAL_CusEntryHeader(declaration, EDIInterchangeStatusList.Codes.Failed, CustomsMessageStatusTypeList.Codes.OriginalSent, CustomsMessageStatusTypeList.Codes.OriginalSent);
			AssertStatusUpdateFromQUEToERROrFAL_CusEntryHeader(declaration, EDIInterchangeStatusList.Codes.Error, CustomsMessageStatusTypeList.Codes.AmendmentSent, CustomsMessageStatusTypeList.Codes.AmendmentSent);
			AssertStatusUpdateFromQUEToERROrFAL_CusEntryHeader(declaration, EDIInterchangeStatusList.Codes.Failed, CustomsMessageStatusTypeList.Codes.CancellationSent, CustomsMessageStatusTypeList.Codes.CancellationSent);

			AssertStatusUpdateFromSNTToERROrFAL_CusEntryHeader(declaration, EDIInterchangeStatusList.Codes.Error, CustomsMessageStatusTypeList.Codes.OriginalSent, CustomsMessageStatusTypeList.Codes.OriginalSent);
			AssertStatusUpdateFromSNTToERROrFAL_CusEntryHeader(declaration, EDIInterchangeStatusList.Codes.Failed, CustomsMessageStatusTypeList.Codes.AmendmentSent, CustomsMessageStatusTypeList.Codes.AmendmentSent);
			AssertStatusUpdateFromSNTToERROrFAL_CusEntryHeader(declaration, EDIInterchangeStatusList.Codes.Error, CustomsMessageStatusTypeList.Codes.CancellationSent, CustomsMessageStatusTypeList.Codes.CancellationSent);
		}

		void AssertStatusUpdateFromQUEToERROrFAL_CusEntryHeader(JobDeclaration declaration, ZString ei_status, ZString oldCH_Staus, ZString newCH_Staus)
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = oldCH_Staus;
			var inInterchange = CreateIncomingEDIInterchangeForTest(ei_status);
			inInterchange.ContainedMessages.Add(entry.Messages.AddNew());
			inInterchange.EI_Status = EDIInterchangeStatusList.Codes.Error;
			inInterchange.Factory.Save();
			AssertEquals(newCH_Staus, entry.CH_Status);
		}

		void AssertStatusUpdateFromSNTToERROrFAL_CusEntryHeader(JobDeclaration declaration, ZString ei_status, ZString oldCH_Staus, ZString newCH_Staus)
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = oldCH_Staus;
			var outInterchange = CreateOutgoingEDIInterchangeForTest(ei_status);
			outInterchange.ContainedMessages.Add(entry.Messages.AddNew());
			outInterchange.EI_Status = EDIInterchangeStatusList.Codes.Error;
			outInterchange.Factory.Save();
			AssertEquals(newCH_Staus, entry.CH_Status);
		}

		public void TestStatusUpdateFromOtherValueToERROrFAL_CusMiscRequestHeader()
		{
			AssertStatusUpdateToERROrFAL_CusMiscRequestHeader(EDIInterchangeStatusList.Codes.Sent, EDIInterchangeStatusList.Codes.Queued, CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal);
		}

		public void TestIfStatusValueIsAlreadyERROrFAL_CusMiscRequestHeader()
		{
			AssertStatusUpdateToERROrFAL_CusMiscRequestHeader(EDIInterchangeStatusList.Codes.Failed, EDIInterchangeStatusList.Codes.Error, CustomsMessageStatusTypeList.Codes.OriginalSent);
		}

		void AssertStatusUpdateToERROrFAL_CusMiscRequestHeader(ZString ei_status1, ZString ei_status2, ZString newCH_Staus)
		{
			var miscRequestHeader1 = Factory.New<CusMiscRequestHeader>();
			miscRequestHeader1.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;
			miscRequestHeader1.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			miscRequestHeader1.CMR_GB = GlbBranch.CurrentBranch.PK;
			miscRequestHeader1.CMR_RequestDate = ZDateTime.Today;
			miscRequestHeader1.CMR_CustomsOffice = "01020";
			var outInterchange = CreateIncomingEDIInterchangeForTest(ei_status1);
			outInterchange.ContainedMessages.Add(miscRequestHeader1.Messages.AddNew());
			outInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outInterchange.Factory.Save();
			AssertEquals(newCH_Staus, miscRequestHeader1.CMR_Status);

			var miscRequestHeader2 = Factory.New<CusMiscRequestHeader>();
			miscRequestHeader2.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;
			miscRequestHeader2.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			miscRequestHeader2.CMR_GB = GlbBranch.CurrentBranch.PK;
			miscRequestHeader2.CMR_RequestDate = ZDateTime.Today;
			miscRequestHeader2.CMR_CustomsOffice = "01020";
			var inInterchange = CreateOutgoingEDIInterchangeForTest(ei_status2);
			inInterchange.ContainedMessages.Add(miscRequestHeader2.Messages.AddNew());
			inInterchange.EI_Status = EDIInterchangeStatusList.Codes.Error;
			inInterchange.Factory.Save();
			AssertEquals(newCH_Staus, miscRequestHeader2.CMR_Status);
		}

		EDIInterchange CreateIncomingEDIInterchangeForTest(ZString status)
		{
			var inInterchange = Factory.New<EDIInterchange>();
			inInterchange.EI_Status = status;
			inInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			inInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.KRCustoms;
			inInterchange.EI_InterchangeType = EDIInterchangeType.ESR;
			inInterchange.EI_SessionGUID = new ZGuid("7F23B397-03C4-42AE-A3E8-EA47559E52A8");
			inInterchange.EI_To = ApplicationCodeList.Codes.KRCustoms;
			inInterchange.EI_From = "XI";
			inInterchange.EI_IsActive = true;
			inInterchange.Factory.Save();

			return inInterchange;
		}

		EDIInterchange CreateOutgoingEDIInterchangeForTest(ZString status)
		{
			var outInterchange = Factory.New<EDIInterchange>();
			outInterchange.EI_Status = status;
			outInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.KRCustoms;
			outInterchange.EI_InterchangeType = ElectronicDocumentTypeList.Codes._830;
			outInterchange.EI_SessionGUID = new ZGuid("7F23B397-03C4-42AE-A3E8-EA47559E52A8");
			outInterchange.EI_GB = GlbBranch.CurrentBranch.PK;
			outInterchange.EI_To = "eHub";
			outInterchange.EI_From = "KRCustoms";
			outInterchange.Factory.Save();

			return outInterchange;
		}
	}
}
