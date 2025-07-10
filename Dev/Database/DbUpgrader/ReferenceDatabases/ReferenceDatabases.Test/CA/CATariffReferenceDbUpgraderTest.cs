using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases.CA
{
	[TestedType(typeof(CATariffReferenceDbUpgrader))]
	sealed class CATariffReferenceDbUpgraderTest : ReferenceDbUpgraderTest<CATariffReferenceDbUpgrader>
	{
		protected override string[] ExpectedReferenceTables
		{
			get
			{
				return new string[]
				{
					"CACClass",
				};
			}
		}
	}
}
