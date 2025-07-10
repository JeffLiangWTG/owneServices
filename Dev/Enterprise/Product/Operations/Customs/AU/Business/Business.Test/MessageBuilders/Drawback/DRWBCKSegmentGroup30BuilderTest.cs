using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Messages.CUSDEC;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DRWBCKSegmentGroup30BuilderTest : TestCaseWithFactory
	{
		public void TestDRWBCKSegmentGroup30Builder()
		{
			var factory = new BusinessObjectFactory();
			var jobDec = factory.New<JobDeclaration>();
			var jobComInvHeader = jobDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var jobComInvLine = jobComInvHeader.JobComInvoiceLines.AddNew();
			jobComInvLine.JI_Description = "LINE DESCRIPTION";
			jobComInvLine.JI_Tariff = "1111.11.11 11";
			jobComInvLine.JI_CustomsUnitQty = "NO";
			jobComInvLine.JI_CustomsQuantity = 123m;
			jobComInvLine.AddInfo.ZA_DAM_Hidden = "A";
			jobComInvLine.AddInfo.ZA_DCV_Hidden = 12000m;
			jobComInvLine.AddInfo.ZA_DARC_Hidden = "C";
			jobComInvLine.AddInfo.ZA_DDN_Hidden = "IMPORTDEC";
			jobComInvLine.AddInfo.ZA_DDL_Hidden = 3;
			jobComInvLine.AddInfo.ZA_EDN_Hidden = "EDNNUM";
			jobComInvLine.AddInfo.ZA_DTR_Hidden = 7.5m;
			jobComInvLine.AddInfo.ZA_DDT_Hidden = 3543.21m;
			var group30 = new SegmentGroup30();
			var builder = new DRWBCKSegmentGroup30Builder(jobComInvLine, group30, 0);
			builder.PopulateSegment();
			var expectedString = "CST+1+I::95'FTX+AAA+++LINE DESCRIPTION'FTX+ABA++C::95'MEA+AAA++NO:123'MOA+40:12000.00'RFF+ABD:11111111'RFF+ABT:IMPORTDEC:3'RFF+AEH:A'RFF+ED:EDNNUM'TAX+1+CUD+++:::7.50'MOA+55:3543.21'";
			AssertEquals("SegmentValue", expectedString, group30.ToString(new UNOCCMRCharacterSet()));
		}
	}
}
