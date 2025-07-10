using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(GACPGAHeaderAddInfo))]
	sealed class GACPGAHeaderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = "Y";
			var header = invoiceLine.GACPGAHeader;
			return new GACPGAHeaderAddInfo(header.B7_AddInfoDataInfo);
		}

		public void TestRunPreSaveValidationCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.CA_PermitApplication = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;

			var gac = invoiceLine.GACPGAHeader;
			gac.CA_AllProgramInd = YesNoList.Codes.Yes;
			gac.RunPreSaveValidation();
			AssertHasMessageErrorContaining(gac.CA_CommodityCodeInfo, GACPGAHeaderAddInfoValidation.CommodityCodeMustBe14Digit);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			declaration2.CA_PermitApplication = true;
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_GACInd = YesNoList.Codes.Yes;

			var gac2 = invoiceLine2.GACPGAHeader;
			gac2.CA_AllProgramInd = YesNoList.Codes.Yes;
			gac2.RunPreSaveValidation();
			AssertNoMessageErrorContaining(gac2.CA_CommodityCodeInfo, GACPGAHeaderAddInfoValidation.CommodityCodeMustBe14Digit);
		}
	}
}
