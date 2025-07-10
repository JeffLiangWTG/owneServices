using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ElectronicMessaging.China.Testing
{
	public class EInvoicingEventMessageCNProcessorTest : TestCaseWithFactory
	{
		public void TestMapVoidedAndCreditedAmountToDatabaseWhenIAK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var objectCreator = new TestObjectCreator(UOFactory.BOFactory);
				var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.VND, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				var batch = objectCreator.CreateEInvoicingBatch(100, Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Constants.EInvoicingPivotState.Sent);

				Factory.Save();

				var message = UOFactory.New<EDIMessage>();
				message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
				message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.UniversalDataMessaging;
				message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
				message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
				message.EM_Status = EDIMessageStatusList.Codes.Queued;
				message.EM_MessageText = string.Format(IAKMessageForRDN, 100, "CDV");

				UOFactory.SaveForTesting();

				var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
				var logger = (TestServiceLogger)serviceTask.ServiceLogger;

				var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS);
				query.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_AH, invoice.PK);
				var reference = UOFactory.LoadTop1<AccTransactionHeaderReference>(query);

				AssertNull("Precondition", reference);

				serviceTask.RunTask();

				var newFactory = new BusinessObjectFactory();
				reference = newFactory.LoadTop1<AccTransactionHeaderReference>(query);

				AssertNotNull("PostCondition", reference);
				AssertEquals("PostCondition", "CDV", reference.AH1_Reference);
				AssertEquals("PostCondition", 10m, reference.AH1_Amount);
			}
		}

		public void TestRefreshComplianceNumberAndComplianceDateForRDN()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var objectCreator = new TestObjectCreator(UOFactory.BOFactory);
				var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.VND, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				invoice.AH_ComplianceDocumentDate = new ZDate(2019, 08, 01);
				invoice.AH_TransactionReference = "1111";

				var batch = objectCreator.CreateEInvoicingBatch(100, Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Constants.EInvoicingPivotState.Sent);

				var reference = UOFactory.BOFactory.New<AccTransactionHeaderReference>();
				reference.AH1_AH = invoice.PK;
				reference.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS;
				reference.AH1_Reference = ChinaComplianceInfo.ComplianceDocumentStatusTypes.CDI.Code;

				var message = UOFactory.New<EDIMessage>();
				message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
				message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.UniversalDataMessaging;
				message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
				message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
				message.EM_Status = EDIMessageStatusList.Codes.Queued;
				message.EM_MessageText = string.Format(IAKMessageForRDN, 100, "CDI");

				UOFactory.SaveForTesting();

				var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
				serviceTask.RunTask();

				var newFactory = new BusinessObjectFactory();
				reference = newFactory.Load<AccTransactionHeaderReference>(reference.PK);
				invoice = newFactory.Load<ARInvoice>(invoice.PK);

				AssertNotNull("PostCondition", reference);
				AssertEquals("PostCondition", ChinaComplianceInfo.ComplianceDocumentStatusTypes.CDI.Code, reference.AH1_Reference);
				AssertEquals("PostCondition", "2019-08-02", invoice.AH_ComplianceDocumentDate.ToString("yyyy-MM-dd"));
				AssertEquals("PostCondition", "15200018635723479938", invoice.AH_TransactionReference);
			}
		}

		readonly string IAKMessageForRDN = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<Company>
				<Code>EDI</Code>
				<Country>
					<Code>CN</Code>
					<Name>China</Name>				</Country>
				<Name>Eagle Datamation International</Name>
			</Company>

			<DataProvider>EDIDATEDI</DataProvider>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>DAT</ServerID>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>{0}</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2019-08-01T17:51:00</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageSubType>RDN</MessageSubType>
			<MessageType>CN</MessageType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DCN</Value>
			</Context>
			<Context>
				<Type>EINV_ComplianceNumber</Type>
				<Value>15200018635723479938</Value>
			</Context>
			<Context>
				<Type>EINV_ComplianceDate</Type>
				<Value>2019-08-02 20:21:26</Value>
			</Context>
			<Context>
				<Type>EINV_ComplianceSubType</Type>
				<Value>01</Value>
			</Context>
			<Context>
				<Type>EINV_ComplianceDocumentStatus</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>EINV_CN_VoidedAndCreditedAmount</Type>
				<Value>10.00</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		UniversalObjectFactory UOFactory => uoFactory ?? (uoFactory = new UniversalObjectFactory());
		UniversalObjectFactory uoFactory;
	}
}
