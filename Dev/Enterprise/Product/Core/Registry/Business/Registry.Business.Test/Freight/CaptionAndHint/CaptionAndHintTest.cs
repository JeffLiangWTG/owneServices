using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CaptionAndHint))]
	sealed class CaptionAndHintTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestConstructor()
		{
			CaptionAndHint captionAndHint = new CaptionAndHint("C", "H");
			AssertEquals("Caption", "C", captionAndHint.Caption);
			AssertEquals("Hint", "H", captionAndHint.Hint);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new CaptionAndHint("Caption", "Hint");
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
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
