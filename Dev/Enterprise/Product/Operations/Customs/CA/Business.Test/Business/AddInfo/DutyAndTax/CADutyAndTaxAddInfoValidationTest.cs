using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CADutyAndTaxAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		#region TestCheckC1_TaxType

		public void TestShouldHasEXSForLuxuryWhenGSTExisted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.ShouldDeleteLuxuryTaxInvoiceLine += () => true;

			line.CA_ApplyLuxuryTax = true;
			AssertEquals(2, invoice.JobComInvoiceLines.Count);
			var luxuryTaxInvoiceLine = line.LuxuryTaxInvoiceLine;
			luxuryTaxInvoiceLine.DutiesAndTaxes.DeleteAll();
			var gst = luxuryTaxInvoiceLine.DutiesAndTaxes.AddNew();
			gst.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			AssertHasMessageError(gst.C1_TaxTypeInfo, "Please create an Excise Tax when GST exists.");
			var exs = luxuryTaxInvoiceLine.DutiesAndTaxes.AddNew();
			exs.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			gst.AddInfoValidation.ValidateC1_TaxType();
			AssertNoMessageError(gst.C1_TaxTypeInfo, "Please create an Excise Tax when GST exists.");
		}

		public void TestCheckC1_TaxType()
		{
			var tax = Factory.New<DutyAndTax>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			tax.Parent = invoiceLine;

			AssertMoreThanNecessaryRatesMessageError(DutyAndTaxTypes.Codes.CustomsDuty, 3, invoiceLine);
			AssertMoreThanNecessaryRatesMessageError(DutyAndTaxTypes.Codes.ADD, 3, invoiceLine);
			AssertMoreThanNecessaryRatesMessageError(DutyAndTaxTypes.Codes.ExciseTax, 1, invoiceLine);
			AssertMoreThanNecessaryRatesMessageError(DutyAndTaxTypes.Codes.GST, 1, invoiceLine);
			AssertMoreThanNecessaryRatesMessageError(DutyAndTaxTypes.Codes.SIMADuty, 1, invoiceLine);

			tax.C1_TaxType = DutyAndTaxTypes.Codes.CTA;
			tax.C1_RateType = RateTypes.Codes.AcceptX;
			tax.AddInfoValidation.ValidateC1_TaxType();
			const string errorMessageForAcceptT = "No rate on file. Please calculate and enter provincial taxes manually, and add the mark-up dummy HS code line(s) manually.";
			AssertHasMessageError(tax.C1_TaxTypeInfo, errorMessageForAcceptT);

			tax.C1_TaxType = "~";
			AssertHasMessageErrorContaining(tax.C1_TaxTypeInfo, ListValidation.InvalidCodeMessageError);
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			AssertNoMessageErrorContaining(tax.C1_TaxTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoWarningContaining(tax.C1_TaxTypeInfo, "The 'SIM' group has been replaced by it’s individual components, please select the specific additional duties or taxes required.");
			tax.C1_TaxType = DutyAndTaxTypes.Codes.SIMADuty;
			AssertHasWarningContaining(tax.C1_TaxTypeInfo, "The 'SIM' group has been replaced by it’s individual components, please select the specific additional duties or taxes required.");
			invoiceLine.CA_IsSeeded = true;
			tax.AddInfoValidation.ValidateC1_TaxType();
			AssertNoWarningContaining(tax.C1_TaxTypeInfo, "The 'SIM' group has been replaced by it’s individual components, please select the specific additional duties or taxes required.");
		}

		void AssertMoreThanNecessaryRatesMessageError(string dutyOrTaxType, int count, JobComInvoiceLine invoiceLine)
		{
			invoiceLine.DutiesAndTaxes.DeleteAll();
			var codeInMessage = DutyAndTaxTypes.IsSIMATaxCode(dutyOrTaxType) ? DutyAndTaxTypes.Codes.SIMADuty : dutyOrTaxType;
			var errorMessage = string.Format("More than {0} {1} rates apply to this Classification Number", count, new DutyAndTaxTypes().GetDescriptionFromCode(codeInMessage));

			for (var i = 0; i < count; i++)
			{
				AssertNoMessageError(AddDutyOrTax(dutyOrTaxType, invoiceLine).C1_TaxTypeInfo, errorMessage);
			}

			AddDutyOrTax(dutyOrTaxType, invoiceLine);
			foreach (var dutyOrTax in invoiceLine.DutiesAndTaxes.Find(dutyOrTax => dutyOrTax.C1_TaxType == dutyOrTaxType))
			{
				AssertHasMessageError(dutyOrTax.C1_TaxTypeInfo, errorMessage);
			}

			invoiceLine.DutiesAndTaxes.Delete(invoiceLine.DutiesAndTaxes[0]);
			foreach (var dutyOrTax in invoiceLine.DutiesAndTaxes.Find(dutyOrTax => dutyOrTax.C1_TaxType == dutyOrTaxType))
			{
				AssertNoMessageError(dutyOrTax.C1_TaxTypeInfo, errorMessage);
			}
		}

		#endregion

		#region TestCheckC1_Amount

		public void TestCheckC1_Amount()
		{
			var tax = Factory.New<DutyAndTax>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			tax.Parent = invoiceLine;

			tax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			tax.C1_RateType = RateTypes.Codes.AcceptT;
			tax.AddInfoValidation.ValidateC1_Amount();
			const string errorMessageForAcceptT = "Excise Tax Rate Type T specified so you must override and enter the Rate and Amount manually";
			AssertHasMessageError(tax.C1_AmountInfo, errorMessageForAcceptT);

			tax.C1_RateType = RateTypes.Codes.AcceptX;
			tax.AddInfoValidation.ValidateC1_Amount();
			const string errorMessageForAcceptX = "Excise Tax Rate Type X specified so you must override and enter the Rate and Amount manually";
			AssertHasMessageError(tax.C1_AmountInfo, errorMessageForAcceptX);

			tax.C1_Override = true;
			tax.AddInfoValidation.ValidateC1_Amount();
			AssertNoWarning(tax.C1_AmountInfo, errorMessageForAcceptX);

			tax.C1_RateType = RateTypes.Codes.AcceptT;
			tax.AddInfoValidation.ValidateC1_Amount();
			AssertNoWarning(tax.C1_AmountInfo, errorMessageForAcceptT);

			var casualImportTax = invoiceLine.DutiesAndTaxes.AddNew();
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, invoiceLine.Declaration);
			casualImportTax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			casualImportTax.C1_ExemptCode = "22";
			casualImportTax.C1_Amount = 0;
			AssertHasMessageError(casualImportTax.C1_AmountInfo, "Amount is mandatory for SIMA Exempt Code 22");
			casualImportTax.C1_Amount = 1;
			AssertNoMessageError(casualImportTax.C1_AmountInfo, "Amount is mandatory for SIMA Exempt Code 22");
			casualImportTax.C1_ExemptCode = "20";
			casualImportTax.C1_Amount = 1;
			AssertHasMessageError(casualImportTax.C1_AmountInfo, "Amount must be zero for SIMA Code 20");
			casualImportTax.C1_Amount = 0;
			AssertNoMessageError(casualImportTax.C1_AmountInfo, "Amount must be zero for SIMA Code 20");

			casualImportTax.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			casualImportTax.C1_ExemptCode = "20";
			casualImportTax.C1_Amount = 1;
			AssertHasMessageError(casualImportTax.C1_AmountInfo, "Amount must be zero for SIMA Code 20");
			casualImportTax.C1_Amount = 0;
			AssertNoMessageError(casualImportTax.C1_AmountInfo, "Amount must be zero for SIMA Code 20");

			casualImportTax.C1_TaxType = DutyAndTaxTypes.Codes.SIMADuty;
			casualImportTax.C1_ExemptCode = "22";
			casualImportTax.C1_Amount = 0;
			AssertHasMessageError(casualImportTax.C1_AmountInfo, "Amount is mandatory for SIMA Exempt Code 22");
			casualImportTax.C1_Amount = 1;
			AssertNoMessageError(casualImportTax.C1_AmountInfo, "Amount is mandatory for SIMA Exempt Code 22");
			casualImportTax.C1_ExemptCode = "20";
			casualImportTax.C1_Amount = 1;
			AssertHasMessageError(casualImportTax.C1_AmountInfo, "Amount must be zero for SIMA Code 20");
			casualImportTax.C1_Amount = 0;
			AssertNoMessageError(casualImportTax.C1_AmountInfo, "Amount must be zero for SIMA Code 20");
		}

		public void TestCheckC1_AmountForDutyOfLuxuryLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.ShouldDeleteLuxuryTaxInvoiceLine += () => true;
			line.CA_ApplyLuxuryTax = true;
			var luxuryLine = line.LuxuryTaxInvoiceLine;
			var dutyOrExs = luxuryLine.DutiesAndTaxes.AddNew();
			dutyOrExs.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			dutyOrExs.C1_Override = false;
			dutyOrExs.C1_RateType = RateTypes.Codes.AcceptX;
			dutyOrExs.AddInfoValidation.ValidateC1_Amount();
			AssertNoMessageErrors(dutyOrExs.C1_AmountInfo);
			dutyOrExs.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dutyOrExs.AddInfoValidation.ValidateC1_Amount();
			AssertNoMessageErrors(dutyOrExs.C1_AmountInfo);
		}
		#endregion

		#region TestCheckC1_AmountWithExemptCode
		public void TestCheckC1_AmountWithExemptCode()
		{
			var tax = Factory.New<DutyAndTax>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			tax.Parent = invoiceLine;

			var casualImportTax1 = invoiceLine.DutiesAndTaxes.AddNew();
			var casualImportTax2 = invoiceLine.DutiesAndTaxes.AddNew();
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, invoiceLine.Declaration);
			casualImportTax1.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			casualImportTax1.C1_ExemptCode = "51";
			casualImportTax1.C1_Amount = 10;
			casualImportTax2.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			casualImportTax2.C1_ExemptCode = "51";
			casualImportTax2.C1_Amount = 0;
			var waringMessage = "No SIMA amount has been entered, please confirm this is correct prior to filing the entry";
			AssertHasWarning(casualImportTax2.C1_AmountInfo, waringMessage);

			casualImportTax1.C1_Amount = 0;
			AssertHasMessageError(casualImportTax1.C1_AmountInfo, "Amount is mandatory for SIMA Exempt Code 51");

			casualImportTax1.C1_Amount = 10;
			casualImportTax1.C1_ExemptCode = "52";
			casualImportTax2.C1_ExemptCode = "52";
			AssertHasWarning(casualImportTax2.C1_AmountInfo, waringMessage);

			casualImportTax1.C1_Amount = 0;
			AssertHasMessageError(casualImportTax1.C1_AmountInfo, "Amount is mandatory for SIMA Exempt Code 52");
		}
		#endregion

		#region TestCheckC1_Rate

		public void TestCheckC1_RateMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var tax = Factory.New<DutyAndTax>();
			tax.Parent = invoiceLine;
			tax.B7_ParentID = ZGuid.NewZGuid();
			tax.B7_ParentTableCode = invoiceLine.TablePrefix;
			tax.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			tax.C1_Amount = 1m;
			tax.C1_Rate = 2607988m;
			AssertNoErrors(tax.C1_RateInfo);
			tax.C1_Rate = 1914726.79000m;
			AssertNoErrors(tax.C1_RateInfo);
			tax.C1_Rate = 19147263333.33888m;
			var msg = "The number 19,147,263,333.33888 is too large, the maximum value allowed for Rate is 9,999,999,999.99999.";
			AssertHasError(tax.C1_RateInfo, msg);
		}

		#endregion

		#region TestCheckC1_ForeignRate

		public void TestCheckC1_ForeignRateMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var tax = Factory.New<DutyAndTax>();
			tax.Parent = invoiceLine;
			tax.B7_ParentID = ZGuid.NewZGuid();
			tax.B7_ParentTableCode = invoiceLine.TablePrefix;
			tax.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			tax.C1_Amount = 1m;
			tax.C1_Rate = 1.5m;
			tax.C1_ForeignRate = 2607988m;
			AssertNoErrors(tax.C1_ForeignRateInfo);
			tax.C1_ForeignRate = 1914726.79000m;
			AssertNoErrors(tax.C1_ForeignRateInfo);
			tax.C1_ForeignRate = 19147263333.33888m;
			var msg = "The number 19,147,263,333.33888 is too large, the maximum value allowed for Foreign Rate is 9,999,999,999.99999.";
			AssertHasError(tax.C1_ForeignRateInfo, msg);
		}

		#endregion

		#region TestCheckC1_Code

		public void TestCheckC1_CodeWhenEXSAmountZero()
		{
			var dutyAndTax = Factory.New<DutyAndTax>();
			var validation = dutyAndTax.AddInfoValidation;
			var messageError = "Self calculated Amount is required for this excise code. Refer to CN24-35 for details.";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(UniversalReferenceConstants.RefCusCodeListType.Codes.SLFCALCEXS, Core.Constants.CountryCodes.Canada, ZDateTime.Today, true))
			{
				validation.ValidateC1_Code();
				AssertNoMessageError(dutyAndTax.C1_CodeInfo, messageError);

				dutyAndTax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
				validation.ValidateC1_Code();
				AssertNoMessageError(dutyAndTax.C1_CodeInfo, messageError);

				dutyAndTax.C1_Code = "C01";
				validation.ValidateC1_Code();
				AssertHasMessageError(dutyAndTax.C1_CodeInfo, messageError);

				dutyAndTax.C1_Amount = 12m;
				validation.ValidateC1_Code();
				AssertNoMessageError(dutyAndTax.C1_CodeInfo, messageError);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(UniversalReferenceConstants.RefCusCodeListType.Codes.SLFCALCEXS, Core.Constants.CountryCodes.Canada, ZDateTime.Today, false))
			{
				dutyAndTax.C1_Amount = 0m;
				validation.ValidateC1_Code();
				AssertNoMessageError(dutyAndTax.C1_CodeInfo, messageError);
			}
		}

		public void TestCheckC1_Code()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var exsRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.ExciseTax);
			var exsRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, "C00", exsRateType.PK);
			var exsTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var exsTaxRate = universalHelper.CreateRate(exsTariff, exsRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "0.1 * VFD");
			Factory.Save();

			var tax = Factory.New<DutyAndTax>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			tax.Parent = invoiceLine;

			var errorMesssage = "Please select the appropriate Tax Code or select 'NO' if the tax does not apply";
			var surtaxMessageError = "Surtax Code required. The Surtax Code to be used can be found on the Customs Notice announcing the Surtax";
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			tax.AddInfoValidation.ValidateC1_Code();
			AssertHasMessageError(tax.C1_CodeInfo, errorMesssage);

			tax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			tax.AddInfoValidation.ValidateC1_Code();
			AssertNoMessageError(tax.C1_CodeInfo, errorMesssage);

			tax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			tax.AddInfoValidation.ValidateC1_Code();
			AssertHasMessageError(tax.C1_CodeInfo, errorMesssage);

			tax.C1_Override = true;
			tax.AddInfoValidation.ValidateC1_Code();
			AssertNoMessageError(tax.C1_CodeInfo, errorMesssage);

			tax.C1_Override = false;

			tax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			tax.C1_Code = "XXX";
			tax.AddInfoValidation.ValidateC1_Code();
			AssertNoMessageErrorContaining(tax.C1_CodeInfo, ListValidation.InvalidCodeMessageError);

			tax.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			tax.AddInfoValidation.ValidateC1_Code();
			AssertNoMessageErrorContaining(tax.C1_CodeInfo, ListValidation.InvalidCodeMessageError);

			tax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			tax.AddInfoValidation.ValidateC1_Code();
			AssertHasMessageErrorContaining(tax.C1_CodeInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_Tariff = "1234567890";
			tax.AddInfoValidation.ValidateC1_Code();
			AssertNoMessageError(tax.C1_CodeInfo, errorMesssage);

			tax.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			tax.C1_Code = ZString.Empty;
			tax.AddInfoValidation.ValidateC1_Code();
			AssertHasMessageError(tax.C1_CodeInfo, surtaxMessageError);

			tax.C1_Code = "ZZZ";
			tax.AddInfoValidation.ValidateC1_Code();
			AssertNoNotifications(tax.C1_CodeInfo);
			AssertNoMessageError(tax.C1_CodeInfo, surtaxMessageError);

			tax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			tax.C1_Code = ZString.Empty;
			tax.AddInfoValidation.ValidateC1_Code();
			AssertNoMessageError(tax.C1_CodeInfo, surtaxMessageError);

			tax.C1_Code = "ZZZ";
			tax.AddInfoValidation.ValidateC1_Code();
			AssertHasWarning(tax.C1_CodeInfo, ListValidation.InvalidCodeMessage);

			tax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			tax.AddInfoValidation.ValidateC1_Code();
			AssertHasWarning(tax.C1_CodeInfo, ListValidation.InvalidCodeMessage);

			errorMesssage = "No details were found for current tax so you have to override this line";
			tax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			tax.AddInfoValidation.ValidateC1_Code();
			AssertHasMessageError(tax.C1_CodeInfo, errorMesssage);

			tax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			tax.AddInfoValidation.ValidateC1_Code();
			AssertHasMessageError(tax.C1_CodeInfo, errorMesssage);

			tax.AddInfoValidation.ValidateC1_Code();
			AssertHasMessageError(tax.C1_CodeInfo, errorMesssage);

			CACustomsDataRegistry.Instance.DefaultToThisExciseTaxRateCodeWhenApplicable.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "C00");
			tax.C1_Code = "C00";
			tax.AddInfoValidation.ValidateC1_Code();
			AssertNoMessageError(tax.C1_CodeInfo, errorMesssage);
		}

		public void TestCheckC1_CodeForGST()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "CAGSTRateCodes", Core.Constants.CountryCodes.Canada);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "HXU", "HXU DESC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var errorMesssage = "No details were found for current tax so you have to override this line";
			var tax = Factory.New<DutyAndTax>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			tax.Parent = invoiceLine;
			tax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			tax.C1_Code = "HXU";
			tax.AddInfoValidation.ValidateC1_Code();
			AssertNoMessageError(tax.C1_CodeInfo, errorMesssage);
			tax.C1_Code = "HX1";
			tax.AddInfoValidation.ValidateC1_Code();
			AssertHasMessageError(tax.C1_CodeInfo, errorMesssage);
		}

		#endregion

		#region TestCheckC1_Rate

		public void TestCheckC1_Rate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var tax = Factory.New<DutyAndTax>();
			tax.Parent = invoiceLine;
			tax.B7_ParentID = ZGuid.NewZGuid();
			tax.B7_ParentTableCode = invoiceLine.TablePrefix;

			const string errorMessage = "No duty rate found so the default general rate of duty has been used";

			tax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			tax.C1_Override = false;
			tax.AddInfoValidation.ValidateC1_Rate();
			AssertHasWarning(tax.C1_RateInfo, errorMessage);

			tax.C1_Override = true;
			tax.AddInfoValidation.ValidateC1_Rate();
			AssertNoWarning(tax.C1_RateInfo, errorMessage);

			tax.C1_Override = false;
			tax.IsInRefFiles = true;
			tax.AddInfoValidation.ValidateC1_Rate();
			AssertNoWarning(tax.C1_RateInfo, errorMessage);

			tax.IsInRefFiles = false;
			tax.C1_RateType = RateTypes.Codes.Free;
			tax.AddInfoValidation.ValidateC1_Rate();
			AssertNoWarning(tax.C1_RateInfo, errorMessage);

			tax.C1_RateType = RateTypes.Codes.Specific;
			Factory.Save();
			tax.AddInfoValidation.ValidateC1_Rate();
			AssertNoWarning(tax.C1_RateInfo, errorMessage);
		}

		#endregion

		#region TestCheckC1_PreviousTranLine

		public void TestCheckC1_PreviousTranLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var tax = invoiceLine.DutiesAndTaxes.AddNew();

			const string errorMessage = "Previous Tran. Line Number required.";
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Supplementary;
			invoiceLine.JI_Tariff = "0000999902";
			tax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			tax.C1_Override = true;
			tax.C1_PreviousTranLine = 0;
			tax.AddInfoValidation.ValidateC1_PreviousTranLine();
			AssertHasMessageError(tax.C1_PreviousTranLineInfo, errorMessage);
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			tax.AddInfoValidation.ValidateC1_PreviousTranLine();
			AssertNoMessageError(tax.C1_PreviousTranLineInfo, errorMessage);
			tax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			tax.C1_PreviousTranLine = 1;
			tax.AddInfoValidation.ValidateC1_PreviousTranLine();
			AssertNoMessageError(tax.C1_PreviousTranLineInfo, errorMessage);
			tax.C1_PreviousTranLine = 0;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			tax.AddInfoValidation.ValidateC1_PreviousTranLine();
			AssertNoMessageError(tax.C1_PreviousTranLineInfo, errorMessage);

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			tax.AddInfoValidation.ValidateC1_PreviousTranLine();
			AssertNoMessageError(tax.C1_PreviousTranLineInfo, errorMessage);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			tax.AddInfoValidation.ValidateC1_PreviousTranLine();
			AssertNoMessageError(tax.C1_PreviousTranLineInfo, errorMessage);
		}

		#endregion

		#region TestCheckC1_PreviousTranNumber

		public void TestCheckC1_PreviousTranNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var tax = invoiceLine.DutiesAndTaxes.AddNew();

			const string errorMessage = "Previous Tran. Number required.";
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Supplementary;
			invoiceLine.JI_Tariff = "0000999902";
			tax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			tax.C1_Override = true;
			tax.C1_PreviousTranNumber = "";
			tax.AddInfoValidation.ValidateC1_PreviousTranNumber();
			AssertHasMessageError(tax.C1_PreviousTranNumberInfo, errorMessage);
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			tax.AddInfoValidation.ValidateC1_PreviousTranNumber();
			AssertNoMessageError(tax.C1_PreviousTranNumberInfo, errorMessage);
			tax.C1_PreviousTranNumber = "1";
			tax.AddInfoValidation.ValidateC1_PreviousTranNumber();
			AssertNoMessageError(tax.C1_PreviousTranNumberInfo, errorMessage);
			tax.C1_PreviousTranNumber = "";
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			tax.AddInfoValidation.ValidateC1_PreviousTranNumber();
			AssertNoMessageError(tax.C1_PreviousTranNumberInfo, errorMessage);

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			tax.AddInfoValidation.ValidateC1_PreviousTranNumber();
			AssertNoMessageError(tax.C1_PreviousTranNumberInfo, errorMessage);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			tax.AddInfoValidation.ValidateC1_PreviousTranNumber();
			AssertNoMessageError(tax.C1_PreviousTranNumberInfo, errorMessage);
		}

		#endregion

		#region TestCheckC1_ExemptCode

		public void TestCheckC1_ExemptCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var tax = invoiceLine.DutiesAndTaxes.AddNew();

			var casualImportTax = invoiceLine.DutiesAndTaxes.AddNew();
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, invoiceLine.Declaration);
			casualImportTax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			casualImportTax.C1_ExemptCode = "";
			casualImportTax.C1_Amount = 0;
			AssertNoMessageError(casualImportTax.C1_ExemptCodeInfo, "You have not entered a SIMA Code.");
			casualImportTax.C1_Amount = 2;
			AssertHasMessageErrorContaining(casualImportTax.C1_ExemptCodeInfo, "You have not entered a SIMA Code.");
			casualImportTax.C1_ExemptCode = SIMACodes.Codes.C10;
			AssertNoMessageError(casualImportTax.C1_ExemptCodeInfo, "The code you have selected is not in the list.");
			casualImportTax.C1_ExemptCode = "99";
			AssertHasMessageError(casualImportTax.C1_ExemptCodeInfo, "The code you have selected is not in the list.");
			casualImportTax.C1_TaxType = DutyAndTaxTypes.Codes.SIMADuty;
			casualImportTax.C1_Amount = 2;
			casualImportTax.C1_ExemptCode = "";
			AssertHasMessageError(casualImportTax.C1_ExemptCodeInfo, "You have not entered a SIMA Code.");
			casualImportTax.C1_ExemptCode = "~";
			AssertNoMessageError(casualImportTax.C1_ExemptCodeInfo, "You have not entered a SIMA Code.");
			AssertHasMessageError(casualImportTax.C1_ExemptCodeInfo, "The code you have selected is not in the list.");
			casualImportTax.C1_ExemptCode = SIMACodes.Codes.C10;
			AssertNoMessageError(casualImportTax.C1_ExemptCodeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckC1_ExemptCode_SIMADuties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			const string errorMessage = "All SIMA type rates must have the same SIMA Code.";
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, invoiceLine.Declaration);

			var tax1 = invoiceLine.DutiesAndTaxes.AddNew();
			tax1.C1_Override = true;
			tax1.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			var tax2 = invoiceLine.DutiesAndTaxes.AddNew();
			tax2.C1_Override = true;
			tax2.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			var tax3 = invoiceLine.DutiesAndTaxes.AddNew();
			tax3.C1_Override = true;
			tax3.C1_TaxType = DutyAndTaxTypes.Codes.SUR;

			tax1.C1_ExemptCode = SIMACodes.Codes.C10;
			AssertHasMessageError(tax1.C1_ExemptCodeInfo, errorMessage);
			tax1.AddInfoValidation.ValidateC1_ExemptCode();
			AssertNoMessageError(tax1.C1_ExemptCodeInfo, errorMessage);
			AssertNoMessageError(tax2.C1_ExemptCodeInfo, errorMessage);
			AssertNoMessageError(tax3.C1_ExemptCodeInfo, errorMessage);
			tax2.C1_ExemptCode = SIMACodes.Codes.C20;
			AssertHasMessageError(tax2.C1_ExemptCodeInfo, errorMessage);
			tax2.AddInfoValidation.ValidateC1_ExemptCode();
			AssertNoMessageError(tax1.C1_ExemptCodeInfo, errorMessage);
			AssertNoMessageError(tax2.C1_ExemptCodeInfo, errorMessage);
			AssertNoMessageError(tax3.C1_ExemptCodeInfo, errorMessage);
			tax3.C1_ExemptCode = SIMACodes.Codes.C30;
			AssertNoMessageError(tax1.C1_ExemptCodeInfo, errorMessage);
			AssertNoMessageError(tax2.C1_ExemptCodeInfo, errorMessage);
			AssertNoMessageError(tax3.C1_ExemptCodeInfo, errorMessage);
		}

		#endregion

		#region TestValidateOnOverrideValueChanged

		public void TestValidateOnOverrideValueChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var tax = Factory.New<DutyAndTax>();
			tax.Parent = invoiceLine;

			tax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			tax.C1_RateType = RateTypes.Codes.AcceptT;
			tax.AddInfoValidation.ValidateOnOverrideValueChanged();
			AssertHasMessageError(tax.C1_CodeInfo, "Please select the appropriate Tax Code or select 'NO' if the tax does not apply");
			AssertHasMessageError(tax.C1_AmountInfo, "Excise Tax Rate Type T specified so you must override and enter the Rate and Amount manually");

			tax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			tax.AddInfoValidation.ValidateOnOverrideValueChanged();
			AssertHasWarning(tax.C1_RateInfo, "No duty rate found so the default general rate of duty has been used");
		}

		#endregion

		#region TestCheckC1_NormalValueCurrency

		public void TestCheckC1_NormalValueCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var tax = invoiceLine.DutiesAndTaxes.AddNew();

			tax.C1_NormalValueCurrency = "~";
			AssertHasMessageErrorContaining(tax.C1_NormalValueCurrencyInfo, ListValidation.InvalidCodeMessageError);
			tax.C1_NormalValueCurrency = Core.Constants.CurrencyCodes.Canada;
			AssertNoMessageErrorContaining(tax.C1_NormalValueCurrencyInfo, ListValidation.InvalidCodeMessageError);

			tax.C1_NormalValuePerUnit = 100m;
			tax.C1_NormalValueCurrency = ZString.Empty;
			AssertHasMessageErrorContaining(tax.C1_NormalValueCurrencyInfo, MandatoryValidation.YouHaveNotEntered);
			tax.C1_NormalValueCurrency = Core.Constants.CurrencyCodes.Canada;
			AssertNoMessageErrorContaining(tax.C1_NormalValueCurrencyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#endregion

		#region TestCheckC1_UnitOfMeasure

		public void TestCheckC1_UnitOfMeasure()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var tax = invoiceLine.DutiesAndTaxes.AddNew();

			tax.C1_UnitOfMeasure = "~";
			AssertHasMessageErrorContaining(tax.C1_UnitOfMeasureInfo, ListValidation.InvalidCodeMessageError);
			tax.C1_UnitOfMeasure = "KGM";
			AssertNoMessageErrorContaining(tax.C1_UnitOfMeasureInfo, ListValidation.InvalidCodeMessageError);
			tax.C1_UnitOfMeasure = ZString.Empty;
			AssertNoMessageErrorContaining(tax.C1_UnitOfMeasureInfo, MandatoryValidation.YouHaveNotEntered);
			tax.C1_NormalValuePerUnit = 100m;
			tax.AddInfoValidation.ValidateC1_UnitOfMeasure();
			AssertHasMessageErrorContaining(tax.C1_UnitOfMeasureInfo, MandatoryValidation.YouHaveNotEntered);
			tax.C1_UnitOfMeasure = "TNE";
			AssertNoMessageErrorContaining(tax.C1_UnitOfMeasureInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(tax.C1_UnitOfMeasureInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#endregion

		#region Implementation

		DutyAndTax AddDutyOrTax(string dutyOrTaxType, JobComInvoiceLine invoiceLine)
		{
			var dutyOrTax = invoiceLine.DutiesAndTaxes.AddNew();
			dutyOrTax.C1_Override = true;
			dutyOrTax.C1_TaxType = dutyOrTaxType;
			return dutyOrTax;
		}

		#endregion
	}
}
