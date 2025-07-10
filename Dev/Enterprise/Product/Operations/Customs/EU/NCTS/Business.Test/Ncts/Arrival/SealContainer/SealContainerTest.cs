using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(SealContainer))]
	sealed class SealContainerTest : Customs.Business.Testing.CusInBondContainerTest<SealContainer>
	{
		public void TestBC_Seal1()
		{
			var sealContainer = Factory.New<SealContainer>();
			AssertEquals("Caption is set in the business layer so that validation and UI can use it without setting it twice.", "Seal Number", sealContainer.BC_Seal1Info.HumanReadableName);
		}

		protected override BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var enRouteSeal = header.EnRouteSeals.AddNew();
			return enRouteSeal.SealContainers.AddNew();
		}
	}
}
