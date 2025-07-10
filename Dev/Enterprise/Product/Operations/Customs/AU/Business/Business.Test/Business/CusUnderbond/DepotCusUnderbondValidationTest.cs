using CargoWise.ComponentModel;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DepotCusUnderbondValidation))]
	public class DepotCusUnderbondValidationTest : Customs.Business.Testing.CusUnderbondValidationTest
	{
		public void TestDestinationPremiseID()
		{
			Underbond.C4_DestinationPremiseID = "";
			AssertEquals("Destination Premise ID is necessary", true, Underbond.C4_DestinationPremiseIDInfo.HasErrors());

			Underbond.C4_DestinationPremiseID = "12345";
			AssertEquals("Destination Premise ID is necessary", false, Underbond.C4_DestinationPremiseIDInfo.HasErrors());
		}

		#region Implementation

		protected new CusUnderbond Underbond => (CusUnderbond)base.Underbond;

		protected override Customs.Business.CusUnderbond CreateNewUnderbond()
		{
			var wrapper = CFSShipmentWrapper.Load(Factory.New<CFSShipment>());
			var underbond = Factory.New<CusUnderbond>();
			underbond.LinkedObject = wrapper;
			return underbond;
		}

		#endregion
	}
}
