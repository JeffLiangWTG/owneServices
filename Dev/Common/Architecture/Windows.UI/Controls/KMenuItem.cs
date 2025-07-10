using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.Windows.UI
{
	/// <summary><see cref="MenuItem"/></summary>
	public class KMenuItem : MenuItem, IMenuItem
	{
		#region Constructors

		public KMenuItem()
		{
		}

		public KMenuItem(string text)
			: base(text)
		{
		}

		public KMenuItem(string text, EventHandler onClick)
			: base(text, onClick)
		{
		}

		public KMenuItem(string text, EventHandler onClick, Shortcut shortcut)
			: base(text, onClick, shortcut)
		{
		}

		public KMenuItem(string text, MenuItem[] items)
			: base(text, items)
		{
		}

		public KMenuItem(MenuMerge mergeType, int mergeOrder, Shortcut shortcut, string text, EventHandler onClick, EventHandler onPopup, EventHandler onSelect, MenuItem[] items)
			: base(mergeType, mergeOrder, shortcut, text, onClick, onPopup, onSelect, items)
		{
		}

		#endregion

		#region Dispose

		public bool IsMenuDisposed => isDisposed;

		bool isDisposed;
		protected override void Dispose(bool disposing)
		{
			isDisposed = true;
			base.Dispose(disposing);
		}

		#endregion

		#region Override

		protected override void OnPopup(EventArgs e)
		{
			try
			{
				base.OnPopup(e);
			}
			catch (NullReferenceException)
			{
				if (!isDisposed) // Do not remove this as it's for fixing NRE caused by SwitchForms in \Enterprise\Architecture\GUI\Controls\ZPreviousNextControl.cs
				{
					throw;
				}
			}
		}

		#endregion

		#region IMenuItem Members

		IList IMenuItem.MenuItems
		{
			get { return MenuItems; }
		}

		#endregion

		public static string StripAcceleratorKeys(string text)
		{
			// for chinese
			int chineseAccIndex = text.LastIndexOf("(&");
			return !((ZString)text).IsWesternEuropeanOrEmpty && chineseAccIndex != -1
								? text.Substring(0, chineseAccIndex)
								: text.Replace("&&", "ZZZZZ").Replace("&", "").Replace("ZZZZZ", "&");
		}

		public static string StripAcceleratorKeysButKeepAmpersandInText(string text)
		{
			// for chinese
			int chineseAccIndex = text.LastIndexOf("(&");
			return !((ZString)text).IsWesternEuropeanOrEmpty && chineseAccIndex != -1
								? text.Substring(0, chineseAccIndex)
								: text.Replace("&&", "ZZZZZ").Replace("&", "").Replace("ZZZZZ", "&&");
		}
	}
}
