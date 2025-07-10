using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(UniversalReferenceConstantsTest))]
sealed class UniversalReferenceConstantsTest : TestCase
{
	public void TestDuplicateConstants()
	{
		IEnumerable<FieldInfo> fields(Type type) => type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.IsLiteral && f.FieldType == typeof(string));

		var ignoredClasses = Array.Empty<(Type, Type)>();

		var duplicatesFound = false;
		var classes = typeof(UniversalReferenceConstants).GetNestedTypes(BindingFlags.Public);

		CombineAssertions(() =>
		{
			for (var c1 = 0; c1 < classes.Length; c1++)
			{
				var class1 = classes[c1];
				for (var c2 = c1 + 1; c2 < classes.Length; c2++)
				{
					var class2 = classes[c2];
					if (!ignoredClasses.Contains((class1, class2)))
					{
						foreach (var field1 in fields(class1))
						{
							foreach (var field2 in fields(class2))
							{
								if (field1.Name == field2.Name)
								{
									var value1 = (string)field1.GetValue(null);
									var value2 = (string)field2.GetValue(null);
									if (value1 == value2)
									{
										if (!duplicatesFound)
										{
											Fail($@"If false duplicates are reported add them to ""{nameof(ignoredClasses)}"" in {nameof(UniversalReferenceConstantsTest)}.{nameof(TestDuplicateConstants)}");
											duplicatesFound = true;
										}
										Fail($@"Constant {field1.Name}=""{value1}"" defined by {class1.Name} and {class2.Name}");
									}
								}
							}
						}
					}
				}
			}
			Assert("Duplicates found", !duplicatesFound);
		});
	}

	public void TestProcedureCodesEdec_IsReturnedGoods()
	{
		AssertIsCodeSet(typeof(ProcedureCodesEdec), ProcedureCodesEdec.IsReturnedGoods, ProcedureCodesEdec.ReturnedGoods, ProcedureCodesEdec.ReturnedGoodsVAT);
	}

	public void TestProcedureCodesEdec_IsWithoutDuty()
	{
		AssertIsCodeSet(typeof(ProcedureCodesEdec), ProcedureCodesEdec.IsWithoutDuty, ProcedureCodesEdec.ReturnedGoods, ProcedureCodesEdec.ReturnedGoodsVAT, ProcedureCodesEdec.ExemptFromDuty);
	}

	public void TestProcedureCodesEdec_IsCustomsRelief()
	{
		AssertIsCodeSet(typeof(ProcedureCodesEdec), ProcedureCodesEdec.IsCustomsRelief, ProcedureCodesEdec.ReturnedGoods, ProcedureCodesEdec.ReturnedGoodsVAT, ProcedureCodesEdec.RepairTransportation, ProcedureCodesEdec.RefinementTransportation, ProcedureCodesEdec.CustomsRelief);
	}

	public void TestProcedureCodesEdec_IsRepairOrRefinement()
	{
		AssertIsCodeSet(typeof(ProcedureCodesEdec), ProcedureCodesEdec.IsRepairOrRefinement, ProcedureCodesEdec.RepairTransportation, ProcedureCodesEdec.RefinementTransportation);
	}

	public void TestTariffNumbers_IsCigarsTariffCode()
	{
		AssertIsCodeSet(typeof(TariffNumbers), TariffNumbers.IsCigarsTariffCode, CigarsTariffCodeList);
	}

	public void TestTariffNumbers_IsIndustrialManufactureTariffCode()
	{
		AssertIsCodeSet(typeof(TariffNumbers), TariffNumbers.IsIndustrialManufactureTariffCode, IndustrialManufactureTariffCodeList);
	}

	public void TestTariffNumbers_IsChewingRollingOtherTobacco()
	{
		AssertIsCodeSet(typeof(TariffNumbers), TariffNumbers.IsChewingRollingOtherTobacco, ChewingRollingOtherTobaccoList);
	}

	public void TestTariffStatisticalCodes_IsSnuffTobaccoStatisticalCode()
	{
		AssertIsCodeSet(typeof(TariffStatisticalCodes), TariffStatisticalCodes.IsSnuffTobaccoStatisticalCode, TariffStatisticalCodes.SnuffCigars, TariffStatisticalCodes.SnuffCigarettes, TariffStatisticalCodes.SnuffSmokingTobacco);
	}

	void AssertIsCodeSet(Type type, Func<string, bool> function, params string[] expectedTrue)
	{
		foreach (var code in type.GetConstantValues())
		{
			AssertEquals($"code={code}", expectedTrue.Contains(code), function(code));
		}
	}

	public void TestAdditionalTaxesIsForcedManualRate() => CombineAssertions(() =>
	{
		AssertIsForcedManualRate(true, "450-001");
		AssertIsForcedManualRate(true, "450-009");
		AssertIsForcedManualRate(false, "450-010");
		AssertIsForcedManualRate(true, "450-201");
		AssertIsForcedManualRate(true, "450-209");
		AssertIsForcedManualRate(false, "450-219");

		void AssertIsForcedManualRate(bool expectedResult, string tariffNumber)
		{
			AssertEquals(tariffNumber, expectedResult, AdditionalTaxesTariffs.IsForcedManualRate(tariffNumber));
		}
	});

	string[] CigarsTariffCodeList => new string[] { TariffNumbers.CigarCherootsCigarillosContainingTobacco, TariffNumbers.CigarettesContainingTobaccoMoreThan
			, TariffNumbers.CigarettesContainingTobaccoNotMoreThan, TariffNumbers.CigarCherootsCigarillosOthers, TariffNumbers.WaterPipeTobaccoSpecifiedInSubheading,
			TariffNumbers.SmokingTobaccoOther, TariffNumbers.ChewingTobaccoRollTobaccoAndSnuff, TariffNumbers.OtherManufacturedTobaccoOtherOther };

	string[] IndustrialManufactureTariffCodeList => new string[]  { TariffNumbers.ForIndustrialManufacture, TariffNumbers.ForIndustrialManufacturePartlyStemmed,
				TariffNumbers.ForIndustrialManufactureTobaccoRefuse };

	string[] ChewingRollingOtherTobaccoList => new string[] { TariffNumbers.HomogenisedTobacco, TariffNumbers.ExpandedTobacco };
}
