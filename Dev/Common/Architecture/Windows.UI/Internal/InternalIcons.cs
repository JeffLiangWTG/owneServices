using System;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	internal enum InternalIconTypes
	{
		Error,
		Warning
	}

	/// <summary>
	/// Central repository for application icons and images. You can use the IconTypes enumeration
	/// to get icons and images from here.
	/// </summary>
	internal class InternalIcons : KIcons
	{
		static InternalIcons Instance
		{ get { return instance ?? (instance = new InternalIcons()); } }
		[ThreadStatic]
		static InternalIcons instance;

		protected override ResourceManager[] NewResourceManagers()
		{
			return new ResourceManager[]
			{
				GetResourceManager("CargoWise.Windows.UI.Internal.Notifications.Notifications", typeof(InternalIcons).Assembly),
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static ImageList ImageList
		{ get { return Instance.ImageListCore; } }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static int GetImageListIndex(InternalIconTypes typeKey)
		{ return Instance.GetImageListIndexCore(typeKey); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static bool IsIcon(InternalIconTypes typeKey)
		{ return Instance.IsIconCore(typeKey); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static bool IsImage(InternalIconTypes typeKey)
		{ return Instance.IsImageCore(typeKey); }

		public static Icon GetIcon(InternalIconTypes typeKey)
		{ return Instance.GetIconCore(typeKey); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static Image GetImage(InternalIconTypes typeKey)
		{ return Instance.GetImageCore(typeKey); }
	}
}
