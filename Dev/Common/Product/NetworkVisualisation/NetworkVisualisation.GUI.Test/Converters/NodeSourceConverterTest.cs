
using NUnit.Framework;
namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(NodeSourceConverter))]
	class NodeSourceConverterTest : MultiValueConverterTestCase<NodeSourceConverter>
	{
		public override void TestConvert()
		{
			var converter = new NodeSourceConverter();
			var value1 = new object();
			var value2 = new object();

			var result = converter.Convert(new[] { value1, value2, false }, null, null, null);
			AssertEquals(value1, result);

			result = converter.Convert(new[] { value1, value2, true }, null, null, null);
			AssertEquals(value2, result);
		}
	}
}
