using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ARLDocumentSupporter))]
	sealed class ARLDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestShowReasonForNotPrinting()
		{
			var message1 = (ARLMessage)GetDocumentSupportableBusinessObject();
			AssertEquals(false, ((IDocumentSupportable)message1).DocumentSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		public void TestARLDocumentSupporter()
		{
			var message1 = (ARLMessage)GetDocumentSupportableBusinessObject();
			var dataContextValue = new DataContextValue(".ARLReport");
			Assert(((IDocumentSupportable)message1).DocumentSupporter.IsDataContextSupported(dataContextValue));

			var providers = ((IDocumentSupportable)message1).DocumentSupporter.GetBODocDataProviders(dataContextValue, null);
			AssertEquals("BODocDataProviders Count", 1, providers.Length);
			AssertEquals("BODocDataProvider Type", typeof(ARLDailyNoticeDocumentWrapper), providers[0].ParentBusinessObject.GetType());
			message1.EM_MessageSubType = ARLMessageTypes.Codes.StatementOfAccount;
			message1.EM_MessageText = message1.EM_MessageText.Replace("<Code>DN</Code>", "<Code>SOA</Code>");
			providers = ((IDocumentSupportable)message1).DocumentSupporter.GetBODocDataProviders(dataContextValue, null);
			AssertEquals("BODocDataProviders Count", 1, providers.Length);
			AssertEquals("BODocDataProvider Type", typeof(ARLStatementOfAccountDocumentWrapper), providers[0].ParentBusinessObject.GetType());
			message1.SetSystemDefinedValue(EDIMessage.Schema.XMLCustomsMessageType, new ZString("XXX"));
			providers = ((IDocumentSupportable)message1).DocumentSupporter.GetBODocDataProviders(dataContextValue, null);
			AssertNull(providers);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var result = Factory.New<ARLMessage>();
			result.EM_MessageSubType = ARLMessageTypes.Codes.DailyNotice;
			result.EM_MessageText = @"<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CAAccountsReceivableLedger</Type>
					<Key></Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<BatchType>
			<Code>DN</Code>
		</BatchType>
		<TransactionCollection>
			<Transaction>
				<Category>SUM</Category>
				<CreateTime>2014-11-18T00:00:00</CreateTime>
				<PostDate>2014-11-17T00:00:00</PostDate>
				<PostingJournalCollection>
					<PostingJournal>
						<Description>TotalPaymentReceived</Description>
						<LocalAmount>0</LocalAmount>
					</PostingJournal>
				</PostingJournalCollection>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";

			return result;
		}
	}
}
