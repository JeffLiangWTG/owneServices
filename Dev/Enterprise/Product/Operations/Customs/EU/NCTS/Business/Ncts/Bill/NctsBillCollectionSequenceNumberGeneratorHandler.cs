using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	static class NctsBillCollectionSequenceNumberGeneratorHandler
	{
		public static void OnCollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			if (e?.BizObject is not NctsBill nctsBill
				|| sender is not INctsBillCollection<NctsBill> billCollection
				|| billCollection.SequenceGenerator is not ShortSequenceNumberGenerator sequenceNumberGenerator)
			{
				return;
			}

			HandleSequenceRecalculation(e, nctsBill, sequenceNumberGenerator);
		}

		static void HandleSequenceRecalculation(CollectionCountChangedEventArgs e, NctsBill nctsBill, ShortSequenceNumberGenerator sequenceNumberGenerator)
		{
			if (e.ItemAdded)
			{
				sequenceNumberGenerator.RecalculateWhenAdded(nctsBill);
			}
			else if (e.ItemRemoved)
			{
				sequenceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(nctsBill);
			}
		}
	}
}
