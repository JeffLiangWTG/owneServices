using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MasterFiles.Testing
{
	[TestedType(typeof(SupplementaryCodeProvider))]
	sealed class SupplementaryCodeProviderTest : BaseSupplementaryCodeProviderTest<SupplementaryCodeProvider, SupplementaryCode>
	{
		public void TestValidation()
		{
			var supplementaryCode = Factory.New<SupplementaryCode>();
			AssertType<SupplementaryCodeValidation>(supplementaryCode.Validation);
		}

		protected override ZShort ExpectedAdditionalSupplementaryCodesStartingOrder => 3;

		protected override ZShort ExpectedNumberOfAdditionalSupplementaryCodes => 8;

		protected override Type ExpectedGetNewValidationReturnType => typeof(SupplementaryCodeValidation);

		protected override Type ExpectedGetNewLookupsReturnType => typeof(BaseSupplementaryCodeLookups);
	}
}
