using System;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Central repository for application icons and images. You can use the IconTypes enumeration to
	/// get icons and images from here and apply them to forms using the IconType property of the form.
	///
	/// To edit these icons, use the freeware 'Resourcer' application. Make sure you have checked out the
	/// relevant .resources file before editing. If you want to add resource files, create the file with
	/// the extension .resources.resources and add it to the project, then add the file to the array 
	/// below (in the constructor).
	/// </summary>
	public sealed class Icons : KIcons
	{
		Icons()
		{
			ImageListCore.ColorDepth = ColorDepth.Depth32Bit;
		}

		public static ImageList ImageList
		{
			get { return Instance.ImageListCore; }
		}

		#region Retrieval

		public static bool IsIcon(IconTypes type)
		{
			return Instance.IsIconCore(type);
		}

		public static bool IsImage(IconTypes type)
		{
			return Instance.IsImageCore(type);
		}

		/// <summary>
		/// Get an icon matching the IconTypes enumeration.
		/// </summary>
		/// <param name="Type">The icon to retrieve.</param>
		public static Icon GetIcon(IconTypes type)
			=> Instance.GetIconCore(type)
				?? throw new ArgumentException("Icon " + type + " is not an icon!");

		/// <summary>
		/// Get an icon matching the IconTypes enumeration.
		/// </summary>
		/// <param name="Type">The icon to retrieve.</param>
		public static Image GetImage(IconTypes type)
			=> Instance.GetImageCore(type)
				?? throw new ArgumentException("Icon " + type + " is not an image!");

		/// <summary>
		/// Get an icon matching the IconTypes enumeration.
		/// </summary>
		/// <param name="type">The icon to retrieve.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public static Image GetMiniImage(IconTypes type)
			=> Instance.GetImageCore(type + "Mini")
				?? throw new ArgumentException("Icon " + type + " is not an image!");

		/// <summary>
		/// Get an icon's ImageList index matching the IconTypes enumeration.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static int GetImageIndex(IconTypes type)
		{
			return Instance.GetImageListIndexCore(type);
		}

		#endregion

		#region Implementation

		static Icons Instance
		{
			get { return instance ?? (instance = new Icons()); }
		}
		[ThreadStatic]
		static Icons instance;

		protected override ResourceManager[] NewResourceManagers()
		{
			return new ResourceManager[]
			{
				GetResourceManager("Enterprise.ZArchitecture.GUI.Icons.Icons", Assembly.GetAssembly(typeof(Icons))),
				GetResourceManager("Enterprise.ZArchitecture.GUI.Icons.Icons20x16", Assembly.GetAssembly(typeof(Icons))),
				GetResourceManager("Enterprise.ZArchitecture.GUI.Icons.Notifications", Assembly.GetAssembly(typeof(Icons))),
				GetResourceManager("Enterprise.ZArchitecture.GUI.Icons.ToolbarButtons", Assembly.GetAssembly(typeof(Icons))),
				GetResourceManager("Enterprise.ZArchitecture.GUI.Icons.Branding", Assembly.GetAssembly(typeof(Icons))),
			};
		}

		#endregion
	}
}
