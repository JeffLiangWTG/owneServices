using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsDepartureMovementHeaderPhase5Validation : EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Validation
	{
		public NctsDepartureMovementHeaderPhase5Validation(NctsDepartureMovementHeader parent)
			: base(parent)
		{
		}

		public new NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)base.Parent;

		protected new IFRNctsDepartureMovementHeaderPhase5ValidationDecider ValidationDecider => (IFRNctsDepartureMovementHeaderPhase5ValidationDecider)base.ValidationDecider;

		protected override void CheckTirCarnetExpiryDateMandatory()
		{
		}

		protected override void CheckGoodsLocationDescription()
		{
			base.CheckGoodsLocationDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.GoodsLocationDescriptionInfo);
		}

		protected override void CheckBM_ExportDate()
		{
			base.CheckBM_ExportDate();
			CheckRuleNAT103();
		}

		void CheckRuleNAT103()
		{
			if (ValidationDecider is IFRNctsDepartureMovementHeaderPhase5ValidationDecider decider && decider.IsRuleNAT103Active
				&& Parent.BM_ExportDate == ZDateTime.Empty)
			{
				Parent.BM_ExportDateInfo.AddMessageError(Res.GetString("811bafcd-e441-477b-b1b1-d3222d3f0a6f", "[NAT103] Date Limit cannot be empty."));
			}
		}

		protected override void CheckBM_MessageStatus()
		{
			base.CheckBM_MessageStatus();

			var parent = Parent;
			if (parent.BM_MessageStatus == EDIMessageStatusList.Codes.Rejected)
			{
				var rejectionDetails = parent.Header.GetLastFRMEventErrorDescription();

				if (!string.IsNullOrEmpty(rejectionDetails))
				{
					parent.BM_MessageStatusInfo.AddWarning(rejectionDetails);
				}
			}
		}

		protected override void CheckBM_PresentationDateTime()
		{
			base.CheckBM_PresentationDateTime();
			var parent = Parent;

			CheckRuleNAT050(parent);
		}

		void CheckRuleNAT050(NctsDepartureMovementHeader parent)
		{
			if (ValidationDecider.IsRuleNAT050Active && parent.BM_AdditionalDeclarationType == EU.NCTS.Business.NctsTypeOfAdditionalDeclarationList.Codes.D && parent.BM_PresentationDateTime.IsEmpty)
			{
				parent.BM_PresentationDateTimeInfo.AddMessageError(Res.GetString("63FA9E5E-D1B0-4AB2-AE3C-F63D40668D14", "[NAT050] A date is mandatory in case of Pre-Lodged declaration"));
			}
		}

		public void ValidateChargePaymentOrDestinationID()
		{
			ValidateCalculatedProperty(Parent.ChargePaymentOrDestinationIDInfo);
		}

		protected void CheckChargePaymentOrDestinationID()
		{
			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.ChargePaymentOrDestinationIDInfo);
			if (parent.ChargePaymentOrDestinationID.IsEmpty)
			{
				if (!UniversalReferenceDataHelper.GetChargePaymentOrDestinationID(Parent.Factory, Parent).IsEmpty())
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.ChargePaymentOrDestinationIDInfo);
				}
			}
		}
	}
}
