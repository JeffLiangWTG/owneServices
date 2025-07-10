using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessagesWrappers.Testing;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class TransportWrapperTest : TestCaseWithFactory
	{
		public void TestTransportWrapperConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TransportWrapper(null));
		}

		public void TestTransportID()
		{
			AssertEquals(WrapperTestHelper.VesselCode, transportWrapperExp.TransportID);
		}

		public void TestTransportMethodPayment()
		{
			AssertEquals(TransportChargesModeOfPayment.Codes.CreditCard, transportWrapperExp.TransportMethodPayment);
		}

		public void TestContainerMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = "IMP";

			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();

			var entryheader = declaration.CustomsEntryHeaders.AddNew();
			var entryline = entryheader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryline.PK;

			AssertcontainerTraandContainerMode(declaration, "0");

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = ZString.Empty;
			AssertcontainerTraandContainerMode(declaration, "0");

			container.CO_ContainerNumber = "container";
			AssertcontainerTraandContainerMode(declaration, "1");
		}

		void AssertcontainerTraandContainerMode(JobDeclaration declaration, string containerTra)
		{
			var entry = declaration.CustomsEntryHeaders[0];
			var transportWrapperExp = new TransportWrapper(entry);
			declaration.JE_ContainerMode = ZString.Empty;
			AssertEquals("0", transportWrapperExp.ContainerMode);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("0", transportWrapperExp.ContainerMode);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("0", transportWrapperExp.ContainerMode);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.RollOnRollOff;
			AssertEquals("0", transportWrapperExp.ContainerMode);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("0", transportWrapperExp.ContainerMode);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Loose;
			AssertEquals("0", transportWrapperExp.ContainerMode);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LTL;
			AssertEquals("0", transportWrapperExp.ContainerMode);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FTL;
			AssertEquals("0", transportWrapperExp.ContainerMode);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("0", transportWrapperExp.ContainerMode);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(containerTra, transportWrapperExp.ContainerMode);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals(containerTra, transportWrapperExp.ContainerMode);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.ULD;
			AssertEquals(containerTra, transportWrapperExp.ContainerMode);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals(containerTra, transportWrapperExp.ContainerMode);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var wrapperHelper = new WrapperTestHelper();
			var cusEntryHeaderExp = wrapperHelper.CreateTestCusEntryHeader(false);
			transportWrapperExp = new TransportWrapper(cusEntryHeaderExp);
		}
		ITransport transportWrapperExp;
	}
}
