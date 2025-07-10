using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business
{
	public class EUUniversalRateCalcData : EntryLineUniversalRateCalcData
	{
		public EUUniversalRateCalcData(CusEntryLine entryLine, RateView rateView) : base(entryLine, rateView)
		{
		}
		protected new CusEntryLine EntryLine => (CusEntryLine)base.EntryLine;

		protected override IList<Tuple<string, string>> GetInitialAdditionalInformationList()
		{
			var set = new HashSet<string>();
			if (EntryLine.Header is CusEntryHeader entryHeader)
			{
				AddSupportingDocuments(set, entryHeader.SupportingDocuments);
			}
			AddSupportingDocuments(set, EntryLine.SupportingDocuments);
			return set.OrderBy(x => x).Select(y => Tuple.Create(UniversalReferenceConstants.SupportingDocumentTypes.Certificate, y)).ToList();
		}

		void AddSupportingDocuments(HashSet<string> set, IEnumerable<SupportingDocument> supportingDocuments)
		{
			supportingDocuments.Where(x => x.CSI_Type.EqualsIgnoringCase(CusSupportingInfoTypeList.Codes.SupportingDocument)).ForEach(s => set.Add(s.CSI_Code));
		}

		protected override IDictionary<string, string> GetMeursingExpressionList()
			=> new RateCalcMeursingExpressionReplacer(EntryLine).ReplaceApplicableMeursingExpressions(RateView);
	}
}
