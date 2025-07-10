using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	sealed class IncoTermsCodeDescriptionPairListTest : TestCaseWithFactory
	{
		public void TestGetIncoTerms()
		{
			var defaultIncoTerms = new[]
			{
				Constants.IncoTerms.ExWorks,
				Constants.IncoTerms.FreeCarrier,
				Constants.IncoTerms.CarriagePaidTo,
				Constants.IncoTerms.CarriageAndInsurancePaidTo,
				Constants.IncoTerms.DeliveredAtPlace,
				Constants.IncoTerms.DeliveredAtPlaceUnloaded,
				Constants.IncoTerms.DeliveredDutyPaid,
				Constants.IncoTerms.Other
			};

			var seaInlandWaterway = new[]
			{
				Constants.IncoTerms.FreeOnBoard,
				Constants.IncoTerms.CostAndFreight,
				Constants.IncoTerms.CostInsuranceAndFreight,
				Constants.IncoTerms.FreeAlongsideShip
			};

			CombineAssertions(() =>
			{
				var defaultAndSeaInlandWaterway = defaultIncoTerms.Union(seaInlandWaterway);
				AssertContainsExactElementsInAnyOrder(defaultAndSeaInlandWaterway, new IncoTermsCodeDescriptionPairList(TransportTypeList.Codes.Sea).GetAllCodes());
				AssertContainsExactElementsInAnyOrder(defaultAndSeaInlandWaterway, new IncoTermsCodeDescriptionPairList(TransportTypeList.Codes.InlandWaterwayTransport).GetAllCodes());
				AssertContainsExactElementsInAnyOrder(defaultIncoTerms, new IncoTermsCodeDescriptionPairList(TransportTypeList.Codes.Road).GetAllCodes());
			});
		}
	}
}
