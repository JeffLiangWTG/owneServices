using System;
using System.Collections.Generic;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	public class TestArchiveSchedule : IArchiveSchedule
	{
		public void AttachEDoc(string filepath, string eDocFileName, string docType, string description)
		{
			return;
		}

		public IArchiveWatermark GetWatermark(string stageName)
		{
			if (!WatermarkDictionary.TryGetValue(stageName, out var result))
			{
				result = null;
			}

			return result;
		}

		public void SetWatermark(string stageName, IArchiveWatermark watermark)
		{
			if (WatermarkDictionary.ContainsKey(stageName))
			{
				WatermarkDictionary[stageName] = watermark;
			}
			else
			{
				WatermarkDictionary.Add(stageName, watermark);
			}
		}

		readonly Dictionary<string, IArchiveWatermark> WatermarkDictionary = new Dictionary<string, IArchiveWatermark>();

		readonly Guid schedulePK = Guid.Empty;

		public Guid SchedulePK => schedulePK;
	}
}
