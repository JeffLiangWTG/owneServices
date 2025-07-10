using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class GEIDeliveryModeDeciderTest : TestCaseWithFactory
	{
		public void TestDeliveryMode_EAD()
		{
			var context = EInvoicingTestHelper.GetDeliveryContext(Factory, null, "GEI", "REQ", "REQ", new Logger());
			var mode = EInvoicingTestHelper.CreateCommunicaitonMode(EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface, "ITDestination");
			var provider = EInvoicingTestHelper.GetProvider(context, true, mode);
			var deliveryMode = new GEIDeliveryModeDecider(provider).GetDeliveryMode(mode, false);
			AssertType(typeof(GEIEAdaptorDelivery), deliveryMode);
		}

		public void TestDeliveryMode_EAD_Failed()
		{
			var context = EInvoicingTestHelper.GetDeliveryContext(Factory, null, "GEI", "REQ", "REQ", new Logger());
			var mode = EInvoicingTestHelper.CreateCommunicaitonMode(EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface, "ITDestination");
			var provider = EInvoicingTestHelper.GetProvider(context, true, mode);
			var deliveryMode = new GEIDeliveryModeDecider(provider).GetDeliveryMode(mode, true);
			AssertType(typeof(FailedGEIEAdaptorDelivery), deliveryMode);
		}

		public void TestDeliveryMode_HUB()
		{
			var context = EInvoicingTestHelper.GetDeliveryContext(Factory, null, "GEI", "REQ", "REQ", new Logger());
			var mode = EInvoicingTestHelper.CreateCommunicaitonMode(EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "ITDestination");
			var provider = EInvoicingTestHelper.GetProvider(context, true, mode);
			var deliveryMode = new GEIDeliveryModeDecider(provider).GetDeliveryMode(mode, false);
			AssertType(typeof(GEIEHubDelivery), deliveryMode);
		}

		public void TestDeliveryMode_HUB_Failed()
		{
			var context = EInvoicingTestHelper.GetDeliveryContext(Factory, null, "GEI", "REQ", "REQ", new Logger());
			var mode = EInvoicingTestHelper.CreateCommunicaitonMode(EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "ITDestination");
			var provider = EInvoicingTestHelper.GetProvider(context, true, mode);
			var deliveryMode = new GEIDeliveryModeDecider(provider).GetDeliveryMode(mode, true);
			AssertType(typeof(FailedGEIEHubDelivery), deliveryMode);
		}

		public void TestDeliveryMode_DirectXT()
		{
			var context = EInvoicingTestHelper.GetDeliveryContext(Factory, null, "GEI", "GEN", "GEN", new Logger());
			var mode = EInvoicingTestHelper.CreateCommunicaitonMode(EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface, "ITDestination");
			var provider = EInvoicingTestHelper.GetProvider(context, true, mode);
			var deliveryMode = new GEIDeliveryModeDecider(provider).GetDeliveryMode(mode, false);
			AssertType(typeof(GEIDirectXTDelivery), deliveryMode);
		}

		public void TestDeliveryMode_DirectXT_Failed()
		{
			var context = EInvoicingTestHelper.GetDeliveryContext(Factory, null, "GEI", "GEN", "GEN", new Logger());
			var mode = EInvoicingTestHelper.CreateCommunicaitonMode(EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface, "ITDestination");
			var provider = EInvoicingTestHelper.GetProvider(context, true, mode);
			var deliveryMode = new GEIDeliveryModeDecider(provider).GetDeliveryMode(mode, true);
			AssertType(typeof(FailedGEIDirectXTDelivery), deliveryMode);
		}

		public void TestDeliveryMode_NotSupported()
		{
			var context = EInvoicingTestHelper.GetDeliveryContext(Factory, null, "GEI", "REQ", "REQ", new Logger());
			var mode = EInvoicingTestHelper.CreateCommunicaitonMode(EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector, "ITDestination");
			var provider = EInvoicingTestHelper.GetProvider(context, true, mode);
			AssertExceptionThrown<NotSupportedException>(() => new GEIDeliveryModeDecider(provider).GetDeliveryMode(mode, true));
		}
	}
}
