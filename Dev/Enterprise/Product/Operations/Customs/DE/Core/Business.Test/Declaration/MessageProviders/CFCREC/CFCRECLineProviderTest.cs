using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CFCRECLineProvider))]
	sealed class CFCRECLineProviderTest : ImportDecLineProviderAbstractTest<CFCRECLineProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CFCRECLineProvider(null));
		}

		public void TestTobaccoRevenueStampNumber()
		{
			invoiceLine.JI_TobaccoStamp = "AN123";
			AssertEquals("AN123", Provider.TobaccoRevenueStampNumber);
		}

		public void TestPreferentialTreatment()
		{
			invoiceLine.JI_ConcessionOrder = "A12345";
			invoiceLine.ZG_SecondQuota = "B12345";
			invoiceLine.JI_Procedure = "40005F0";

			var preferentialTreatment = Provider.PreferentialTreatment;
			AssertEquals("Cached", preferentialTreatment, Provider.PreferentialTreatment);
		}

		public void TestPreferentialTreatment_ConcessionF01() => AssertPreferentialTreatment_Concession("F01");

		public void TestPreferentialTreatment_ConcessionF02() => AssertPreferentialTreatment_Concession("F02");

		public void TestPreferentialTreatment_ConcessionF03() => AssertPreferentialTreatment_Concession("F03");

		public void TestPreferentialTreatment_Concession_CO()
		{
			invoiceLine.JI_ConcessionOrder = "A12345";
			invoiceLine.ZG_SecondQuota = "B12345";
			invoiceLine.JI_Procedure = "4000";
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromSpecialTerritory;
			AssertNull("Null", Provider.PreferentialTreatment);
		}

		public void TestPreferentialOriginCountry()
		{
			invoiceLine.JI_PrimaryPreference = "200";
			invoiceLine.ZG_CountryOfSupply = "AU";
			AssertEquals("AU", Provider.PreferentialOriginCountry);
		}

		public void TestPreferentialOriginCountry_NotPopulated()
		{
			invoiceLine.JI_PrimaryPreference = "199";
			invoiceLine.ZG_CountryOfSupply = "AU";
			AssertNull(Provider.PreferentialOriginCountry);
		}

		public void TestPreferentialOriginCountry_NotPopulatedDueToInvalidPrimaryPreference()
		{
			invoiceLine.JI_PrimaryPreference = "A";
			invoiceLine.ZG_CountryOfSupply = "AU";
			AssertNull(Provider.PreferentialOriginCountry);
		}

		public void TestCessionManagementFlag()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_CessionFlag = "01";
				AssertNull("Empty procedure", Provider.CessionManagementFlag);

				invoiceLine.JI_Procedure = "40005F0";
				AssertEquals("Cession flag", "01", Provider.CessionManagementFlag);
			});
		}

		public void TestAssessmentCustomsValue()
		{
			declaration.ZG_IsHighValueOvrd = false;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 502.58;
			AssertEquals("IsHighValueOvrdValidForAssessmentCustomsValue is always true", 502.58m, Provider.AssessmentCustomsValue);
		}

		public void TestAssessmentCustomsValue_DeclarationDV1()
		{
			declaration.ZG_IsHighValueOvrd = true;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 502.58;
			AssertEquals("IsHighValueOvrdValidForAssessmentCustomsValue is always true", 502.58m, Provider.AssessmentCustomsValue);
		}

		protected override IEnumerable<Expression<Func<CFCRECLineProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.PreferentialTreatment;
		}

		protected override CFCRECLineProvider GetProvider() => new CFCRECLineProvider(entryLine);

		void AssertPreferentialTreatment_Concession(ZString concession)
		{
			invoiceLine.JI_ConcessionOrder = "A12345";
			invoiceLine.ZG_SecondQuota = "B12345";
			invoiceLine.JI_Procedure = "4000" + concession;
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromEFTAMember;
			AssertNull("Null", Provider.PreferentialTreatment);
		}
	}
}
