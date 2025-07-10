using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class JobDeclarationTransportValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJW_LegOrder()
		{
			CombineAssertions(() =>
			{
				var legNoHasAlreadyBeenEntered = "This Leg No. has already been entered.";
				transport.Validation.ValidateJW_LegOrder();
				transport.JW_LegOrder = 0;
				AssertHasMessageError("Not Entered", transport.JW_LegOrderInfo, "You have not entered a Leg Order.");
				transport.JW_LegOrder = 1;
				AssertNoMessageError("Only One", transport.JW_LegOrderInfo, legNoHasAlreadyBeenEntered);

				var transport2 = transportParent.Transports.AddNew();
				transport2.JW_LegOrder = 1;
				AssertHasMessageError("Not unique", transport2.JW_LegOrderInfo, legNoHasAlreadyBeenEntered);
				transport2.JW_LegOrder = 2;
				AssertNoMessageError("Unique.", transport2.JW_LegOrderInfo, legNoHasAlreadyBeenEntered);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			transportParent = Factory.New<JobDeclaration>();
			transport = transportParent.Transports.AddNew();
		}

		Transport transport;
		ITransportParent transportParent;
	}
}
