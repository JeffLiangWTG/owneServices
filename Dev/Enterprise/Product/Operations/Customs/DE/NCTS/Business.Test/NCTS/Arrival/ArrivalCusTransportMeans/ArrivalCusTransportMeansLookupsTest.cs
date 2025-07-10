using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class ArrivalCusTransportMeansLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportStateList_OriginalStatusNotNEW()
		{
			arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;

			var list = lookups.TransportStateList;
			CombineAssertions(() =>
			{
				AssertEquals("Values", "DEC, DIF, MIS, NEW", list.CodesAsString);
				AssertSame("Cached", list, lookups.TransportStateList);
			});
		}

		public void TestTransportStateListWhenTypeOfIdentificationIsChanged()
		{
			string[] identificationTypes = { "10", "11", "30", "40", "41", "80", "81" };
			arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;

			foreach (var type in identificationTypes)
			{
				arrivalCusTransportMeans.TPM_TypeOfIdentification = type;
				var lookups = arrivalCusTransportMeans.Lookups;
				var list = lookups.TransportStateList;

				CombineAssertions(() =>
				{
					AssertEquals($"Values for TypeOfIdentification {type}", "DEC, DIF", list.CodesAsString);
					AssertSame("Cached", list, lookups.TransportStateList);
				});
			}
			arrivalCusTransportMeans.TPM_TypeOfIdentification = "99";
			CombineAssertions(() =>
			{
				AssertEquals($"Values for TypeOfIdentification 99", "DEC, DIF, MIS, NEW", arrivalCusTransportMeans.Lookups.TransportStateList.CodesAsString);
				AssertSame("Cached", arrivalCusTransportMeans.Lookups.TransportStateList, lookups.TransportStateList);
			});
		}

		public void TestTransportStateList_OriginalStatusNEW()
		{
			AssertEquals("Precondition", NctsUnloadedStateList.Codes.NEW, arrivalCusTransportMeans.TPM_TransportState);

			var list = lookups.TransportStateList;
			CombineAssertions(() =>
			{
				AssertEquals("Values", "DEC, DIF, MIS, NEW", list.CodesAsString);
				AssertSame("Cached", list, lookups.TransportStateList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalCusTransportMeans = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();

			lookups = arrivalCusTransportMeans.Lookups;
		}
		ArrivalCusTransportMeansLookups lookups;
		ArrivalCusTransportMeans arrivalCusTransportMeans;
	}
}
