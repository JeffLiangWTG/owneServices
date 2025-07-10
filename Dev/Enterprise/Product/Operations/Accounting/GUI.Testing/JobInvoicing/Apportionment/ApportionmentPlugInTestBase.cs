using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting.Testing
{
	public abstract class ApportionmentPlugInTestBase : TestCaseWithFactory
	{
		public abstract void TestName();
		public abstract void TestRefreshGatewayElements();
		public abstract void TestCheckIsUsedForGatewayApportionments();

		protected abstract ApportionmentPlugin GetTestApportionmentPluginObject(IBusiness consol);

		protected void AssertCheckIsUsedForGatewayApportionments(IBusiness consol, bool expectedResult)
		{
			using (var plugIn = GetTestApportionmentPluginObject(consol))
			{
				AssertEquals(expectedResult, plugIn.CheckIsUsedForGatewayApportionments());
			}
		}

		protected ForwardingConsol CreateConsol(string portOfLoading = "AUBNE", string portOfDischarge = "NZAKL")
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportCodes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_RL_NKDischargePort = portOfLoading;
			consol.JK_RL_NKLoadPort = portOfDischarge;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = portOfLoading;
			transport.JW_RL_NKDiscPort = portOfDischarge;

			return consol;
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return fCreator ?? (fCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fCreator;

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;
	}
}
