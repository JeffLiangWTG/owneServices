
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(EntityStateImageConverter))]
	class EntityStateImageConverterTest : ValueConverterTestCase<EntityStateImageConverter>
	{
		public override void TestConvert()
		{
			AssertConvertResult(o => o != null, EntityState.Approved);
			AssertConvertResult(o => o != null, EntityState.NotApproved);
			AssertConvertResult(o => o != null, EntityState.Fixed);

			AssertConvertResult(o => o == null, EntityState.None);
		}
	}
}
