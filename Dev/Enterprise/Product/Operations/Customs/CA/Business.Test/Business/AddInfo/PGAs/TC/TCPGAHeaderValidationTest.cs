using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TCPGAHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckManufacturerLetterAttached()
		{
			var messageError = "Either Statement of Compliance Label or Manufacturer Letter of Compliance Label should be provided.";
			header.CA_VPRProgramInd = YesNoList.Codes.Yes;
			header.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VCR;
			header.Validation.ValidateManufacturerLetterAttached();
			AssertHasMessageError(header.ManufacturerLetterAttachedInfo, messageError);

			header.ManufacturerLetterAttached = true;
			AssertNoMessageErrors(header.ManufacturerLetterAttachedInfo);

			header.ManufacturerLetterAttached = false;
			AssertHasMessageError(header.ManufacturerLetterAttachedInfo, messageError);

			header.StatementLabelAttached = true;
			AssertNoMessageErrors(header.ManufacturerLetterAttachedInfo);
		}

		public void TestCheckStatementLabelAttached()
		{
			header.CA_VPRProgramInd = YesNoList.Codes.Yes;
			header.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.PIL;
			header.Validation.ValidateStatementLabelAttached();
			AssertHasMessageError(header.StatementLabelAttachedInfo, "Statement of Compliance Label should be provided.");

			header.StatementLabelAttached = true;
			AssertNoMessageErrors(header.StatementLabelAttachedInfo);

			header.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VCC;
			header.StatementLabelAttached = false;
			AssertHasMessageError(header.StatementLabelAttachedInfo, "Either Statement of Compliance Label or Manufacturer Letter of Compliance Label should be provided.");

			header.ManufacturerLetterAttached = true;
			AssertNoMessageErrors(header.StatementLabelAttachedInfo);
		}

		public void TestCheckIsUSImporterDeclared()
		{
			var message = "Either Importer Declaration for All Countries or Importer Declaration for US should be provided.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = "Y";
			var pgaHeader = invoiceLine.TCPGAHeader;

			pgaHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_ImportReasonCode = TCIntendedUseCodes.Codes.TC01;
			pgaHeader.CA_ProductType = TCProductCategories.Codes.TC04;
			pgaHeader.CA_ProductClass = TCProductCategories.Codes.TC01;
			invoiceLine.JI_Tariff = "4011100011";
			invoiceLine.JI_CountryOfOrigin = "US";

			pgaHeader.Validation.ValidateIsUSImporterDeclared();
			AssertHasMessageErrorContaining(pgaHeader.IsUSImporterDeclaredInfo, message);

			pgaHeader.IsUSImporterDeclared = true;
			AssertNoMessageErrorContaining(pgaHeader.IsUSImporterDeclaredInfo, message);

			pgaHeader.IsZZImporterDeclared = true;
			AssertNoMessageErrorContaining(pgaHeader.IsUSImporterDeclaredInfo, message);
		}

		public void TestCheckIsZZImporterDeclared()
		{
			var message = "Importer Declaration should be provided.";
			var message2 = "Either Importer Declaration for All Countries or Importer Declaration for US should be provided.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = "Y";
			var pgaHeader = invoiceLine.TCPGAHeader;

			pgaHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_ImportReasonCode = TCIntendedUseCodes.Codes.TC01;
			pgaHeader.CA_ProductType = TCProductCategories.Codes.TC04;
			pgaHeader.CA_ProductClass = TCProductCategories.Codes.TC01;
			invoiceLine.JI_Tariff = "4011100011";
			invoiceLine.JI_CountryOfOrigin = "US";

			pgaHeader.Validation.ValidateIsZZImporterDeclared();
			AssertNoMessageErrorContaining(pgaHeader.IsZZImporterDeclaredInfo, message);
			AssertHasMessageErrorContaining(pgaHeader.IsZZImporterDeclaredInfo, message2);

			invoiceLine.JI_CountryOfOrigin = "CN";
			pgaHeader.Validation.ValidateIsZZImporterDeclared();
			AssertHasMessageErrorContaining(pgaHeader.IsZZImporterDeclaredInfo, message);
			AssertNoMessageErrorContaining(pgaHeader.IsZZImporterDeclaredInfo, message2);

			pgaHeader.IsZZImporterDeclared = true;
			AssertNoMessageErrorContaining(pgaHeader.IsZZImporterDeclaredInfo, message);
			AssertNoMessageErrorContaining(pgaHeader.IsZZImporterDeclaredInfo, message2);
		}

		public void TestCheckIsVPRImporterDeclared()
		{
			var message = "Importer Statement should be provided.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = "Y";
			var pgaHeader = invoiceLine.TCPGAHeader;

			pgaHeader.CA_VPRProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VFS;
			pgaHeader.Validation.ValidateIsVPRImporterDeclared();
			AssertNoMessageErrorContaining(pgaHeader.IsVPRImporterDeclaredInfo, message);

			pgaHeader.IsVPRImporterDeclared = false;
			AssertHasMessageErrorContaining(pgaHeader.IsVPRImporterDeclaredInfo, message);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = YesNoList.Codes.Yes;

			header = invoiceLine.TCPGAHeader;
		}
		TCPGAHeader header;

		#endregion
	}
}
