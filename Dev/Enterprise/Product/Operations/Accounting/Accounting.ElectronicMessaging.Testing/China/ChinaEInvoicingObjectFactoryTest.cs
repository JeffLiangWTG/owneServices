using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Core;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Management;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.China.Testing
{
	[TestedType(typeof(ChinaEInvoicingObjectFactory))]
	class ChinaEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		public override void TestGetMessageType()
		{
			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			var actualMessageType = countryFactory.GetMessageType(null, null);

			AssertEquals(ZString.Empty, actualMessageType);

			var creator = new TestObjectCreator(Factory);
			var arInvoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV0001", creator.AUD, 1m, 100m, 0m, 100m, 0m, creator.AALSHI, creator.CC1.PK);
			var pivot = creator.CreateEInvoicingTransactionPivot(arInvoice);
			var eInvoicingBatch = creator.CreateEInvoicingBatchForPivot(pivot, 10, Constants.EInvoicingBatchState.Ready);

			actualMessageType = countryFactory.GetMessageType(new TransactionBatch(), eInvoicingBatch);

			AssertEquals(Constants.EInvoicingPivotActionType.Submit, pivot.AIP_ActionType);
			AssertEquals(ChinaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, actualMessageType);
		}

		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new ChinaEInvoicingObjectFactory();

		protected override Type GetExpectedTransactionBatchToPayloadWriterType() => typeof(ChinaPayloadWriter);

		protected override Type GetExpectedEInvoicingDataValidatorType() => typeof(EInvoicingDataValidatorForChina);

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(ChinaGlobalXUEFunctionalityProvider);

		public override void TestGetInvoiceEventMessageTargetQuery()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV0001", testObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testObjectCreator.Debtor, testObjectCreator.CC1.PK);
			Factory.Save();

			AssertGetInvoiceEventMessageTargetQuery(new ZGuid[] { invoice.PK });
		}

		public void TestGetInvoiceEventMessageTargetQueryWithMoreThanOnePK()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice1 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV0001", testObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testObjectCreator.Debtor, testObjectCreator.CC1.PK);
			var invoice2 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV0002", testObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testObjectCreator.Debtor, testObjectCreator.CC1.PK);
			Factory.Save();

			AssertGetInvoiceEventMessageTargetQuery(new ZGuid[] { invoice1.PK, invoice2.PK });
		}

		void AssertGetInvoiceEventMessageTargetQuery(ZGuid[] invoicePKs)
		{
			var pKs = string.Join("|", invoicePKs);
			var ediMessage = Factory.New<EDIMessage>();
			ediMessage.EM_MessageText = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
<Event>
<ContextCollection>
<Context>
<Type>EINV_CN_SerialNumber</Type>
<Value>{pKs}|WUTDCN3AB</Value>
</Context>
</ContextCollection>
</Event>
</UniversalEvent>";
			var universalEvent = ediMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var getInvoiceEventMessageTargetQuery = GetCountryFactory().GetInvoiceEventMessageTargetQuery(universalEvent);
			var invoicesInDB = Factory.CreateNewFactory().Load<InvoicingBase>(getInvoiceEventMessageTargetQuery);
			AssertContainsExactElementsInAnyOrder(invoicePKs, invoicesInDB.Select(x => x.PK));
		}
	}
}
