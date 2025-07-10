using CargoWise.Customs.Shared.MessageContracts;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ManufacturerIndicatorListTest : TestCase
	{
		public void TestMapToCustomsCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("The Manufacturer is the Supplier", "EXPORTADOR_IGUAL_FABRICANTE", ManufacturerIndicatorList.MapToCustomsCode(ManufacturerIndicatorList.Codes._1));
				AssertEquals("The Manufacturer is not the Supplier", "EXPORTADOR_DIFERENTE_FABRICANTE", ManufacturerIndicatorList.MapToCustomsCode(ManufacturerIndicatorList.Codes._2));
				AssertEquals("The Manufacturer is unknown", "EXPORTADOR_DIFERENTE_FABRICANTE", ManufacturerIndicatorList.MapToCustomsCode(ManufacturerIndicatorList.Codes._3));
				Assert("When code not found, should be Empty", ManufacturerIndicatorList.MapToCustomsCode("X").IsEmpty());
			});
		}
	}
}
