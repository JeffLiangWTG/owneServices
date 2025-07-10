using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	internal sealed class AUCustomsScheduleValidationProviderTest : TestCaseWithFactory
	{
		public void TestGetExtraVoyageValidation()
		{
			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Ukraine);
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Road;
			AssertNull("Should be NULL", provider.GetExtraVoyageValidation(voyage));

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			AssertNull("Should be NULL", provider.GetExtraVoyageValidation(voyage));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AssertType("Should be ExtraVoyageValidation", typeof(ExtraVoyageValidation), provider.GetExtraVoyageValidation(voyage));

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Road;
			AssertNull("Should be NULL", provider.GetExtraVoyageValidation(voyage));
		}

		#region Implementation

		readonly IScheduleValidationProvider provider = new AUCustomsScheduleValidationProvider();

		#endregion
	}
}
