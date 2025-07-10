using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	class HCPGAHeaderValidationTest : TestCaseWithFactory
	{
		public void TestCheckDangerousGoodsDGSubs()
		{
			var undg = Factory.New<UNDGSubstance>();
			undg.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undg.DG_Code = "9999";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_HCInd = "Y";
			var hc = invoiceLine.HCPGAHeader;
			hc.CA_PESProgramInd = "Y";
			hc.DangerousGoodsDGSubs = ZGuid.Invalid;
			AssertHasErrorContaining(hc.DangerousGoodsDGSubsInfo, ListValidation.InvalidCodeError);

			hc.DangerousGoodsDGSubs = undg.PK;
			AssertNoErrorContaining(hc.DangerousGoodsDGSubsInfo, ListValidation.InvalidCodeError);
		}
	}
}
