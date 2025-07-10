using System;
using System.Drawing;
using System.Resources;
using CargoWise.Windows.UI;

namespace CargoWise.Design.TypeIntellisense
{
	internal class IntellisenseListIcons : KIcons
	{
		protected override ResourceManager[] NewResourceManagers()
		{
			return new ResourceManager[]
			{
				GetResourceManager(typeof(IntellisenseListIcons).Namespace + ".Internal." + nameof(IntellisenseListIcons), typeof(IntellisenseListIcons).Assembly)
			};
		}

		public static Image GetImage(IntellisenseItemType typeKey)
		{ return Instance.GetImageCore(typeKey); }

		static IntellisenseListIcons Instance
		{
			get { return instance ?? (instance = new IntellisenseListIcons()); }
		}
		[ThreadStatic]
		static IntellisenseListIcons instance;
	}
}
