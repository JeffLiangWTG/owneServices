using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.MX.Business.Testing
{
	class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJI_CEI()
		{
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Description = "ENT1";
			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_CEI = instruction1.PK;
			AssertNoMessageErrorContaining(invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.MakeNonPersistent();
			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_RN_NKCountryOfExport()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_RN_NKCountryOfExportInfo);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.JI_RN_NKCountryOfExport = ZString.Empty;
			AssertNoMessageError(invoiceLine.JI_RN_NKCountryOfExportInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckObservations()
		{
			AssertNoNotifications(invoiceLine.ObservationsInfo);
			invoiceLine.Observations = "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
			AssertNoNotifications(invoiceLine.ObservationsInfo);
			invoiceLine.Observations = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			AssertHasMessageError(invoiceLine.ObservationsInfo, "Each observation cannot have more than 120 characters.");
			invoiceLine.Observations = "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789\r\n0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			AssertHasMessageError(invoiceLine.ObservationsInfo, "Each observation cannot have more than 120 characters.");
			invoiceLine.Observations = "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789\r\n012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
			AssertNoNotifications(invoiceLine.ObservationsInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
		}

		protected JobDeclaration declaration;
		protected JobComInvoiceLine invoiceLine;
	}
}
