using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ComplianceSubTypeLocalDescription))]
	sealed class ComplianceSubTypeLocalDescriptionTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<ComplianceSubTypeLocalDescription(  text   )>", Passes.FirstPass));
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<ComplianceSubTypeLocalDescription(C101003)>", Passes.FirstPass));
			AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<ComplianceSubTypeLocalDescription (C101003) >", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			var originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				AssertComplianceSubTypeLocalDescription(PeruComplianceInfo.ComplianceSubTypeCodes.TXI, Core.Constants.CountryCodes.Peru, "FACTURA");
				AssertComplianceSubTypeLocalDescription(PeruComplianceInfo.ComplianceSubTypeCodes.TCR, Core.Constants.CountryCodes.Peru, "NOTA DE CREDITO");
				AssertComplianceSubTypeLocalDescription(PeruComplianceInfo.ComplianceSubTypeCodes.TCD, Core.Constants.CountryCodes.Peru, "NOTA DE DEBITO");

				AssertComplianceSubTypeLocalDescription(VietnamComplianceInfo.ComplianceSubTypeCodes.TXI, Core.Constants.CountryCodes.VietNam, "HÓA ĐƠN GIÁ TRỊ GIA TĂNG");

				AssertComplianceSubTypeLocalDescription(IndonesiaComplianceInfo.ComplianceSubTypeCodes.TXI, Core.Constants.CountryCodes.Indonesia, "Faktur Pajak");
				AssertComplianceSubTypeLocalDescription(IndonesiaComplianceInfo.ComplianceSubTypeCodes.BKP, Core.Constants.CountryCodes.Indonesia, "Ekspor Barang Kena Pajak Tidak Berwujud");
				AssertComplianceSubTypeLocalDescription(IndonesiaComplianceInfo.ComplianceSubTypeCodes.JKP, Core.Constants.CountryCodes.Indonesia, "Ekspor Jasa Kena Pajak");

				AssertComplianceSubTypeLocalDescription(PolandComplianceInfo.ComplianceSubTypeCodes.TCD, Core.Constants.CountryCodes.Poland, "FAKTURA KORYGUJĄCA");
				AssertComplianceSubTypeLocalDescription(PolandComplianceInfo.ComplianceSubTypeCodes.TXI, Core.Constants.CountryCodes.Poland, "FAKTURA VAT");
				AssertComplianceSubTypeLocalDescription(PolandComplianceInfo.ComplianceSubTypeCodes.DSB, Core.Constants.CountryCodes.Poland, "NOTA OBCIĄŻENIOWA");
				AssertComplianceSubTypeLocalDescription(PolandComplianceInfo.ComplianceSubTypeCodes.DCR, Core.Constants.CountryCodes.Poland, "NOTA KREDYTOWA");

				AssertComplianceSubTypeLocalDescription(MexicoComplianceInfo.ComplianceSubTypeCodes.TCR, Core.Constants.CountryCodes.Mexico, "Nota de Crédito");
				AssertComplianceSubTypeLocalDescription(MexicoComplianceInfo.ComplianceSubTypeCodes.TXI, Core.Constants.CountryCodes.Mexico, "Factura");
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		void AssertComplianceSubTypeLocalDescription(string subType, string countryCode, string expectedLocalDescription)
		{
			GlbCompany.CurrentCompany.SetCountry(countryCode);
			AssertEquals(expectedLocalDescription, ValueProviderToTest.GetReplacement(string.Format("<ComplianceSubTypeLocalDescription({0})>", subType), Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new ComplianceSubTypeLocalDescription();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("SubType", PeruComplianceInfo.ComplianceSubTypeCodes.TXI));
		}
	}
}
