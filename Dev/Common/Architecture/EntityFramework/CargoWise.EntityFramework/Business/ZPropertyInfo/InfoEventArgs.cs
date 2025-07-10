using System;

namespace CargoWise.EntityFramework
{
	public class InfoEventArgs : EventArgs
	{
		public InfoEventArgs(ZPropertyInfo info)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			Info = info;
		}

		/// <summary>
		/// The ZPropertyInfo that was changed - it contains the new value
		/// </summary>
		public readonly ZPropertyInfo Info;
	}
}
