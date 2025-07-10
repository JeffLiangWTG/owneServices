using System.Collections.Generic;
using Enterprise.DocumentEngine.DocBuilder;

namespace Enterprise.DocumentEngine.Business
{
	class StmMenuDocumentConfigItemComparer : IComparer<StmMenuDocumentConfigItem>
	{
		internal StmMenuDocumentConfigItemComparer()
		{
			sectionTypePrintOrders = new Dictionary<string, int>();

			sectionTypePrintOrders.Add(ConfigurableSectionTypeList.Codes.ConfigSection, 0);
			sectionTypePrintOrders.Add(ConfigurableSectionTypeList.Codes.DocumentHeader, 1);
			sectionTypePrintOrders.Add(ConfigurableSectionTypeList.Codes.PageHeader, 2);
			sectionTypePrintOrders.Add(ConfigurableSectionTypeList.Codes.BodySection, 3);
			sectionTypePrintOrders.Add(ConfigurableSectionTypeList.Codes.GenericSection, 3);
			sectionTypePrintOrders.Add(ConfigurableSectionTypeList.Codes.PageFooter, 4);
			sectionTypePrintOrders.Add(ConfigurableSectionTypeList.Codes.DocumentFooter, 5);
			sectionTypePrintOrders.Add(ConfigurableSectionTypeList.Codes.BackPage, 6);
			sectionTypePrintOrders.Add(ConfigurableSectionTypeList.Codes.EndOfReport, 7);

			sectionTypePrintOrders.Add(GenericSectionUsageList.Codes.PageHeaderFirstPageOnly, 1);
			sectionTypePrintOrders.Add(GenericSectionUsageList.Codes.PageHeaderAll, 2);
			sectionTypePrintOrders.Add(GenericSectionUsageList.Codes.PageHeaderStartFromSecondPage, 2);
			sectionTypePrintOrders.Add(GenericSectionUsageList.Codes.BodySection, 3);
			sectionTypePrintOrders.Add(GenericSectionUsageList.Codes.BodySectionExpanding, 3);
			sectionTypePrintOrders.Add(GenericSectionUsageList.Codes.PageFooterAll, 4);
			sectionTypePrintOrders.Add(GenericSectionUsageList.Codes.PageFooterFallbackDefault, 4);
			sectionTypePrintOrders.Add(GenericSectionUsageList.Codes.PageFooterFirstPageOnly, 4);
			sectionTypePrintOrders.Add(GenericSectionUsageList.Codes.PageFooterAllExceptLastPage, 4);
			sectionTypePrintOrders.Add(GenericSectionUsageList.Codes.PageFooterLastPage, 4);
			sectionTypePrintOrders.Add(GenericSectionUsageList.Codes.FirstPageFooterUnlessOnlyOnePage, 4);
			sectionTypePrintOrders.Add(GenericSectionUsageList.Codes.FirstPageFooterWhenOnlyOnePage, 4);
			sectionTypePrintOrders.Add(GenericSectionUsageList.Codes.BackPage, 6);
		}

		readonly Dictionary<string, int> sectionTypePrintOrders;

		public int Compare(StmMenuDocumentConfigItem x, StmMenuDocumentConfigItem y)
		{
			var result = CompareByType(x, y);

			if (result == 0)
			{
				result = x.S4_PrintOrder.CompareTo(y.S4_PrintOrder);
			}

			return result;
		}

		public int CompareByType(StmMenuDocumentConfigItem x, StmMenuDocumentConfigItem y)
		{
			if (sectionTypePrintOrders.ContainsKey(x.S4_SectionType) && sectionTypePrintOrders.ContainsKey(y.S4_SectionType))
			{
				return sectionTypePrintOrders[x.S4_SectionType].CompareTo(sectionTypePrintOrders[y.S4_SectionType]);
			}

			if (!sectionTypePrintOrders.ContainsKey(x.S4_SectionType) && sectionTypePrintOrders.ContainsKey(y.S4_SectionType))
			{
				return 1;
			}

			if (sectionTypePrintOrders.ContainsKey(x.S4_SectionType) && !sectionTypePrintOrders.ContainsKey(y.S4_SectionType))
			{
				return -1;
			}

			return 0;
		}
	}
}
