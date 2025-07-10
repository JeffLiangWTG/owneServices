using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class QuarantineJobComInvoiceLineValidationTest : BaseJobComInvoiceLineValidationTest
	{
		protected override JobComInvoiceLineValidation GetNewValidationProvider(JobComInvoiceLine invoiceLine)
		{
			return new QuarantineJobComInvoiceLineValidation(invoiceLine);
		}

		public void TestCheckJI_Tariff()
		{
			Assert("Pre-condition, no message errors", !testInvoiceLine.JI_TariffInfo.HasMessageErrors());
			testInvoiceLine.InvoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = false;
			testInvoiceLine.Validation.ValidateJI_Tariff();
			Assert("Tariff has no message errors", !testInvoiceLine.JI_TariffInfo.HasMessageErrors());
			testInvoiceLine.InvoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
			testInvoiceLine.Validation.ValidateJI_Tariff();
			Assert("Tariff has message errors as customs flag is true", testInvoiceLine.JI_TariffInfo.HasMessageErrors());
		}

		public void TestCheckJI_WeightForNEXDOCGrossWeight()
		{
			string messageError = "You have not entered a Gross Metric Weight.";
			testInvoiceLine.JI_Weight = 0m;
			testInvoiceLine.QuarantineExDocLine.Validation.ValidateQL_GrossMetricWeight();
			testInvoiceLine.Validation.ValidateJI_Weight();
			AssertEquals(false, testInvoiceLine.QuarantineExDocLine.QuarantineExDocHeader.IsNEXDOCSActive);
			AssertNoMessageErrorContaining(testInvoiceLine.JI_WeightInfo, messageError);
			testInvoiceLine.JI_Weight = 2m;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				testInvoiceLine.QuarantineExDocLine.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
				testInvoiceLine.JI_Weight = 0m;
				testInvoiceLine.Validation.ValidateJI_Weight();
				AssertEquals(0m, testInvoiceLine.QuarantineExDocLine.QL_GrossMetricWeight);
				AssertEquals(true, testInvoiceLine.QuarantineExDocLine.QuarantineExDocHeader.IsNEXDOCSActive);
				AssertHasMessageErrorContaining(testInvoiceLine.JI_WeightInfo, messageError);
			}
		}

		public void TestCheckJI_LinePrice()
		{
			const string messageError = "When Quarantine is to obtain a customs EDN a price is required.";
			testInvoiceLine.QuarantineExDocLine.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			testInvoiceLine.JI_LinePrice = 0.0m;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				testInvoiceLine.Validation.ValidateJI_LinePrice();
				AssertNoMessageError(testInvoiceLine.JI_LinePriceInfo, messageError);
				AssertHasMessageErrorContaining(testInvoiceLine.JI_LinePriceInfo, MandatoryValidation.YouHaveNotEntered);

				testInvoiceLine.InvoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
				testInvoiceLine.Validation.ValidateJI_LinePrice();
				AssertHasMessageError(testInvoiceLine.JI_LinePriceInfo, messageError);
				AssertNoMessageErrorContaining(testInvoiceLine.JI_LinePriceInfo, MandatoryValidation.YouHaveNotEntered);

				testInvoiceLine.Declaration.IsAQISCertificateRequest = true;
				testInvoiceLine.Validation.ValidateJI_LinePrice();
				AssertNoMessageError(testInvoiceLine.JI_LinePriceInfo, messageError);
				AssertNoMessageError(testInvoiceLine.JI_LinePriceInfo, MandatoryValidation.YouHaveNotEntered);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				testInvoiceLine.Declaration.IsAQISCertificateRequest = false;
				testInvoiceLine.InvoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = false;

				testInvoiceLine.Validation.ValidateJI_LinePrice();
				AssertNoMessageError(testInvoiceLine.JI_LinePriceInfo, messageError);
				AssertNoMessageErrorContaining(testInvoiceLine.JI_LinePriceInfo, MandatoryValidation.YouHaveNotEntered);

				testInvoiceLine.InvoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
				testInvoiceLine.Validation.ValidateJI_LinePrice();
				AssertHasMessageError(testInvoiceLine.JI_LinePriceInfo, messageError);
				AssertNoMessageErrorContaining(testInvoiceLine.JI_LinePriceInfo, MandatoryValidation.YouHaveNotEntered);

				testInvoiceLine.Declaration.IsAQISCertificateRequest = true;
				testInvoiceLine.Validation.ValidateJI_LinePrice();
				AssertNoMessageError(testInvoiceLine.JI_LinePriceInfo, messageError);
				AssertNoMessageError(testInvoiceLine.JI_LinePriceInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			jobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
		}

		#endregion
	}
}
