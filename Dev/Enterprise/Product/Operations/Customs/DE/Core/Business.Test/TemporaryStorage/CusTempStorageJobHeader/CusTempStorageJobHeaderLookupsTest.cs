using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class CusTempStorageJobHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportMeansList()
		{
			CombineAssertions(() =>
			{
				var transportMeansList = lookups.TransportMeansList;
				AssertEquals("Values", "01, 02, 03, 04, 05, 06, 07", transportMeansList.CodesAsString);
				AssertSame("Cached", transportMeansList, lookups.TransportMeansList);
			});
		}

		public void TestPreviousReferenceTypeList()
		{
			CombineAssertions(() =>
			{
				var previousReferenceTypeList = lookups.PreviousReferenceTypeList;
				AssertEquals("Values", "199, 200, 444T1, 444T2, 444TF, 447T1, 447T2, 447TF, A, AE, ATA, AV, ENST2L, ESUMA, FREIZ, FV, MAN, MO, N355, OESUMA, OHNE, POUS, PVV, T-, T1, T1CF, T1DF, T1IC, T1IE, T1IF, T2, T2AN, T2CF, T2DF, T2F, T2IC, T2IE, T2IF, T2L, T2LF, T2M, T2SM, T5, TIR, TRPPVW, V, VV, Z, ZL", previousReferenceTypeList.CodesAsString);
				AssertSame("Cached", previousReferenceTypeList, lookups.PreviousReferenceTypeList);
			});
		}

		public void TestTransportModeList_Lastkraftwagen()
		{
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Truck;
			var transportModeList = lookups.TransportModeList;
			CombineAssertions(() =>
			{
				AssertEquals("Values", "ROA", transportModeList.CodesAsString);
				AssertSame("Cached", transportModeList, lookups.TransportModeList);
			});
		}

		public void TestTransportModeList_Schiff()
		{
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Vessel;
			var transportModeList = lookups.TransportModeList;
			AssertEquals("Values", "IWT, SEA", transportModeList.CodesAsString);
		}

		public void TestTransportModeList_Waggon()
		{
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Wagon;
			var transportModeList = lookups.TransportModeList;
			AssertEquals("Values", "RAI", transportModeList.CodesAsString);
		}

		public void TestTransportModeList_Flugzeug()
		{
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Aircraft;
			var transportModeList = lookups.TransportModeList;
			AssertEquals("Values", "AIR", transportModeList.CodesAsString);
		}

		public void TestTransportModeList_Pkw()
		{
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Car;
			var transportModeList = lookups.TransportModeList;
			AssertEquals("Values", "ROA", transportModeList.CodesAsString);
		}

		public void TestTransportModeList_Ohne()
		{
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Without;
			var transportModeList = lookups.TransportModeList;
			AssertEquals("Values", "FIX, OWN, MAI", transportModeList.CodesAsString);
		}

		public void TestTransportModeList_Andere()
		{
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Other;
			var transportModeList = lookups.TransportModeList;
			AssertEquals("Values", "FIX, OWN, MAI", transportModeList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusTempStorageJobHeader>();
			lookups = new CusTempStorageJobHeaderLookups(header);
		}
		CusTempStorageJobHeader header;
		CusTempStorageJobHeaderLookups lookups;
	}
}
