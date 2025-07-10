using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class GuaranteeForEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLiabilityApplicablePercentageList()
		{
			var lookups = Factory.New<GuaranteeForEntryInstruction>().Lookups;
			AssertEquals($"{nameof(lookups.LiabilityApplicablePercentageList)} values", "FUL, HAL, THI, ZER", lookups.LiabilityApplicablePercentageList.CodesAsString);
		}

		public void TestReferenceNumbers_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				var guaranteeHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				guaranteeHeader1.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
				guaranteeHeader1.CPH_Number = "GUARANTEE1";

				var guaranteeHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				guaranteeHeader2.CPH_Type = EUGuaranteeTypeList.Codes.IMP;
				guaranteeHeader2.CPH_Number = "GUARANTEE2";

				var guaranteeHeader3 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				guaranteeHeader3.CPH_Type = EUGuaranteeTypeList.Codes.COD;
				guaranteeHeader3.CPH_Number = "GUARANTEE3";
				var g3Reference = guaranteeHeader3.AdditionalGuaranteeReferences.AddNew();
				g3Reference.CY_Code = AdditionalCustomsReferenceTypeList.Codes.H1;

				var guaranteeHeader4 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				guaranteeHeader4.CPH_Type = EUGuaranteeTypeList.Codes.COD;
				guaranteeHeader4.CPH_Number = "GUARANTEE4";
				var g4Reference = guaranteeHeader4.AdditionalGuaranteeReferences.AddNew();
				g4Reference.CY_Code = AdditionalCustomsReferenceTypeList.Codes.H2;

				Factory.Save();

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "H1";
				var guarantee = entryInstruction.Guarantees.AddNew();

				var referenceNumbers = guarantee.Lookups.ReferenceNumbers;
				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("Retrieved guarantee types", new List<ZString> { "GUARANTEE2", "GUARANTEE3", }, referenceNumbers.Select(x => x.CPH_Number));

					AssertEquals("TRA Should not match filter", false, guaranteeHeader1.MatchesFilter(referenceNumbers.CompleteFilter));
					AssertEquals("IMP should match filter", true, guaranteeHeader2.MatchesFilter(referenceNumbers.CompleteFilter));
					AssertEquals("COD with reference 'H1' should match filter", true, guaranteeHeader3.MatchesFilter(referenceNumbers.CompleteFilter));
					AssertEquals("COD with reference 'H2' (not equal to CEI_Style - H1) should NOT match filter", false, guaranteeHeader4.MatchesFilter(referenceNumbers.CompleteFilter));
				});
			}
		}

		public void TestReferenceNumbers_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				var guaranteeHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				guaranteeHeader1.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
				guaranteeHeader1.CPH_Number = "GUARANTEE1";

				var guaranteeHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				guaranteeHeader2.CPH_Type = EUGuaranteeTypeList.Codes.COD;
				guaranteeHeader2.CPH_Number = "GUARANTEE2";

				var guaranteeHeader3 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				guaranteeHeader3.CPH_Type = EUGuaranteeTypeList.Codes.COD;
				guaranteeHeader3.CPH_Number = "GUARANTEE3";
				var g3Reference = guaranteeHeader3.AdditionalGuaranteeReferences.AddNew();
				g3Reference.CY_Code = AdditionalCustomsReferenceTypeList.Codes.B1;

				var guaranteeHeader4 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				guaranteeHeader4.CPH_Type = EUGuaranteeTypeList.Codes.COD;
				guaranteeHeader4.CPH_Number = "GUARANTEE4";
				var g4Reference = guaranteeHeader4.AdditionalGuaranteeReferences.AddNew();
				g4Reference.CY_Code = AdditionalCustomsReferenceTypeList.Codes.B2;

				var guaranteeHeader5 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				guaranteeHeader5.CPH_Type = EUGuaranteeTypeList.Codes.COD;
				guaranteeHeader5.CPH_Number = "GUARANTEE5";
				var g5Reference = guaranteeHeader5.AdditionalGuaranteeReferences.AddNew();
				g5Reference.CY_Code = AdditionalCustomsReferenceTypeList.Codes.B3;

				Factory.Save();

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "B1";
				var guarantee = entryInstruction.Guarantees.AddNew();

				var referenceNumbers = guarantee.Lookups.ReferenceNumbers;
				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("Retrieved guarantee types", new List<ZString> { "GUARANTEE2", "GUARANTEE3", }, referenceNumbers.Select(x => x.CPH_Number));

					AssertEquals("TRA Should not match filter", false, Factory.Load<CusGuaranteeHeader>(guaranteeHeader1.PK).MatchesFilter(referenceNumbers.CompleteFilter));
					AssertEquals("COD should match filter", true, Factory.Load<CusGuaranteeHeader>(guaranteeHeader2.PK).MatchesFilter(referenceNumbers.CompleteFilter));
					AssertEquals("COD with reference 'B1' should match filter", true, Factory.Load<CusGuaranteeHeader>(guaranteeHeader3.PK).MatchesFilter(referenceNumbers.CompleteFilter));
					AssertEquals("COD with reference 'B2' (not equal to CEI_Style - B1) should NOT match filter", false, Factory.Load<CusGuaranteeHeader>(guaranteeHeader4.PK).MatchesFilter(referenceNumbers.CompleteFilter));
					AssertEquals("COD with reference 'B3' (not equal to CEI_Style - B1) should NOT match filter", false, Factory.Load<CusGuaranteeHeader>(guaranteeHeader5.PK).MatchesFilter(referenceNumbers.CompleteFilter));
				});
			}
		}
	}
}
