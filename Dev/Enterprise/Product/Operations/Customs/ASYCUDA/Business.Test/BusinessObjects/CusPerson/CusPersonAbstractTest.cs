using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(CusPerson))]
	public abstract class CusPersonAbstractTest<TCusPersonCountry> : Customs.Business.Testing.CusPersonAbstractTest<TCusPersonCountry>
		where TCusPersonCountry : CusPersonCountry
	{
		public override void TestCountriesType()
		{
			var person = (CusPerson)GetNewBusinessObject();

			AssertEquals("CusPersonCountryCollection type", typeof(CusPersonCountryCollection<TCusPersonCountry>), person.Countries.GetType());
		}
	}
}
