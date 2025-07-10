using System;

using Enterprise.ZArchitecture;

namespace Enterprise.Client.Wow
{
	[Serializable]
	internal class WowWarningType : WarningType
	{
		internal WowWarningType(string message)
			: base(message)
		{
		}

		public static readonly WarningType NewPartNumberAdded = new WowWarningType("New part number added");
	}
}
