using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.BrandManager;
using Enterprise.Core.Forms;
using Enterprise.Core.GUI.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class EnterpriseFormLookStrategyTest : TransactionedTestCase
	{
		public void TestFont()
		{
			using (var form = new ZForm())
			{
				AssertEquals("The correct font should be set", OFont.GetFont(), form.Font);
			}
		}

		#region Icon

		public void TestIcon()
		{
			using (var form = new ZForm())
			{
				var productIcon = BrandingFactory.Instance.ProductIcon.ToBitmap();
				var formIcon = form.Icon.ToBitmap();

				for (var x = 0; x < formIcon.Width; x++)
				{
					for (var y = 0; y < formIcon.Height; y++)
					{
						AssertPixelColor("Icon should be the enterprise logo", productIcon.GetPixel(x, y), formIcon.GetPixel(x, y));
					}
				}
			}
		}

		void AssertPixelColor(string message, Color expected, Color actual)
		{
			AssertPixelChannelWithinAllowedTolerance(message, expected.A, actual.A);
			AssertPixelChannelWithinAllowedTolerance(message, expected.R, actual.R);
			AssertPixelChannelWithinAllowedTolerance(message, expected.G, actual.G);
			AssertPixelChannelWithinAllowedTolerance(message, expected.B, actual.B);
		}

		void AssertPixelChannelWithinAllowedTolerance(string message, byte expected, byte actual)
		{
			Assert(message, expected + iconColorChannelTolerance >= actual);
			Assert(message, expected - iconColorChannelTolerance <= actual);
		}

		readonly int iconColorChannelTolerance = 2;

		#endregion

		public void TestKeyPreview()
		{
			using (var form = new ZForm())
			{
				AssertEquals("KeyPreview should be true", true, form.KeyPreview);
			}
		}

		#region TestFormPosition

#if  !WINZOR
		public void TestFormPosition()
		{
			try
			{ DoTestFormPosition(); }
			catch { DoTestFormPosition(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1063:DoNotUseSystemWindowsFormsScreen", Justification = "Used for testing only")]
		static void DoTestFormPosition()
		{
			using (var testForm = new TestZForm())
			{
				var screenWidth = Screen.GetWorkingArea(testForm).Width;  // Used for testing only
				var screenHeight = Screen.GetWorkingArea(testForm).Height;  // Used for testing only
				var testWidth = 550;

				testForm.RememberFormPosition = true;
				testForm.RememberFormSize = true;
				testForm.MinimumSize = new Size(0, 0);

				EnterpriseFormLookStrategy.SetPositionAndSize(testForm, 50, 60, testWidth, 200);
				AssertEquals("Left", 50, testForm.Left);
				AssertEquals("Top", 60, testForm.Top);
				AssertEquals("Width", testWidth, testForm.Width);
				AssertEquals("Height", 200, testForm.Height);

				EnterpriseFormLookStrategy.SetPositionAndSize(testForm, 50, 60, screenWidth + 40, screenHeight + 100);
				AssertEquals("Left", 0, testForm.Left);
				AssertEquals("Top", 0, testForm.Top);
				AssertEquals("Width", screenWidth, testForm.Width);
				AssertEquals("Height", screenHeight, testForm.Height);

				EnterpriseFormLookStrategy.SetPositionAndSize(testForm, -50, -60, testWidth, 200);
				AssertEquals("Left", (screenWidth - testForm.Width) / 2, testForm.Left);
				AssertEquals("Top", (screenHeight - testForm.Height) / 2, testForm.Top);
				AssertEquals("Width", testWidth, testForm.Width);
				AssertEquals("Height", 200, testForm.Height);

				EnterpriseFormLookStrategy.SetPositionAndSize(testForm, 0, 0, 0, 0);
				AssertEquals("Left", (screenWidth - testForm.Width) / 2, testForm.Left);
				AssertEquals("Top", (screenHeight - testForm.Height) / 2, testForm.Top);
				AssertEquals("Width", testWidth, testForm.Width);
				AssertEquals("Height", 200, testForm.Height);

				EnterpriseFormLookStrategy.SetPositionAndSize(testForm, -100, -100, screenWidth + 1000, screenHeight + 1000);
				AssertEquals(FormWindowState.Maximized, testForm.WindowState);

				testForm.WindowState = FormWindowState.Normal;
				var currentScreen = CachedScreenInfo.Instance.FromControl(testForm);
				AssertEquals("Left", currentScreen.X + ((screenWidth - testForm.Width) / 2), testForm.Left);
				AssertEquals("Top", currentScreen.Y + ((screenHeight - testForm.Height) / 2), testForm.Top);
				AssertEquals("Width", testWidth, testForm.Width);
				AssertEquals("Height", 200, testForm.Height);

				EnterpriseFormLookStrategy.SetPositionAndSize(testForm, screenWidth - (testWidth / 2), 200, testWidth, 100);
				currentScreen = CachedScreenInfo.Instance.FromControl(testForm);
				AssertEquals("Left", currentScreen.X + ((screenWidth - testForm.Width) / 2), testForm.Left);
				AssertEquals("Top", currentScreen.Y + 200, testForm.Top);
				AssertEquals("Width", testWidth, testForm.Width);
				AssertEquals("Height", 100, testForm.Height);
			}
		}

		public void TestRestoreJustPositionAndSizeWhenNoFormCache()
		{
			const string FormName = "TestFormRestoreJustPositionAndSize";
			const int ExpectedWidth = 150;
			const int ExpectedHeight = 200;

			// Arrange
			using var testForm = new ZForm();

			testForm.ForceRememberPositionAndSize = true; // A debug-only flag to enter the core function
			AssertEquals("Form RememberFormPosition should be true by default", true, testForm.RememberFormPosition);
			AssertEquals("Form RememberFormSize should be true by default", true, testForm.RememberFormSize);

			testForm.Name = FormName;
			testForm.WindowState = FormWindowState.Normal;
			testForm.Width = ExpectedWidth;
			testForm.Height = ExpectedHeight;

			// Act
			EnterpriseFormLookStrategy.RestoreJustPositionAndSize(testForm, justPosition: false);

			// Assert
			AssertEquals(FormWindowState.Normal, testForm.WindowState);

			var currentScreen = CachedScreenInfo.Instance.FromControl(testForm);
			AssertEquals(
				new Rectangle(
					currentScreen.Left + (currentScreen.Width - testForm.Width) / 2,
					currentScreen.Top + (currentScreen.Height - testForm.Height) / 2,
					ExpectedWidth,
					ExpectedHeight),
				testForm.Bounds);
		}

		public void TestRestoreJustPositionAndSizeWhenFormCacheExists()
		{
			const string FormName = "TestFormRestoreJustPositionAndSize";

			foreach (var validRect in GetValidRects())
			{
				Test(validRect);
			}

			void Test(Rectangle validCache)
			{
				// Arrange
				((WinFormsEnvironment)EnvProxy.Instance).FormRegistry.SetFormLocationAndSize(FormName, validCache);

				using var testForm = new ZForm();

				testForm.ForceRememberPositionAndSize = true; // A debug-only flag to enter the core function
				AssertEquals("Form RememberFormPosition should be true by default", true, testForm.RememberFormPosition);
				AssertEquals("Form RememberFormSize should be true by default", true, testForm.RememberFormSize);

				testForm.Name = FormName;
				testForm.WindowState = FormWindowState.Normal;

				// The initial width and height below should be valid to any modern screen
				testForm.Width = 150;
				testForm.Height = 200;

				// Act
				EnterpriseFormLookStrategy.RestoreJustPositionAndSize(testForm, justPosition: false);

				// Assert
				AssertEquals(FormWindowState.Normal, testForm.WindowState);
				AssertEquals(new Rectangle(validCache.Left, validCache.Top, validCache.Width, validCache.Height), testForm.Bounds);
			}

			IEnumerable<Rectangle> GetValidRects()
			{
				foreach (var screenWorkingArea in CachedScreenInfo.Instance.ScreenInfos)
				{
					yield return new Rectangle(screenWorkingArea.X + 10, screenWorkingArea.Y + 10, screenWorkingArea.Width - 20, screenWorkingArea.Height - 20);
				}
			}
		}

		public void TestRestoreJustPositionAndSizeWhenFormCacheIsInvalid()
		{
			const string FormName = "TestFormRestoreJustPositionAndSize";

			// The initial width and height below should be valid to any modern screen
			const int ExpectedWidth = 150;
			const int ExpectedHeight = 200;

			foreach (var invalidRect in GetInvalidRects())
			{
				Test(invalidRect);
			}

			void Test(Rectangle invalidCache)
			{
				// Arrange
				((WinFormsEnvironment)EnvProxy.Instance).FormRegistry.SetFormLocationAndSize(FormName, invalidCache);

				using var testForm = new ZForm();

				testForm.ForceRememberPositionAndSize = true; // A debug-only flag to enter the core function
				AssertEquals("Form RememberFormPosition should be true by default", true, testForm.RememberFormPosition);
				AssertEquals("Form RememberFormSize should be true by default", true, testForm.RememberFormSize);

				testForm.Name = FormName;
				testForm.WindowState = FormWindowState.Normal;
				testForm.Width = ExpectedWidth;
				testForm.Height = ExpectedHeight;

				// Act
				EnterpriseFormLookStrategy.RestoreJustPositionAndSize(testForm, justPosition: false);

				// Assert
				AssertEquals(FormWindowState.Normal, testForm.WindowState);

				var currentScreen = CachedScreenInfo.Instance.FromControl(testForm);
				AssertEquals(
					new Rectangle(
						currentScreen.Left + (currentScreen.Width - testForm.Width) / 2,
						currentScreen.Top + (currentScreen.Height - testForm.Height) / 2,
						ExpectedWidth,
						ExpectedHeight),
					testForm.Bounds);
			}

			IEnumerable<Rectangle> GetInvalidRects()
			{
				yield return Rectangle.Empty;
				yield return new Rectangle(0, 0, 10_000_000, 10_000_000);
				yield return new Rectangle(-10_000_000, -10_000_000, ExpectedWidth, ExpectedHeight);

				foreach (var screenWorkingArea in CachedScreenInfo.Instance.ScreenInfos)
				{
					yield return new Rectangle(screenWorkingArea.X - 10, screenWorkingArea.Y - 10, ExpectedWidth, ExpectedHeight);
					yield return new Rectangle(screenWorkingArea.X + screenWorkingArea.Width - 10, screenWorkingArea.Y + screenWorkingArea.Height - 10, ExpectedWidth, ExpectedHeight);
				}
			}
		}

#endif

		public void TestPositionForMultiScreens()
		{
			if (CachedScreenInfo.Instance.ScreenInfos.Length < 2)
			{
				Assert("No need to test if single monitor is available", true);
				return;
			}

			using (var testForm = new TestZForm())
			{
				testForm.RememberFormPosition = true;
				testForm.RememberFormSize = true;
				testForm.MinimumSize = new Size(0, 0);

				foreach (var screenBounds in CachedScreenInfo.Instance.ScreenInfos)
				{
					AssertPositionForSpecificScreen(testForm, screenBounds);
					AssertMaximizingForm(testForm, screenBounds);
				}
			}
		}

		void AssertPositionForSpecificScreen(TestZForm testForm, Rectangle screenBounds)
		{
			var testWidth = 550;

			EnterpriseFormLookStrategy.SetPositionAndSize(testForm, screenBounds.Left + 50, screenBounds.Top + 60, testWidth, 200);
			AssertEquals("Left", screenBounds.Left + 50, testForm.Left);
			AssertEquals("Top", screenBounds.Top + 60, testForm.Top);
			AssertEquals("Width", testWidth, testForm.Width);
			AssertEquals("Height", 200, testForm.Height);

			EnterpriseFormLookStrategy.SetPositionAndSize(testForm, screenBounds.Right - (testWidth - 10), screenBounds.Bottom - 190, testWidth, 200);
			AssertEquals("Left", screenBounds.Left + (screenBounds.Width - testWidth) / 2, testForm.Left);
			AssertEquals("Top", screenBounds.Top + (screenBounds.Height - 200) / 2, testForm.Top);
			AssertEquals("Width", testWidth, testForm.Width);
			AssertEquals("Height", 200, testForm.Height);

			EnterpriseFormLookStrategy.SetPositionAndSize(testForm, screenBounds.Left - 10, screenBounds.Top - 10, testWidth, 200);
			AssertEquals("Left", screenBounds.Left + (screenBounds.Width - testWidth) / 2, testForm.Left);
			AssertEquals("Top", screenBounds.Top + (screenBounds.Height - 200) / 2, testForm.Top);
			AssertEquals("Width", testWidth, testForm.Width);
			AssertEquals("Height", 200, testForm.Height);

			EnterpriseFormLookStrategy.SetPositionAndSize(testForm, screenBounds.Left + 10, screenBounds.Top + 10, screenBounds.Width + 10, screenBounds.Height + 10);
			AssertEquals("Left", screenBounds.Left, testForm.Left);
			AssertEquals("Top", screenBounds.Top, testForm.Top);
			AssertEquals("Width", screenBounds.Width, testForm.Width);
			AssertEquals("Height", screenBounds.Height, testForm.Height);
		}

		void AssertMaximizingForm(TestZForm testForm, Rectangle screenBounds)
		{
			var testWidth = 550;

			testForm.WindowState = FormWindowState.Normal;
			testForm.Width = testWidth;
			testForm.Height = 200;
			EnterpriseFormLookStrategy.SetPositionAndSize(testForm, screenBounds.Left, screenBounds.Top, screenBounds.Width, screenBounds.Height);
			AssertEquals(FormWindowState.Maximized, testForm.WindowState);

			testForm.WindowState = FormWindowState.Normal;
			AssertEquals("Left", screenBounds.Left + (screenBounds.Width - testWidth) / 2, testForm.Left);
			AssertEquals("Top", screenBounds.Top + (screenBounds.Height - 200) / 2, testForm.Top);
			AssertEquals("Width", testWidth, testForm.Width);
			AssertEquals("Height", 200, testForm.Height);
		}

		#endregion
	}
}
