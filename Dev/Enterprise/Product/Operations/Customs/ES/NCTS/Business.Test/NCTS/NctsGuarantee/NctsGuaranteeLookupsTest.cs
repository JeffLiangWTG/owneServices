using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NctsGuaranteeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReferenceNumbers_FilterBusinessObjectDefaults()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var declarantPK = header.DeclarantOrgPK;
			var referenceNumbers = header.MovementHeader.Guarantees.AddNew().Lookups.ReferenceNumbers;
			CombineAssertions(() =>
			{
				AssertEquals("DeclarantOrgPK is valid", true, declarantPK.IsValid);
				AssertEquals("Declarant is set, SecondaryGuaranteeHolderAddress is not empty", declarantPK, referenceNumbers.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Single(x => x.FilterName == Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeHolders && x.PropertyName == "Property2").Value);
			});
		}

		public void TestBondTypesList()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			header.BH_ApplicationCode = "NCT";

			var guarantee = header.Guarantees.AddNew();

			CombineAssertions(() =>
			{
				AssertType<EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList>("BondTypeList Type for Departure and NCT", guarantee.Lookups.BondTypeList);

				header.BH_ApplicationCode = "NC5";
				AssertType<ESNCTS5GuaranteeTypeList>("BondTypeList Type for Departure and NC5", guarantee.Lookups.BondTypeList);

				var bondTypes = guarantee.Lookups.BondTypeList;
				AssertEquals("0, 1, 2, 3, 4, 5, 6, 8, B", bondTypes.CodesAsString);

				header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				AssertType<EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList>("BondTypeList Type for Arrival and NC5", guarantee.Lookups.BondTypeList);

				header.BH_ApplicationCode = "NCT";
				AssertType<EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList>("BondTypeList Type for Arrival and NCT", guarantee.Lookups.BondTypeList);

				header.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
				AssertType<EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList>("BondTypeList Type for Departure And Arrival and NCT", guarantee.Lookups.BondTypeList);

				header.BH_ApplicationCode = "NC5";
				AssertType<ESNCTS5GuaranteeTypeList>("BondTypeList Type for Departure And Arrival and NC5", guarantee.Lookups.BondTypeList);
			});
		}
	}
}
