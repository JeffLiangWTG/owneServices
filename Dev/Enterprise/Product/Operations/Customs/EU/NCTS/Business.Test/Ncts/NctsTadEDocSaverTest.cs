using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsTadEDocSaverTest : TestCaseWithFactory
	{
		public void TestRenderTadAndStoreInEdocsWithoutOptions()
		{
			SetupFor(CusInBondApplicationCodeList.Codes.NCTS4);
			new NctsTadEdocSaver(nctsHeader).RenderTadAndStoreInEdocs(nctsHeader);
			AssertPrintedDocumentIsInEnglishPhase4();
		}

		public void TestRenderTadAndStoreInEdocsWithEmptyLanguageOption()
		{
			SetupFor(CusInBondApplicationCodeList.Codes.NCTS4);
			new NctsTadEdocSaver(nctsHeader, new NctsTadEdocSaverOptions()).RenderTadAndStoreInEdocs(nctsHeader);
			AssertPrintedDocumentIsInEnglishPhase4();
		}

		public void TestRenderTadAndStoreInEdocsWithoutOptions_Phase5()
		{
			SetupFor(CusInBondApplicationCodeList.Codes.NCTS5);
			new NctsTadEdocSaver(nctsHeader).RenderTadAndStoreInEdocs(nctsHeader);
			AssertPrintedDocumentIsInEnglishPhase5();
		}

		public void TestRenderTadAndStoreInEdocsWithEmptyLanguageOption_Phase5()
		{
			SetupFor(CusInBondApplicationCodeList.Codes.NCTS5);
			new NctsTadEdocSaver(nctsHeader, new NctsTadEdocSaverOptions()).RenderTadAndStoreInEdocs(nctsHeader);
			AssertPrintedDocumentIsInEnglishPhase5();
		}

		#region Implementation

		void SetupFor(string applicationCode)
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = applicationCode;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			Factory.Save();
		}

		NctsHeader nctsHeader;

		void AssertPrintedDocumentIsInEnglishPhase4()
		{
			AssertPrintedDocumentContains("EUROPEAN COMMUNITY");
		}

		void AssertPrintedDocumentIsInEnglishPhase5()
		{
			AssertPrintedDocumentContains("EUROPEAN UNION");
		}

		void AssertPrintedDocumentContains(string containedString)
		{
			var queuedPrintJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, nctsHeader.PK));
			AssertEquals("There should be a TAD/TSAD in the transit declaration print job queue.", 1, queuedPrintJobs.Length);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(queuedPrintJobs[0].SP_CustomProperties);
				AssertContains("Contains labels in english", containedString, excelInterface.WorkSheets[0].ToString().ToUpper());
			}
		}

		#endregion
	}
}
