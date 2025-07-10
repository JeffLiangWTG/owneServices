namespace Enterprise.ZArchitecture.Environment
{
	public class FreightRegistry
	{
		public FreightRegistry(RawDataRegistry rawRegistry)
		{
			RawRegistry = rawRegistry;
		}
		readonly RawDataRegistry RawRegistry;

		#region Pack Line

		public PackLineRegistry PackLine
		{
			get
			{
				if (fPackLineRegistry == null)
				{
					fPackLineRegistry = new PackLineRegistry(RawRegistry);
				}

				return fPackLineRegistry;
			}
		}

		PackLineRegistry fPackLineRegistry;

		#endregion

		#region Air Way Bill

		public AirWaybillRegistry AirWaybill
		{
			get
			{
				if (fAirWaybillRegistry == null)
				{
					fAirWaybillRegistry = new AirWaybillRegistry(RawRegistry);
				}

				return fAirWaybillRegistry;
			}
		}

		AirWaybillRegistry fAirWaybillRegistry;

		#endregion

		#region PRA Messaging

		public PRAMessagingRegistry PRAMessaging
		{
			get
			{
				if (fPRAMessaging == null)
				{
					fPRAMessaging = new PRAMessagingRegistry(RawRegistry);
				}

				return fPRAMessaging;
			}
		}

		PRAMessagingRegistry fPRAMessaging;

		#endregion
	}
}
