using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusReconCustomsCharge))]
	sealed class CusReconCustomsChargeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var reconEntry = factory.New<CusReconEntry>();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			reconEntry.CRE_EntryDate = ZDate.Today;
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_OA_DeclarantAddress = factory.NewWithValidTestData<OrgAddress>().PK;
			var reconEntryLine = factory.New<CusReconEntryLine>();
			reconEntryLine.CRL_LineNumber = 1;
			reconEntryLine.CRL_CustomsStatus = "AA";
			reconEntryLine.CRL_Description = "AA";
			reconEntryLine.CRL_OriginalEntryLineNumber = 1;
			reconEntryLine.CRL_CRE = reconEntry.PK;
			var cusReconCustomsCharge = factory.New<CusReconCustomsCharge>();
			cusReconCustomsCharge.CRC_CRL_Line = reconEntryLine.PK;
			cusReconCustomsCharge.CRC_Amount = 1;
			cusReconCustomsCharge.CRC_ChargeType = "AA";
			return cusReconCustomsCharge;
		}
	}
}
