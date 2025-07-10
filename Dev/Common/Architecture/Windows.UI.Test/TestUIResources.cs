using System;

namespace CargoWise.Windows.UI.Testing
{
	sealed class TestUIResources : UIResources
	{
		public override TimeSpan intervalTime => intervalOverride ?? base.intervalTime;
		public TimeSpan? intervalOverride;

		public void SetGdiObjectsCount(int value)
		{
			gdiObjectsCount = value;
		}

		public override int GdiObjectsCount
		{
			get { return gdiObjectsCount ?? base.GdiObjectsCount; }
		}
		int? gdiObjectsCount;

		public void SetWindowsRegistryKeyName(string value)
		{
			windowsRegistryKeyName = value;
		}

		protected override string WindowsRegistryKeyName
		{
			get { return windowsRegistryKeyName ?? base.WindowsRegistryKeyName; }
		}
		string windowsRegistryKeyName;

		public override int GdiObjectsCountUnsafe
		{
			get
			{
				if (triggerException)
				{
					triggerException = false;
					throw new Exception();
				}
				return base.GdiObjectsCountUnsafe;
			}
		}
		public void SetUserObjectsCount_ForTest(int value)
		{
			userObjectsCount = value;
		}

		public override int UserObjectsCount
		{
			get { return userObjectsCount ?? base.UserObjectsCount; }
		}
		int? userObjectsCount;

		public override int UserObjectsCountUnsafe
		{
			get
			{
				if (triggerException)
				{
					triggerException = false;
					throw new Exception();
				}
				return base.UserObjectsCountUnsafe;
			}
		}
		public bool triggerException;

		public void SetUserWindowHandlesCount(int value)
		{
			windowHandlesCount = value;
		}

		public override int UserWindowHandlesCount
		{
			get { return windowHandlesCount ?? base.UserWindowHandlesCount; }
		}
		int? windowHandlesCount;
	}
}
