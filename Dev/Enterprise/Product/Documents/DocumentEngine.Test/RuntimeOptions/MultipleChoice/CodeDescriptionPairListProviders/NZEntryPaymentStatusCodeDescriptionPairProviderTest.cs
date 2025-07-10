using CargoWise.Application;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class NZEntryPaymentStatusCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new NZEntryPaymentStatusCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			CodeDescriptionPairList eCIConsignmentStatusPairList = (CodeDescriptionPairList)ObjectFactory.Get<Enterprise.Integration.Customs.NZ.ILowValueConsignmentStatusList>();
			CodeDescriptionPairList formalEntryStatusPairList = (CodeDescriptionPairList)ObjectFactory.Get<Enterprise.Integration.Customs.NZ.IFormalEntryStatusList>();
			CodeDescriptionPairList nZEntryPaymentStatusCode = new CodeDescriptionPairList(eCIConsignmentStatusPairList + formalEntryStatusPairList);

			AssertEquals(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList().GetType(), nZEntryPaymentStatusCode.GetType());
		}
	}
}
