using CargoWise.Types;
using Enterprise.Edifact.D99B.Messages.CUSRES;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class IMDRInfoProvider : D99BCUSRESInfoProvider
	{
		public IMDRInfoProvider(CUSRESMessage edifactMessage)
			: base(edifactMessage)
		{
		}

		public override ZString DocumentName
		{
			get { return "FID"; }
		}

		#region Monetary Amounts

		public ZDecimal TotalPayableAdmin
		{
			get { return TotalPayableAdminCore; }
		}

		public ZDecimal AQISContainerCharge
		{
			get { return AQISContainerChargeCore; }
		}

		public ZDecimal AQISProcessingCharge
		{
			get { return AQISProcessingChargeCore; }
		}

		public ZDecimal DeclarationProcessingCharge
		{
			get { return DeclarationProcessingChargeCore; }
		}

		public ZDecimal TotalWoodLevy
		{
			get { return TotalWoodLevyCore; }
		}

		public ZDecimal TotalTILV
		{
			get { return TotalTILVCore; }
		}

		public ZDecimal TotalPayable
		{
			get { return TotalPayableCore; }
		}

		public ZDecimal AQISServicePayment
		{
			get { return AQISServicePaymentCore; }
		}

		public ZDecimal TotalPayableDuty
		{
			get { return TotalPayableDutyCore; }
		}

		public ZDecimal TotalPayableWET
		{
			get { return TotalPayableWETCore; }
		}

		public ZDecimal TotalPayableLCT
		{
			get { return TotalPayableLCTCore; }
		}

		public ZDecimal TotalPayableGST
		{
			get { return TotalPayableGSTCore; }
		}

		public ZDecimal TotalDeferredGST
		{
			get { return TotalDeferredGSTCore; }
		}

		public ZDecimal TotalOtherCharges
		{
			get { return TotalOtherChargesCore; }
		}

		public ZDecimal TotalSecurityConcession
		{
			get { return TotalSecurityConcessionCore; }
		}

		public ZDecimal TotalSecurityLiability
		{
			get { return TotalSecurityLiabilityCore; }
		}

		#endregion
	}
}
