using System.Drawing;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(ColorPairRegistryDataType))]
	class ColorPairRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ColorPairRegistryDataType>
	{
		#region Implementation

		protected override ColorPairRegistryDataType GetNewDataType()
		{
			return new ColorPairRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ColorPairSelectorRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var pairSelector = new ColorPairSelector {
					PrimaryColor = Color.CornflowerBlue,
					SecondaryColor = Color.Firebrick };

			var byteArrayValue = System.Array.Empty<byte>();

			return new[] { new ValidSampleAndBinaryValueInDB(pairSelector, byteArrayValue) };
		}

		#endregion
	}
}
