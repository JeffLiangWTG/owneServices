using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.EdocSaver.Testing
{
	sealed class AdviceOnPlacementInIstEDocsSaverTest : TestCaseWithFactory
	{
		public void TestRenderedAndSavedToEDocs()
		{
			var header = CusTempStorageJobHeader.New(Factory);
			header.SJH_JobReference = "FRJ000020";
			header.DDTNumber = "DDT123456";
			header.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			SetUpCustomsOffice();
			header.SJH_CustomsOffice = "FR001";
			var storageDec = header.CusTempStorageDec;
			storageDec.STH_OwnerReferenceNumber = "TST1";
			AssertEquals(ZDate.Empty, header.SJH_TempStorageEndDateUtc.Date);

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, header.PK));
			AssertEquals("There should not be any print job in the queue yet.", 0, printJobs.Length);

			new AdviceOnPlacementInIstEDocsSaver(header).RenderDocumentAndSaveInEDocs();
			Factory.Save();
			printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, header.PK));
			AssertEquals("A new print job should have been added to the queue.", 1, printJobs.Length);
		}

		void SetUpCustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "CustomsOffice");
			var codeList = helper.CreateCusCodeList("FR", "CUSOF", "FR001", "FR001 Customs", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("EmailAddress", "EmailAddress", "CUSOF", "FR");
			codeList.Attributes.AddNew("EmailAddress", "Check.Yao@wisetechglobal.com");

			var codeListWithoutEmail = helper.CreateCusCodeList("FR", "CUSOF", "FR002", "FR002 Customs", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}
	}
}
