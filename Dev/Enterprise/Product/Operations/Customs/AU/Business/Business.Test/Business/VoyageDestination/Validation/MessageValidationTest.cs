using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class MessageValidationTest : TestCaseWithFactory
	{
		public void TestCheckEntered()
		{
			using (testJobDeclaration.SuspendValidationTesting())
			{
				AssertEquals(false, testJobDeclaration.HasMessageErrors);
				testJobDeclaration.JE_DeclarationReference = "ES";
				messageValidation.CheckEntered(testJobDeclaration.JE_DeclarationReferenceInfo);
				AssertEquals(false, testJobDeclaration.HasMessageErrors);

				AssertEquals(false, testJobDeclaration.HasMessageErrors);
				testJobDeclaration.JE_DeclarationReference = "";
				messageValidation.CheckEntered(testJobDeclaration.JE_DeclarationReferenceInfo);
				AssertEquals(true, testJobDeclaration.HasMessageErrors);
			}
		}

		public void TestValidatePortType()
		{
			var airPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_HasAirport, "Y"));
			var mawb = Factory.New<CusMAWB>();
			var houseBill = mawb.ChildBills.AddNew();
			houseBill.CS_RL_NKDestination = airPort.RL_Code;
			houseBill.Validation.ValidateCS_RL_NKDestination();
			Assert("AirPort", !houseBill.CS_RL_NKDestinationInfo.HasWarnings());

			var seaPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_HasAirport, "N"));
			houseBill.CS_RL_NKDestination = seaPort.RL_Code;

			houseBill.Validation.ValidateCS_RL_NKDestination();
			Assert("Not Air Port", houseBill.CS_RL_NKDestinationInfo.HasWarnings());
		}

		public void TestValidateAirCargoDataDifferentFromFreight()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			hawb.CS_JS = shipment.PK;
			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			hawb.CS_FreightPrepaidCollect = "CC";
			messageValidation.ValidateAirCargoDataDifferentFromFreight(hawb.CS_FreightPrepaidCollectInfo, shipment.JS_PaymentTerm);
			AssertEquals(false, hawb.CS_FreightPrepaidCollectInfo.HasWarnings());
			hawb.CS_FreightPrepaidCollect = "AA";
			AssertEquals(true, hawb.CS_FreightPrepaidCollectInfo.HasWarnings());
		}

		JobDeclaration testJobDeclaration;
		MessageValidation messageValidation;

		protected override void SetUp()
		{
			base.SetUp();
			var factory = new BusinessObjectFactory();
			testJobDeclaration = factory.New<JobDeclaration>();
			messageValidation = new MessageValidation(testJobDeclaration);
		}
	}
}
