using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageLine))]
	public class CusTempStorageLineTest : CusTempStorageLineTestCase<CusTempStorageLine>
	{
		public void TestDelete()
		{
			var cusTempStorageDec = Factory.NewWithValidTestData<CusTempStorageDec>();
			var line1 = Factory.New<CusTempStorageLine>();
			line1.TSL_LineNo = 1;
			line1.TSL_STH = cusTempStorageDec.PK;

			var line2 = Factory.New<CusTempStorageLine>();
			line2.TSL_LineNo = 1;
			line2.TSL_STH = cusTempStorageDec.PK;

			var divot = Factory.New<CusTempStorageLinePivot>();
			divot.SLR_TSL_FromLine = line1.PK;
			divot.SLR_TSL_ToLine = line2.PK;

			Factory.Save();

			AssertEquals(1, Factory.GetDatabaseCount(typeof(CusTempStorageLinePivot)));
			line1.Delete();

			Factory.Save();

			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusTempStorageLinePivot)));
		}

		public void TestShouldDeleteRelatedToLinesWhenDeleting()
		{
			var cusTempStorageDec = Factory.NewWithValidTestData<CusTempStorageDec>();
			var line = cusTempStorageDec.CusTempStorageLines.AddNew();

			Assert(line.ShouldDeleteReleatedToLinesWhenDeleting);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewCusTempStorageLine();

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewCusTempStorageLine();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewCusTempStorageLine(factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewCusTempStorageLine();

		protected override CusTempStorageLine GetNewCusTempStorageLine(BusinessObjectFactory factory)
		{
			var customer = factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "C1";
			var presenter = factory.NewWithValidTestData<OrgAddress>();
			var representative = factory.NewWithValidTestData<OrgAddress>();

			var fromJob = factory.New<CusTempStorageJobHeader>();
			fromJob.SJH_GB = GlbBranch.CurrentBranch.PK;
			fromJob.SJH_JobReference = "From1";
			fromJob.SJH_OH_Customer = customer.PK;
			fromJob.SJH_OA_Presenter = presenter.PK;
			fromJob.SJH_OA_Representative = representative.PK;

			var fromDec = fromJob.CusTempStorageDecs.AddNew();
			fromDec.STH_SJH = fromJob.PK;
			fromDec.STH_DeclarationType = "A";

			var fromLine = fromDec.CusTempStorageLines.AddNew();
			fromLine.TSL_STH = fromDec.PK;
			fromLine.TSL_LineNo = 1;
			fromLine.TSL_ReferenceNumberLine = 1;
			fromLine.TSL_ReferenceNumberType = "T1";

			return fromLine;
		}
	}
}
