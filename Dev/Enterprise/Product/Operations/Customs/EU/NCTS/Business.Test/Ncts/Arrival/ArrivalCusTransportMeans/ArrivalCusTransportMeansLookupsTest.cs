using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class ArrivalCusTransportMeansLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportStateList_IsNew()
		{
			var list = lookups.TransportStateList;
			CombineAssertions(() =>
			{
				AssertEquals("Values", "DEC, MIS, NEW", list.CodesAsString);
				AssertSame("Cached", list, lookups.TransportStateList);
			});
		}

		public void TestTransportStateList()
		{
			CombineAssertions(() =>
			{
				arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
				Factory.Save();

				var list = lookups.TransportStateList;
				AssertEquals("Values", "DEC, MIS", list.CodesAsString);
				AssertSame("Cached", list, lookups.TransportStateList);
			});
		}

		public void TestTransportStateList_ShouldIncludeAllInUnloadedStatesList()
		{
			arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
			var lookups = new ArrivalCusTransportMeansLookupsForUnloadedStatesTest(arrivalCusTransportMeans);
			var list = lookups.TransportStateList;

			AssertEquals("CodesAsString", "DAM, DEC, DIF, MIS", list.CodesAsString);
		}

		public void TestTypeOfIdentificationList()
		{
			var list = lookups.TypeOfIdentificationList;
			CombineAssertions(() =>
			{
				AssertEquals("Values", "10, 11, 20, 21, 30, 31, 40, 41, 80, 81, 99", list.CodesAsString);
				AssertSame("Cached", list, lookups.TypeOfIdentificationList);
			});
		}

		public void TestTransportNationalityList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT);
			Factory.Save();

			var list = lookups.TransportNationalityList;
			list.Load();
			AssertContainsExactElementsInAnyOrder("Values", new ZString[] { "AU", "DE", "FR" }, list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalCusTransportMeans = nctsHeader.Bills.AddNew().ArrivalTransportInfos.AddNew();

			lookups = arrivalCusTransportMeans.Lookups;
		}
		NctsHeader nctsHeader;
		ArrivalCusTransportMeans arrivalCusTransportMeans;
		ArrivalCusTransportMeansLookups lookups;

		class ArrivalCusTransportMeansLookupsForUnloadedStatesTest : ArrivalCusTransportMeansLookups
		{
			public ArrivalCusTransportMeansLookupsForUnloadedStatesTest(ArrivalCusTransportMeans parent) : base(parent)
			{
			}

			protected override ZBool ShouldIncludeDIFInUnloadedStatesList => true;

			protected override ZBool ShouldIncludeDAMInUnloadedStatesList => true;
		}
	}
}
