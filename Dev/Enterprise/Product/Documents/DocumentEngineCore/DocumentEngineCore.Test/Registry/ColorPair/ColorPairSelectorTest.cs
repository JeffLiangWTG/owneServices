using System.Drawing;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(ColorPairSelector))]
	public class ColorPairSelectorTest : RegistryBusinessObjectTemplateTestCase<ColorPairSelector>
	{
		public void TestDefaultIsSet()
		{
			var selector = new ColorPairSelector();
			AssertColorEquals("selector.PrimaryColor", Color.White, selector.PrimaryColor);
			AssertColorEquals("selector.SecondaryColor", Color.Black, selector.SecondaryColor);
		}

		#region Implementation
		protected override void CheckAllPropertiesAreEqual(ColorPairSelector pair1, ColorPairSelector pair2, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(pair1, pair2, isClone);
			AssertColorEquals("PrimaryColor", pair1.PrimaryColor, pair2.PrimaryColor);
			AssertColorEquals("SecondaryColor", pair1.SecondaryColor, pair2.SecondaryColor);
		}

		protected override ColorPairSelector GetBusinessObjectToClone()
		{
			var result = new ColorPairSelector {
				PrimaryColor = Color.CornflowerBlue,
				SecondaryColor = Color.Firebrick };
			return result;
		}

		protected override ColorPairSelector GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
		#endregion
	}
}
