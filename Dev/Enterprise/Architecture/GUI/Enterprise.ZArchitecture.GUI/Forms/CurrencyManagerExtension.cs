using System;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public static class CurrencyManagerExtension
	{
		///<summary>
		///Tries to return Current. If that threw an exception, Refresh(), return null if Position is now nonsensical and finally try to return Current again.
		///</summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule", Justification = "This is the replacement for CurrencyManager.Current")]
		public static object GetCurrent(this BindingManagerBase bmb)
		{
			try
			{
				if (bmb == null || bmb.Position < 0 || bmb.Position >= bmb.Count)
				{
					return null;
				}

				return bmb.Current;
			}
			catch (IndexOutOfRangeException)
			{
				var cm = bmb as CurrencyManager;
				if (cm != null)
				{
					cm.Refresh();
				}
				if (bmb.Position < 0 || bmb.Position >= bmb.Count)
				{
					return null;
				}

				//if bmb.Current still throws we're probably screwed
				return bmb.Current;
			}
		}
	}
}