using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	[TestedType(typeof(MXInterchange))]
	public class MXInterchangeTest : EDIInterchangeTest
	{
		public void TestProperties()
		{
			var interchange = Factory.New<MXInterchange>();
			AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.MXCustoms, interchange.EI_ApplicationCode);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<MXInterchange>();
	}
}
