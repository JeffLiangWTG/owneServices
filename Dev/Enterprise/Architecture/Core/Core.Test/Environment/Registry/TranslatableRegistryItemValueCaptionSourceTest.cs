using System.Linq;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class TranslatableRegistryItemValueCaptionSourceTest : TestCase
	{
		public void TestGetRunTimeCaptionsDoNotCreateResourceString()
		{
			var registryInstance = new DummyDataRegistryTest();
			var captionSource = new TranslatableRegistryItemValueCaptionSource(registryInstance.TestCodeDescriptionPairListRegistry, registryInstance.TestCodeDescriptionPairListRegistry.Value);

			var expected = string.Join(";", registryInstance.TestCodeDescriptionPairListRegistry.Value.OfType<CodeDescriptionPair>().OrderBy(a => a.MultilingualDescription.ToString()).Select(a => a.MultilingualDescription.ToString()).ToArray());
			var actual = string.Join(";", captionSource.GetRuntimeCaptions().OrderBy(a => a.ToString()).Select(a => a.ToString()).ToArray());

			AssertEquals(expected, actual);
		}
	}
}
