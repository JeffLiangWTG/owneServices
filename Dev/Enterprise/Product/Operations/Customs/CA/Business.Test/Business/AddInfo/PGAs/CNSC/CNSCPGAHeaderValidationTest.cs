using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CNSCPGAHeaderValidationTest : TestCaseWithFactory
	{
		public void TestCheckDangerousGoodsDGSubs()
		{
			var undg1 = Factory.New<UNDGSubstance>();
			undg1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.RID;
			undg1.DG_Code = "9999";

			var undg2 = Factory.New<UNDGSubstance>();
			undg2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undg2.DG_Code = "8888";

			var undg3 = Factory.New<UNDGSubstance>();
			undg3.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undg3.DG_Code = new CNSCApplicableUNDGs().GetAllCodes()[0];

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CNSCInd = "Y";
			var cnsc = invoiceLine.CNSCPGAHeader;
			cnsc.CA_Category = CNSCCategories.Codes.RD;
			AssertHasMessageErrorContaining(cnsc.DangerousGoodsDGSubsInfo, MandatoryValidation.YouHaveNotEntered);

			cnsc.CA_Category = CNSCCategories.Codes.NS;
			AssertHasMessageErrorContaining(cnsc.DangerousGoodsDGSubsInfo, MandatoryValidation.YouHaveNotEntered);

			var expectedMessage = "This UNDG code is not valid for Canadian Nuclear Safety Commission lines.";

			cnsc.DangerousGoodsDGSubs = undg1.PK;
			AssertHasMessageErrorContaining(cnsc.DangerousGoodsDGSubsInfo, expectedMessage);

			cnsc.DangerousGoodsDGSubs = undg2.PK;
			AssertHasMessageErrorContaining(cnsc.DangerousGoodsDGSubsInfo, expectedMessage);

			cnsc.DangerousGoodsDGSubs = undg3.PK;
			AssertNoMessageErrorContaining(cnsc.DangerousGoodsDGSubsInfo, expectedMessage);

			cnsc.DangerousGoodsDGSubs = ZGuid.Invalid;
			AssertHasMessageErrorContaining(cnsc.DangerousGoodsDGSubsInfo, expectedMessage);
			AssertHasErrorContaining(cnsc.DangerousGoodsDGSubsInfo, ListValidation.InvalidCodeError);
		}
	}
}
