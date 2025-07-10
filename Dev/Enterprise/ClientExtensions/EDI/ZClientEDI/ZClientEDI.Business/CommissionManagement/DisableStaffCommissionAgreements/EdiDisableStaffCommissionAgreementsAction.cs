using System;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.CommissionManagement.Business
{
	public class EdiDisableStaffCommissionAgreementsAction : DisableStaffCommissionAgreementsAction
	{
		#region New
		public new static EdiDisableStaffCommissionAgreementsAction New(GlbStaff staff)
		{
			return new EdiDisableStaffCommissionAgreementsAction(staff);
		}

		#endregion

		#region Register/Unregister SubType Override

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		protected EdiDisableStaffCommissionAgreementsAction(GlbStaff staff)
			: base(staff)
		{
		}

		protected override CreateCommissionContext CreateContext()
		{
			var context = new CreateCommissionContext();
			context.OverwriteOldValues = true;

			var registryDate = OrganisationRegistry.Instance.DefaultSpecifiedBackdate.Value;
			if (registryDate != DateTime.MinValue)
			{
				context.FromDate = new ZDateTime(registryDate);
			}
			return context;
		}
	}
}
