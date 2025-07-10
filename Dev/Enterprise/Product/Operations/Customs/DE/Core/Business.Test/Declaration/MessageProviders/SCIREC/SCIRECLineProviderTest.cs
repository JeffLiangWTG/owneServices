using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCIRECLineProvider))]
	sealed class SCIRECLineProviderTest : ImportDecLineProviderAbstractTest<SCIRECLineProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SCIRECLineProvider(null));
		}

		public void TestInwardMovementAmount()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			invoiceLine.JI_BondedWhsQuantity = 10.5;
			invoiceLine.JI_BondedWhsUnitQty = "KGMA";
			invoiceLine2.JI_BondedWhsQuantity = 5.1;
			invoiceLine2.JI_BondedWhsUnitQty = "KGMA";
			var amount = Provider.InwardMovementAmount;

			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 15.6m, amount.Quantity);
				AssertEquals("MeasurementUnit", "KGM", amount.MeasurementUnit);
				AssertEquals("Qualifier", "A", amount.Qualifier);
			});
		}

		public void TestRequestedPreferentialTreatment()
		{
			invoiceLine.JI_PrimaryPreference = "150";
			AssertEquals("150", Provider.RequestedPreferentialTreatment);
		}

		public void TestRequestedPreferentialTreatment_ConcessionF01() => AssertRequestedPreferentialTreatment_Concession("F01");

		public void TestRequestedPreferentialTreatment_ConcessionF02() => AssertRequestedPreferentialTreatment_Concession("F02");

		public void TestRequestedPreferentialTreatment_ConcessionF03() => AssertRequestedPreferentialTreatment_Concession("F03");

		public void TestRequestedPreferentialTreatment_EntryStyle_CO()
		{
			SetupInvoiceLineForPreferentialTreatmentAndConcession("A00", Core.Constants.CountryCodes.India, EntryStyleListImport.Codes.ImportFromSpecialTerritory);
			AssertEquals(Core.Constants.CountryCodes.India, Provider.RequestedPreferentialTreatment);
		}

		public void TestRequestedPreferentialTreatment_EntryStyle_CO_ConcessionF01()
		{
			SetupInvoiceLineForPreferentialTreatmentAndConcession("F01", string.Empty, EntryStyleListImport.Codes.ImportFromSpecialTerritory);
			AssertNull(Provider.RequestedPreferentialTreatment);
		}

		public void TestOriginCountry_EmptyCountryOfSupply()
		{
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Indonesia;
			invoiceLine.JI_PrimaryPreference = string.Empty;
			AssertEquals(Core.Constants.CountryCodes.Indonesia, Provider.OriginCountry);
		}

		public void TestOriginCountry_NotEqualCountryOfSupply()
		{
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Indonesia;
			invoiceLine.JI_PrimaryPreference = Core.Constants.CountryCodes.India;
			AssertEquals(Core.Constants.CountryCodes.Indonesia, Provider.OriginCountry);
		}

		public void TestOriginCountry_EqualCountryOfSupply()
		{
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Indonesia;
			invoiceLine.JI_PrimaryPreference = Core.Constants.CountryCodes.Indonesia;

			AssertNull(Provider.OriginCountry);
		}

		public void TestAssessmentCustomsValue()
		{
			declaration.ZG_IsHighValueOvrd = false;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 502.58;
			AssertEquals(502.58m, Provider.AssessmentCustomsValue);
		}

		public void TestAssessmentCustomsValue_DeclarationDV1()
		{
			declaration.ZG_IsHighValueOvrd = true;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 502.58;
			AssertEquals(502.58m, Provider.AssessmentCustomsValue);
		}

		protected override IEnumerable<Expression<Func<SCIRECLineProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.InwardMovementAmount;
		}

		protected override SCIRECLineProvider GetProvider() => new SCIRECLineProvider(entryLine);

		void AssertRequestedPreferentialTreatment_Concession(string concession)
		{
			SetupInvoiceLineForPreferentialTreatmentAndConcession(concession, Core.Constants.CountryCodes.India, EntryStyleListImport.Codes.ImportFromEFTAMember);
			AssertEquals(Core.Constants.CountryCodes.India, Provider.RequestedPreferentialTreatment);
		}

		void SetupInvoiceLineForPreferentialTreatmentAndConcession(string concession, string primaryPreference, string entryStyle)
		{
			invoiceLine.JI_PrimaryPreference = primaryPreference;
			invoiceLine.JI_ConcessionOrder = "A12345";
			invoiceLine.ZG_SecondQuota = "B12345";
			invoiceLine.JI_Procedure = "4000" + concession;
			declaration.JE_EntryStyle = entryStyle;
		}

		new ISCIRECLine Provider => base.Provider;
	}
}
