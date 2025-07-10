using CargoWise.EntityFramework;

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUpgradesToClientValidation
//
//    This class should be used for overriding validation in AutoUpgradesToClientValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.AutoDeploy.Business
{
	public class UpgradesToClientValidation : AutoUpgradesToClientValidation
	{
		public UpgradesToClientValidation(AutoUpgradesToClient parent)
			: base(parent)
		{
			ParentUpgradeToClient = (UpgradesToClient)parent;
		}

		#region UpgradeMethods

		protected override void CheckL1_RequestedUpgradeMethod()
		{
			base.CheckL1_RequestedUpgradeMethod();
			ListValidation.ErrorIfInvalidCode(Parent.L1_RequestedUpgradeMethodInfo, Parent.Lookups.UpgradeMethodsList);
		}

		protected override void CheckL1_ActualUpgradeMethod()
		{
			base.CheckL1_ActualUpgradeMethod();
			ListValidation.ErrorIfInvalidCode(Parent.L1_ActualUpgradeMethodInfo, Parent.Lookups.UpgradeMethodsList);
			if (ParentUpgradeToClient.LicDatabase != null)
			{
				if (!ParentUpgradeToClient.LicDatabase.IsUpgradeMethodSupported(Parent.L1_ActualUpgradeMethodInfo.Value.ToString()))
				{
					Parent.L1_ActualUpgradeMethodInfo.AddError("Actual Upgrade Method is not supported by the selected server.");
				}
			}
			else if (!Parent.L1_ActualUpgradeMethod.IsEmpty)
			{
				Parent.L1_ActualUpgradeMethodInfo.AddError("Actual Upgrade Method should be empty when no server selected.");
			}
		}

		#endregion

		#region CurrentStatus

		protected override void CheckL1_CurrentStatus()
		{
			base.CheckL1_CurrentStatus();
			ListValidation.ErrorIfInvalidCode(Parent.L1_CurrentStatusInfo, Parent.Lookups.UpgradeStatusList);
		}

		#endregion

		#region Implementation

		readonly UpgradesToClient ParentUpgradeToClient;

		#endregion
	}
}

