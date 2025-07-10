#region Test
#if DEBUG

using System;
using System.Collections.Generic;
using System.Xml;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.com.enett991;
using Enterprise.Core;
using Moq;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration.Testing
{
	public class MockENettWebService
	{
		MockENettWebService()
		{
			mockIntegrationService = new Mock<IeNettWebServiceClient>();
			SetupMock();
		}

		public static MockENettWebService Instance
		{
			get { return instance ?? (instance = new MockENettWebService()); }
		}
		[ThreadStatic]
		static MockENettWebService instance;

		public static void ClearInstance()
		{
			instance = null;
		}

		public IeNettWebServiceClient WebService
		{
			get { return mockIntegrationService.Object; }
		}

		readonly Mock<IeNettWebServiceClient> mockIntegrationService;

		#region Setup for test

		public void SetupForTesting(string integrator)
		{
			expectedIntegrator = integrator;
			RegisteredInvoices.Clear();

			ResetProperties();
		}

		public void ResetProperties()
		{
			FailInGetNewInvoicesInvalidIntegrator = false;
			CountCreateInvoiceWasInvoked = CountGetNewInvoicesWasInvoked = CountCancelInvoiceWasInvoked = CountGetNewPaymentsWasInvoked = CountOfflinePaymentNotificationWasInvoked = CountProcessDirectDebitWasInvoked = 0;
		}

		string expectedIntegrator;

		public List<String> RegisteredInvoices = new List<String>();
		public bool FailInGetNewInvoicesInvalidIntegrator { get; set; }

		public int CountCreateInvoiceWasInvoked { get; private set; }
		public int CountGetNewInvoicesWasInvoked { get; private set; }
		public int CountCancelInvoiceWasInvoked { get; private set; }
		public int CountGetNewPaymentsWasInvoked { get; private set; }
		public int CountOfflinePaymentNotificationWasInvoked { get; private set; }
		public int CountProcessDirectDebitWasInvoked { get; private set; }
		public int CountProcessCreditCardWasInvoked { get; private set; }
		public ZString LastBrokerPassedWhenCreateInvoiceCalled { get; private set; }
		public ZString LastTransactionNumberWhenCreateInvoiceCalled { get; private set; }

		public string NewInvoicePayload { get; set; } = ENettWebServiceTestConstants.GoodNewInvoice;
		public XmlElement NewInvoice
		{
			get
			{
				var doc = new XmlDocument();
				doc.LoadXml(NewInvoicePayload);
				return doc.DocumentElement;
			}
		}

		public XmlElement NewPayment
		{
			get
			{
				var doc = new XmlDocument();
				doc.LoadXml(ENettWebServiceTestConstants.NewPayment);
				return doc.DocumentElement;
			}
		}

		#endregion

		#region Mocks

		void SetupMock()
		{
			mockIntegrationService.Setup(x =>
				x.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
				.Returns<string, string, string, DateTime, int, int, string>((integrator, _, _, _, _, _, _) => MockGetNewInvoices(integrator == expectedIntegrator));

			mockIntegrationService.Setup(x =>
				x.GetNewPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
				.Returns<string, string, string, DateTime, int, int, string>((integrator, _, _, _, _, _, _) => MockGetNewPayments(integrator == expectedIntegrator));

			mockIntegrationService.Setup(x =>
				x.CreateInvoice(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns<string, string, string, string, string, string, string, string, string, string, string, string>(MockCreateInvoice);

			mockIntegrationService.Setup(x =>
				x.CancelInvoice(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns<string, string, string, string, string, string, string, string, string, string, string, string>(
					(integrator, _, _, _, _, _, _, invoice, _, _, _, _) =>
					MockCancelInvoice(integrator == expectedIntegrator, RegisteredInvoices.Contains(invoice))
				);

			mockIntegrationService.Setup(x =>
				x.OfflinePaymentNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns<string, string, string, string, string, string, string, string, string, string, string, bool, string, decimal, string, string>(
					(_, integrator, _, _, _, _, _, _, _, _, _, _, _, _, _, _) =>
					MockOfflinePaymentNotification(integrator == expectedIntegrator)
				);

			mockIntegrationService
				.Setup(x => x.ProcessDirectDebit(It.IsAny<ProcessDirectDebitDTO>()))
				.Returns<ProcessDirectDebitDTO>((dto) => MockProcessDirectDebit(dto.integrator == expectedIntegrator));

			mockIntegrationService
				.Setup(x => x.ProcessCreditCard(It.IsAny<ProcessCreditCardDTO>()))
				.Returns<ProcessCreditCardDTO>((dto) => MockProcessCreditCard(dto.integrator == expectedIntegrator));

			mockIntegrationService.Setup(x =>
				x.GetFxQuote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()))
				.Returns<string, string, string, string, string, string, string, string, string, decimal, string>(
				(_, _, _, _, _, _, _, method, _, _, _) => MockGetFxQuote(method == "Wire"));

			mockIntegrationService.Setup(x => x.StorageCalculation(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
				.Returns<string, string, string, string, DateTime>(MockStorageCalculation);
		}

		Response_GetNewInvoices MockGetNewInvoices(bool correctIntegrator)
		{
			CountGetNewInvoicesWasInvoked++;

			var result = new Response_GetNewInvoices();
			result.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

			if (!correctIntegrator || FailInGetNewInvoicesInvalidIntegrator)
			{
				result.errorCode = "100";
				result.errorMessage = "Invalid Integrator Details";
			}
			else
			{
				result.success = true;
				result.updates = NewInvoice;
			}

			return result;
		}

		Response_GetNewPayments MockGetNewPayments(bool correctIntegrator)
		{
			CountGetNewPaymentsWasInvoked++;

			var result = new Response_GetNewPayments();
			result.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

			if (!correctIntegrator)
			{
				result.errorCode = "100";
				result.errorMessage = "Invalid Integrator Details";
			}
			else
			{
				result.success = true;
				result.updates = NewPayment;
			}

			return result;
		}

		Response_CreateInvoice MockCreateInvoice(string integrator, string integratorKey, string version, string sourceDescription, string integratorRef, string fromClient, string toClient, string broker, string invoiceNo, string replacingInvoiceNo, string invoicePayload, string integrationAuthCode)
		{
			CountCreateInvoiceWasInvoked++;

			LastBrokerPassedWhenCreateInvoiceCalled = broker;
			LastTransactionNumberWhenCreateInvoiceCalled = invoiceNo;
			var result = new Response_CreateInvoice();
			result.receivedDateTime = ZDateTime.Now.ToDateTime();
			result.success = true;

			return result;
		}

		Response_CancelInvoice MockCancelInvoice(bool correctIntegrator, bool correctInvoiceNo)
		{
			CountCancelInvoiceWasInvoked++;

			var result = new Response_CancelInvoice();
			result.processedDateTime = ZDateTime.Now.ToDateTime();

			if (!correctIntegrator)
			{
				result.errorCode = "100";
				result.errorMessage = "Invalid Integrator Details";
			}
			else if (!correctInvoiceNo)
			{
				result.errorCode = "170";
				result.errorMessage = "Invalid Invoice Number";
			}
			else
			{
				result.success = true;
			}

			return result;
		}

		Response_OfflinePaymentNotification MockOfflinePaymentNotification(bool correctIntegrator)
		{
			CountOfflinePaymentNotificationWasInvoked++;

			var result = new Response_OfflinePaymentNotification();
			result.processedDateTime = ZDateTime.Now.ToDateTime();

			if (!correctIntegrator)
			{
				result.errorCode = "100";
				result.errorMessage = "Invalid Integrator Details";
			}
			else
			{
				result.success = true;
			}

			return result;
		}

		Response_ProcessDirectDebit MockProcessDirectDebit(bool correctIntegrator)
		{
			CountProcessDirectDebitWasInvoked++;

			var result = new Response_ProcessDirectDebit();
			result.processedDateTime = ZDateTime.Now.ToDateTime();

			if (!correctIntegrator)
			{
				result.errorCode = "100";
				result.errorMessage = "Invalid Integrator Details";
			}
			else
			{
				DateTime batchDate = new DateTime(2050, 1, 1);
				batchDate = DateTime.SpecifyKind(batchDate, DateTimeKind.Local);
				result.batchDate = batchDate;
				result.success = true;
			}

			return result;
		}

		Response_ProcessCreditCard MockProcessCreditCard(bool correctIntegrator)
		{
			CountProcessCreditCardWasInvoked++;

			var result = new Response_ProcessCreditCard();
			result.processedDateTime = ZDateTime.Now.ToDateTime();

			if (!correctIntegrator)
			{
				result.errorCode = "100";
				result.errorMessage = "Invalid Integrator Details";
			}
			else
			{
				result.success = true;
			}

			return result;
		}

		Response_CustomHouseOutboundMessage MockGetFxQuote(bool correctPaymentMethod)
		{
			var result = new Response_CustomHouseOutboundMessage();
			result.quoteID = 1234;
			result.rate = 0.8123m;
			result.currency = Constants.CurrencyCodes.UnitedStates;

			if (!correctPaymentMethod)
			{
				result.errorCode = "100";
				result.errorMessage = "Wrong type";
			}
			else
			{
				result.success = true;
			}

			return result;
		}

		Response_StorageCalculation MockStorageCalculation(string integrator, string integratorKey, string port, string container, DateTime pickupDate)
		{
			var result = new Response_StorageCalculation();
			result.amount = 0m;
			if (port == "AUSYD")
			{
				result.amount += 5;
			}
			else
			{
				result.amount += 7;
			}

			if (container == "C00001000")
			{
				result.amount += 50;
			}
			else
			{
				result.amount += 70;
			}

			if (pickupDate == new DateTime(2012, 1, 1))
			{
				result.amount += 500;
			}
			else
			{
				result.amount += 700;
			}

			result.success = true;
			return result;
		}

		#endregion
	}
}

#endif
#endregion
