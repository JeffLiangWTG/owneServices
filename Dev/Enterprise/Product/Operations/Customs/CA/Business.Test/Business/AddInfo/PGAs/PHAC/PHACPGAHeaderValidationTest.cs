using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PHACPGAHeaderValidationTest : BusinessObjectValidationTestCase
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
			undg3.DG_Code = new PHACApplicableUNDGs().GetAllCodes()[0];

			var expectedMessage = "This UNDG code is not valid for Public Health Agency of Canada lines.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_PHACInd = YesNoList.Codes.Yes;
			var phac = invoiceLine.PHACPGAHeader;

			phac.DangerousGoodsDGSubs = undg1.PK;

			AssertHasMessageErrorContaining(phac.DangerousGoodsDGSubsInfo, expectedMessage);
			phac.DangerousGoodsDGSubs = undg2.PK;
			AssertHasMessageErrorContaining(phac.DangerousGoodsDGSubsInfo, expectedMessage);

			phac.DangerousGoodsDGSubs = undg3.PK;
			AssertNoMessageErrorContaining(phac.DangerousGoodsDGSubsInfo, expectedMessage);

			phac.DangerousGoodsDGSubs = ZGuid.Invalid;
			AssertHasMessageErrorContaining(phac.DangerousGoodsDGSubsInfo, expectedMessage);
			AssertHasErrorContaining(phac.DangerousGoodsDGSubsInfo, ListValidation.InvalidCodeError);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_PHACInd = YesNoList.Codes.Yes;
		}

		#endregion
	}
}
