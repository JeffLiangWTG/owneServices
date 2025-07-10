using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(ITCustomsNumberViewStmNumsSetting))]
sealed class ITCustomsNumberViewStmNumsSettingTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
{
	public void TestSettings()
	{
		var codes = new[]
		{
			NumberRangeTypeList.Codes.EntrySummaryDeclaration,
			NumberRangeTypeList.Codes.EntrySummaryDeclarationAmendment,
			NumberRangeTypeList.Codes.EntrySummaryDeclarationDiversion,
			NumberRangeTypeList.Codes.ExitSummaryDeclaration,
			NumberRangeTypeList.Codes.ExitSummaryDeclarationAmendment
		};

		foreach (ICodeDescription pair in new NumberRangeTypeList())
		{
			var setting = new ITCustomsNumberViewStmNumsSetting(Company, pair.Code);
			CombineAssertions(() =>
			{
				AssertEquals("GetThresholdRunOutWarning()", 100L, setting.GetThresholdRunOutWarning());
				if (codes.Contains(pair.Code))
				{
					AssertEquals("Code not contained: RequiredDigit()", 8, setting.RequiredDigit());
					AssertEquals("Code not contained: DefaultTypeRangeMax()", 99999999L, setting.DefaultTypeRangeMax());
				}
				else
				{
					AssertEquals("Contained code: RequiredDigit()", 6, setting.RequiredDigit());
					AssertEquals("Contained code: DefaultTypeRangeMax()", 999999L, setting.DefaultTypeRangeMax());
				}
			});
		}
	}

	GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
	GlbCompany company;

	protected override BusinessObject GetNewBusinessObject()
	{
		return new ITCustomsNumberViewStmNumsSetting(Company, NumberRangeTypeList.Codes.EntrySummaryDeclaration);
	}
}
