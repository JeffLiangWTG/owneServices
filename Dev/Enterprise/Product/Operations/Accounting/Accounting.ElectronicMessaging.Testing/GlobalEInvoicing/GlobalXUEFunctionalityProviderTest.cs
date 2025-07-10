using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing
{
	[TestsSubclassesOf(typeof(GlobalXUEFunctionalityProvider))]
	abstract class GlobalXUEFunctionalityProviderTest : TestCaseWithFactory
	{
		protected abstract CountryEInvoicingObjectFactory GetTestCountryFactory();

		public void TestConstructorRequiredParameter()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new GlobalXUEFunctionalityProvider(null));
		}

		public void TestDefaultConstructorDoesNotThrowException()
		{
			AssertNoExceptionThrown("Component dependency mut be created.", () => new GlobalXUEFunctionalityProvider(CountryEInvoicingObjectFactory));
		}

		#region GetEventMessageProcessor

		public virtual void TestGetEventMessageProcessor()
		{
			var objects = GetEventMessageProcessorObjects();

			AssertGetEventMessageProcessor("IAK", "", objects, ExpectedEventMessageProcessor);
			AssertGetEventMessageProcessor("IAK", "PQR", objects, ExpectedEventMessageProcessor);
			AssertGetEventMessageProcessor("IAK", "GEN", objects, ExpectedEventMessageProcessor);
			AssertGetEventMessageProcessor("IRJ", "", objects, ExpectedEventMessageProcessor);
			AssertGetEventMessageProcessor("IRJ", "PQR", objects, ExpectedEventMessageProcessor);
			AssertGetEventMessageProcessor("IRJ", "GEN", objects, ExpectedEventMessageProcessor);
		}

		protected virtual Type ExpectedEventMessageProcessor => typeof(GlobalEInvoicingEventMessageProcessor);

		protected void AssertGetEventMessageProcessor(ZString eventType, ZString messageSubType, (IXmlSessionTracker logger, IEDIMessage ediMessage, UniversalEvent universalEvent, AccEInvoicingBatch batch) objects, Type expectedEventMessageProcessorType)
		{
			var actualEventMessageProcessor = GetGlobalXUEFunctionalityProvider.GetEventMessageProcessor(new EventMessageProcessorData(eventType, messageSubType, objects.logger, objects.ediMessage, objects.universalEvent, objects.batch));

			if (expectedEventMessageProcessorType == null)
			{
				AssertEquals(null, actualEventMessageProcessor);
			}
			else
			{
				Assert("actualEventMessageProcessor should not be null.", actualEventMessageProcessor != null);
				AssertEquals(expectedEventMessageProcessorType, actualEventMessageProcessor.GetType());
			}
		}

		protected (IXmlSessionTracker logger, IEDIMessage ediMessage, UniversalEvent universalEvent, AccEInvoicingBatch batch) GetEventMessageProcessorObjects(string messageText = null)
		{
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			var ediMessage = Factory.New<EDIMessage>();
			ediMessage.EM_MessageText = messageText ?? @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""></UniversalEvent>";
			ediMessage.MessageNumberStrategy = new MessageNumberStrategy_ForTest("1");
			var universalEvent = ediMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();

			var objectCreator = new TestObjectCreator(Factory);
			var batch = objectCreator.CreateEInvoicingBatch(1, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);

			return (logger, ediMessage, universalEvent, batch);
		}

		internal class MessageNumberStrategy_ForTest : IMessageNumberStrategy
		{
			public MessageNumberStrategy_ForTest(string number)
			{
				ReferenceNumber = number;
			}

			readonly string ReferenceNumber;
			public string GetMessageReferenceNumber() => ReferenceNumber;
		}

		#endregion GetEventMessageProcessor

		#region GetEventMessageChildrenProcessor

		public void TestGetEventMessageChildrenProcessor()
		{
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			var ediMessage = Factory.New<EDIMessage>();
			ediMessage.EM_MessageText = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11""></UniversalEvent>";
			var universalEvent = ediMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var objectCreator = new TestObjectCreator(Factory);
			var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.CNY, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var processor = GetGlobalXUEFunctionalityProvider.GetEventMessageChildrenProcessor(new EventMessageChildrenProcessorData(logger, ediMessage, universalEvent, invoice));

			AssertEventMessageChildrenProcessor(processor);
		}

		protected virtual void AssertEventMessageChildrenProcessor(EInvoicingEventMessageProcessor childrenProcessor)
		{
			AssertNull(childrenProcessor);
		}

		#endregion GetEventMessageChildrenProcessor

		#region IsEventMessageChildrenProcessSupported

		public virtual void TestIsEventMessageChildrenProcessSupported()
		{
			var isEventMessageProcessSupported = GetGlobalXUEFunctionalityProvider.IsEventMessageChildrenProcessSupported("IAK");
			Assert(!isEventMessageProcessSupported);

			isEventMessageProcessSupported = GetGlobalXUEFunctionalityProvider.IsEventMessageChildrenProcessSupported("XXX");
			Assert(!isEventMessageProcessSupported);
		}

		#endregion IsEventMessageChildrenProcessSupported

		#region IsInvoiceEventMessageProcessSupported

		public virtual void TestIsInvoiceEventMessageProcessSupported()
		{
			var isInvoiceEventMessageProcessSupported = GetGlobalXUEFunctionalityProvider.IsInvoiceEventMessageProcessSupported("XXX");
			Assert(!isInvoiceEventMessageProcessSupported);
		}

		#endregion IsInvoiceEventMessageProcessSupported

		#region IsBatchEventMessageProcessSupported

		public virtual void TestIsEventMessageProcessSupported()
		{
			AssertIsEventMessageProcessSupported("IAK", "", true);
			AssertIsEventMessageProcessSupported("IAK", "XYZ", true);
			AssertIsEventMessageProcessSupported("IAK", "GEN", true);

			AssertIsEventMessageProcessSupported("IRJ", "", true);
			AssertIsEventMessageProcessSupported("IRJ", "XYZ", true);
			AssertIsEventMessageProcessSupported("IRJ", "GEN", true);

			AssertIsEventMessageProcessSupported("OTR", "", false);
			AssertIsEventMessageProcessSupported("OTR", "XYZ", false);
			AssertIsEventMessageProcessSupported("OTR", "GEN", false);
		}

		protected void AssertIsEventMessageProcessSupported(string eventType, string messageSubType, bool expectedIsEventMessageProcessSupported)
		{
			var actualIsEventMessageProcessSupported = GetGlobalXUEFunctionalityProvider.IsBatchEventMessageProcessSupported(eventType, messageSubType);
			AssertEquals(expectedIsEventMessageProcessSupported, actualIsEventMessageProcessSupported);
		}

		#endregion IsBatchEventMessageProcessSupported

		protected override void SetUp()
		{
			base.SetUp();
			CountryEInvoicingObjectFactory = GetTestCountryFactory();
			GetGlobalXUEFunctionalityProvider = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider();
		}

		protected ICountryEInvoicingObjectFactory CountryEInvoicingObjectFactory;
		protected IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider;
	}
}
