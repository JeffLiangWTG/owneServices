using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusReconEntryCollection))]
	public class CusReconEntryCollectionTest : ActiveBusinessObjectCollectionTestCase<CusReconEntryCollection>
	{
		protected override CusReconEntryCollection GetCollectionToTest()
		{
			var declaration = Factory.New<CusReconDeclaration>();
			return new CusReconEntryCollection(declaration);
		}
	}
}
