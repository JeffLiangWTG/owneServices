namespace Enterprise.BufferManagement.Business
{
	public class BMComponentAcceptabilityBandLookups : AutoBMComponentAcceptabilityBandLookups
	{
		public BMComponentAcceptabilityBandLookups(AutoBMComponentAcceptabilityBand parent)
			: base(parent)
		{
		}

		#region Components

		public BMComponentCollection Components
		{
			get { return Factory.GetCachedValue("BMComponentAcceptabilityBandLookups.Components", GetComponentCollection); }
		}

		BMComponentCollection GetComponentCollection()
		{
			return new BMComponentCollection(Factory);
		}

		#endregion

		#region Types

		public AcceptabilityBandTypes Types
		{
			get { return Factory.GetCachedValue<AcceptabilityBandTypes>(); }
		}

		#endregion
	}
}
