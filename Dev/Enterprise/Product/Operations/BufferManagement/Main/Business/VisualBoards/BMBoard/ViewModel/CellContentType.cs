namespace Enterprise.BufferManagement.Business
{
	public static class CellContentTypeExtensions
	{
		public static bool IsHeadingType(this CellContentType type)
		{
			switch (type)
			{
				case CellContentType.AgeHeading:
				case CellContentType.ChannelHeading:
				case CellContentType.ZoneHeading:
				case CellContentType.SubComponentZoneHeading:
				case CellContentType.CCRHeading:
					return true;
			}

			return false;
		}

		public static bool IsZoneHeading(this CellContentType type)
		{
			return type == CellContentType.ZoneHeading || type == CellContentType.SubComponentZoneHeading;
		}
	}

	public enum CellContentType
	{
		None,
		ZoneHeading,
		SubComponentZoneHeading,
		ChannelHeading,
		AgeHeading,
		Cards,
		Label,
		CCRHeading,
	}
}
