using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.DocumentWrappers.HelperClasses.DocRollUpSort;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.Quotation.RollUpSort
{
	sealed class RatingDocRollUpSorter : BaseDocRollUpSorter<ZString, MultilingualString, PricingPage, DocLineList>
	{
		public RatingDocRollUpSorter(PricingPage docHeader, BusinessObjectFactory factory)
			: base(docHeader, factory)
		{
		}

		override protected IComparer GetAlphabeticComparer() => new AlphabeticComparer();

		class AlphabeticComparer : IComparer
		{
			public int Compare(object docLine1, object docLine2)
				=> DocLineSorter.CompareAlphabetically((ISortableDocLine)docLine1, (ISortableDocLine)docLine2);
		}

		override protected IComparer GetSequenceSortComparer() => new SequenceSortComparer();

		class SequenceSortComparer : IComparer
		{
			public int Compare(object docLine1, object docLine2)
			{
				if ((docLine1 is IDocLine && docLine2 is IDocLine)
					|| (docLine1 is IRolledUpDocLine && docLine2 is IRolledUpDocLine))
				{
					return DocLineSorter.CompareBySequence((ISortableDocLine)docLine1, (ISortableDocLine)docLine2);
				}

				if (docLine1 is IDocLine && docLine2 is IRolledUpDocLine)
				{
					return 1;
				}

				if (docLine1 is IRolledUpDocLine && docLine2 is IDocLine)
				{
					return -1;
				}

				return 0;
			}
		}

		override protected IComparer GetUserEnteredComparer() => new UserEnteredComparer();

		class UserEnteredComparer : IComparer
		{
			public int Compare(object docLine1, object docLine2)
			{
				if ((docLine1 is IDocLine && docLine2 is IDocLine) || (docLine1 is IRolledUpDocLine && docLine2 is IRolledUpDocLine))
				{
					return DocLineSorter.CompareByUserEntered((ISortableDocLine)docLine1, (ISortableDocLine)docLine2);
				}

				if (docLine1 is IDocLine && docLine2 is IRolledUpDocLine)
				{
					return 1;
				}

				if (docLine1 is IRolledUpDocLine && docLine2 is IDocLine)
				{
					return -1;
				}

				return 0;
			}
		}

		protected override BaseDocRollUpper<ZString, MultilingualString, DocLineList> GetChargeDocRollUpper(PricingPage header, BusinessObjectFactory factory, DocLineList lines, ZString rollUpStyleId, ZString rollUpGroupId, params ZString[] chargeGroups)
			=> new RatingChargeDocRollUpper(header, factory, lines, rollUpStyleId, rollUpGroupId, chargeGroups);

		protected override BaseDocRollUpper<ZString, MultilingualString, DocLineList> GetChargeDocRollUpper(PricingPage header, BusinessObjectFactory factory, DocLineList lines, ZString rollUpStyleId, Dictionary<ZString, ZString> chargeGroupToGroupIdMap)
			=> new RatingChargeDocRollUpper(header, factory, lines, rollUpStyleId, chargeGroupToGroupIdMap);

		protected override DocLineList GetNewLineList()
			=> new DocLineList();
	}
}
