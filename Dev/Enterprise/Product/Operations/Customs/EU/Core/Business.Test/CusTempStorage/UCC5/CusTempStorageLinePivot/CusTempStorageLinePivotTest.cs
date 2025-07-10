using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageLinePivot))]
	public class CusTempStorageLinePivotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
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

			var toJob = factory.New<CusTempStorageJobHeader>();
			toJob.SJH_GB = GlbBranch.CurrentBranch.PK;
			toJob.SJH_JobReference = "To1";
			toJob.SJH_OH_Customer = customer.PK;
			toJob.SJH_OA_Presenter = presenter.PK;
			toJob.SJH_OA_Representative = representative.PK;

			var toDec = fromJob.CusTempStorageDecs.AddNew();
			toDec.STH_SJH = toJob.PK;
			toDec.STH_DeclarationType = "A";

			var toLine = fromDec.CusTempStorageLines.AddNew();
			toLine.TSL_LineNo = 1;
			toLine.TSL_ReferenceNumberLine = 1;
			toLine.TSL_STH = toDec.PK;

			var divot = factory.New<CusTempStorageLinePivot>();
			divot.SLR_TSL_FromLine = fromLine.PK;
			divot.SLR_TSL_ToLine = toLine.PK;

			return divot;
		}

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
			divot.Delete();

			Factory.Save();
			Assert(line2.IsDeleted);
		}

		public void TestDelete_ShouldDeleteReleatedToLinesWhenDeletingCore()
		{
			var cusTempStorageDec = Factory.NewWithValidTestData<CusTempStorageDec>();
			var line1 = Factory.New<CusTempStorageLine>();
			line1.TSL_LineNo = 1;
			line1.TSL_STH = cusTempStorageDec.PK;

			var line2 = Factory.New<CusTempStorageLineForTesting>();
			line2.TSL_LineNo = 1;
			line2.TSL_STH = cusTempStorageDec.PK;

			var divot = Factory.New<CusTempStorageLinePivot>();
			divot.SLR_TSL_FromLine = line1.PK;
			divot.SLR_TSL_ToLine = line2.PK;

			divot.Delete();

			Assert("To line would not be deleted after deleting divot", !line2.IsDeleted);
		}

		class CusTempStorageLineForTesting : CusTempStorageLine
		{
			public CusTempStorageLineForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool ShouldDeleteReleatedToLinesWhenDeletingCore() => false;
		}
	}
}
