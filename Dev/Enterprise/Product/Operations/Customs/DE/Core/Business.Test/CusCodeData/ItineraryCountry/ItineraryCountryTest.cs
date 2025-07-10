using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ItineraryCountry))]
	sealed class ItineraryCountryTest : Customs.Business.Testing.CusCodeDataTest<ItineraryCountry>
	{
		public void TestValidation()
		{
			var itineraryCountry = Factory.New<ItineraryCountry>();
			AssertType<ItineraryCountryValidation>(itineraryCountry.Validation);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => NewBusinessObjectCore(Factory);

		protected override IEnumerable<ItineraryCountry> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (ItineraryCountry)NewBusinessObjectCore(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => NewBusinessObjectCore(factory);

		static BusinessObject NewBusinessObjectCore(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			return declaration.ItineraryCountries.AddNew();
		}

		#endregion
	}
}
