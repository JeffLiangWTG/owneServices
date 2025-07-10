using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(NodeStatusImageConverter))]
	class NodeStatusImageConverterTest : ValueConverterTestCase<NodeStatusImageConverter>
	{
		public override void TestConvert()
		{
			AssertConvertResult(o => o != null, WorkStatus.Cancelled);
			AssertConvertResult(o => o != null, WorkStatus.Complete);
			AssertConvertResult(o => o != null, WorkStatus.Startable);
			AssertConvertResult(o => o != null, WorkStatus.Suspended);
			AssertConvertResult(o => o != null, WorkStatus.Working);

			AssertConvertResult(o => o == null, WorkStatus.Blocked);
			AssertConvertResult(o => o == null, EntityState.None);
		}
	}
}
