using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases.NZ.Testing
{
	[TestedType(typeof(NZTariffReferenceDbUpgrader))]
	sealed class NZTariffReferenceDbUpgraderTest : ReferenceDbUpgraderTest<NZTariffReferenceDbUpgrader>
	{
		protected override string[] ExpectedReferenceTables
		{
			get
			{
				return new string[]
				{
					"NZCClassification",
					"NZCClassificationChapter",
					"NZCClassificationDutyRate",
					"NZCClassificationLevyRate",
					"NZCClassificationSection",
					"NZCConcession",
					"NZCConcessionClassificationLink",
					"NZCConcessionDutyRate",
					"NZCCountry",
					"NZCCountryGroup",
					"NZCCustomsExchangeRate",
					"NZCGroup",
					"NZCSupplier",
					"NZCTariffsPermitsApplyTo",
				};
			}
		}
	}
}
