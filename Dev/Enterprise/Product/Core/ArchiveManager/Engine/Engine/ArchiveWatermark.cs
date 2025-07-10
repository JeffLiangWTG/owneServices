using System;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Engine
{
	public class ArchiveWatermark : IArchiveWatermark
	{
		public ArchiveWatermark()
		{
		}

		public ZDateTime WatermarkDate { get; set; } = ZDateTime.MinSmallDateTimeValue;

		public string WatermarkNK { get; set; } = string.Empty;

		public Guid WatermarkPK { get; set; } = Guid.Empty;
	}
}
