using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobDeclarationAutoSendingMessageSupporterTest : JobDeclarationMessageSupporterTest<JobDeclaration>
	{
		public override void TestIJobDeclarationMessageSupporterMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var supporter = (IJobDeclarationAutoSendingMessageSupporter)declaration;
			AssertEquals(false, supporter.SupportEntryDeclarationMessage);
			AssertEquals(JobDeclaration.SendEntryDeclarationTriggerOnlySupportedForExportJobMessage, supporter.GetReasonForNotSupportEntryDeclarationMessage);
			AssertEquals(null, supporter.CreateEntryDeclarationMessageProcessor());
			AssertEquals(false, supporter.SupportReleaseMessage);
			AssertEquals(JobDeclaration.SendReleaseMessageOnlySupportForImportJobMessage, supporter.GetReasonForNotSupportReleaseMessage);
			AssertEquals(null, supporter.CreateReleaseMessageProcessor());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(false, supporter.SupportEntryDeclarationMessage);
			AssertEquals(JobDeclaration.SendEntryDeclarationTriggerOnlySupportedForExportJobMessage, supporter.GetReasonForNotSupportEntryDeclarationMessage);
			AssertEquals(null, supporter.CreateEntryDeclarationMessageProcessor());
			AssertEquals(true, supporter.SupportReleaseMessage);
			AssertEquals(ZString.Empty, supporter.GetReasonForNotSupportReleaseMessage);
			AssertType<AutoSendIIDMessageProcessor>(supporter.CreateReleaseMessageProcessor());

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			AssertEquals(false, supporter.SupportEntryDeclarationMessage);
			AssertEquals(JobDeclaration.SendEntryDeclarationTriggerOnlySupportedForExportJobMessage, supporter.GetReasonForNotSupportEntryDeclarationMessage);
			AssertEquals(null, supporter.CreateEntryDeclarationMessageProcessor());
			AssertEquals(true, supporter.SupportReleaseMessage);
			AssertEquals(ZString.Empty, supporter.GetReasonForNotSupportReleaseMessage);
			AssertType<AutoSendACROSSMessageProcessor>(supporter.CreateReleaseMessageProcessor());

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			AssertEquals(false, supporter.SupportEntryDeclarationMessage);
			AssertEquals(JobDeclaration.SendEntryDeclarationTriggerOnlySupportedForExportJobMessage, supporter.GetReasonForNotSupportEntryDeclarationMessage);
			AssertEquals(null, supporter.CreateEntryDeclarationMessageProcessor());
			AssertEquals(true, supporter.SupportReleaseMessage);
			AssertNotNull(ZString.Empty, supporter.GetReasonForNotSupportReleaseMessage);
			AssertType<AutoSendIIDMessageProcessor>(supporter.CreateReleaseMessageProcessor());

			CACustomsDataRegistry.Instance.ExportDeclarationActive.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(true, supporter.SupportEntryDeclarationMessage);
			AssertEquals(ZString.Empty, supporter.GetReasonForNotSupportEntryDeclarationMessage);
			AssertType<AutoSendG7ExportMessageProcessor>(supporter.CreateEntryDeclarationMessageProcessor());
			AssertEquals(false, supporter.SupportReleaseMessage);
			AssertEquals(JobDeclaration.SendReleaseMessageOnlySupportForImportJobMessage, supporter.GetReasonForNotSupportReleaseMessage);
			AssertEquals(null, supporter.CreateReleaseMessageProcessor());

			CACustomsDataRegistry.Instance.ExportDeclarationActive.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(false, supporter.SupportEntryDeclarationMessage);
			AssertEquals(JobDeclaration.ExportDeclarationIsNotActiveMessage, supporter.GetReasonForNotSupportEntryDeclarationMessage);
			AssertEquals(null, supporter.CreateEntryDeclarationMessageProcessor());
			AssertEquals(false, supporter.SupportReleaseMessage);
			AssertEquals(JobDeclaration.SendReleaseMessageOnlySupportForImportJobMessage, supporter.GetReasonForNotSupportReleaseMessage);
			AssertEquals(null, supporter.CreateReleaseMessageProcessor());
		}
	}
}
