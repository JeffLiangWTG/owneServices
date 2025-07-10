using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ImportIncoTermAndCustomsChargeFactoryTest : Common.Testing.IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestIncoTermAndCustomsChargeConfiguration()
		{
			ZStringBuilder zStringBuilder = new ZStringBuilder("IncoTerm,CustomsCharge,IsIncludedInInvoiceAmountFixed,GetDefaultIsIncludedInInvoice,CanThisIncoTermHaveThisCharge,IsThisChargeMandatory,IsThisChargeRecommendedForThisIncoTerm,IsIncludedInITOTReadOnlyForGroupCharge");
			var incotermFactory = new ImportIncoTermAndCustomsChargeFactory();
			var incotermChargeRelationshipConfigurations = (SortedDictionary<ZString, SortedDictionary<ZString, ChargeConfiguration>>)incotermFactory.GetIncotermChargeRelationshipConfigurations();
			foreach (var item in incotermChargeRelationshipConfigurations)
			{
				var incoterm = item.Key;
				var configurations = item.Value;

				foreach (var configuration in configurations)
				{
					var code = configuration.Key;
					var customsChargeCode = incotermFactory.GetCharge(code);

					zStringBuilder.Append($"{incoterm},{code},{incotermFactory.IsIncludedInInvoiceAmountFixed(incoterm, customsChargeCode)},{incotermFactory.GetDefaultIsIncludedInInvoice(incoterm, customsChargeCode)},{incotermFactory.CanThisIncoTermHaveThisCharge(incoterm, customsChargeCode)},{incotermFactory.IsThisChargeMandatory(incoterm, code)},{incotermFactory.IsThisChargeRecommendedForThisIncoTerm(incoterm, code)},{incotermFactory.IsIncludedInITOTReadOnlyForGroupCharge(code)}");
				}
			}
			var resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
			AssertMultilineASCIIEquals(resourceRetriever.Value.GetString(IncoTermAndCustomsChargeConfigurationFilename), zStringBuilder.ToStringWithNewLineBetweenAppends());
		}

		public override void TestGetAllIncoTerms()
		{
			var list = new ZString[] { "CFR", "CPT", "CIN", "CIF", "CIP", "DAF", "DAP", "DAT", "DDP", "DDU", "DEQ", "DES", "DPU", "EXW", "FAS", "FCA", "FOB" };
			var incoTerms = incoTermAndChargeFactory.GetAllIncoTerms();
			AssertEquals("Count", 17, incoTerms.Length);
			AssertEquals("Count", 17, incoTerms.Where(x => list.Contains(x)).Count());
		}

		public override void TestGetAllCharges()
		{
			SetupData();
			AssertEquals("There should be 3 charges", 38, charges.Length);

			AssertNotNull("Code 'A102' exists", a102);
			AssertNotNull("Code 'A104' exists", a104);
			AssertNotNull("Code 'A105' exists", a105);
			AssertNotNull("Code 'A106' exists", a106);
			AssertNotNull("Code 'A107' exists", a107);
			AssertNotNull("Code 'A108' exists", a108);
			AssertNotNull("Code 'A109' exists", a109);
			AssertNotNull("Code 'A110' exists", a110);
			AssertNotNull("Code 'A111' exists", a111);
			AssertNotNull("Code 'A112' exists", a112);
			AssertNotNull("Code 'A114' exists", a114);
			AssertNotNull("Code 'A115' exists", a115);
			AssertNotNull("Code 'A116' exists", a116);
			AssertNotNull("Code 'A118' exists", a118);
			AssertNotNull("Code 'A119' exists", a119);
			AssertNotNull("Code 'A120' exists", a120);
			AssertNotNull("Code 'A121' exists", a121);

			AssertNotNull("Code 'B303' exists", b303);
			AssertNotNull("Code 'B304' exists", b304);
			AssertNotNull("Code 'B305' exists", b305);
			AssertNotNull("Code 'B306' exists", b306);
			AssertNotNull("Code 'B307' exists", b307);
			AssertNotNull("Code 'B309' exists", b309);
			AssertNotNull("Code 'B310' exists", b310);
			AssertNotNull("Code 'B311' exists", b311);
			AssertNotNull("Code 'B312' exists", b312);
			AssertNotNull("Code 'B313' exists", b313);

			AssertNotNull("Code 'B404' exists", b404);
			AssertNotNull("Code 'B405' exists", b405);
			AssertNotNull("Code 'B406' exists", b406);
			AssertNotNull("Code 'B407' exists", b407);
			AssertNotNull("Code 'B408' exists", b408);
			AssertNotNull("Code 'B409' exists", b409);
			AssertNotNull("Code 'B410' exists", b410);
			AssertNotNull("Code 'B411' exists", b411);

			AssertNotNull("Code 'B501' exists", b501);
			AssertNotNull("Code 'B502' exists", b502);
			AssertNotNull("Code 'B503' exists", b503);
		}

		public override void TestGetCharge()
		{
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A102, CustomsChargeCodeProvider.A102);
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A104, CustomsChargeCodeProvider.A104);
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A105, CustomsChargeCodeProvider.A105);
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A106, CustomsChargeCodeProvider.A106);
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A107, CustomsChargeCodeProvider.A107);
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A108, CustomsChargeCodeProvider.A108);
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A109, CustomsChargeCodeProvider.A109);
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A110, CustomsChargeCodeProvider.A110);
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A111, CustomsChargeCodeProvider.A111);
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A112, CustomsChargeCodeProvider.A112);
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A114, CustomsChargeCodeProvider.A114);
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A115, CustomsChargeCodeProvider.A115);
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A116, CustomsChargeCodeProvider.A116);
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A118, CustomsChargeCodeProvider.A118);
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A119, CustomsChargeCodeProvider.A119);
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A120, CustomsChargeCodeProvider.A120);
			AssertGetCharge(ImportChargeMethodOneCodeList.Codes.A121, CustomsChargeCodeProvider.A121);

			AssertGetCharge(ImportChargeMethodTwoAndThreeCodeList.Codes.B303, CustomsChargeCodeProvider.B303);
			AssertGetCharge(ImportChargeMethodTwoAndThreeCodeList.Codes.B304, CustomsChargeCodeProvider.B304);
			AssertGetCharge(ImportChargeMethodTwoAndThreeCodeList.Codes.B305, CustomsChargeCodeProvider.B305);
			AssertGetCharge(ImportChargeMethodTwoAndThreeCodeList.Codes.B306, CustomsChargeCodeProvider.B306);
			AssertGetCharge(ImportChargeMethodTwoAndThreeCodeList.Codes.B307, CustomsChargeCodeProvider.B307);
			AssertGetCharge(ImportChargeMethodTwoAndThreeCodeList.Codes.B309, CustomsChargeCodeProvider.B309);
			AssertGetCharge(ImportChargeMethodTwoAndThreeCodeList.Codes.B310, CustomsChargeCodeProvider.B310);
			AssertGetCharge(ImportChargeMethodTwoAndThreeCodeList.Codes.B311, CustomsChargeCodeProvider.B311);
			AssertGetCharge(ImportChargeMethodTwoAndThreeCodeList.Codes.B312, CustomsChargeCodeProvider.B312);
			AssertGetCharge(ImportChargeMethodTwoAndThreeCodeList.Codes.B313, CustomsChargeCodeProvider.B313);

			AssertGetCharge(ImportChargeMethodFourCodeList.Codes.B404, CustomsChargeCodeProvider.B404);
			AssertGetCharge(ImportChargeMethodFourCodeList.Codes.B405, CustomsChargeCodeProvider.B405);
			AssertGetCharge(ImportChargeMethodFourCodeList.Codes.B406, CustomsChargeCodeProvider.B406);
			AssertGetCharge(ImportChargeMethodFourCodeList.Codes.B407, CustomsChargeCodeProvider.B407);
			AssertGetCharge(ImportChargeMethodFourCodeList.Codes.B408, CustomsChargeCodeProvider.B408);
			AssertGetCharge(ImportChargeMethodFourCodeList.Codes.B409, CustomsChargeCodeProvider.B409);
			AssertGetCharge(ImportChargeMethodFourCodeList.Codes.B410, CustomsChargeCodeProvider.B410);
			AssertGetCharge(ImportChargeMethodFourCodeList.Codes.B411, CustomsChargeCodeProvider.B411);

			AssertGetCharge(ImportChargeMethodFiveAndSixCodeList.Codes.B501, CustomsChargeCodeProvider.B501);
			AssertGetCharge(ImportChargeMethodFiveAndSixCodeList.Codes.B502, CustomsChargeCodeProvider.B502);
			AssertGetCharge(ImportChargeMethodFiveAndSixCodeList.Codes.B503, CustomsChargeCodeProvider.B503);
		}

		public void TestChargesProperties()
		{
			SetupData();
			AssertChargeCode(a102, true, true, true, true);
			AssertChargeCode(a104, true, true, true, false);
			AssertChargeCode(a105, true, true, true, true);
			AssertChargeCode(a106, true, true, true, true);
			AssertChargeCode(a107, true, true, true, true);
			AssertChargeCode(a108, true, true, true, true);
			AssertChargeCode(a109, true, true, true, true);
			AssertChargeCode(a110, true, true, true, true);
			AssertChargeCode(a111, true, true, true, true);
			AssertChargeCode(a112, true, true, true, true);
			AssertChargeCode(a114, true, true, false, true);
			AssertChargeCode(a115, true, true, true, true);
			AssertChargeCode(a116, true, true, false, true);
			AssertChargeCode(a118, false, true, true, true);
			AssertChargeCode(a119, false, true, true, true);
			AssertChargeCode(a120, false, true, true, true);
			AssertChargeCode(a121, false, true, true, true);

			AssertChargeCode(b303, false, true, true, true);
			AssertChargeCode(b304, false, true, true, true);
			AssertChargeCode(b305, false, true, true, true);
			AssertChargeCode(b306, false, true, true, true);
			AssertChargeCode(b307, false, true, true, true);
			AssertChargeCode(b309, true, true, true, true);
			AssertChargeCode(b310, true, true, true, false);
			AssertChargeCode(b311, true, true, false, true);
			AssertChargeCode(b312, true, true, true, true);
			AssertChargeCode(b313, true, true, false, true);

			AssertChargeCode(b404, false, true, true, false);
			AssertChargeCode(b405, false, true, true, false);
			AssertChargeCode(b406, false, true, true, false);
			AssertChargeCode(b407, false, true, true, false);
			AssertChargeCode(b408, false, true, true, false);
			AssertChargeCode(b409, false, true, true, false);
			AssertChargeCode(b410, false, true, true, false);
			AssertChargeCode(b411, false, true, true, false);

			AssertChargeCode(b501, true, true, false, false);
			AssertChargeCode(b502, true, true, true, false);
			AssertChargeCode(b503, true, true, false, false);

			void AssertChargeCode(ICustomsChargeCode chargeCode, bool isDutiable, bool isDutiableReadOnly, bool isIncoTermNeutral, bool isPercentageApplicable)
			{
				AssertEquals("IsDutiable", isDutiable, chargeCode.IsDutiable);
				AssertEquals("IsDutiable ReadOnly", isDutiableReadOnly, chargeCode.IsDutiableDeemedForThisCharge);

				AssertEquals("IsIncoTermNeutral", isIncoTermNeutral, chargeCode.IsIncoTermNeutral);
				AssertEquals("IsPercentageApplicable", isPercentageApplicable, chargeCode.IsPercentageApplicable);
			}
		}

		void SetupData()
		{
			charges = incoTermAndChargeFactory.GetAllCharges();

			a102 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A102);
			a104 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A104);
			a105 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A105);
			a106 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A106);
			a107 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A107);
			a108 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A108);
			a109 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A109);
			a110 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A110);
			a111 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A111);
			a112 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A112);
			a114 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A114);
			a115 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A115);
			a116 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A116);
			a118 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A118);
			a119 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A119);
			a120 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A120);
			a121 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodOneCodeList.Codes.A121);

			b303 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodTwoAndThreeCodeList.Codes.B303);
			b304 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodTwoAndThreeCodeList.Codes.B304);
			b305 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodTwoAndThreeCodeList.Codes.B305);
			b306 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodTwoAndThreeCodeList.Codes.B306);
			b307 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodTwoAndThreeCodeList.Codes.B307);
			b309 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodTwoAndThreeCodeList.Codes.B309);
			b310 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodTwoAndThreeCodeList.Codes.B310);
			b311 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodTwoAndThreeCodeList.Codes.B311);
			b312 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodTwoAndThreeCodeList.Codes.B312);
			b313 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodTwoAndThreeCodeList.Codes.B313);

			b404 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodFourCodeList.Codes.B404);
			b405 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodFourCodeList.Codes.B405);
			b406 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodFourCodeList.Codes.B406);
			b407 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodFourCodeList.Codes.B407);
			b408 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodFourCodeList.Codes.B408);
			b409 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodFourCodeList.Codes.B409);
			b410 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodFourCodeList.Codes.B410);
			b411 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodFourCodeList.Codes.B411);

			b501 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodFiveAndSixCodeList.Codes.B501);
			b502 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodFiveAndSixCodeList.Codes.B502);
			b503 = charges.FirstOrDefault(x => x.Code == ImportChargeMethodFiveAndSixCodeList.Codes.B503);
		}

		ICustomsChargeCode[] charges;
		ICustomsChargeCode a102;
		ICustomsChargeCode a104;
		ICustomsChargeCode a105;
		ICustomsChargeCode a106;
		ICustomsChargeCode a107;
		ICustomsChargeCode a108;
		ICustomsChargeCode a109;
		ICustomsChargeCode a110;
		ICustomsChargeCode a111;
		ICustomsChargeCode a112;
		ICustomsChargeCode a114;
		ICustomsChargeCode a115;
		ICustomsChargeCode a116;
		ICustomsChargeCode a118;
		ICustomsChargeCode a119;
		ICustomsChargeCode a120;
		ICustomsChargeCode a121;
		ICustomsChargeCode b303;
		ICustomsChargeCode b304;
		ICustomsChargeCode b305;
		ICustomsChargeCode b306;
		ICustomsChargeCode b307;
		ICustomsChargeCode b309;
		ICustomsChargeCode b310;
		ICustomsChargeCode b311;
		ICustomsChargeCode b312;
		ICustomsChargeCode b313;
		ICustomsChargeCode b404;
		ICustomsChargeCode b405;
		ICustomsChargeCode b406;
		ICustomsChargeCode b407;
		ICustomsChargeCode b408;
		ICustomsChargeCode b409;
		ICustomsChargeCode b410;
		ICustomsChargeCode b411;
		ICustomsChargeCode b501;
		ICustomsChargeCode b502;
		ICustomsChargeCode b503;

		protected override string IncoTermAndCustomsChargeConfigurationFilename => "Enterprise.Customs.KR.Business.Testing.Business.JobComInvHeaderCharge.IncoTermAndCustomsChargeFactory.TestFile.ImportIncoTermAndCustomsChargeConfiguration.csv";
		protected override string GetCountryContext() => Core.Constants.CountryCodes.KoreaSouth + Common.Shared.SharedJobMessageTypeList.Codes.Import;
	}
}
