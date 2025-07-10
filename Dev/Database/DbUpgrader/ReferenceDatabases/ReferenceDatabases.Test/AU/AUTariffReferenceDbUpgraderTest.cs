using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases.AU
{
	[TestedType(typeof(AUTariffReferenceDbUpgrader))]
	sealed class AUTariffReferenceDbUpgraderTest : ReferenceDbUpgraderTest<AUTariffReferenceDbUpgrader>
	{
		protected override string[] ExpectedReferenceTables
		{
			get
			{
				return new string[]
				{
					"AUCAHECC",
					"AUCChapter",
					"AUCClass",
					"AUCSection",
				};
			}
		}
	}
}
