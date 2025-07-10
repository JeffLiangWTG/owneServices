using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[ToolboxItem(false)]
	public sealed partial class ZFilterControlImages : Component
	{
		/// <summary>
		/// FilterImageList loads image list every time we call it and it could be null even if we called it before and got not null value. call it once and cache the value.
		/// </summary>
		public static ImageList FilterImageList
		{
			get
			{
				ZFilterControlImages result = null;

				if (filterImageList == null)
				{
					filterImageList = new WeakReference<ZFilterControlImages>(null);
				}

				var retries = 0;
				while (retries < 3 && !filterImageList.TryGetTarget(out result))
				{
					try
					{
						if (retries > 0)
						{
							Thread.Sleep((int)Math.Pow(10, retries)); //0, 10, 100ms
						}

						result = new ZFilterControlImages();
						filterImageList.SetTarget(result);
						break;
					}
					catch (Exception e) when (
						e is TargetInvocationException //Loading of the ImageList did not succeed. I00853226. When this happens, available physical memory is always <1000MB
						|| e is IndexOutOfRangeException) //Index was outside the bounds of the array. I00854645. Some correlation with low available physical memory.
					{
						++retries;
					}
				}

				return result != null ? result.ImageList : null;
			}
		}

		[ThreadStatic]
		static WeakReference<ZFilterControlImages> filterImageList;

		ZFilterControlImages()
		{
			InitializeComponent();
		}
	}
}