using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class CommissionStreamCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new CommissionStreamCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			var commissionStreams = new CodeDescriptionBoolCollection();
			commissionStreams.Add("AAA", (NoResString)"AAA Description");
			commissionStreams.Add("BBB", (NoResString)"BBB Description");
			commissionStreams.Add("CCC", (NoResString)"CCC Description");
			OrganisationRegistry.Instance.CommissionAgreementStreams.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, commissionStreams);

			var expected = new CodeDescriptionPairList();
			expected.AddPair(string.Empty, (NoResString)"No Stream");
			expected.AddRange(commissionStreams);
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), expected);
		}
	}
}
