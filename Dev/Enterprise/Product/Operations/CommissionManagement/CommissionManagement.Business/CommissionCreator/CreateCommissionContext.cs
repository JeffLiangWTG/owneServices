using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class CreateCommissionContext
	{
		public CreateCommissionContext()
		{
			FromDate = ZDateTime.MinSmallDateTimeValue;
		}

		public ZDateTime FromDate { get; set; }

		public ZBool OverwriteOldValues { get; set; }

		public ZBool OnlyCreateForAgreementAndRatesOverrideStreams { get; set; }

		public bool RegeneratingCommissions { get; set; }

		public BusinessObjectFactory OverrideFactory { get; set; }

		public ZQuery GetCommissionHeaderStreamsFilter()
		{
			if (AgreementAndRatesOverride != null && OnlyCreateForAgreementAndRatesOverrideStreams)
			{
				return new ZQuery(AccCommissionHeaderSchema.CH0_CommissionStream, AgreementAndRatesOverride.Keys);
			}
			else
			{
				return new ZQuery();
			}
		}

		public string GetLogMessage()
		{
			var fromDate = FromDate == ZDateTime.MinSmallDateTimeValue ? "ALL" : FromDate.ToShortDateString();
			var reworkOldCommissions = OverwriteOldValues ? "Y" : "N";

			return $"From Date = {fromDate}, Rework Old Commissions = {reworkOldCommissions}";
		}

		public Dictionary<ZString, ICommissionAgreementAndRates> AgreementAndRatesOverride { get; set; }

		public ISet<OrgCommissionAgreement> AgreementsBeingApproved
		{
			get { return agreementsBeingApproved; }
			set
			{
				agreementsBeingApproved = value;
				pksOfAgreementsBeingApproved = agreementsBeingApproved != null ? new HashSet<ZGuid>(agreementsBeingApproved.Select(x => x.PK)) : null;
			}
		}
		ISet<OrgCommissionAgreement> agreementsBeingApproved;

		public ISet<ZGuid> PksOfAgreementsBeingApproved
		{
			get { return pksOfAgreementsBeingApproved; }
		}
		ISet<ZGuid> pksOfAgreementsBeingApproved;

		public ZBool OnlyCreateForAgreementsBeingApproved
		{
			get { return AgreementsBeingApproved != null; }
		}
	}
}
