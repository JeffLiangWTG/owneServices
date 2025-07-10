using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusReconCustomsChargeCollection))]
	sealed class CusReconCustomsChargeCollectionTest : ActiveBusinessObjectCollectionTestCase<CusReconCustomsChargeCollection>
	{
		protected override CusReconCustomsChargeCollection GetCollectionToTest() => new CusReconCustomsChargeCollection(ReconEntryLine);
		CusReconEntryLine ReconEntryLine => reconEntryLine ?? (reconEntryLine = Factory.New<CusReconEntryLine>());
		CusReconEntryLine reconEntryLine;
	}
}
