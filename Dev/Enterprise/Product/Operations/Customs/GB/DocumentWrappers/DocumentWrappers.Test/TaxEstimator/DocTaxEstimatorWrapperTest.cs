using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.DocumentWrappers.Testing
{
	class DocTaxEstimatorWrapperTest : TestCaseWithFactory
	{
		public void TestLinesAreTypeBasedOnTariffSwitch()
		{
			var entry = SetupEntry();
			var parentUni = DocTaxEstimatorWrapper.New(entry, Factory);
			var lineUni = parentUni.LinesForTax[0];
			AssertType(typeof(LineForTax), lineUni);
		}

		public void TestAllHeaderProperties()
		{
			var entry = SetupEntry();
			var parent = DocTaxEstimatorWrapper.New(entry, Factory);
			AssertEquals("62", parent.Box62);
			AssertEquals("125", parent.Box63);
			AssertEquals("65", parent.Box65);
			AssertEquals("66", parent.Box66);
			AssertEquals("67", parent.Box67);
			AssertEquals(68m, parent.Box68);
			AssertEquals(2, parent.LinesForTax.Count);
		}

		protected CusEntryHeader SetupEntry(bool setupCharges = true)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_ManualCalc = false;
			declaration.JE_MessageType = "IMP";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			if (setupCharges)
			{
				declaration.JE_TotalWeight = 2000m;
				declaration.JE_TotalWeightUnit = "KG";

				SetCharges(declaration);
			}
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = currencyCode;

			var line1 = (EU.Business.Declaration.JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line1.JI_LinePrice = 1000m;
			line1.JI_Weight = 1000m;
			line1.JI_Tariff = "0101.10.90 90";
			line1.JI_SupplementaryCode1 = "B999";
			line1.JI_SupplementaryCode2 = "A888";
			line1.JI_CountryOfOrigin = "CN";
			var line1A10Full = line1.Taxes.AddNew();
			line1A10Full.Data.G4_Type = "A10";
			line1A10Full.Data.G4_RateDuty = "F";
			var line1B00Standard = line1.Taxes.AddNew();
			line1B00Standard.Data.G4_Type = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			line1B00Standard.Data.G4_RateDuty = "S";
			var line1A10Gsp = line1.Taxes.AddNew();
			line1A10Gsp.Data.G4_Type = "A10";
			line1A10Gsp.Data.G4_RateDuty = "G";

			var line2 = (EU.Business.Declaration.JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line2.JI_LinePrice = 3000m;
			line2.JI_Weight = 1000m;
			line2.JI_Tariff = "0101.10.90 90";
			line2.JI_CountryOfOrigin = "MX";
			var line2A10 = line2.Taxes.AddNew();
			line2A10.Data.G4_Type = "A10";
			line2A10.Data.G4_RateDuty = "A";
			var line2B00 = line2.Taxes.AddNew();
			line2B00.Data.G4_Type = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			line2B00.Data.G4_RateDuty = "A";

			declaration.JE_MergeBy = "NON";
			declaration.DoMerge();
			var entry = declaration.CustomsEntryHeaders[0];
			return entry;
		}

		void SetCharges(JobDeclaration declaration)
		{
			// set charges
			var topInvoiceCharges = declaration.TopGroupInvoice.Charges;

			var aft = topInvoiceCharges.AddNew();
			aft.J7_ChargeType = ChargesProvider.AirFreight.Code; // AFT, box62
			aft.J7_Amount = 62m;
			aft.J7_IsDutiable = true;
			aft.J7_IsGSTApplicable = true;
			aft.J7_RX_NKCurrency = currencyCode;

			var awb = topInvoiceCharges.AddNew();
			awb.J7_ChargeType = ChargesProvider.InternationalFreight.Code; // AWB, (box63 = AWB+AFT)
			awb.J7_IsDutiable = true;
			awb.J7_IsGSTApplicable = true;
			awb.J7_Amount = 63m;
			awb.J7_RX_NKCurrency = currencyCode;

			var dis = topInvoiceCharges.AddNew();
			dis.J7_ChargeType = ChargesProvider.Discount.Code; // DIS, box65
			dis.J7_Amount = 65m;
			dis.J7_IsDutiable = false;
			dis.J7_IsGSTApplicable = false;
			dis.J7_RX_NKCurrency = currencyCode;

			var ons = topInvoiceCharges.AddNew();
			ons.J7_ChargeType = ChargesProvider.InternationalInsurance.Code; // ONS, box66
			ons.J7_Amount = 66m;
			ons.J7_IsDutiable = true;
			ons.J7_IsGSTApplicable = true;
			ons.J7_RX_NKCurrency = currencyCode;

			var add = topInvoiceCharges.AddNew();
			add.J7_ChargeType = ChargesProvider.AdditionCharge.Code; // ADD, box67
			add.J7_Amount = 67m;
			add.J7_IsDutiable = false;
			add.J7_IsGSTApplicable = false;
			add.J7_RX_NKCurrency = currencyCode;

			var vat = topInvoiceCharges.AddNew();
			vat.J7_ChargeType = ChargesProvider.VATAdjustment.Code; // VAT, box68
			vat.J7_Amount = 68m;
			vat.J7_IsDutiable = false;
			vat.J7_IsGSTApplicable = true;
			vat.J7_RX_NKCurrency = currencyCode;

			declaration.ResumeApportionment();
		}

		readonly string currencyCode = Core.Constants.CurrencyCodes.UnitedKingdom;
	}

	[TestedType(typeof(LineForTaxCollection))]
	public class LineForTaxCollectionTest : DocBaseWrapperCollectionTest<LineForTaxCollection>
	{
		protected override LineForTaxCollection GetNewDocumentWrapperCollection()
		{
			SetUp();
			return parentWrapper.LinesForTax;
		}

		protected override object GetNewObjectToWrap()
		{
			return null;
		}

		protected override DocumentWrapper AddNewDocumentWrapperToCollection(DocumentWrapperCollection collection)
		{
			var result = new LineForTax(entry.MergedLines.AddNew(), Factory);
			collection.Add(result);
			return result;
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entry = declaration.CustomsEntryHeaders.AddNew();
			var line1 = entry.MergedLines.AddNew();
			var line2 = entry.MergedLines.AddNew();
			parentWrapper = DocTaxEstimatorWrapper.New(entry, Factory);
		}

		DocTaxEstimatorWrapper parentWrapper;
		JobDeclaration declaration;
		CusEntryHeader entry;
	}
}
