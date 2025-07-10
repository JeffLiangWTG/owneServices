using System.Linq;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.Quotation.RollUpSort
{
	sealed class RolledUpDocRateLine : IRolledUpDocLine
	{
		public RolledUpDocRateLine(ISortableDocLineList docLineList, string description)
		{
			DocLineList = docLineList;
			Description = description;
		}

		public ISortableDocLineList DocLineList { get; }

		public string Description { get; }

		DocRateLine DocRateLine => docRateLine ??= DocLineList.Cast<DocRateLine>().FirstOrDefault(x => x is DocRateLine);
		DocRateLine docRateLine;

		public int OrgLevelSortOrder => ((ISortableDocLine)DocRateLine).OrgLevelSortOrder;

		public int ChargePrintSeqSortOrder => ((ISortableDocLine)DocRateLine).ChargePrintSeqSortOrder;

		public int UserEnteredSortOrder => ((ISortableDocLine)DocRateLine).UserEnteredSortOrder;

		public string AlphabeticalSortOrder => Description;
	}
}
