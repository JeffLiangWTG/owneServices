using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsHeaderPhase5FetchStrategyTest : NctsHeaderFetchStrategyTest
	{
		public void TestFetchForView_Explanation()
		{
			AssertCusAddInfoFetchForView(nameof(NctsHeader.Explanation));
		}

		public void TestFetchForView_HeaderUnloadingNotes()
		{
			AssertCusAddInfoFetchForView(nameof(NctsHeader.HeaderUnloadingNotes));
		}

		public void TestFetchForView_UnloadedMeansOfTransportAtDepartureIdentity()
		{
			AssertCusAddInfoFetchForView(nameof(NctsHeader.UnloadedMeansOfTransportAtDepartureIdentity));
		}

		public void TestFetchForView_UnloadedMeansOfTransportAtDepartureNationality()
		{
			AssertCusAddInfoFetchForView(nameof(NctsHeader.UnloadedMeansOfTransportAtDepartureNationality));
		}

		void AssertCusAddInfoFetchForView(string propertyName)
		{
			AssertFetchForView(propertyName, new Dictionary<string, int>
			{
				{ CusAddInfo.Schema.TableName, 1 }
			});
		}

		void AssertFetchForView(string propertyName, Dictionary<string, int> expectedDbHits)
		{
			for (var i = 0; i <= 10; i++)
			{
				CreateNctsHeader();
			}
			Factory.Save();

			var newFactory = NewFactory();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
			var headers = newFactory.Load<NctsHeader>(new ZQuery());
			newFactory.ResetDatabaseLoadCount();

			foreach (var header in headers)
			{
				header.FetchStrategy.FetchForView(new[]
				{
					new TableColumn(string.Empty, propertyName)
				});
			}

			foreach (var header in headers)
			{
				_ = header.ZPropertyInfoHash.GetPropertySafe(propertyName).Value;
			}

			//AssertDbHits(expectedDbHits, newFactory);
				// Code that does something with it
			}
		}

		protected override NctsHeader CreateNctsHeader()
		{
			var nctsHeader = base.CreateNctsHeader();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			return nctsHeader;
		}
	}
}
