using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DocJobComInvoiceLine))]
	sealed class DocJobComInvoiceLineTest : DocBaseJobComInvoiceLineAbstractTest<JobComInvoiceLine, DocJobComInvoiceLine>
	{
		public void TestCountries()
		{
			InvoiceLine.InvoiceHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Canada;
			InvoiceLine.InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.Canada;
			AssertEquals("EffectiveCountryAndStateOfOrigin", "CA", InvoiceLineWrapper.EffectiveCountryAndStateOfOrigin);
			AssertEquals("CountryAndStateOfOrigin", "CA", InvoiceLineWrapper.CountryAndStateOfOrigin);
			AssertEquals("CountryAndStateOfExport", string.Empty, InvoiceLineWrapper.CountryAndStateOfExport);

			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			InvoiceLine.JI_StateOrRegionOfOrigin = USStatesList.Codes.NewYork;
			InvoiceLine.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			InvoiceLine.CA_USStateOfExport = USStatesList.Codes.NewYork;

			AssertEquals("EffectiveCountryAndStateOfOrigin", "UNY", InvoiceLineWrapper.EffectiveCountryAndStateOfOrigin);
			AssertEquals("CountryAndStateOfOrigin", "UNY", InvoiceLineWrapper.CountryAndStateOfOrigin);
			AssertEquals("CountryAndStateOfExport", "UNY", InvoiceLineWrapper.CountryAndStateOfExport);
		}

		public void TestFormattedTariff()
		{
			InvoiceLine.JI_Tariff = "0123456789";
			AssertEquals("FormattedTariff", "0123.45.67 89", InvoiceLineWrapper.FormattedTariff);

			InvoiceLine.CA_99TariffCode = "9902";
			AssertEquals("TariffCode", "9902", InvoiceLineWrapper.TariffCode);
		}

		public void TestInvoiceQuantityFormatted()
		{
			InvoiceLine.JI_InvoiceUQ = Core.Constants.PkgUnit.Piece;
			AssertEquals(string.Empty, InvoiceLineWrapper.InvoiceQuantityFormatted);
			InvoiceLine.JI_CustomsQuantity = 11.12;
			InvoiceLine.JI_CustomsUnitQty = "MNB";
			InvoiceLine.JI_InvoiceQuantity = 10.12;
			AssertEquals("10.12 PCE", InvoiceLineWrapper.InvoiceQuantityFormatted);
			InvoiceLine.JI_InvoiceQuantity = 10.1234;
			AssertEquals("10.123 PCE", InvoiceLineWrapper.InvoiceQuantityFormatted);
			InvoiceLine.JI_InvoiceQuantity = 10;
			AssertEquals("10 PCE", InvoiceLineWrapper.InvoiceQuantityFormatted);
			InvoiceLine.JI_InvoiceUQ = string.Empty;
			AssertEquals("10", InvoiceLineWrapper.InvoiceQuantityFormatted);
			InvoiceLine.JI_InvoiceQuantity = 0m;
			AssertEquals("11.12 MNB", InvoiceLineWrapper.InvoiceQuantityFormatted);
		}

		public void TestCustomsQuantityFormatted()
		{
			InvoiceLine.JI_CustomsQuantity = 10.12;
			AssertEquals("CustomsQuantityFormatted", "10.12", InvoiceLineWrapper.CustomsQuantityFormatted);
			InvoiceLine.JI_CustomsQuantity = 10.1234;
			AssertEquals("CustomsQuantityFormatted", "10.123", InvoiceLineWrapper.CustomsQuantityFormatted);
			InvoiceLine.JI_CustomsQuantity = 10;
			AssertEquals("CustomsQuantityFormatted", "10", InvoiceLineWrapper.CustomsQuantityFormatted);

			InvoiceLine.JI_CustomsSecondUnitQty = UnitOfWeightList.Codes.Kilogram;
			InvoiceLine.JI_CustomsSecondQuantity = 10.12;
			AssertEquals("CustomsQuantity2Formatted", "10.12", InvoiceLineWrapper.CustomsQuantity2Formatted);
			AssertEquals("CustomsUnitQty2", UnitOfWeightList.Codes.Kilogram, InvoiceLineWrapper.CustomsUnitQty2);
			InvoiceLine.JI_CustomsSecondQuantity = 10.1234;
			AssertEquals("CustomsQuantity2Formatted", "10.123", InvoiceLineWrapper.CustomsQuantity2Formatted);
			InvoiceLine.JI_CustomsSecondQuantity = 10;
			AssertEquals("CustomsQuantity2Formatted", "10", InvoiceLineWrapper.CustomsQuantity2Formatted);

			InvoiceLine.JI_CustomsThirdUnitQty = UnitOfWeightList.Codes.Kilogram;
			InvoiceLine.JI_CustomsThirdQuantity = 10.12;
			AssertEquals("CustomsQuantity3Formatted", "10.12", InvoiceLineWrapper.CustomsQuantity3Formatted);
			AssertEquals("CustomsUnitQty3", UnitOfWeightList.Codes.Kilogram, InvoiceLineWrapper.CustomsUnitQty3);
			InvoiceLine.JI_CustomsThirdQuantity = 10.1234;
			AssertEquals("CustomsQuantity3Formatted", "10.123", InvoiceLineWrapper.CustomsQuantity3Formatted);
			InvoiceLine.JI_CustomsThirdQuantity = 10;
			AssertEquals("CustomsQuantity3Formatted", "10", InvoiceLineWrapper.CustomsQuantity3Formatted);
		}

		public void TestAmounts()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			InvoiceLine.CA_CVforCurrConvOvr = true;
			InvoiceLine.CA_CVforCurrConv = 10;
			InvoiceLine.CA_CustomsValueOvr = true;
			InvoiceLine.CA_CustomsValue = 20;
			InvoiceLine.DutiesAndTaxes.DeleteAll();
			DocDutyOrTaxTest.AddDutyOrTax(InvoiceLine, DutyAndTaxTypes.Codes.ADD, SIMACodes.Codes.C10, 0m, string.Empty, 70m, string.Empty);
			DocDutyOrTaxTest.AddDutyOrTax(InvoiceLine, DutyAndTaxTypes.Codes.ExciseTax, ExciseTaxExemptionCodes.Codes.C94, 0m, string.Empty, 80m, string.Empty);
			DocDutyOrTaxTest.AddDutyOrTax(InvoiceLine, DutyAndTaxTypes.Codes.GST, string.Empty, 5m, RateTypes.Codes.AdValorem, 50m, string.Empty);
			DocDutyOrTaxTest.AddDutyOrTax(InvoiceLine, DutyAndTaxTypes.Codes.CustomsDuty, string.Empty, 10m, RateTypes.Codes.Specific, 10m, string.Empty);
			DocDutyOrTaxTest.AddDutyOrTax(InvoiceLine, DutyAndTaxTypes.Codes.CustomsDuty, string.Empty, 0.000745m, RateTypes.Codes.Specific, 20m, UnitOfWeightList.Codes.Gram);
			DocDutyOrTaxTest.AddDutyOrTax(InvoiceLine, DutyAndTaxTypes.Codes.CustomsDuty, string.Empty, 0.000745m, RateTypes.Codes.AdValorem, 30m, UnitOfWeightList.Codes.Kilogram);
			declaration.ResumeApportionment();
			AssertEquals("ValueForCurrencyConversion", 10m, InvoiceLineWrapper.ValueForCurrencyConversion);
			AssertEquals("ValueForDuty", 20m, InvoiceLineWrapper.ValueForDuty);
			AssertEquals("ValueForTax", 160m, InvoiceLineWrapper.ValueForTax);
			AssertEquals("SIMADuty.ExemptCode", SIMACodes.Codes.C10, InvoiceLineWrapper.SIMADuty.ExemptCode);
			AssertEquals("ExciseTax.ExemptCode", ExciseTaxExemptionCodes.Codes.C94, InvoiceLineWrapper.ExciseTax.ExemptCode);
			AssertEquals("GST.Amount", 50m, InvoiceLineWrapper.GST.Amount);
			AssertEquals("CustomsDuties.Amount", 60m, InvoiceLineWrapper.CustomsDuties.Amount);
			AssertEquals("CustomsDuty2.Amount", 10m, InvoiceLineWrapper.CustomsDuty1.Amount);
			AssertEquals("CustomsDuty1.Amount", 20m, InvoiceLineWrapper.CustomsDuty2.Amount);
			AssertEquals("CustomsDuty3.Amount", 30m, InvoiceLineWrapper.CustomsDuty3.Amount);
			AssertEquals("TotalDutiesAndTaxes", 260m, InvoiceLineWrapper.TotalDutiesAndTaxes);
			AssertEquals("SIMADuty.Amount", 70m, InvoiceLineWrapper.SIMADuty.Amount);
			AssertEquals("ExciseTax.Amount", 80m, InvoiceLineWrapper.ExciseTax.Amount);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			AssertNull("GST", InvoiceLineWrapper.GST);
			AssertNull("SIMADuty", InvoiceLineWrapper.SIMADuty);
			AssertNull("ExciseTax", InvoiceLineWrapper.ExciseTax);
			AssertNull("CustomsDuties", InvoiceLineWrapper.CustomsDuties);
			AssertNull("CustomsDuty1", InvoiceLineWrapper.CustomsDuty1);
			AssertNull("CustomsDuty2", InvoiceLineWrapper.CustomsDuty2);
			AssertNull("CustomsDuty3", InvoiceLineWrapper.CustomsDuty3);
			AssertEquals("TotalDutiesAndTaxes", 0m, InvoiceLineWrapper.TotalDutiesAndTaxes);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			AssertNull("GST", InvoiceLineWrapper.GST);
			AssertNull("SIMADuty", InvoiceLineWrapper.SIMADuty);
			AssertNull("ExciseTax", InvoiceLineWrapper.ExciseTax);
			AssertNull("CustomsDuties", InvoiceLineWrapper.CustomsDuties);
			AssertNull("CustomsDuty1", InvoiceLineWrapper.CustomsDuty1);
			AssertNull("CustomsDuty2", InvoiceLineWrapper.CustomsDuty2);
			AssertNull("CustomsDuty3", InvoiceLineWrapper.CustomsDuty3);
			AssertEquals("TotalDutiesAndTaxes", 0m, InvoiceLineWrapper.TotalDutiesAndTaxes);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			AssertNull("GST", InvoiceLineWrapper.GST);
			AssertNull("SIMADuty", InvoiceLineWrapper.SIMADuty);
			AssertNull("ExciseTax", InvoiceLineWrapper.ExciseTax);
			AssertNull("CustomsDuties", InvoiceLineWrapper.CustomsDuties);
			AssertNull("CustomsDuty1", InvoiceLineWrapper.CustomsDuty1);
			AssertNull("CustomsDuty2", InvoiceLineWrapper.CustomsDuty2);
			AssertNull("CustomsDuty3", InvoiceLineWrapper.CustomsDuty3);
			AssertEquals("TotalDutiesAndTaxes", 0m, InvoiceLineWrapper.TotalDutiesAndTaxes);
		}

		public void TestReferencesAndOtherTextFields()
		{
			InvoiceLine.InvoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			AssertEquals("EffectiveTreatmentCode", TariffTreatmentCodes.Codes.UnitedStates, InvoiceLineWrapper.EffectiveTreatmentCode);
			AssertEquals("TreatmentCode", TariffTreatmentCodes.Codes.UnitedStates, InvoiceLineWrapper.TreatmentCode);
			InvoiceLine.CA_TreatmentCode = TariffTreatmentCodes.Codes.Norway;
			AssertEquals("EffectiveTreatmentCode", TariffTreatmentCodes.Codes.Norway, InvoiceLineWrapper.EffectiveTreatmentCode);
			AssertEquals("TreatmentCode", TariffTreatmentCodes.Codes.Norway, InvoiceLineWrapper.TreatmentCode);

			InvoiceLine.InvoiceHeader.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsIdenticalGoods;
			AssertEquals("EffectiveValueForDutyCode", ValueForDutyCodes.Codes.RelatedFirmsIdenticalGoods, InvoiceLineWrapper.EffectiveValueForDutyCode);
			InvoiceLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsComputedValue;
			AssertEquals("EffectiveValueForDutyCode", ValueForDutyCodes.Codes.RelatedFirmsComputedValue, InvoiceLineWrapper.EffectiveValueForDutyCode);

			InvoiceLine.CA_TRSNumber = "123";
			InvoiceLine.CA_AuthorityNumber = "123-4567";
			AssertEquals("TRSNumber", "123", InvoiceLineWrapper.TRSNumber);
			AssertEquals("AuthorityNumber", "123-4567", InvoiceLineWrapper.AuthorityNumber);
		}

		public void TestAddData()
		{
			InvoiceLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsComputedValue;
			AssertEquals(ZString.Empty, InvoiceLineWrapper.AddData);
			InvoiceLine.CA_99TariffCode = "9901";
			AssertEquals("TC=9901", InvoiceLineWrapper.AddData);
			InvoiceLine.JI_CustomsSecondQuantity = 1m;
			InvoiceLine.JI_CustomsSecondUnitQty = "LPA";
			InvoiceLine.JI_CustomsThirdQuantity = 2m;
			InvoiceLine.JI_CustomsThirdUnitQty = "KGM";
			InvoiceLine.CA_AuthorityNumber = "A12345";
			InvoiceLine.CA_TRSNumber = "T6789";
			AssertEquals("TC=9901,QTY2=1LPA,QTY3=2KGM,S/Auth=A12345,TRS=T6789", InvoiceLineWrapper.AddData);
		}

		public override void TestLinePriceCurr()
		{
			AssertEquals("LinePriceCurr", "CAD", InvoiceLineWrapper.LinePriceCurr.ToString());
		}

		public override void TestMergedLineNo()
		{
			AssertEquals(InvoiceLineWrapper.MergedLineNo, "Not Merged");
		}

		public override void TestMergedNumericLineNo()
		{
			AssertEquals(ZShort.Zero, InvoiceLineWrapper.MergedNumericLineNo);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 10;
			InvoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);

			AssertEquals((ZShort)10, InvoiceLineWrapper.MergedNumericLineNo);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Canada; }
		}

		protected override DocJobComInvoiceLine CreateInvoiceLineWrapper(JobComInvoiceLine invoiceLineInternal)
		{
			return DocJobComInvoiceLine.New(invoiceLineInternal, Factory);
		}

		#endregion
	}
}
