using System.Collections.Generic;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer
{
	internal class UpdatedChargesStrategy : AllChargesStrategy
	{
		public UpdatedChargesStrategy(IValueObjectImportContext context, bool ignorePosted)
			: base(context, ignorePosted)
		{
			matchedChargeCodes = new HashSet<AccChargeCode>();
		}

		public override Charge MatchCharge(Job job, AccChargeCode chargeCode, GlbBranch branch, GlbDepartment department, Xsd.ChargeLine line)
		{
			Charge result = base.MatchCharge(job, chargeCode, branch, department, line);

			if (result != null)
			{
				matchedChargeCodes.Add(chargeCode);
			}

			return result;
		}

		protected override bool IsChargeSafeToRemove(Charge charge)
		{
			return base.IsChargeSafeToRemove(charge)
				&& matchedChargeCodes.Contains(charge.ChargeCode);
		}

		readonly HashSet<AccChargeCode> matchedChargeCodes;
	}
}
