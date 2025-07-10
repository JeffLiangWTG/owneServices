using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	public class CusTempStorageJobHeaderFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView_Customer()
		{
			AssertFetchForViewDbHits(new string[] { "Customer+OH_Code", "Customer+OH_FullName" },
				new Dictionary<string, int>() { { OrgHeaderSchema.Constants.TableName, 1 } });
		}

		public void TestFetchForView_Presenter_Representative()
		{
			AssertFetchForViewDbHits(new string[] { "Presenter+EffectiveCompanyName", "Representative+EffectiveCompanyName" },
				new Dictionary<string, int>() { { OrgAddressSchema.Constants.TableName, 1 } },
				(bo) =>
				{
					var address = Factory.NewWithValidTestData<OrgAddress>();
					bo.SJH_OA_Presenter = address.PK;
					bo.SJH_OA_Representative = address.PK;
				});
		}

		public void TestFetchForView_DDTNumber()
		{
			AssertFetchForViewDbHits(new string[] { CusTempStorageJobHeader.Schema.DDTNumber },
				new Dictionary<string, int>() { { CusEntryNumSchema.Constants.TableName, 1 } });
		}

		void AssertFetchForViewDbHits(string[] viewColumnNames, Dictionary<string, int> expectedDbHits, Action<CusTempStorageJobHeader> additionalInit = null)
		{
			List<ZGuid> headerPKs = new List<ZGuid>();
			for (var i = 0; i < 10; i++)
			{
				var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
				additionalInit?.Invoke(header);
				headerPKs.Add(header.PK);
			}
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var headers = newFactory.Load<CusTempStorageJobHeader>(new ZQuery(CusTempStorageJobHeaderSchema.PK, headerPKs));
			headers.ForEach(bo =>
			{
				var columns = viewColumnNames.Select(x => new TableColumn("", x)).ToArray();
				bo.FetchStrategy.FetchForView(columns);
			});
			headers.ForEach(bo =>
			{
				viewColumnNames.ForEach(prop => _ = bo[prop]);
			});
			expectedDbHits.ForEach(e =>
			{
				AssertEquals($"Test Table {e.Key} Hint", e.Value, newFactory.GetTableHitCount(e.Key));
			});
		}
	}
}
