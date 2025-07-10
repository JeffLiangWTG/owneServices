using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class RequestHeaderValidation : EUMemberStateCommunicationValidation
	{
		public RequestHeaderValidation(AutoEUMemberStateCommunication parent) : base(parent)
		{
		}

		protected override void CheckEUS_Type()
		{
			base.CheckEUS_Type();
			ListValidation.MessageErrorIfInvalidCode(Parent.EUS_TypeInfo, RequestHeader.Lookups.RequestTypeList);
		}

		protected override void CheckEUS_IncludeScreeningDetails()
		{
			if (RequestHeader.EUS_IncludeScreeningDetails && !HasHRCMScreeningData())
			{
				RequestHeader.EUS_IncludeScreeningDetailsInfo.AddMessageError(Res.GetString("AA2E04FB-DEE1-42A0-BEA0-9620B81317FA", "No HRCM Screening results exist for the House Bill or the Master Header. The Screening Results will not be included in the Reply"));
			}

			bool HasHRCMScreeningData()
			{
				var header = (AsycudaManifestHeader)RequestHeader.Parent;
				return header.BillScreenings.Count > 0 || RequestHeader.RelatedHouseBill?.BillScreenings?.Count > 0;
			}
		}

		protected override void CheckEUS_HouseBillNumber()
		{
			base.CheckEUS_HouseBillNumber();
			ListValidation.MessageErrorIfInvalidCode(Parent.EUS_HouseBillNumberInfo, RequestHeader.Lookups.HouseBillList);
			ValidateEUS_IncludeScreeningDetails();
		}

		RequestHeader RequestHeader => (RequestHeader)Parent;
	}
}
