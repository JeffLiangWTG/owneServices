using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(WebRefUNLOCOFilterBusinessObject))]
	sealed class WebRefUNLOCOFilterBusinessObjectTest : FilterBusinessObjectTestCase
	{
		public void TestFilter_DoesNotExceedMaxLengths()
		{
			var filter = (WebRefUNLOCOFilterBusinessObject)GetNewBusinessObject();
			var expectedCode = new string('A', AutoRefUNLOCO.Schema.RL_CodeMaxLength);
			var expectedIATA = new string('B', AutoRefUNLOCO.Schema.RL_IATAMaxLength);
			var expectedPortName = new string('C', AutoRefUNLOCO.Schema.RL_PortNameMaxLength);

			filter.RL_Code = expectedCode + "A";
			filter.RL_IATA = expectedIATA + "B";
			filter.RL_PortName = expectedPortName + "C";

			var expectedFilter = $"{AutoRefUNLOCO.Schema.RL_PortName} = '{expectedPortName}' and {AutoRefUNLOCO.Schema.RL_IATA} = '{expectedIATA}' and {AutoRefUNLOCO.Schema.RL_Code} = '{expectedCode}'";
			AssertEquals(expectedFilter, filter.Filter.LiteralTextADO);
		}
	}
}
