using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(NonPersistentRenomination))]
	class NonPersistenRenominationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NonPersistentRenomination(Factory);
		}

		public void TestSetDefaltValues()
		{
			var rn = (NonPersistentRenomination)GetNewBusinessObject();
			AssertEquals("", rn.NewAgent);
			AssertEquals(true, rn.SendNewAgentGenral);
		}
	}

	internal class NonPersistenRenominationOrchestratorTest : TestCaseWithFactory
	{
		public void TestMakeFRNAndGenral()
		{
			var mock = new Mock<IBranding>();
			mock.Setup(m => m.CompanyName).Returns("Fake Company Name");
			mock.Setup(m => m.ProductName).Returns("Fake Product Name");
			using (BrandingFactory.ConfigureTemporary(() => mock.Object))
			{
				var awb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
				awb.Profile = "CUKFFW98000XXX";
				awb.CargoTerminalOperator = "CAX";
				awb.CargoTerminalOperatorAirport = "LHR";
				var controller = new NonPersistentRenominationOrchestrator(awb);
				controller.Renomination = new NonPersistentRenomination(Factory);
				controller.Renomination.NewAgent = "DJC";
				var shutUp = new SendsMessagesToCustomsShutterUpperer();
				controller.RenominateToAgentAndMaybeSendGenral(shutUp);
				AssertEquals(2, awb.Messages.Count);
				var frnMessage = awb.Messages[0];
				AssertEquals(EDIMessage.Schema.EM_ApplicationReference + " = shed's PIMA", "CUKAIR98LHRCAX", frnMessage.EM_ApplicationReference);
				AssertEquals(EDIMessage.Schema.EM_ApplicationCode, "CUK", frnMessage.EM_ApplicationCode);
				AssertContains(EDIMessage.Schema.EM_MessageText + " is CargoFact", "CIMFRN:0:0:IA", frnMessage.EM_MessageText);
				AssertContains(EDIMessage.Schema.EM_MessageText + " is renominating to DJC", "AGT/DJC", frnMessage.EM_MessageText);
				var genralMessage = awb.Messages[1];

				AssertContains("Fake Company Name".ToUpper(), genralMessage.EM_MessageText);
				AssertContains("Fake Product Name".ToUpper(), genralMessage.EM_MessageText);
				AssertEquals("CUK", genralMessage.EM_ApplicationCode);
				AssertEquals("GEN", genralMessage.EM_MessageType);
				AssertContains("GENRAL", genralMessage.EM_MessageText);
				AssertContains("Receipient is new agent", "CUKFFW98000DJC", genralMessage.EM_ApplicationReference);
				AssertContains("Sender is current profile", "CUKFFW98000XXX", genralMessage.EM_MessageOwner);
				AssertContains("RENOMINATE RECORD 801-12345678 TO YOU, DJC", genralMessage.EM_MessageText);
				AssertContains("renominate record 801-12345678 to you, DJC", genralMessage.EM_MessageInterpretation);
			}
		}

		public void TestMakeFRN_ValidationFailures()
		{
			var awb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			awb.Profile = "CUKFFW98000XXX";
			awb.CargoTerminalOperator = "";
			awb.CargoTerminalOperatorAirport = "LHR";
			var controller = new NonPersistentRenominationOrchestrator(awb);
			controller.Renomination = new NonPersistentRenomination(Factory);
			controller.Renomination.NewAgent = "DJC";
			controller.Renomination.SendNewAgentGenral = false;
			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			controller.RenominateToAgentAndMaybeSendGenral(shutUp);
			AssertEquals(0, awb.Messages.Count);
			AssertContains("Missing data for mandatory field: Shed code", shutUp.LastErrorsAsString);
			awb.CargoTerminalOperator = "BAC";
			controller.RenominateToAgentAndMaybeSendGenral(shutUp);
			AssertEquals("FRN only, not GENRAL too", 1, awb.Messages.Count);
			AssertEquals("FRN", awb.Messages[0].EM_MessageSubType);
		}
	}
}
