using System;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZLabelCaptionCache : IDisposable
	{
		protected ZLabelCaptionCache()
		{
			Res.Changed += ClearCache;
		}

		public static ZLabelCaptionCache Instance
		{
			get { return instance ?? (instance = new ZLabelCaptionCache()); }
		}
		[SuppressThreadStaticFieldMessage]
		static ZLabelCaptionCache instance;

		public string[] GetCaptions(Control control)
		{
			return GetCaptions(control, false);
		}

		public virtual string[] GetCaptions(Control control, bool invalidateCache)
		{
			string[] captions = null;
			if (!control.IsDisposed)
			{
				if (control is IResCaptionedControl && ((IResCaptionedControl)control).CaptionResourceString != null && !((IResCaptionedControl)control).CaptionResourceString.IsEmpty())
				{
					captions = ((IResCaptionedControl)control).CaptionResourceString.GetCaptions();
#if DEBUG
					OnDataHit(((IResCaptionedControl)control).CaptionResourceString, control);
#endif
				}
				else
				{
					captions = invalidateCache ? null : control.GetCachedLabelCaptions(validCacheKey);
				}
				if (captions == null)
				{
					var data = new ResourceStringKeyCalculator(control).DataString;
					if (data != null && !data.IsEmpty())
					{
#if DEBUG
						OnDataHit(data, control);
#endif
						captions = data.GetCaptions();
						control.SetCachedLabelCaption(captions, validCacheKey);
					}
				}
			}
			return captions ?? Array.Empty<string>();
		}

#if DEBUG
		public delegate void DataHitDelegate(ResourceStringData data, Control control);
		public event DataHitDelegate DataHit;

		public void OnDataHit(ResourceStringData data, Control control)
		{
			if (DataHit != null)
			{
				DataHit(data, control);
			}
		}
#endif

		void ClearCache(object sender, EventArgs e)
		{
			ClearCache();
		}

		public void ClearCache()
		{
			validCacheKey = DateTime.Now.Ticks; // This is used as a local memory cache key
		}

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			Res.Changed -= ClearCache;
		}

		#endregion

		#region Implementation

		long validCacheKey = DateTime.Now.Ticks; // This is used as a local memory cache key

		#endregion
	}
}
