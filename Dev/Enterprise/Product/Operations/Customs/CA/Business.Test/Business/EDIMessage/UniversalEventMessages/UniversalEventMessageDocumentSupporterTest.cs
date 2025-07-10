using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(UniversalEventMessageDocumentSupporter))]
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class UniversalEventMessageDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestShowReasonForNotPrinting()
		{
			var message = (UniversalEventMessage)GetDocumentSupportableBusinessObject();
			AssertEquals(false, ((IDocumentSupportable)message).DocumentSupporter.ShowReasonForNotPrinting(Constants.DataContext.None, null));
		}

		public void TestUniversalEventMessageDocumentSupporter()
		{
			var message = (UniversalEventMessage)GetDocumentSupportableBusinessObject();
			var dataContextValue = new DataContextValue(".UniversalEventReport");
			Assert(((IDocumentSupportable)message).DocumentSupporter.IsDataContextSupported(dataContextValue));

			var providers = ((IDocumentSupportable)message).DocumentSupporter.GetBODocDataProviders(dataContextValue, null);
			AssertEquals("BODocDataProviders Count", 1, providers.Length);
			AssertEquals("BODocDataProvider Type", typeof(D4NoticeDocumentWrapper), providers[0].ParentBusinessObject.GetType());

			message.SetSystemDefinedValue(EDIMessage.Schema.XMLCustomsMessageType, new ZString("XXX"));
			providers = ((IDocumentSupportable)message).DocumentSupporter.GetBODocDataProviders(dataContextValue, null);
			AssertNull(providers);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var d4NoticeMessage = Factory.New<UniversalEventMessage>();
			d4NoticeMessage.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			d4NoticeMessage.EM_MessageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\D4NoticeMessage.xml");
			return d4NoticeMessage;
		}
	}
}
