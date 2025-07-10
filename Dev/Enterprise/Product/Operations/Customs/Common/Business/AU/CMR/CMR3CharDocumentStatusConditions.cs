using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMR3CharDocumentStatusConditions : CodeDescriptionPair
	{
		protected CMR3CharDocumentStatusConditions(object code, MultilingualString description)
			: base(code, description)
		{
		}
		public static readonly CMR3CharDocumentStatusConditions Validation = new CMR3CharDocumentStatusConditions("VAL", ResString.GetMultilingualString("CMR3CharDocumentStatusConditions|VAL", "The lodged or amended information has not passed the required edits."));
		public static readonly CMR3CharDocumentStatusConditions Suspended = new CMR3CharDocumentStatusConditions("SUS", ResString.GetMultilingualString("CMR3CharDocumentStatusConditions|SUS", "Customs has temporarily stopped the authority to deal."));
		public static readonly CMR3CharDocumentStatusConditions Embargoed = new CMR3CharDocumentStatusConditions("EMB", ResString.GetMultilingualString("CMR3CharDocumentStatusConditions|EMB", "A country code subject to UN Sanctions has been detected by Customs. Please contact Customs."));
		public static readonly CMR3CharDocumentStatusConditions Expired = new CMR3CharDocumentStatusConditions("EXP", ResString.GetMultilingualString("CMR3CharDocumentStatusConditions|EXP", "Customs does not expect further quotation of the related CAN."));

		public static CMR3CharDocumentStatusConditions GetFromStatusText(ZString statusText)
		{
			CMR3CharDocumentStatusConditions result = null;
			if (statusText.IndexOf("EMBARGOED") != -1)
			{
				result = CMR3CharDocumentStatusConditions.Embargoed;
			}
			else if (statusText.IndexOf("EXPIRED") != -1)
			{
				result = CMR3CharDocumentStatusConditions.Expired;
			}
			else if (statusText.IndexOf("SUSPENDED") != -1)
			{
				result = CMR3CharDocumentStatusConditions.Suspended;
			}
			else if (statusText.IndexOf("VALIDATION") != -1)
			{
				result = CMR3CharDocumentStatusConditions.Validation;
			}

			return result;
		}
	}
}
