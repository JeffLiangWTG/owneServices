using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.Wow
{
	public class WowNumberFountains
	{
		protected WowNumberFountains()
		{
		}

		public static WowNumberFountains Instance
		{
			get { return instance ?? (instance = new WowNumberFountains()); }
		}
		[ThreadStatic]
		static WowNumberFountains instance;

		public INumberFountainProxy GetWowEdiTrackMessageID(ZString numberFountainID)
		{
			string fountainKey = "WowEdiTrackMessageID" + numberFountainID.Substring(0, 15);
			NonFormattedNumberFountainFactory factory = new NonFormattedNumberFountainFactory(fountainKey, maxValue: 10001);
			return factory.New();
		}
	}
}
