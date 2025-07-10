using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.CDS.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(SupplementaryCodeProviderCDS))]
	class SupplementaryCodeProviderCDSTest : BaseSupplementaryCodeProviderTest<SupplementaryCodeProviderCDS, SupplementaryCode>
	{
		protected override ZString CountryCodeForSupplementaryCodeProvider => "GBCDS";

		protected override ZShort ExpectedAdditionalSupplementaryCodesStartingOrder => 3;

		protected override ZShort ExpectedNumberOfAdditionalSupplementaryCodes => 32767;

		protected override Type ExpectedGetNewValidationReturnType => typeof(SupplementaryCodeValidation);

		protected override Type ExpectedGetNewLookupsReturnType => typeof(BaseSupplementaryCodeLookups);
	}
}
