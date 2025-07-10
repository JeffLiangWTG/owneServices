using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(DecCusTempStorageLineCollectionFrom<CusTempStorageLine, CusTempStorageDec>))]
	public class CusTempStorageLineCollectionFromTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationshipFilter()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "C1";
			var presenter = Factory.NewWithValidTestData<OrgAddress>();
			var representative = Factory.NewWithValidTestData<OrgAddress>();

			var header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_GB = GlbBranch.CurrentBranch.PK;
			header.SJH_JobReference = "From1";
			header.SJH_OH_Customer = customer.PK;
			header.SJH_OA_Presenter = presenter.PK;
			header.SJH_OA_Representative = representative.PK;

			var cusTempStorageDec = Factory.New<CusTempStorageDec>();
			cusTempStorageDec.STH_SJH = header.PK;
			cusTempStorageDec.STH_DeclarationType = "TEST";
			var line1 = Factory.New<CusTempStorageLine>();
			line1.TSL_LineNo = 1;
			line1.TSL_STH = cusTempStorageDec.PK;

			var line2 = Factory.New<CusTempStorageLine>();
			line2.TSL_LineNo = 1;
			line2.TSL_STH = cusTempStorageDec.PK;

			var line3 = Factory.New<CusTempStorageLine>();
			line3.TSL_STH = cusTempStorageDec.PK;
			line3.TSL_LineNo = 1;

			var divot = Factory.New<CusTempStorageLinePivot>();
			divot.SLR_TSL_FromLine = line1.PK;
			divot.SLR_TSL_ToLine = line3.PK;

			Factory.Save();

			var collection = new DecCusTempStorageLineCollectionFrom<CusTempStorageLine, CusTempStorageDec>(cusTempStorageDec);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(new[] { line1.PK, line2.PK }, collection.Select(x => x.PK));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var cusTempStorageDec = Factory.New<CusTempStorageDec>();
			return new DecCusTempStorageLineCollectionFrom<CusTempStorageLine, CusTempStorageDec>(cusTempStorageDec);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusTempStorageLine>();
	}
}
