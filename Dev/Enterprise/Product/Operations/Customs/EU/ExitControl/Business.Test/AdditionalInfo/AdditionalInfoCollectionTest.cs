using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(AdditionalInfoCollection<AdditionalInfo>))]
	sealed class AdditionalInfoCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<AdditionalInfo>
	{
		protected override Customs.Business.CusSupportingInfoCollection<AdditionalInfo> GetCusSupportingInfoCollection()
		{
			(var report, _, _) = CusExitReportItemTest.GetNewBusinessObject(Factory);
			return new AdditionalInfoCollection<AdditionalInfo>(report);
		}
	}
}
