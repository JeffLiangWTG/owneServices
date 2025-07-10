using System;
using System.Net;
using System.Net.Sockets;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.eNett_Integration;
using Enterprise.Accounting.Business.eNett_Integration.Testing;
using Enterprise.Accounting.DataTransfer.com.enett991;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration.Testing
{
	public class eNettWebServiceWrapperTransactionedTest : TransactionedTestCase
	{
		public void TestWebServiceUsesCorrectUrl()
		{
			eNettWebServiceWrapper.UseRealWebService_ForTesting = true;
			AccountingConfigurationRegistry.Instance.ENettWebServiceLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.microsoft.com/");
			AssertEquals("Should have referenced http://www.microsoft.com/", "http://www.microsoft.com/", eNettWebServiceWrapper.CreateNewWebService().Url);
			AccountingConfigurationRegistry.Instance.ENettWebServiceLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.cargowise.com/");
			AssertEquals("Should have referenced http://www.cargowise.com/ now", "http://www.cargowise.com/", eNettWebServiceWrapper.CreateNewWebService().Url);
		}

		public void TestGetContainerStorageFeeHandleWebException()
		{
			var mockService = new IntegrationService_ForExceptionTests(new WebException());
			var wrapper = new eNettWebServiceWrapper();
			var invoice = new StorageFeeInvoicePayment(new MockContainerStorageDataProvider());
			invoice.PortCode = "AUSYD";
			invoice.PickupDate = ZDateTime.BrettsBirthday;

			var result = wrapper.GetContainerStorageFee(mockService, invoice);

			AssertCollectionContains("An error occurred when connecting to the eNett Web Service", result.Messages);
			mockService.AssertGetWebRequestWasCalled();
		}

		public void TestGetContainerStorageFeeHandleSocketException()
		{
			var mockService = new IntegrationService_ForExceptionTests(new SocketException());
			var wrapper = new eNettWebServiceWrapper();
			var invoice = new StorageFeeInvoicePayment(new MockContainerStorageDataProvider());
			invoice.PortCode = "AUSYD";
			invoice.PickupDate = ZDateTime.BrettsBirthday;

			var result = wrapper.GetContainerStorageFee(mockService, invoice);

			AssertCollectionContains("An error occurred when connecting to the eNett Web Service", result.Messages);
			mockService.AssertGetWebRequestWasCalled();
		}

		public void TestGetContainerStorageFeeHandleInvalidOperationException()
		{
			var mockService = new IntegrationService_ForExceptionTests(new InvalidOperationException());
			var wrapper = new eNettWebServiceWrapper();
			var invoice = new StorageFeeInvoicePayment(new MockContainerStorageDataProvider());
			invoice.PortCode = "AUSYD";
			invoice.PickupDate = ZDateTime.BrettsBirthday;

			var result = wrapper.GetContainerStorageFee(mockService, invoice);

			AssertCollectionContains("An error occurred when connecting to the eNett Web Service", result.Messages);
			mockService.AssertGetWebRequestWasCalled();
		}

		public void TestGetContainerStorageFeeHandleNullReferenceException()
		{
			var mockService = new IntegrationService_ForExceptionTests(new NullReferenceException());
			var wrapper = new eNettWebServiceWrapper();
			var invoice = new StorageFeeInvoicePayment(new MockContainerStorageDataProvider());
			invoice.PortCode = "AUSYD";
			invoice.PickupDate = ZDateTime.BrettsBirthday;

			var result = wrapper.GetContainerStorageFee(mockService, invoice);

			AssertCollectionContains("An error occurred when connecting to the eNett Web Service", result.Messages);
			mockService.AssertGetWebRequestWasCalled();
		}

		public void TestDisplayClientListHandleWebException()
		{
			var enettCode = new EnettRegistrationCode();
			enettCode.RegistrationCode = "556";
			enettCode.OrganisationPK = new TestObjectCreator(new BusinessObjectFactory()).AALSHI.PK;
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enettCode);

			var mockService = new IntegrationService_ForExceptionTests(new WebException());
			var wrapper = new eNettWebServiceWrapper();

			var error = wrapper.DisplayClientList(mockService, new IntercompanyCostsApportionmentCollection(null, null));

			AssertEquals("There was a problem connecting to the ComPay web service. Please try again later.", error);
			mockService.AssertGetWebRequestWasCalled();
		}

		public void TestDisplayClientListHandleSocketException()
		{
			var enettCode = new EnettRegistrationCode();
			enettCode.RegistrationCode = "556";
			enettCode.OrganisationPK = new TestObjectCreator(new BusinessObjectFactory()).AALSHI.PK;
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enettCode);

			var mockService = new IntegrationService_ForExceptionTests(new SocketException());
			var wrapper = new eNettWebServiceWrapper();

			var error = wrapper.DisplayClientList(mockService, new IntercompanyCostsApportionmentCollection(null, null));

			AssertEquals("There was a problem connecting to the ComPay web service. Please try again later.", error);
			mockService.AssertGetWebRequestWasCalled();
		}

		public void TestDisplayClientListHandleInvalidOperationException()
		{
			var enettCode = new EnettRegistrationCode();
			enettCode.RegistrationCode = "556";
			enettCode.OrganisationPK = new TestObjectCreator(new BusinessObjectFactory()).AALSHI.PK;
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enettCode);

			var mockService = new IntegrationService_ForExceptionTests(new SocketException());
			var wrapper = new eNettWebServiceWrapper();

			var error = wrapper.DisplayClientList(mockService, new IntercompanyCostsApportionmentCollection(null, null));

			AssertEquals("There was a problem connecting to the ComPay web service. Please try again later.", error);
			mockService.AssertGetWebRequestWasCalled();
		}

		class IntegrationService_ForExceptionTests : IntegrationService
		{
			internal IntegrationService_ForExceptionTests(Exception ex)
				: base()
			{
				ThrowOnWebRequest = ex;
			}

			readonly Exception ThrowOnWebRequest;
			bool WasCalled;

			protected override WebRequest GetWebRequest(Uri uri)
			{
				WasCalled = true;
				throw ThrowOnWebRequest;
			}

			internal void AssertGetWebRequestWasCalled()
			{
				if (!WasCalled)
				{
					throw new Exception("Expected call to GetWebRequest() but none happened.");
				}
			}
		}
	}
}
