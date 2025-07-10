using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class ArrivalCusTransportMeansValidation : Customs.Business.CusTransportMeansValidation
	{
		public ArrivalCusTransportMeansValidation(ArrivalCusTransportMeans parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckRuleNR0081();
		}

		public new ArrivalCusTransportMeans Parent => base.Parent as ArrivalCusTransportMeans;

		protected IArrivalCusTransportMeansValidationDecider ValidationDecider => Parent.ValidationDecider;

		protected override void CheckTPM_TransportState()
		{
			base.CheckTPM_TransportState();

			if (Parent.Parent is NctsBill bill && bill.Header is NctsHeader nctsHeader && nctsHeader.IsArrivalMovement ||
				Parent.Parent is NctsCommonMovementHeader movementHeader && movementHeader.Header is NctsHeader header && header.IsArrivalMovement)
			{
				MandatoryValidation.CheckEntered(Parent.TPM_TransportStateInfo);
			}
			CheckRuleNR0082();
		}

		void CheckRuleNR0081()
		{
			var parent = Parent;
			var movementHeader = parent.MovementHeader;
			var arrivalTransportInfos = parent.Parent is NctsBill nctsBill ? nctsBill.ArrivalTransportInfos : parent.Parent is NctsArrivalMovementHeader arrivalMovementHeader ? arrivalMovementHeader.ArrivalTransportInfos : null;
			parent.RemoveRowMessageError(movementHeader.Header.Configuration.ValidationRuleConfiguration.Messages.NR0081Message);
			if (arrivalTransportInfos != null &&
				ValidationDecider is IArrivalCusTransportMeansPhase5ValidationDecider { IsRuleNR0081Active: true }	&&
				parent.TPM_TransportState == NctsUnloadedStateList.Codes.MIS && (parent.TPM_TypeOfIdentification == NctsTransportTypeOfIdList.Codes._20 || parent.TPM_TypeOfIdentification == NctsTransportTypeOfIdList.Codes._21) &&
				!arrivalTransportInfos.Any(x => x.TPM_TransportState == NctsUnloadedStateList.Codes.NEW))
			{
				parent.AddRowMessageError(movementHeader.Header.Configuration.ValidationRuleConfiguration.Messages.NR0081Message);
			}
		}

		void CheckRuleNR0082()
		{
			var parent = Parent;
			if (ValidationDecider is IArrivalCusTransportMeansPhase5ValidationDecider { IsRuleNR0082Active: true } &&
				parent.TPM_TransportState == NctsUnloadedStateList.Codes.DIF && parent.TPM_TypeOfIdentification == ZString.Empty && parent.TPM_IdentificationNumber == ZString.Empty && parent.TPM_RN_NKTransportNationality == ZString.Empty)
			{
				parent.TPM_TransportStateInfo.AddMessageError(Parent.MovementHeader.Header.Configuration.ValidationRuleConfiguration.Messages.NR0082Message);
			}
		}

		protected override void CheckTPM_TypeOfIdentification()
		{
			base.CheckTPM_TypeOfIdentification();

			var propertyInfo = Parent.TPM_TypeOfIdentificationInfo;

			ListValidation.ErrorIfInvalidCode(propertyInfo);

			CheckPropertyIsMandatory((x) => x.IsRuleTR0036Active, Parent.TPM_TypeOfIdentificationInfo, $"[{ValidationRuleCodeConstants.TR0036}] ");
		}

		protected override void CheckTPM_IdentificationNumber()
		{
			base.CheckTPM_IdentificationNumber();

			CheckPropertyIsMandatory((x) => x.IsRuleTR0037Active, Parent.TPM_IdentificationNumberInfo, $"[{ValidationRuleCodeConstants.TR0037}] ");
		}

		protected override void CheckTPM_RN_NKTransportNationality()
		{
			base.CheckTPM_RN_NKTransportNationality();

			var propertyInfo = Parent.TPM_RN_NKTransportNationalityInfo;

			ListValidation.ErrorIfInvalidCode(propertyInfo);

			CheckPropertyIsMandatory((x) => x.IsRuleTR0038Active, propertyInfo, $"[{ValidationRuleCodeConstants.TR0038}] ");
		}

		void CheckPropertyIsMandatory(Func<ValidationRuleConfiguration, bool> isRuleActive, ZPropertyInfo propertyInfo, ZString messagePrefix)
		{
			var movementHeader = Parent.MovementHeader;
			if (movementHeader != null && Parent.TPM_TransportState == NctsUnloadedStateList.Codes.NEW && isRuleActive(movementHeader.Header.Configuration.ValidationRuleConfiguration))
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, messagePrefix: messagePrefix);
			}
		}
	}
}
