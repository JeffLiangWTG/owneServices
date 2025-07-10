using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaArrivalLineLookupsTest : TestCaseWithFactory
	{
		public void TestPacks()
		{
			var header1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			var arrivalHeader = header1.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "FL2";
			var arrivalLine = arrivalHeader.ArrivalDetails.AddNew();
			arrivalLine.ATL_CargoStatus = "STA";
			Factory.Save();
			AssertEquals("should be generic type", true, arrivalLine.Lookups.Packs.GetType().IsGenericType);
			var genericParams = arrivalLine.Lookups.Packs.GetType().GetGenericArguments();
			AssertEquals("should be 2 generic arguments", 2, genericParams.Length);
			AssertEquals("first argument should be AsycudaPack", true, typeof(AsycudaPack).IsAssignableFrom(genericParams[0]));
			AssertEquals("first argument should be AsycudaBill", true, typeof(AsycudaBill).IsAssignableFrom(genericParams[1]));
		}
	}
}
