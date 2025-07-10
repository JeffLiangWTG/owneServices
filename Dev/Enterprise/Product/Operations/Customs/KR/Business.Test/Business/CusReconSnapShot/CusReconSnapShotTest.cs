using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusReconSnapshot))]
	sealed class CusReconSnapShotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var reconEntry = factory.New<CusReconEntry>();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			reconEntry.CRE_EntryDate = ZDate.Today;
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_OA_DeclarantAddress = factory.NewWithValidTestData<OrgAddress>().PK;
			var reconEntryLine = reconEntry.CusReconEntryLines.AddNew();
			reconEntryLine.CRL_OriginalEntryLineNumber = 1;
			var reconEntrySnapshot = factory.New<CusReconSnapshot>();
			reconEntrySnapshot.CRS_CRL_Line = reconEntryLine.PK;

			return reconEntrySnapshot;
		}
	}
}
