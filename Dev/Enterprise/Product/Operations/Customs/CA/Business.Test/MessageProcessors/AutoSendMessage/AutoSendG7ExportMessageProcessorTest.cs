using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AutoSendG7ExportMessageProcessor))]
	sealed class AutoSendG7ExportMessageProcessorTest : CAAutoSendCustomsMessageProcessorTest
	{
		protected override void PrepareDeclaration(BaseJobDeclaration declaration)
		{
			var caDeclaration = (JobDeclaration)declaration;
			caDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		}

		protected override Customs.Business.CusEntryHeader GetEntryHeader(BaseJobDeclaration declaration)
		{
			return ((JobDeclaration)declaration).GetEntryHeaderFor(MessageTypeList.Codes.G7Export);
		}

		protected override IProcessor CreateProcessor(BaseJobDeclaration declaration)
		{
			return new AutoSendG7ExportMessageProcessor((JobDeclaration)declaration);
		}

		protected override ZString ExpectedMessageDescription => JobDeclaration.Constants.MessageNames.G7;

		protected override void AssertEntryAndMessageResultForEndToEndTest(Customs.Business.CusEntryHeader entry)
		{
			AssertEquals("Original G7 message should be generated.", 1, entry.Messages.Count);
			AssertEquals(MessageStatusList.Codes.AwaitingOriginal, entry.CH_Status);
			var originalMessage = entry.Messages[0];
			AssertEquals(MessageTypeList.Codes.G7Export, originalMessage.EM_MessageType);
			AssertEquals(MessageSubTypeCodes.Codes.Original, originalMessage.EM_MessageSubType);
			AssertEquals(EDIMessage.Status.Queued, originalMessage.EM_Status);
		}

		protected override void SetEntryClearedStatus(Customs.Business.CusEntryHeader entry)
		{
			entry.CH_EntryStatus = MessageStatusList.Codes.ClearOriginal;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var company = GlbCompany.GetCurrentCompany(Factory);
			var companyProxy = company.OrgProxy;
			companyProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "123456");
		}
	}
}
