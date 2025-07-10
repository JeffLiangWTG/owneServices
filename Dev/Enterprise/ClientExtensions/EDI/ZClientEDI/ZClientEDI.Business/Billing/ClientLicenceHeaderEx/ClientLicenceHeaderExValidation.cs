//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientLicenceHeaderExValidation
//
//    This class should be used for overriding validation in AutoClientLicenceHeaderExValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	using CargoWise.EntityFramework;

	public class ClientLicenceHeaderExValidation : AutoClientLicenceHeaderExValidation
	{
		public ClientLicenceHeaderExValidation(AutoClientLicenceHeaderEx parent) : base(parent)
		{
		}

		protected override void CheckL0_NextMaintenancePercent()
		{
			base.CheckL0_NextMaintenancePercent();
			MandatoryValidation.CheckNotNegative(Parent.L0_NextMaintenancePercentInfo);
			CompareValidation.CheckLessThanOrEqualTo(Parent.L0_NextMaintenancePercentInfo, 99m);
		}

		protected override void CheckL0_LastNewSeatMaintenancePercent()
		{
			base.CheckL0_LastNewSeatMaintenancePercent();
			MandatoryValidation.CheckNotNegative(Parent.L0_NextNewSeatMaintenancePercentInfo);
			CompareValidation.CheckLessThanOrEqualTo(Parent.L0_NextNewSeatMaintenancePercentInfo, 99m);
		}

		protected override void CheckL0_RX_NKFixedMaintenanceCurrency()
		{
			if (Parent.L0_FixedMaintenanceAmount != 0m)
			{
				MandatoryValidation.CheckEntered(Parent.L0_RX_NKFixedMaintenanceCurrencyInfo);
			}
		}

		protected override void CheckL0_Surcharge()
		{
			base.CheckL0_Surcharge();
			CompareValidation.CheckLessThanOrEqualTo(Parent.L0_SurchargeInfo, 100m);
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.L0_SurchargeInfo, -100m);
		}
	}
}

