using CargoWise.ComponentModel;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class ABadClass
	{
		// CW1164 Do not use string literals in ListAttribute.
		[List("Lookups.SomePropertyValues")]
		public string SomeProperty { get; set; }

		public LookupClass Lookups { get; set; }
	}

	class LookupClass
	{
		public string SomePropertyValues { get; set; }
	}
}
