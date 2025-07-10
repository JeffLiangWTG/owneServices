using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class TaxRegimeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var ipiTaxRegime = invoiceLine.IPITaxRegimeSupportingInfo;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(ipiTaxRegime.CSI_CodeInfo, "X", "5");

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;

			var pisCofinsTaxRegime = invoiceLine.PisCofinsTaxRegimeSupportingInfo;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(pisCofinsTaxRegime.CSI_CodeInfo, "X", "2");

			var dutyTaxRegime = invoiceLine.DutyTaxRegimeSupportingInfo;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(dutyTaxRegime.CSI_CodeInfo, "X", "2");

			var icmsTaxRegime = invoiceLine.ICMSTaxRegimeSupportingInfo;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(icmsTaxRegime.CSI_CodeInfo, "X", "1");

			var fmmTaxRegime = invoiceLine.FMMTaxRegimeSupportingInfo;
			fmmTaxRegime.CSI_Code = "X";
			ValidationTestHelper.AssertFieldIsNotMandatory(fmmTaxRegime.CSI_CodeInfo);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			fmmTaxRegime.CSI_Code = ZString.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(fmmTaxRegime.CSI_CodeInfo);
			ValidationTestHelper.AssertInvalidCodeMessageError(fmmTaxRegime.CSI_CodeInfo, "X", FMMBenefitTypeList.Codes.Exemption);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			dutyTaxRegime = invoiceLine.DutyTaxRegimeSupportingInfo;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(dutyTaxRegime.CSI_CodeInfo, "X", "2");

			icmsTaxRegime = invoiceLine.ICMSTaxRegimeSupportingInfo;
			icmsTaxRegime.CSI_Code = ZString.Empty;
			AssertNoNotifications(icmsTaxRegime.CSI_CodeInfo);

			pisCofinsTaxRegime = invoiceLine.PisCofinsTaxRegimeSupportingInfo;
			pisCofinsTaxRegime.CSI_Code = ZString.Empty;
			AssertNoNotifications(pisCofinsTaxRegime.CSI_CodeInfo);

			ipiTaxRegime = invoiceLine.IPITaxRegimeSupportingInfo;
			ipiTaxRegime.CSI_Code = ZString.Empty;
			AssertNoNotifications(ipiTaxRegime.CSI_CodeInfo);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			icmsTaxRegime = invoiceLine.ICMSTaxRegimeSupportingInfo;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(icmsTaxRegime.CSI_CodeInfo, "X", "2");

			dutyTaxRegime.CSI_Code = ZString.Empty;
			AssertNoNotifications(ipiTaxRegime.CSI_CodeInfo);
			AssertNoNotifications(pisCofinsTaxRegime.CSI_CodeInfo);
			AssertNoNotifications(dutyTaxRegime.CSI_CodeInfo);

			declaration.MakeNonPersistent();

			declaration.Invoices[0].JZ_MessageType = BRJobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertFieldIsNotMandatory(fmmTaxRegime.CSI_CodeInfo);
		}

		public void TestCheckCSI_Procedure()
		{
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			var taxRegime = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().PisCofinsTaxRegimeSupportingInfo;

			ReferenceTestDataHelper.CreateReferenceDataForPISLegalBaseList(Factory, taxRegime.CSI_Code);

			ValidationTestHelper.AssertInvalidCodeMessageError(taxRegime.CSI_ProcedureInfo, "XX", "01");
		}

		public void TestCheckCSI_Procedure_MandatoryCheck()
		{
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			AssertCSI_ProcedureIsMandatory(invoiceLine.DutyTaxRegimeSupportingInfo);
			AssertCSI_ProcedureIsMandatory(invoiceLine.PisCofinsTaxRegimeSupportingInfo);

			void AssertCSI_ProcedureIsMandatory(TaxRegime taxRegime)
			{
				taxRegime.CSI_Code = ZString.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(taxRegime.CSI_ProcedureInfo, "You have not entered a Legal Base.");

				taxRegime.CSI_Code = TaxRegimeList.Codes.FullCollection;
				ValidationTestHelper.AssertFieldIsNotMandatory(taxRegime.CSI_ProcedureInfo, "You have not entered a Legal Base.");

				taxRegime.CSI_Code = TaxRegimeList.Codes.PaymentMade;
				ValidationTestHelper.AssertFieldIsNotMandatory(taxRegime.CSI_ProcedureInfo, "You have not entered a Legal Base.");

				taxRegime.CSI_Code = TaxRegimeList.Codes.Immunity;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(taxRegime.CSI_ProcedureInfo, "You have not entered a Legal Base.");
			}
		}

		JobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			ReferenceTestDataHelper.CreateReferenceDataForIPITaxRegimeList(Factory);
			ReferenceTestDataHelper.CreateReferenceDataForTaxRegimeList(Factory, BRJobMessageTypeList.Codes.ImportSiscomex);
			ReferenceTestDataHelper.CreateReferenceDataForICMSTaxRegimeList(Factory);
		}
	}
}
