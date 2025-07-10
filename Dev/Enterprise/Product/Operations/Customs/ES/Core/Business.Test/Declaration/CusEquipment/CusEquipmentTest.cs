using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(CusEquipment))]
	class CusEquipmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			AssertType<CusEquipmentValidation>(Factory.New<CusEquipment>().Validation);
		}
	}
}
