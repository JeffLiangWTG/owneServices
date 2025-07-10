using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business.Testing
{
	class ShipmentIncoTermTest : TestCaseWithFactory
	{
		public void TestMapIncoTermCode()
		{
			AssertEquals(ShipmentIncoTerm.Codes.CIF, ShipmentIncoTerm.MapIncoTermCode(Core.Constants.IncoTerms.CostInsuranceAndFreight));
			AssertEquals(ShipmentIncoTerm.Codes.CAF, ShipmentIncoTerm.MapIncoTermCode(Core.Constants.IncoTerms.CostAndFreight));
			AssertEquals(ShipmentIncoTerm.Codes.FOB, ShipmentIncoTerm.MapIncoTermCode(Core.Constants.IncoTerms.FreeOnBoard));
			AssertEquals(ShipmentIncoTerm.Codes.CAI, ShipmentIncoTerm.MapIncoTermCode(Core.Constants.IncoTerms.CostAndInsurance));
			AssertEquals(ShipmentIncoTerm.Codes.ExWorks, ShipmentIncoTerm.MapIncoTermCode(Core.Constants.IncoTerms.ExWorks));
			Assert("SHipment Incoterm should not be translatable", new ShipmentIncoTerm() is UntranslatableCodeDescriptionPairList);
		}
	}
}
