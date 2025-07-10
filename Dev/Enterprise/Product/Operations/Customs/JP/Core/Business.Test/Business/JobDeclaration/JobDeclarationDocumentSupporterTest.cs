using System.Text;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(JobDeclarationDocumentSupporter))]
	sealed class JobDeclarationDocumentSupporterTest : Customs.Business.Testing.BaseJobDeclarationDocumentSupportTest
	{
		public void TestGetBODocDataProviders()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();

			var providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(MessageDocumentSupporter.ExportPermitMessageDocument), null);
			AssertEquals("Provider for Export Permit", 2, providers.Length);
		}

		public void TestExportPermitIsSupported()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(new DataContextValue(MessageDocumentSupporter.ExportPermitMessageDocument)));
		}

		public void TestGetFilterValue()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			AssertEquals("Y", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.IsJPExportPermitMessageDocumentSupport));
		}

		public override void TestGetDocBusinessObjects()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			AssertNotNull(declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Declaration, null)[0]);
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var messageData = Encoding.ASCII.GetBytes(TestDataHelper.GetResourceStream("MockInboundMessage.txt"));
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var message1 = entryHeader.Messages.AddNew();
			message1.EM_ReceiveTransmit = Direction.Receive;
			message1.EM_MessageData = messageData;
			var message2 = entryHeader.Messages.AddNew();
			message2.EM_ReceiveTransmit = Direction.Receive;
			message2.EM_MessageData = messageData;
			return declaration;
		}
	}
}
