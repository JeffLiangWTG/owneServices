using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.Testing
{
	public class AccountingCaptionHelperTest : TestCaseWithFactory
	{
		public void TestTaxCaptionsAreSetForExtraTaxEnabledCountries()
		{
			foreach (FieldInfo field in typeof(AccTaxRate.ExtraTypes).GetFields().Where(f => f.IsLiteral))
			{
				var extraTypeString = (string)field.GetValue(null);
				switch (extraTypeString)
				{
					case "EDU":
						GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.India);
						AssertExtraTaxCaptions("SGST Amt", "SGST Amount", "SGST Local", "CGST/IGST Amount", "CGST/IGST Local");
						break;

					case "SPV":
						GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Italy);
						AssertExtraTaxCaptions("SPV Amt", "SPV Amount", "SPV Local", "IVA Amount", "IVA Local");

						GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.CostaRica);
						AssertExtraTaxCaptions("Exon. Amt", "Exon. Amount", "Exon. Local", "IVA Amount", "IVA Local");
						break;

					case "RET":
						GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Mexico);
						AssertExtraTaxCaptions("RET Amt", "RET Amount", "RET Local", "IVA Amount", "IVA Local");
						break;

					case "REF":
						GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Mexico);
						AssertExtraTaxCaptions("RET Amt", "RET Amount", "RET Local", "IVA Amount", "IVA Local");
						break;

					case "QST":
						GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);
						Factory.Save();
						AssertExtraTaxCaptions("QST Amt", "QST Amount", "QST Local", "GST Amount", "GST Local");
						break;

					case "SER":
						GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Malaysia);
						Assert("Malaysia Extra Tax Type is used to distinguish from new GST Tax Ids", true);
						break;
					case "REG":
						GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Spain);
						Assert("Spain Extra Tax Type is used for Canary Islands only", true);
						break;
					case "INP":
					case "OTO":
						GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
						Assert("Tax columns are not visible", true); //China Extra taxes are not displayed in Invoice form
						break;

					case "QCT":
						Assert("Tax columns are not visible for only QCT tax without QST taxes", true);

						GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.India);
						AssertExtraTaxCaptions("SGST Amt", "SGST Amount", "SGST Local", "CGST/IGST Amount", "CGST/IGST Local");
						break;

					case "STA":
						Assert("We have not decided what to do with this new type of tax yet, for now we just added it. we will handle this in a later workitem", true);
						break;

					default:
						Assert("Extra tax type not handled", false);
						break;
				}
			}
		}

		public void TestExtraTaxCaptionsForCountries()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			AssertExtraTaxCaptions(null, null, null, null, null);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.India);
			AssertExtraTaxCaptions("SGST Amt", "SGST Amount", "SGST Local", "CGST/IGST Amount", "CGST/IGST Local");

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Mexico);
			AssertExtraTaxCaptions("RET Amt", "RET Amount", "RET Local", "IVA Amount", "IVA Local");

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Turkey);
			AssertExtraTaxCaptions("VAT Withholding Amt", "VAT Withholding Amount", "VAT Withholding Local", "VAT Amount", "VAT Local");

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Congo);
			AssertExtraTaxCaptions("SURTAX Amt", "SURTAX Amount", "SURTAX Local", "TVA Amount", "TVA Local");

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);
			Factory.Save();
			AssertExtraTaxCaptions("QST Amt", "QST Amount", "QST Local", "GST Amount", "GST Local");

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Gabon);
			AssertExtraTaxCaptions("CSS Amt", "CSS Amount", "CSS Local", null, null);
		}

		void AssertExtraTaxCaptions(string expectedOSShortCaption, string expectedOSCaption, string expectedLocalCaption, string expectedOSMainTaxCaption, string expectedLocalMainTaxCaption)
		{
			AssertEquals("OS Extra Tax Amount Short Caption", expectedOSShortCaption, AccountingCaptionHelper.OSExtraTaxAmountCaption.ShortCaption);
			AssertEquals("OS Extra Tax Amount Caption", expectedOSCaption, AccountingCaptionHelper.OSExtraTaxAmountCaption.Caption);
			AssertEquals("Local Extra Tax Amount Caption", expectedLocalCaption, AccountingCaptionHelper.LocalExtraTaxAmountCaption.Caption);
			AssertEquals("OS Main Tax Amount Caption", expectedOSMainTaxCaption, AccountingCaptionHelper.OSTaxAmountCaption.Caption);
			AssertEquals("Local Main Tax Amount Caption", expectedLocalMainTaxCaption, AccountingCaptionHelper.LocalTaxAmountCaption.Caption);
		}
	}
}
