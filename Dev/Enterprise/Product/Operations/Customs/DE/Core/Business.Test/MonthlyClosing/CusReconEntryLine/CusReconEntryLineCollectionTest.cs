using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusReconEntryLineCollection))]
	class CusReconEntryLineCollectionTest : ActiveBusinessObjectCollectionTestCase<CusReconEntryLineCollection>
	{
		protected override CusReconEntryLineCollection GetCollectionToTest()
		{
			var reconEntry = Factory.New<CusReconEntry>();
			return new CusReconEntryLineCollection(reconEntry);
		}
	}
}
