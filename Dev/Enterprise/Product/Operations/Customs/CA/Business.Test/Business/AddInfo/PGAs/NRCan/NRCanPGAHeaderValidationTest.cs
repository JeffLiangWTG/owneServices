using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class NRCanPGAHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDangerousGoodsDGSubs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_NRCanInd = "Y";
			var header = invoiceLine.NRCanPGAHeader;
			AssertNoMessageErrorContaining(header.DangerousGoodsDGSubsInfo, MandatoryValidation.YouHaveNotEntered);
			header.CA_EXPProgramInd = "Y";
			AssertHasMessageErrorContaining(header.DangerousGoodsDGSubsInfo, MandatoryValidation.YouHaveNotEntered);

			var expectedMessage = "This UNDG code is not valid for Natural Resources Canada lines.";
			header.DangerousGoodsDGSubs = UNDGSubstanceLoader.LoadSubstances(Factory, "1442", "", "IMO").First().PK;
			AssertNoMessageErrorContaining(header.DangerousGoodsDGSubsInfo, expectedMessage);

			header.DangerousGoodsDGSubs = UNDGSubstanceLoader.LoadSubstances(Factory, "0007", "", "IMO").First().PK;
			AssertNoMessageErrorContaining(header.DangerousGoodsDGSubsInfo, expectedMessage);

			header.DangerousGoodsDGSubs = UNDGSubstanceLoader.LoadSubstances(Factory, "0171", "", "IMO").First().PK;
			AssertNoMessageErrorContaining(header.DangerousGoodsDGSubsInfo, expectedMessage);

			header.DangerousGoodsDGSubs = UNDGSubstanceLoader.LoadSubstances(Factory, "1006", "", "IMO").First().PK;
			AssertHasMessageErrorContaining(header.DangerousGoodsDGSubsInfo, expectedMessage);

			header.DangerousGoodsDGSubs = ZGuid.Invalid;
			AssertHasMessageErrorContaining(header.DangerousGoodsDGSubsInfo, expectedMessage);
			AssertHasErrorContaining(header.DangerousGoodsDGSubsInfo, ListValidation.InvalidCodeError);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.CA_NRCanInd = YesNoList.Codes.Yes;
		}

		#endregion
	}
}
