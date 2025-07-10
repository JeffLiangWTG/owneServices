using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class TaxChangeAssessmentLookups : EDIMessageLookups
	{
		public TaxChangeAssessmentLookups(TaxChangeAssessment parent) : base(parent)
		{
		}

		public CodeDescriptionPairList EntryStatusList => new TaxChangeAssessmentStatusCodeList();
	}
}
