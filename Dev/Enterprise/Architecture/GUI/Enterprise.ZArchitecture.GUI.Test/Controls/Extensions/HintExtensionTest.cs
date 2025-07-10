using CargoWise.Windows.UI.Testing;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions.Testing
{
	sealed class HintExtensionTest : BaseExtensionTest<HintExtension>
	{
		public void TestShortCaptionReturnsEmptyStringIfNoResourcesAvailable()
		{
			using (var emptyResourceStringCacheInitialized = Res.UseMockData())
			{
				AssertEquals(expected: true, new ResourceStringKeyCalculator(Control).DataString.IsEmpty());
				AssertEquals(string.Empty, HintExtension.ShortCaption);
			}
		}

		public void TestMediumCaptionReturnsEmptyStringIfNoResourcesAvailable()
		{
			using (var emptyResourceStringCacheInitialized = Res.UseMockData())
			{
				AssertEquals(expected: true, new ResourceStringKeyCalculator(Control).DataString.IsEmpty());
				AssertEquals(string.Empty, HintExtension.MediumCaption);
			}
		}

		public void TestCaptionReturnsOverridenCaptionIfSet()
		{
			HintExtension.Caption = "OVERRIDE_TEST";
			AssertEquals("OVERRIDE_TEST", HintExtension.Caption);
		}

		public void TestCaptionReturnsWarningStringIfNoResourcesAvailable()
		{
			using (var emptyResourceStringCacheInitialized = Res.UseMockData())
			{
				AssertEquals(expected: true, new ResourceStringKeyCalculator(Control).DataString.IsEmpty());
				AssertEquals(Extensions.HintExtension.MessageForNonDefinedResourceString, HintExtension.Caption);
			}
		}

		public void TestReturnsOverridenDescriptionIfSet()
		{
			HintExtension.Description = "TEST";
			AssertEquals("TEST", HintExtension.Description);
		}

		public void TestDescriptionReturnsWarningStringIfNoResourcesAvailable()
		{
			using (var emptyResourceStringCacheInitialized = Res.UseMockData())
			{
				ZLabelCaptionCache.Instance.ClearCache();
				AssertEquals(Extensions.HintExtension.MessageForNonDefinedResourceString, HintExtension.Description);
			}
		}

		public void TestReturnsCaptionResourceString()
		{
			using (var control = new ZTextBox() { CaptionResourceString = new ResourceStringData("X", "Cap", "Capti", "Caption", "This is my hint.") })
			{
				var hintExtension = new HintExtension();
				hintExtension.Initialize(control);
				AssertEquals("This is my hint.", hintExtension.Description);
				AssertEquals("Caption", hintExtension.Caption);
				AssertEquals("Capti", hintExtension.MediumCaption);
				AssertEquals("Cap", hintExtension.ShortCaption);
			}
		}

		public void TestCaptionStripsAcceleratorKeys()
		{
			using (var control = new ZTextBox() { CaptionResourceString = new ResourceStringData("X", "C&aption") })
			{
				var hintExtension = new HintExtension();
				hintExtension.Initialize(control);
				AssertEquals("Caption", hintExtension.Caption);
			}
		}

		#region Implementation
		HintExtension HintExtension
		{
			get
			{
				if (hintExtension == null)
				{
					hintExtension = new HintExtension();
					hintExtension.Initialize(Control);
				}
				return hintExtension;
			}
		}
		HintExtension hintExtension;

		GenericExtendedControl Control => control ?? (control = new GenericExtendedControl());
		GenericExtendedControl control;
		#endregion
	}
}
