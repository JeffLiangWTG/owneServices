using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(Airport))]
	public class AirportTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<Airport>();
		}

		public void TestCodeProperty()
		{
			var farp = Factory.New<Airport>();
			farp.RL_IATA = "AGI";
			AssertEquals("CodeProperty(RefUNLOCOSchema.Constants.RL_IATA) - should allow us to pull RL_IATA back from the RefUnloco lookup, not RL_Code", "AGI", CodePropertyAttribute.CodeFromBusinessObject(farp));
		}
	}
}
