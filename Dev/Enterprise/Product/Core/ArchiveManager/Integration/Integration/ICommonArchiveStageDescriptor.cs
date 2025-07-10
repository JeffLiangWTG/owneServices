using System.Collections.Concurrent;
using CargoWise.Types;

namespace Enterprise.ArchiveManager.Integration
{
	public interface ICommonArchiveStageDescriptor : IArchiveStageDescriptor
	{
		IArchiveStageStopwatch ArchiveStageStopWatch { get; }

		IArchiveableBusinessObjectProviderCache BusinessObjectProviderDictionary { get; }

		ZDateTime ArchiveToDateAtBeginning { get; }

		IArchiveWatermark WatermarkAtBeginning { get; }

		ConcurrentDictionary<string, ITableProcessingInfo> ProcessingInfoPerTable { get; }
	}
}
