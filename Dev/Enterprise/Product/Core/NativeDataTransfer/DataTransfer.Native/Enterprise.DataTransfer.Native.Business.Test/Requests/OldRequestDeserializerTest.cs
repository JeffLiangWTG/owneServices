using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	public class OldRequestDeserializerTest : TestCase
	{
		public void TestDeserialize()
		{
			var element = new XElement("JobOrderHeader");
			var deserializer = new OldRequestDeserializer();
			var input = new XElement("Order",
				element, element, element, new XElement("OwnerOrg", "LEOTEST"));

			var result = deserializer.Deserialize(input);
			var setting = result.Settings;
			var entitySets = result.EntitySets;
			AssertEquals(3, entitySets.Count());
			AssertEquals("LEOTEST", setting.OwnerCode);
		}

		public void TestOldRequestDeserializerWithEmptyRequestThrowsException()
		{
			var deserializer = new OldRequestDeserializer();
			AssertExceptionThrown<NativeXMLUserVisibleException>("This should fail.", "No valid elements were included.", () => deserializer.Deserialize(new XElement("Order")));
		}
	}
}
