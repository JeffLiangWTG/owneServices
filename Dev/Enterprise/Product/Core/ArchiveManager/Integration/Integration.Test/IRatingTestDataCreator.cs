using CargoWise.Types;

namespace Enterprise.ArchiveManager.Integration.Test
{
	public interface IRatingTestDataCreator
	{
		ZGuid CreateAttachedRatingData(string quoteNumber, bool isCancelled);
	}
}

