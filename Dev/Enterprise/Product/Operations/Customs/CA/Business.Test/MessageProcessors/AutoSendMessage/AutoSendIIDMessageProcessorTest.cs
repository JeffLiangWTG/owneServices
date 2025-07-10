using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AutoSendIIDMessageProcessor))]
	class AutoSendIIDMessageProcessorTest : CAAutoSendCustomsMessageProcessorTest
	{
		protected override void PrepareDeclaration(BaseJobDeclaration declaration)
		{
			var caDeclaration = (JobDeclaration)declaration;
			caDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			caDeclaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			caDeclaration.CA_AssesmentOption = AssessmentOptions.Codes.AppraisalQualityData;
		}

		protected override Customs.Business.CusEntryHeader GetEntryHeader(BaseJobDeclaration declaration)
		{
			var caDeclaration = (JobDeclaration)declaration;
			return caDeclaration.GetEntryHeaderFor(MessageTypeList.Codes.EDIRelease);
		}

		protected override IProcessor CreateProcessor(BaseJobDeclaration declaration)
		{
			return new AutoSendIIDMessageProcessor((JobDeclaration)declaration);
		}

		protected override ZString ExpectedMessageDescription => JobDeclaration.Constants.MessageNames.IID;

		protected override void AssertEntryAndMessageResultForEndToEndTest(Customs.Business.CusEntryHeader entry)
		{
			AssertEquals("Original IID message should be generated.", 1, entry.Messages.Count);
			AssertEquals(MessageStatusList.Codes.AwaitingOriginal, entry.CH_Status);
			var originalMessage = entry.Messages[0];
			AssertEquals(MessageTypeList.Codes.IntegratedImportDeclaration, originalMessage.EM_MessageType);
			AssertEquals(MessageSubTypeCodes.Codes.Original, originalMessage.EM_MessageSubType);
			AssertEquals(EDIMessage.Status.Queued, originalMessage.EM_Status);
		}

		protected override void SetEntryClearedStatus(Customs.Business.CusEntryHeader entry)
		{
			entry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
		}
	}
}
